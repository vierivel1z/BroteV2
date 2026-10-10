using System;
using UnityEngine;

/// <summary>A failed observer must not abort gameplay or other observers.</summary>
public static class BroteEvents
{
    public static void Notify<T>(Action<T> listeners, T value, UnityEngine.Object context)
    {
        if (listeners == null) return;
        foreach (Action<T> listener in listeners.GetInvocationList())
        {
            try { listener(value); }
            catch (Exception exception) { Debug.LogException(exception, context); }
        }
    }
}
