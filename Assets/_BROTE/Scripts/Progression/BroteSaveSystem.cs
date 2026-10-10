using System;
using System.IO;
using System.Linq;
using UnityEngine;

// One local save, one sector. Loading is restricted to a fresh scene.
public sealed class BroteSaveSystem : MonoBehaviour
{
#if UNITY_EDITOR
    // Native integration tests use an isolated temporary file, never the player's save.
    private static string editorTestSavePath;
    public static void ConfigureEditorSavePath(string path)=>editorTestSavePath=path;
#endif
    public static string SavePath
    {
        get
        {
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(editorTestSavePath)) return editorTestSavePath;
#endif
            return Path.Combine(Application.persistentDataPath, "brote-sector01.json");
        }
    }
    public static bool IsLoading { get; private set; }
    private PlantingSpot[] spots;
    private SeedlingInventory inventory;
    private RestorationManager manager;
    private BroteTutorialState tutorial;
    private BroteIncrementalSession incremental;
    private bool ready, dirty, preserveInvalidSave, loaded, suppressWrites;
    private static bool newGameRequested;
    public string LastError { get; private set; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { IsLoading=false;newGameRequested=false; }
    public static BroteSaveSystem Current => FindObjectsByType<BroteSaveSystem>().FirstOrDefault(x => x.gameObject.scene.name == "Acopalca");
    private void Start()
    {
        if (gameObject.scene.name != "Acopalca") { enabled = false; return; }
        spots = FindObjectsByType<PlantingSpot>().Where(x => x.gameObject.scene == gameObject.scene).OrderBy(x => x.StableId).ToArray();
        inventory = FindObjectsByType<SeedlingInventory>().FirstOrDefault(x => x.gameObject.scene == gameObject.scene);
        manager = FindObjectsByType<RestorationManager>().FirstOrDefault(x => x.gameObject.scene == gameObject.scene);
        tutorial = GetComponent<BroteTutorialState>();
        incremental=GetComponent<BroteIncrementalSession>();
        ready = spots.Length == 10 && inventory != null && manager != null && spots.Select(x => x.StableId).Distinct().Count() == 10;
        if (!ready) { LastError = "Referencias de guardado incompletas"; enabled = false; return; }
        // A confirmed New Game wins even if a stale file is recreated during transition.
        bool startNew=newGameRequested;newGameRequested=false;
        if(startNew)dirty=true;
        else if (File.Exists(SavePath)) preserveInvalidSave = !Load();
        PlantingSpot.Planted += OnPlanted;
        inventory.SeedlingsChanged += OnInventory;
        manager.RestorationChanged += OnRestoration;
        foreach (var spot in spots) if (spot.PlantedTree != null) spot.PlantedTree.StageChanged += OnStage;
    }
    private void OnPlanted(PlantingSpot spot)
    {
        if (!spots.Contains(spot)) return;
        spot.PlantedTree.StageChanged += OnStage; dirty = true;
    }
    private void OnInventory(int _) => dirty = true;
    private void OnRestoration(float _) => dirty = true;
    private void OnStage(int _) => dirty = true;
    public void RequestSave() => dirty = true;
    private void LateUpdate() { if (dirty && !IsLoading && !preserveInvalidSave && !suppressWrites) Save(); }
    public BroteSaveData Capture()
    {
        var data=new BroteSaveData {
            inventory=inventory.CurrentSeedlings,restoration=manager.CurrentRestoration,
            tutorialCompleted=tutorial!=null && tutorial.Completed,
            spots=spots.Select(x=>new BroteSavedSpot { id=x.StableId,planted=x.State==PlantingSpot.SpotState.PLANTED,
                stage=x.PlantedTree!=null ? x.PlantedTree.Stage : 0,state=(int)x.State,preparationActions=x.PreparationActions,
                minkaRewarded=x.PlantedTree!=null && x.PlantedTree.Stage==2 }).ToArray()
        };
        incremental?.Capture(data); return data;
    }
    private bool ValidZones(BroteSaveData data)
    {
        if(data.saveVersion!=2 || incremental==null)return true;
        return data.spots.All(record=>
        {
            var spot=spots.First(x=>x.StableId==record.id);
            return (record.state==(int)PlantingSpot.SpotState.LOCKED)==(spot.Zone>data.expansionLevel);
        });
    }
    #if UNITY_EDITOR || DEVELOPMENT_BUILD
    // Snapshot coherent progress before QA changes health without planting trees.
    // Suppression lasts only for this scene/session, including manual and quit saves.
    public void BeginRestorationDebugPreview()
    {
        if(suppressWrites)return;
        Save();suppressWrites=true;dirty=false;
    }
#endif
    public bool Save()
    {
        if (!ready || IsLoading || preserveInvalidSave || suppressWrites) return false;
        try
        {
            var data = Capture(); if (!data.IsValid() || !ValidZones(data)) throw new InvalidDataException("Estado incoherente; guardado cancelado");
            Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
            string temp = SavePath + ".tmp";
            File.WriteAllText(temp, JsonUtility.ToJson(data, true));
            if (File.Exists(SavePath)) File.Replace(temp, SavePath, null); else File.Move(temp, SavePath);
            dirty = false; LastError = null; return true;
        }
        catch (Exception e) { dirty = false; LastError = e.Message; Debug.LogWarning("BROTE: no se pudo guardar: " + e.Message, this); return false; }
    }
    public bool Load()
    {
        if (!ready || loaded || IsLoading || spots.Any(x => x.PlantedTree != null)) return false;
        BroteSaveData data;
        try { data = JsonUtility.FromJson<BroteSaveData>(File.ReadAllText(SavePath)); if (data == null || !data.IsValid()) throw new InvalidDataException("Save incompatible o corrupto"); }
        catch (Exception e) { LastError = e.Message; Debug.LogWarning("BROTE: " + LastError, this); return false; }
        IsLoading = true;
        try
        {
            // Validate every prefab before any mutation.
            foreach (var spot in spots) if (!spot.HasReferences) throw new InvalidDataException("Spot sin referencias");
            if(data.saveVersion==1 && incremental!=null)
            {
                if(!File.Exists(SavePath+".v1.backup")) File.Copy(SavePath,SavePath+".v1.backup");
                data.MigrateV1();
            }
            if(data.saveVersion==2 && !ValidZones(data))throw new InvalidDataException("Estados de zona incoherentes");
            if(data.saveVersion==2) incremental?.Restore(data);
            foreach(var record in data.spots)
            {
                var spot=spots.First(x=>x.StableId==record.id);spot.RestoreSavedTree(record.planted,record.stage);
                if(data.saveVersion==2) spot.SetProgressState((PlantingSpot.SpotState)record.state,record.preparationActions,record.minkaRewarded);
            }
            inventory.RestoreSavedCount(data.inventory);
            if (tutorial != null) tutorial.RestoreCompleted(data.tutorialCompleted);
            // Publish maturity for Ground only; no +10 or stage/audio feedback.
            foreach (var spot in spots) if (spot.PlantedTree != null) spot.PlantedTree.PublishRestoredMaturity();
            manager.RestoreSavedPoints(data.restoration);
            loaded=true; dirty = data.saveVersion==2; LastError = null; return true;
        }
        catch (Exception e) { LastError = e.Message; Debug.LogException(e, this); return false; }
        finally { IsLoading = false; }
    }
    public static bool HasValidSave()
    {
        try { var data = JsonUtility.FromJson<BroteSaveData>(File.ReadAllText(SavePath)); return data != null && data.IsValid(); }
        catch { return false; }
    }
    // Call only from a confirmed New Game action.
    public static bool DeleteConfirmedSave()
    {
        try { if (File.Exists(SavePath)) File.Delete(SavePath); var current=Current; if(current!=null)current.suppressWrites=true;newGameRequested=true; return true; }
        catch (Exception e) { Debug.LogWarning("BROTE: no se pudo borrar: " + e.Message); return false; }
    }
    private void OnApplicationPause(bool paused) { if (paused && dirty) Save(); }
    private void OnApplicationQuit() { if (ready) Save(); }
    private void OnDestroy()
    {
        PlantingSpot.Planted -= OnPlanted;
        if (inventory != null) inventory.SeedlingsChanged -= OnInventory;
        if (manager != null) manager.RestorationChanged -= OnRestoration;
        if (spots != null) foreach (var spot in spots) if (spot != null && spot.PlantedTree != null) spot.PlantedTree.StageChanged -= OnStage;
    }
}
