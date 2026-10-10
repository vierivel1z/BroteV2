using System;
using UnityEngine;
public sealed class RestorationManager : MonoBehaviour
{
    [SerializeField, Min(1f)] private float maxRestoration = 100f;
    public float CurrentRestoration { get; private set; }
    public float MaxRestoration => float.IsNaN(maxRestoration) || float.IsInfinity(maxRestoration) ? 100f : Mathf.Max(1f, maxRestoration);
    public float RestorationPercentage => 100f * CurrentRestoration / MaxRestoration;
    public event Action<float> RestorationChanged;
    public void RestoreSavedPoints(float points)
    {
        CurrentRestoration = Mathf.Clamp(points, 0f, MaxRestoration);
        BroteEvents.Notify(RestorationChanged, RestorationPercentage, this);
    }
    public void AddRestoration(float points)
    {
        if (float.IsNaN(points) || float.IsInfinity(points))
        {
            BroteDiagnostics.WarnOnce(this, "nonFinitePoints", "Non-finite restoration contribution was ignored.");
            return;
        }
        float next = Mathf.Clamp(CurrentRestoration + Mathf.Max(0f, points), 0f, MaxRestoration);
        if (Mathf.Approximately(next, CurrentRestoration)) return;
        CurrentRestoration = next;
        Debug.Log($"Restauración: {RestorationPercentage:0.#}%", this);
        BroteEvents.Notify(RestorationChanged, RestorationPercentage, this);
    }
}
