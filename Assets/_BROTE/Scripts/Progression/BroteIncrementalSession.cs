using System;
using System.Linq;
using UnityEngine;

[Serializable] public sealed class BroteZoneGroup { public string name; public string[] stableIds; }
[Serializable] public sealed class BroteZoneLayout { public BroteZoneGroup[] zones; public string[] prepared; }
[DefaultExecutionOrder(-100)]
[DisallowMultipleComponent]
public sealed class BroteIncrementalSession : MonoBehaviour
{
    [SerializeField] private TextAsset zoneLayout;
    private BroteZoneLayout layout;
    public PlantingSpot[] Spots { get; private set; }
    public BroteMinkaManager Minka => GetComponent<BroteMinkaManager>();
    public BroteUpgradeManager Upgrades => GetComponent<BroteUpgradeManager>();
    public NurseryProductionManager Production => GetComponent<NurseryProductionManager>();
    public BroteRestorationObjectives Objectives => GetComponent<BroteRestorationObjectives>();
    public PlayerInteraction Player { get; private set; }
    public int DevelopedCount => Spots==null ? 0 : Spots.Count(x=>x.PlantedTree!=null && x.PlantedTree.Stage==2);
    private float saveClock;
    private void Awake()
    {
        layout=zoneLayout!=null ? JsonUtility.FromJson<BroteZoneLayout>(zoneLayout.text) : null;
        Spots=FindObjectsByType<PlantingSpot>().Where(x=>x.gameObject.scene==gameObject.scene).ToArray();
        Player=FindObjectsByType<PlayerInteraction>().FirstOrDefault(x=>x.gameObject.scene==gameObject.scene);
        if(layout==null || layout.zones==null || layout.zones.Length!=3 || Player==null) { enabled=false; Debug.LogError("BROTE: incremental references incomplete",this); return; }
        Production.Initialize(Player.GetComponent<SeedlingInventory>());
        var initial=BroteSaveData.CreateNewGameData(layout);
        Restore(initial);Production.Inventory.RestoreSavedCount(initial.inventory);
        foreach(var spot in Spots)
        {
            int zone=Array.FindIndex(layout.zones,x=>x.stableIds.Contains(spot.StableId));
            if(zone<0) { enabled=false; Debug.LogError("BROTE: spot absent from zone layout",spot); return; }
            spot.ConfigureProgression(this,zone,(PlantingSpot.SpotState)initial.spots.Single(x=>x.id==spot.StableId).state);
        }
        Production.Produced+=OnProduced; Upgrades.Purchased+=OnPurchased;
        PlantingSpot.Planted+=OnPlanted; QuenualGrowth.Matured+=OnMatured;
    }
    private void Start()
    {
        var hud=FindObjectsByType<BroteHUD>().FirstOrDefault(x=>x.gameObject.scene==gameObject.scene);
        if(hud!=null) NurseryManagementPanel.Attach(hud,this);
    }
    private void Update()
    {
        if(!enabled || BroteSaveSystem.IsLoading || BrotePauseMenu.IsPaused || Time.timeScale<=0) return;
        Minka.Tick(Time.deltaTime,DevelopedCount); Production.Tick(Time.deltaTime);
        saveClock+=Time.deltaTime;
        if(saveClock>=5f) { saveClock=0; GetComponent<BroteSaveSystem>()?.RequestSave(); }
    }
    public void Click() { Production.Click(); GetComponent<BroteSaveSystem>()?.RequestSave(); }
    public void Prepared(PlantingSpot spot) { Objectives.Observe(3); Player.ShowFeedback("Terreno preparado"); GetComponent<BroteSaveSystem>()?.RequestSave(); }
    private void OnProduced(int _)
    {
        Objectives.Observe(0); Player.ShowFeedback("+1 pl\u00e1ntula"); GetComponent<BroteSaveSystem>()?.RequestSave();
        var nursery=FindObjectsByType<NurserySeedlingSource>().FirstOrDefault(x=>x.gameObject.scene==gameObject.scene); nursery?.ProductionFeedback();
    }
    private void OnPlanted(PlantingSpot spot) { if(Spots.Contains(spot)) Objectives.Observe(1); }
    private void OnMatured(QuenualGrowth tree)
    {
        if(BroteSaveSystem.IsLoading) return;
        var spot=Spots.FirstOrDefault(x=>x.PlantedTree==tree);
        if(spot==null || spot.MinkaRewarded) return;
        spot.MinkaRewarded=true; Minka.Add(1); Objectives.Observe(2); Player.ShowFeedback("+1 Minka"); GetComponent<BroteSaveSystem>()?.RequestSave();
    }
    private void OnPurchased(BroteUpgrade kind)
    {
        Production.Inventory.SetCapacity(Upgrades.Value(BroteUpgrade.Capacity));
        Objectives.Observe(4);
        if(kind==BroteUpgrade.Expansion)
        {
            foreach(var spot in Spots) if(spot.Zone<=Upgrades.Level(BroteUpgrade.Expansion) && spot.State==PlantingSpot.SpotState.LOCKED) spot.SetProgressState(PlantingSpot.SpotState.AVAILABLE,0,false);
            Objectives.Observe(5); Player.ShowFeedback("Nueva zona de restauraci\u00f3n disponible");
        }
        else Player.ShowFeedback($"Mejora adquirida: Lv.{Upgrades.Level(kind)-1} \u2192 Lv.{Upgrades.Level(kind)}");
        GetComponent<BroteSaveSystem>()?.RequestSave();
    }
    public void Restore(BroteSaveData data)
    {
        Minka.Restore(data.minka,data.minkaFraction);
        Upgrades.Restore(data.clickPowerLevel,data.autoProductionLevel,data.capacityLevel,data.soilPreparationLevel,data.expansionLevel);
        Production.Inventory.SetCapacity(Upgrades.Value(BroteUpgrade.Capacity)); Production.Restore(data.nurseryProgress);
        Objectives.Restore(data.objectivesObserved,data.objectivesClaimed);
    }
    public void Capture(BroteSaveData data)
    {
        data.saveVersion=2; data.minka=Minka.Minka; data.minkaFraction=Minka.Fraction; data.nurseryProgress=Production.Progress;
        data.clickPowerLevel=Upgrades.Level(BroteUpgrade.ClickPower); data.autoProductionLevel=Upgrades.Level(BroteUpgrade.Riego);
        data.capacityLevel=Upgrades.Level(BroteUpgrade.Capacity); data.soilPreparationLevel=Upgrades.Level(BroteUpgrade.Soil); data.expansionLevel=Upgrades.Level(BroteUpgrade.Expansion);
        data.objectivesObserved=Objectives.Observed; data.objectivesClaimed=Objectives.Claimed;
    }
    private void OnDestroy()
    {
        PlantingSpot.Planted-=OnPlanted; QuenualGrowth.Matured-=OnMatured;
        if(Production!=null) Production.Produced-=OnProduced; if(Upgrades!=null) Upgrades.Purchased-=OnPurchased;
    }
}
