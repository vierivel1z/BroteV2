using System;
using UnityEngine;

public enum BroteUpgrade { ClickPower, Riego, Capacity, Soil, Expansion }
[DisallowMultipleComponent]
public sealed class BroteUpgradeManager : MonoBehaviour
{
    public static readonly int[][] Values = { new[]{10,15,20,25}, new[]{0,2,5,10}, new[]{5,8,12,20}, new[]{3,2,1}, new[]{0,1,2} };
    public static readonly int[][] Costs = { new[]{1,2,4}, new[]{2,4,7}, new[]{2,4,7}, new[]{2,5}, new[]{4,8} };
    private readonly int[] levels = new int[5];
    public event Action<BroteUpgrade> Purchased;
    public int Level(BroteUpgrade kind) => levels[(int)kind];
    public int Value(BroteUpgrade kind) => Values[(int)kind][Level(kind)];
    public int NextCost(BroteUpgrade kind) => Level(kind) < Costs[(int)kind].Length ? Costs[(int)kind][Level(kind)] : -1;
    public void Restore(int click, int auto, int capacity, int soil, int expansion)
    { levels[0]=click; levels[1]=auto; levels[2]=capacity; levels[3]=soil; levels[4]=expansion; }
    public bool TryBuy(BroteUpgrade kind)
    {
        var wallet=GetComponent<BroteMinkaManager>(); int cost=NextCost(kind);
        if (wallet==null || cost<0 || BroteSaveSystem.IsLoading || BrotePauseMenu.IsPaused || Time.timeScale<=0 || !wallet.TrySpend(cost)) return false;
        levels[(int)kind]++; BroteEvents.Notify(Purchased,kind,this); return true;
    }
}
