using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class NurseryProductionManager : MonoBehaviour
{
    public double Progress { get; private set; }
    public SeedlingInventory Inventory { get; private set; }
    public event Action<int> Produced;
    public void Initialize(SeedlingInventory inventory) => Inventory=inventory;
    public void Restore(double progress) => Progress=progress;
    public bool Click()
    {
        if(BrotePauseMenu.IsPaused || Time.timeScale<=0 || BroteSaveSystem.IsLoading) return false;
        return Advance(GetComponent<BroteUpgradeManager>().Value(BroteUpgrade.ClickPower)) > 0;
    }
    // Stops at full inventory without discarding any existing progress or click overflow.
    public int Advance(double percent)
    {
        if(Inventory==null || percent<=0 || double.IsNaN(percent) || double.IsInfinity(percent) || Inventory.CurrentSeedlings>=Inventory.MaxSeedlings) return 0;
        Progress+=percent; int produced=0;
        while(Progress+1e-9>=100 && Inventory.CurrentSeedlings<Inventory.MaxSeedlings)
        {
            Progress=Math.Max(0,Progress-100); Inventory.AddSeedlings(1); produced++; BroteEvents.Notify(Produced,1,this);
        }
        // Automatic production is capped by the caller to available storage; manual overflow <25%.
        return produced;
    }
    public void Tick(double seconds)
    {
        if(BrotePauseMenu.IsPaused || Time.timeScale<=0 || BroteSaveSystem.IsLoading || seconds<=0 || double.IsNaN(seconds) || double.IsInfinity(seconds) || Inventory==null || Inventory.CurrentSeedlings>=Inventory.MaxSeedlings) return;
        double room=(Inventory.MaxSeedlings-Inventory.CurrentSeedlings)*100-Progress;
        Advance(Math.Min(Math.Max(0,room),seconds*GetComponent<BroteUpgradeManager>().Value(BroteUpgrade.Riego)));
    }
}
