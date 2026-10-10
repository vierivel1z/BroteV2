using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class SeedlingInventory : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxSeedlings = 5;
    private int currentSeedlings;
    public int CurrentSeedlings => currentSeedlings;
    public int MaxSeedlings => Mathf.Max(1, maxSeedlings);
    public event Action<int> SeedlingsChanged;

    public void SetCapacity(int capacity)
    {
        maxSeedlings=Mathf.Clamp(capacity,1,20); currentSeedlings=Mathf.Clamp(currentSeedlings,0,maxSeedlings);
        BroteEvents.Notify(SeedlingsChanged,currentSeedlings,this);
    }
    public void RestoreSavedCount(int count)
    {
        currentSeedlings = Mathf.Clamp(count, 0, MaxSeedlings);
        BroteEvents.Notify(SeedlingsChanged, currentSeedlings, this);
    }
    public void AddSeedlings(int amount)
    {
        if (amount <= 0) return;
        int added = Mathf.Min(amount, MaxSeedlings - currentSeedlings);
        if (added <= 0) return;
        currentSeedlings += added;
        BroteEvents.Notify(SeedlingsChanged, currentSeedlings, this);
    }

    public bool TryConsumeSeedling()
    {
        if (currentSeedlings <= 0) return false;
        currentSeedlings--;
        BroteEvents.Notify(SeedlingsChanged, currentSeedlings, this);
        return true;
    }

    private void OnValidate()
    {
        maxSeedlings = Mathf.Max(1, maxSeedlings);
        int clamped = Mathf.Clamp(currentSeedlings, 0, maxSeedlings);
        if (clamped == currentSeedlings) return;
        currentSeedlings = clamped;
        BroteEvents.Notify(SeedlingsChanged, currentSeedlings, this);
    }
}
