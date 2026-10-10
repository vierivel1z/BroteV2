using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public sealed class BroteSavedSpot
{
    public string id;
    public bool planted;
    public int stage;
    public int state;
    public int preparationActions;
    public bool minkaRewarded;
}
[Serializable] public sealed class BroteSaveData
{
    // Explicit v2 capture; default v1 keeps old JSON and fixtures compatible.
    public int saveVersion = 1;
    public string sector = "Sector_01";
    public int inventory;
    public float restoration;
    public bool tutorialCompleted;
    public BroteSavedSpot[] spots;
    public int minka;
    public double minkaFraction, nurseryProgress;
    public int clickPowerLevel, autoProductionLevel, capacityLevel, soilPreparationLevel, expansionLevel;
    public int objectivesObserved, objectivesClaimed;
    // Canonical new-game state uses the approved zone layout as the only spot mapping.
    public static BroteSaveData CreateNewGameData(BroteZoneLayout layout)
    {
        if(layout==null || layout.zones==null || layout.zones.Length!=3 || layout.prepared==null)
            throw new ArgumentException("Missing canonical zone layout",nameof(layout));
        var records=new List<BroteSavedSpot>();
        for(int zone=0;zone<layout.zones.Length;zone++)
            foreach(string id in layout.zones[zone].stableIds)
                records.Add(new BroteSavedSpot{id=id,state=(int)(zone>0?PlantingSpot.SpotState.LOCKED:
                    Array.IndexOf(layout.prepared,id)>=0?PlantingSpot.SpotState.PREPARED:PlantingSpot.SpotState.AVAILABLE)});
        var data=new BroteSaveData{saveVersion=2,spots=records.ToArray()};
        if(!data.IsValid())throw new ArgumentException("Invalid canonical zone layout",nameof(layout));
        return data;
    }
    public bool IsValid()
    {
        if ((saveVersion!=1 && saveVersion!=2) || sector!="Sector_01" || inventory<0 ||
            float.IsNaN(restoration) || float.IsInfinity(restoration) || spots==null || spots.Length!=10) return false;
        int capacity=5;
        if(saveVersion==2)
        {
            if(clickPowerLevel<0 || clickPowerLevel>3 || autoProductionLevel<0 || autoProductionLevel>3 || capacityLevel<0 || capacityLevel>3 ||
                soilPreparationLevel<0 || soilPreparationLevel>2 || expansionLevel<0 || expansionLevel>2 || minka<0 ||
                !Finite(minkaFraction) || minkaFraction<0 || minkaFraction>=1 || !Finite(nurseryProgress) || nurseryProgress<0 || nurseryProgress>=100 ||
                objectivesObserved<0 || objectivesObserved>63 || objectivesClaimed<0 || objectivesClaimed>6 ||
                (objectivesObserved & ((1<<objectivesClaimed)-1))!=((1<<objectivesClaimed)-1)) return false;
            capacity=new[]{5,8,12,20}[capacityLevel];
        }
        if(inventory>capacity || (saveVersion==2 && objectivesClaimed<6 && (objectivesObserved & (1<<objectivesClaimed))!=0)) return false;
        var ids=new HashSet<string>(); int developed=0;
        foreach(var spot in spots)
        {
            if(spot==null || !int.TryParse(spot.id,out int id) || id<1 || id>10 || spot.id!=id.ToString("00") || !ids.Add(spot.id) ||
                spot.stage<0 || spot.stage>2 || (!spot.planted && spot.stage!=0)) return false;
            if(saveVersion==2 && (spot.state<0 || spot.state>3 || spot.planted!=(spot.state==(int)PlantingSpot.SpotState.PLANTED) ||
                spot.preparationActions<0 || spot.preparationActions>3 || (spot.minkaRewarded && (!spot.planted || spot.stage!=2)) ||
                (spot.planted && spot.stage==2 && !spot.minkaRewarded))) return false;
            if(spot.planted && spot.stage==2) developed++;
        }
        return Mathf.Approximately(restoration,developed*10f);
    }
    private static bool Finite(double v)=>!double.IsNaN(v)&&!double.IsInfinity(v);
    public void MigrateV1()
    {
        if(saveVersion!=1 || !IsValid()) return;
        saveVersion=2;
        // Preserve access to all ten legacy spots; no retroactive rewards, no offline income.
        expansionLevel=2; minka=0; minkaFraction=0; nurseryProgress=0;
        clickPowerLevel=autoProductionLevel=capacityLevel=soilPreparationLevel=0;
        objectivesObserved=1<<5;objectivesClaimed=0; // Legacy expansion is already open; do not leave objective6 unreachable.
        bool anyPlant=false,anyDeveloped=false;
        foreach(var spot in spots)
        {
            spot.state=(int)(spot.planted ? PlantingSpot.SpotState.PLANTED : PlantingSpot.SpotState.AVAILABLE);
            spot.preparationActions=0;spot.minkaRewarded=spot.planted && spot.stage==2;
            anyPlant|=spot.planted;anyDeveloped|=spot.minkaRewarded;
        }
        if(inventory>0 || anyPlant) objectivesObserved|=1;
        if(anyPlant) objectivesObserved|=2;
        if(anyDeveloped) objectivesObserved|=4;
        // Infer only earlier proven milestones, mark them claimed without paying old progress twice.
        while(objectivesClaimed<3 && (objectivesObserved & (1<<objectivesClaimed))!=0) objectivesClaimed++;
        if(Array.TrueForAll(spots,x=>x.planted)) { objectivesObserved=63; objectivesClaimed=6; }
    }
}
