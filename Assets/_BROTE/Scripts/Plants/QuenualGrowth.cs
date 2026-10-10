using System.Collections;
using UnityEngine;
public sealed class QuenualGrowth : MonoBehaviour
{
    [SerializeField] private SpriteRenderer seedling;
    [SerializeField] private SpriteRenderer young;
    [SerializeField] private SpriteRenderer developed;
    [SerializeField, Min(0f)] private float secondsToYoung = 5f;
    [SerializeField, Min(0f)] private float secondsToDeveloped = 5f;
    [SerializeField, Min(0f)] private float restorationPoints = 10f;
    private RestorationManager manager;
    private bool initialized;
    private bool contributed;
    public static event System.Action<QuenualGrowth> Matured;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSessionEvents()=>Matured=null;
    public PlantingSpot OwningSpot { get; internal set; }
    public int Stage { get; private set; }
    public event System.Action<int> StageChanged;
    public SpriteRenderer CurrentStageRenderer => Stage == 0 ? seedling : Stage == 1 ? young : developed;
    private void Awake()
    {
        if (seedling == null || young == null || developed == null)
            BroteDiagnostics.WarnOnce(this, "stages", "QuenualGrowth needs all three stage renderers.");
        ShowStage(0);
    }
    public void Initialize(RestorationManager restorationManager)
    {
        if (initialized) return;
        if (restorationManager == null)
        {
            BroteDiagnostics.WarnOnce(this, "manager", "QuenualGrowth Initialize received no manager.");
            return;
        }
        initialized = true;
        manager = restorationManager;
        NotifyStageChanged();
        StartCoroutine(Grow());
    }
    // Silent reconstruction: a Developed tree has already contributed.
    public void RestoreSavedStage(RestorationManager restorationManager, int stage)
    {
        stage = Mathf.Clamp(stage, 0, 2);
        StopAllCoroutines(); manager = restorationManager;
        initialized = false; contributed = stage == 2;
        ShowStage(Mathf.Clamp(stage, 0, 2)); initialized = true;
        if (Stage < 2) StartCoroutine(Grow());
    }
    public void PublishRestoredMaturity()
    {
        if (Stage == 2) BroteEvents.Notify(Matured, this, this);
    }
    private IEnumerator Grow()
    {
        if (Stage == 0)
        {
            yield return new WaitForSeconds(secondsToYoung);
            ShowStage(1);
        }
        yield return new WaitForSeconds(secondsToDeveloped);
        ShowStage(2);
        if (!contributed && manager != null)
        {
            contributed = true;
            BroteEvents.Notify(Matured, this, this);
            manager.AddRestoration(restorationPoints);
        }
    }
    private void ShowStage(int stage)
    {
        Stage = stage;
        if (seedling != null) seedling.enabled = stage == 0;
        if (young != null) young.enabled = stage == 1;
        if (developed != null) developed.enabled = stage == 2;
        if (initialized) NotifyStageChanged();
    }
    private void NotifyStageChanged() => BroteEvents.Notify(StageChanged, Stage, this);
}
