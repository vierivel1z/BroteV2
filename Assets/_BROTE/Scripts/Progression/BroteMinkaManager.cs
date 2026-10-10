using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class BroteMinkaManager : MonoBehaviour
{
    public int Minka { get; private set; }
    public double Fraction { get; private set; }
    public event Action<int> Changed;
    public void Restore(int amount, double fraction) { Minka = amount; Fraction = fraction; Notify(); }
    public void Add(int amount) { if (amount <= 0) return; Minka=(int)Math.Min(int.MaxValue,(long)Minka+amount); Notify(); }
    public bool TrySpend(int amount)
    {
        if (amount < 0 || amount > Minka) return false;
        Minka -= amount; Notify(); return true;
    }
    public void Tick(double seconds, int developed)
    {
        if (BrotePauseMenu.IsPaused || Time.timeScale<=0 || BroteSaveSystem.IsLoading || seconds <= 0 || double.IsNaN(seconds) || double.IsInfinity(seconds) || developed <= 0) return;
        Fraction += seconds * developed * .01;
        int whole = (int)Math.Floor(Fraction + 1e-9);
        if (whole > 0) { Fraction = Math.Max(0, Fraction - whole); Add(whole); }
    }
    private void Notify() => BroteEvents.Notify(Changed, Minka, this);
}
