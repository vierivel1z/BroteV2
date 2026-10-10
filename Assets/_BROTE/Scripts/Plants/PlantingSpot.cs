using UnityEngine;
public sealed class PlantingSpot : MonoBehaviour, IInteractable
{
    public enum SpotState { PREPARED, PLANTED, LOCKED, AVAILABLE }
    [SerializeField] private QuenualGrowth plantPrefab;
    [SerializeField] private RestorationManager restorationManager;
    [SerializeField] private Transform plantingPoint;
    [SerializeField] private SeedlingInventory promptInventory;
    private SpotState state;
    private bool planting;
    private BroteIncrementalSession progression;
    private int lastPreparationFrame=-1;
    public int Zone { get; private set; }
    public int PreparationActions { get; private set; }
    public bool MinkaRewarded { get; set; }
    public bool HasReferences => plantPrefab!=null && restorationManager!=null;
    private SpriteRenderer[] spotRenderers, legacySpotRenderers;
    private Transform runtimeTreeRoot;
    private Color[] originalColors;
    private Sprite[] originalSprites;
    private Vector3[] originalVisualScales, originalVisualPositions;
    private PlantingSpotArt spotArt;
    public void ConfigureProgression(BroteIncrementalSession session,int zone,SpotState initial)
    { progression=session; Zone=zone; SetProgressState(initial,0,false); }
    public void SetProgressState(SpotState value,int actions,bool rewarded)
    {
        state=value; PreparationActions=actions; MinkaRewarded=rewarded;
        RefreshSpotVisual();
    }
    private void RefreshSpotVisual()
    {
        if(spotRenderers==null)
        {
            legacySpotRenderers=System.Array.FindAll(GetComponentsInChildren<SpriteRenderer>(true),x=>x.GetComponentInParent<QuenualGrowth>()==null);
            var primary=System.Array.Find(legacySpotRenderers,x=>x.transform==transform);
            if(primary==null && legacySpotRenderers.Length>0)primary=legacySpotRenderers[0];
            spotRenderers=primary!=null?new[]{primary}:System.Array.Empty<SpriteRenderer>();
            // Keep collider/root scale intact while pulsing only the existing artwork.
            for(int i=0;i<spotRenderers.Length;i++) if(spotRenderers[i].transform==transform)
            {
                var source=spotRenderers[i]; var visual=new GameObject("ProgressionSpotVisual");visual.transform.SetParent(transform,false);
                var copy=visual.AddComponent<SpriteRenderer>();copy.sprite=source.sprite;copy.color=source.color;copy.sharedMaterials=source.sharedMaterials;
                copy.sortingLayerID=source.sortingLayerID;copy.sortingOrder=source.sortingOrder;copy.spriteSortPoint=source.spriteSortPoint;
                copy.flipX=source.flipX;copy.flipY=source.flipY;copy.drawMode=source.drawMode;copy.size=source.size;copy.maskInteraction=source.maskInteraction;
                source.enabled=false;spotRenderers[i]=copy;
            }
            originalColors=System.Array.ConvertAll(spotRenderers,x=>x.color);
            originalSprites=System.Array.ConvertAll(spotRenderers,x=>x.sprite);
            originalVisualScales=System.Array.ConvertAll(spotRenderers,x=>x.transform.localScale);
            originalVisualPositions=System.Array.ConvertAll(spotRenderers,x=>x.transform.localPosition);
            spotArt=Resources.Load<PlantingSpotArt>("Art/PlantingSpotArt");
            if(spotArt==null)BroteDiagnostics.WarnOnce(this,"spotArt","PlantingSpot official art configuration is missing.");
        }
        foreach(var legacy in legacySpotRenderers)if(legacy!=null && System.Array.IndexOf(spotRenderers,legacy)<0)legacy.enabled=false;
        Sprite official=spotArt!=null?spotArt.ForState(state):null;
        for(int i=0;i<spotRenderers.Length;i++) if(spotRenderers[i]!=null)
        {
            var renderer=spotRenderers[i];
            renderer.enabled=state!=SpotState.PLANTED && official!=null;
            renderer.sprite=official;
            renderer.color=official!=null?Color.white:originalColors[i];
            // Only the artwork changes: retain the original footprint and center, uniformly.
            // Root transforms, collider and plantingPoint remain untouched.
            float ratio=official!=null && originalSprites[i]!=null && official.bounds.size.x>0
                ?originalSprites[i].bounds.size.x/official.bounds.size.x:1f;
            Vector3 scale=originalVisualScales[i]*ratio;
            renderer.transform.localScale=scale;
            Vector3 before=originalSprites[i]!=null?originalSprites[i].bounds.center:Vector3.zero;
            Vector3 after=renderer.sprite!=null?renderer.sprite.bounds.center:Vector3.zero;
            if(renderer.flipX){before.x=-before.x;after.x=-after.x;}
            if(renderer.flipY){before.y=-before.y;after.y=-after.y;}
            renderer.transform.localPosition=originalVisualPositions[i]
                +Vector3.Scale(before,originalVisualScales[i])-Vector3.Scale(after,scale);
        }
    }
    [SerializeField] private string stableId;
    public string StableId => stableId;
    public QuenualGrowth PlantedTree { get; private set; }
    public static event System.Action<PlantingSpot> Planted;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSessionEvents()=>Planted=null;
    public void RestoreSavedTree(bool planted, int stage)
    {
        if (PlantedTree != null)
        {
            PlantedTree.gameObject.SetActive(false); Destroy(PlantedTree.gameObject);
        }
        PlantedTree = null; state = SpotState.PREPARED;
        if (!planted) { RefreshSpotVisual(); return; }
        Transform point = plantingPoint != null ? plantingPoint : transform;
        PlantedTree = SpawnTree(point.position);
        PlantedTree.RestoreSavedStage(restorationManager, stage); state = SpotState.PLANTED; RefreshSpotVisual();
    }
    // An unscaled ancestor preserves sector ownership without inheriting the spot artwork transform.
    // The spot still owns its lifetime; physics and plantingPoint remain unchanged.
    private QuenualGrowth SpawnTree(Vector3 position)
    {
        if(runtimeTreeRoot==null)
        {
            runtimeTreeRoot=new GameObject("RuntimeTree_"+stableId).transform;
            Transform parent=transform.parent;
            while(parent!=null && ((parent.lossyScale-Vector3.one).sqrMagnitude>.000001f || Quaternion.Angle(parent.rotation,Quaternion.identity)>.001f))parent=parent.parent;
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(runtimeTreeRoot.gameObject,gameObject.scene);
            runtimeTreeRoot.SetParent(parent,false);
        }
        runtimeTreeRoot.SetPositionAndRotation(position,Quaternion.identity);runtimeTreeRoot.localScale=Vector3.one;
        var tree=Instantiate(plantPrefab,runtimeTreeRoot,false);
        tree.OwningSpot=this;
        tree.transform.localPosition=Vector3.zero;tree.transform.localRotation=Quaternion.identity;
        tree.transform.localScale=plantPrefab.transform.localScale;
        return tree;
    }
    private void OnEnable(){if(runtimeTreeRoot!=null)runtimeTreeRoot.gameObject.SetActive(true);}
    private void OnDisable(){if(runtimeTreeRoot!=null)runtimeTreeRoot.gameObject.SetActive(false);}
    private void OnDestroy(){if(runtimeTreeRoot!=null)Destroy(runtimeTreeRoot.gameObject);}
    public SpotState State => state;
    public bool CanInteract => isActiveAndEnabled && !planting && (state == SpotState.PREPARED || state == SpotState.AVAILABLE) && HasReferences;
    public Vector2 InteractionPosition => transform.position;
    // This slice has one player; this reference supplies the prompt before interacting.
    public string InteractionPrompt => state==SpotState.AVAILABLE ? $"[E] Preparar terreno {PreparationActions}/{(progression!=null ? progression.Upgrades.Value(BroteUpgrade.Soil) : 3)}" : promptInventory != null && promptInventory.CurrentSeedlings > 0
        ? "[E] Plantar queñual"
        : "Necesitas una plántula de queñual";
    private void Awake()
    {
        RefreshSpotVisual();
        if (plantPrefab == null) BroteDiagnostics.WarnOnce(this, "prefab", "PlantingSpot has no Quenual prefab.");
        if (restorationManager == null) BroteDiagnostics.WarnOnce(this, "manager", "PlantingSpot has no RestorationManager.");
        if (promptInventory == null) BroteDiagnostics.WarnOnce(this, "inventory", "PlantingSpot has no prompt inventory.");
    }
    public void Interact(PlayerInteraction player)
    {
        if (!CanInteract || player == null || lastPreparationFrame==Time.frameCount || BrotePauseMenu.IsPaused || Time.timeScale<=0) return;
        if(state==SpotState.AVAILABLE)
        {
            if(lastPreparationFrame==Time.frameCount) return;
            lastPreparationFrame=Time.frameCount; PreparationActions++;
            int required=progression!=null ? progression.Upgrades.Value(BroteUpgrade.Soil) : 3;
            player.ShowFeedback($"Preparando terreno {PreparationActions}/{required}");
            if(PreparationActions>=required)
            { SetProgressState(SpotState.PREPARED,PreparationActions,false); progression?.Prepared(this); StartCoroutine(PreparationFeedback()); }
            progression?.GetComponent<BroteSaveSystem>()?.RequestSave(); return;
        }
        planting = true;
        try { Plant(player); }
        finally { planting = false; }
    }
    private System.Collections.IEnumerator PreparationFeedback()
    {
        var effect=BroteVFXPool.Acquire(0,transform);
        int layer=spotRenderers.Length>0 ? spotRenderers[0].sortingLayerID : 0;
        if(effect!=null)effect.Play(new BroteVFXSettings{dustCount=4,leafCount=0,emitPulse=false},null,null,null,null,new Color(.7f,.6f,.45f),Color.white,Color.white,layer,.1f);
        var renderer=spotRenderers.Length>0 ? spotRenderers[0] : null;
        Vector3 scale=renderer!=null ? renderer.transform.localScale : Vector3.one;
        float age=0;
        while(age<.26f)
        { age+=Time.deltaTime;if(renderer!=null && renderer.transform!=transform)renderer.transform.localScale=scale*(1+.05f*Mathf.Sin(Mathf.Clamp01(age/.26f)*Mathf.PI));yield return null; }
        if(renderer!=null && renderer.transform!=transform)renderer.transform.localScale=scale;
        yield return new WaitForSeconds(.25f);BroteVFXPool.Release(effect);
    }
    private void Plant(PlayerInteraction player)
    {
        SeedlingInventory inventory = player.GetComponent<SeedlingInventory>();
        if (inventory == null)
        {
            BroteDiagnostics.WarnOnce(this, "playerInventory", "Interacting player has no SeedlingInventory.");
            return;
        }
        if (inventory.CurrentSeedlings < 1)
        {
            player.ShowFeedback("Necesitas una plántula de queñual");
            return;
        }
        Transform point = plantingPoint != null ? plantingPoint : transform;
        QuenualGrowth plant = null;
        try
        {
            plant = SpawnTree(point.position);
            if (plant == null) return;
            if (!plant.isActiveAndEnabled)
            {
                Destroy(plant.gameObject);
                return;
            }
            plant.Initialize(restorationManager);
        }
        catch (System.Exception exception)
        {
            if (plant != null)
            {
                plant.gameObject.SetActive(false);
                Destroy(plant.gameObject);
            }
            Debug.LogException(exception, this);
            return;
        }
        // Lock the spot before the inventory event can notify other components.
        state = SpotState.PLANTED;
        if (!inventory.TryConsumeSeedling())
        {
            state = SpotState.PREPARED;
            plant.gameObject.SetActive(false);
            Destroy(plant.gameObject);
            return;
        }
        PlantedTree = plant;
        RefreshSpotVisual();
        BroteEvents.Notify(Planted, this, this);
        player.ShowFeedback("Que\u00f1ual plantado");
        Debug.Log("Queñual plantado.", this);
    }
}
