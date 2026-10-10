using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public static class BroteDiagnostics
{
    private static readonly HashSet<string> issued = new HashSet<string>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset() => issued.Clear();
    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void WarnOnce(UnityEngine.Object context, string reason, string message)
    {
        if (!Application.isPlaying || context == null) return;
        string key = context.GetEntityId().ToString() + ":" + reason;
        if (issued.Add(key)) UnityEngine.Debug.LogWarning(message, context);
    }
}
