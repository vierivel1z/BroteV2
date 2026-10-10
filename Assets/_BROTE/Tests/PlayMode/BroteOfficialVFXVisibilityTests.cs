using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public sealed class BroteOfficialVFXVisibilityTests
{
    private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
    private static Type Runtime(string name) => Type.GetType(name + ", Assembly-CSharp", true);
    private static object Call(object owner, string method, params object[] args) => owner.GetType().GetMethod(method, Flags).Invoke(owner, args);
    [UnityTest] public IEnumerator MatureFeedbackIsReleasedAndSuppressedOutsideCamera()
    {
        var pool = Runtime("BroteVFXPool"); var reset = pool.GetMethod("Reset", Flags);
        reset.Invoke(null, null);
        var existingCameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        var enabled = new bool[existingCameras.Length];
        for (int i = 0; i < existingCameras.Length; i++) { enabled[i] = existingCameras[i].enabled; existingCameras[i].enabled = false; }
        var cameraRoot = new GameObject("VFX test camera"); cameraRoot.tag = "MainCamera";
        var camera = cameraRoot.AddComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 4.3f;
        cameraRoot.transform.position = new Vector3(0, 0, -10);
        GameObject tree = null;
        try
        {
            var assets = Type.GetType("UnityEditor.AssetDatabase, UnityEditor.CoreModule", true);
            var prefab = (GameObject)assets.GetMethod("LoadAssetAtPath", new[] { typeof(string), typeof(Type) })
                .Invoke(null, new object[] { "Assets/_BROTE/Prefabs/Plants/QuenualPlaceholder.prefab", typeof(GameObject) });
            tree = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
            var feedback = tree.GetComponent(Runtime("QuenualGrowthFeedback"));
            var growth = tree.GetComponent(Runtime("QuenualGrowth"));
            // A presentation request only: no gameplay stage, timers or rewards are changed.
            Call(feedback, "QueueStage", 2); Call(feedback, "LateUpdate");
            var active = (Component)feedback.GetType().GetField("pooledEffect", Flags).GetValue(feedback);
            Assert.IsNotNull(active); Assert.AreEqual(1, pool.GetProperty("ActiveCount", Flags).GetValue(null));
            var dust = active.transform.Find("PlantDust").GetComponent<ParticleSystem>();
            Assert.AreEqual("NaturalSpark", dust.textureSheetAnimation.GetSprite(0).name);
            Assert.AreEqual(1, dust.particleCount);
            cameraRoot.transform.position = new Vector3(1000, 1000, -10);
            Call(feedback, "LateUpdate");
            Assert.AreEqual(0, pool.GetProperty("ActiveCount", Flags).GetValue(null));
            Assert.AreEqual(0, dust.particleCount);
            Assert.IsNull(feedback.GetType().GetField("pooledEffect", Flags).GetValue(feedback));
            Call(feedback, "QueueStage", 2); Call(feedback, "LateUpdate");
            Assert.AreEqual(0, pool.GetProperty("ActiveCount", Flags).GetValue(null));
            Assert.AreEqual(0, tree.GetComponentsInChildren<ParticleSystem>().Length);
            Assert.AreEqual(0, growth.GetType().GetProperty("Stage").GetValue(growth));
        }
        finally
        {
            if (tree != null) Object.Destroy(tree);
            Object.Destroy(cameraRoot); reset.Invoke(null, null);
            for (int i = 0; i < existingCameras.Length; i++) if (existingCameras[i] != null) existingCameras[i].enabled = enabled[i];
        }
        yield return null;
    }
}
