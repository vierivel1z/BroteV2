using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Presentation only: never advances growth or awards restoration.
[RequireComponent(typeof(QuenualGrowth))]
public sealed class QuenualGrowthFeedback : MonoBehaviour
{
    [SerializeField] private Material particleMaterial;
    [SerializeField] private Sprite dustSprite;
    [UnityEngine.Serialization.FormerlySerializedAs("leafSprite")]
    [SerializeField, HideInInspector] private Sprite legacyLeafSprite;
    [SerializeField] private Sprite pulseSprite;
    [SerializeField, Range(1, 3)] private int maxFullEffects = 3;
    [SerializeField] private Color dustColor = new Color(0.55f, 0.4f, 0.23f, 0.45f);
    [SerializeField] private Color leafColor = new Color(0.45f, 0.78f, 0.35f, 0.9f);
    [SerializeField] private Color pulseColor = new Color(0.4f, 0.82f, 0.47f, 0.42f);
    [Header("Fallback pop values; Resources/VFX presets take precedence")]
    [SerializeField, Range(0.2f, 0.3f)] private float plantPopDuration = 0.26f;
    [SerializeField, Range(0.2f, 0.45f)] private float youngPopDuration = 0.32f;
    [SerializeField, Range(0.25f, 0.45f)] private float developedPopDuration = 0.38f;
    [SerializeField] private AudioClip plantedClip, youngClip, developedClip;
    private static readonly GrowthVFXLimiter budget = new GrowthVFXLimiter(3);
    private struct Pending { public QuenualGrowthFeedback Owner; public int Stage; public float Distance; public bool Visible; }
    private static readonly List<Pending> pending = new List<Pending>(10);
    private static readonly Plane[] frustumPlanes = new Plane[6];
    private static readonly System.Comparison<Pending> NearestFirst = (a, b) => a.Distance.CompareTo(b.Distance);
    private GrowthVFXLimiter.Lease lease;
    private static readonly Sprite[] proceduralSprites = new Sprite[3];
    private QuenualGrowth growth;
    private BroteGrowthVFX pooledEffect;
    private Coroutine animation;
    private Transform animatedStage;
    private Vector3 originalScale;
    private bool holdsSlot;
    private int lastAudioStage=-1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession()
    {
        budget.Clear();
        pending.Clear();
        for (int i = 0; i < proceduralSprites.Length; i++)
        {
            if (proceduralSprites[i] != null)
            {
                Destroy(proceduralSprites[i].texture);
                Destroy(proceduralSprites[i]);
            }
            proceduralSprites[i] = null;
        }
    }

    private void OnEnable()
    {
        growth = GetComponent<QuenualGrowth>();
        var clips=Resources.Load<BroteSFXClips>("Audio/BroteSFXClips");
        if(clips!=null)
        {
            if(plantedClip==null)plantedClip=clips.plant;
            if(youngClip==null)youngClip=clips.growth;
            if(developedClip==null)developedClip=clips.growth;
        }
        lastAudioStage=growth.Stage>0?growth.Stage:-1;
        growth.StageChanged += OnStageChanged;
    }

    private void OnStageChanged(int stage)
    {
        try { QueueStage(stage); }
        catch
        {
            CancelFeedback();
            throw; // Growth's notification isolates optional presentation failures.
        }
    }

    private void QueueStage(int stage)
    {
        CancelFeedback();
        SpriteRenderer renderer = growth.CurrentStageRenderer;
        if (renderer == null) return;
        Camera view = Camera.main;
        bool visible = false;
        float distance = float.PositiveInfinity;
        if (view != null)
        {
            GeometryUtility.CalculateFrustumPlanes(view, frustumPlanes);
            visible = GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds);
            distance = ((Vector2)renderer.bounds.center - (Vector2)view.transform.position).sqrMagnitude;
        }
        pending.Add(new Pending { Owner = this, Stage = stage, Visible = visible, Distance = distance });
    }

    private void LateUpdate()
    {
        if(pooledEffect!=null)
        {
            var view=Camera.main;var renderer=growth.CurrentStageRenderer;
            if(view==null||renderer==null){BroteVFXPool.Release(pooledEffect);pooledEffect=null;}
            else {GeometryUtility.CalculateFrustumPlanes(view,frustumPlanes);if(!GeometryUtility.TestPlanesAABB(frustumPlanes,renderer.bounds)){BroteVFXPool.Release(pooledEffect);pooledEffect=null;}}
        }
        if (pending.Count == 0) return;
        // All stage events from this frame compete before any particle systems are allocated.
        pending.Sort(NearestFirst);
        for (int i = 0; i < pending.Count; i++)
        {
            Pending request = pending[i];
            if (request.Owner == null || !request.Owner.isActiveAndEnabled) continue;
            try { request.Owner.PlayStage(request.Stage, request.Visible); }
            catch (System.Exception exception)
            {
                request.Owner.CancelFeedback();
                Debug.LogException(exception, request.Owner);
                // Cancellation removes this owner from pending; continue without skipping the next.
                i--;
            }
        }
        pending.Clear();
    }

    private void PlayStage(int stage, bool visible)
    {
        SpriteRenderer renderer = growth.CurrentStageRenderer;
        if (renderer == null) return;
        bool full = visible && budget.ActiveCount(Time.time) < Mathf.Clamp(maxFullEffects, 1, 3)
            && budget.TryAcquire(Time.time, 0.5f, out lease);
        holdsSlot = full;
        animatedStage = renderer.transform;
        originalScale = animatedStage.localScale;
        // Stage0 is queued during Initialize, before inventory consumption.
        // Request it only after the spot has confirmed this tree as a successful planting.
        var spot=growth.OwningSpot!=null?growth.OwningSpot:GetComponentInParent<PlantingSpot>();
        bool planted=stage==0 && spot!=null && spot.State==PlantingSpot.SpotState.PLANTED && spot.PlantedTree==growth;
        if(stage==growth.Stage && stage!=lastAudioStage)
        {
            lastAudioStage=stage;
            if(planted || (stage>0 && visible))
                BroteAudioFeedback.PlayOptional(stage==0?plantedClip:stage==1?youngClip:developedClip,transform.position,stage>0);
        }
        if (full) EmitStage(stage);
        animation = StartCoroutine(Animate(stage, full));
    }

    private IEnumerator Animate(int stage, bool full)
    {
        var preset=full?BroteVFXPool.Preset(stage):null;
        float duration = full ? (stage == 0 ? plantPopDuration : stage == 1 ? youngPopDuration : developedPopDuration) : 0.18f;
        if(preset!=null)duration=preset.settings.popDuration;
        duration = Mathf.Clamp(duration, 0.1f, 0.45f);
        float start = full ? (stage == 0 ? 0.85f : 0.9f) : 0.97f;
        float peak = full ? (stage == 2 ? 1.08f : 1.05f) : 1.02f;
        if(full&&preset!=null){start=preset.settings.startScale;peak=preset.settings.peakScale;}
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float factor = BroteTween.PopScale(elapsed, duration, start, peak, stage == 0 ? 0.4f : stage == 1 ? 0.5f : 0.48f);
            if (animatedStage != null) animatedStage.localScale = originalScale * factor;
            elapsed += Time.deltaTime;
            yield return null;
        }
        RestoreScale();
        // All particles expire within this reserved window, including the final glow.
        if (full) yield return new WaitForSeconds(Mathf.Max(0f, 0.48f - duration));
        ReleaseSlot();
        animation = null;
    }

    private void EmitStage(int stage)
    {
        var preset=BroteVFXPool.Preset(stage);
        if(preset==null){BroteDiagnostics.WarnOnce(this,"vfxPreset","Stage VFX resource missing; gameplay remains active.");return;}
        pooledEffect=BroteVFXPool.Acquire(stage,transform);
        if(pooledEffect==null)return;
        pooledEffect.Play(preset.settings,particleMaterial,dustSprite,legacyLeafSprite,pulseSprite,dustColor,leafColor,pulseColor,growth.CurrentStageRenderer.sortingLayerID,stage==0?.12f:Mathf.Clamp(growth.CurrentStageRenderer.bounds.center.y-transform.position.y,.25f,.8f));
    }
    public static Sprite GetProceduralSprite(int kind)
    {
        if (proceduralSprites[kind] != null) return proceduralSprites[kind];
        const int size = 64;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = kind == 0 ? "BroteDust" : kind == 1 ? "BroteLeaf" : "BrotePulse";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float u = (x + 0.5f) / size * 2f - 1f;
            float v = (y + 0.5f) / size * 2f - 1f;
            float r = Mathf.Sqrt(u * u + v * v);
            float alpha = kind == 0 ? Mathf.Clamp01((1f - r) * 1.5f)
                : kind == 1 ? Mathf.Clamp01((1f - Mathf.Abs(u) / 0.45f - v * v) * 8f)
                : Mathf.Clamp01(1f - Mathf.Abs(r - 0.78f) / 0.12f) * 0.75f;
            pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
        }
        texture.SetPixels(pixels);
        texture.Apply(false, true);
        proceduralSprites[kind] = Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, size, 0, SpriteMeshType.FullRect);
        return proceduralSprites[kind];
    }

    private void RestoreScale()
    {
        if (animatedStage != null) animatedStage.localScale = originalScale;
        animatedStage = null;
    }
    private void ReleaseSlot()
    {
        if (!holdsSlot) return;
        if(pooledEffect!=null){BroteVFXPool.Release(pooledEffect);pooledEffect=null;}
        budget.Release(lease);
        holdsSlot = false;
    }
    private void CancelFeedback()
    {
        for (int i = pending.Count - 1; i >= 0; i--) if (pending[i].Owner == this) pending.RemoveAt(i);
        if (animation != null) StopCoroutine(animation);
        animation = null;
        RestoreScale();
        ReleaseSlot();

    }
    private void OnDisable()
    {
        if (growth != null) growth.StageChanged -= OnStageChanged;
        CancelFeedback();
    }
}
