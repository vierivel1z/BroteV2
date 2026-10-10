using UnityEngine;

/// <summary>Small time-based presentation curves, with no global tween runner.</summary>
public static class BroteTween
{
    public static float PopScale(float elapsed, float duration, float start, float peak, float peakFraction = 0.45f)
    {
        if (duration <= 0f || elapsed >= duration) return 1f;
        float t = Mathf.Clamp01(elapsed / duration);
        float split = Mathf.Clamp(peakFraction, 0.1f, 0.9f);
        return t < split ? Mathf.Lerp(start, peak, Ease(t / split))
            : Mathf.Lerp(peak, 1f, Ease((t - split) / (1f - split)));
    }
    public static float Pulse(float elapsed, float duration)
    {
        if (duration <= 0f || elapsed <= 0f || elapsed >= duration) return 0f;
        return Mathf.Sin(elapsed / duration * Mathf.PI);
    }
    public static float Ease(float normalized) => Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(normalized));
}
