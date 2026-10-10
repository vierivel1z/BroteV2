using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public sealed class BroteGrowthLeafPoolTests
{
    private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
    [UnityTest] public IEnumerator GlobalPoolStillCapsAtThreeAndReusesLeafSystem()
    {
        var type = Type.GetType("BroteVFXPool, Assembly-CSharp", true);
        var reset = type.GetMethod("Reset", Flags); reset.Invoke(null, null);
        var owner = new GameObject("Leaf pool test owner");
        var effects = new Component[3];
        try
        {
            for (int i = 0; i < 3; i++)
            {
                effects[i] = (Component)type.GetMethod("Acquire", Flags).Invoke(null, new object[] { i, owner.transform });
                Assert.IsNotNull(effects[i]);
                var settings = effects[i].GetType().GetField("settings").GetValue(effects[i]);
                effects[i].GetType().GetMethod("Play").Invoke(effects[i], new object[] { settings, null, null, null, null,
                    Color.white, Color.white, Color.white, 0, .12f });
                Assert.AreEqual(4, effects[i].transform.Find("GrowthLeaves").GetComponent<ParticleSystem>().textureSheetAnimation.spriteCount);
            }
            Assert.AreEqual(3, type.GetProperty("ActiveCount", Flags).GetValue(null));
            Assert.IsNull(type.GetMethod("Acquire", Flags).Invoke(null, new object[] { 0, owner.transform }));
            var leaves = effects[0].transform.Find("GrowthLeaves");
            type.GetMethod("Release", Flags).Invoke(null, new object[] { effects[0] });
            var reused = (Component)type.GetMethod("Acquire", Flags).Invoke(null, new object[] { 2, owner.transform });
            Assert.AreSame(effects[0], reused);
            Assert.AreSame(leaves, reused.transform.Find("GrowthLeaves"));
            Assert.AreEqual(3, reused.GetComponentsInChildren<ParticleSystem>().Length);
            var mature = type.GetMethod("Preset", Flags).Invoke(null, new object[] { 2 });
            var matureSettings = mature.GetType().GetField("settings").GetValue(mature);
            reused.GetType().GetMethod("Play").Invoke(reused, new object[] { matureSettings, null, null, null, null,
                Color.white, Color.white, Color.white, 0, .6f });
            var dust = reused.transform.Find("PlantDust").GetComponent<ParticleSystem>();
            Assert.AreEqual(1, dust.particleCount);
            Assert.AreEqual("NaturalSpark", dust.textureSheetAnimation.GetSprite(0).name);
            type.GetMethod("Release", Flags).Invoke(null, new object[] { reused });
            var planting = type.GetMethod("Preset", Flags).Invoke(null, new object[] { 0 });
            var plantingSettings = planting.GetType().GetField("settings").GetValue(planting);
            reused = (Component)type.GetMethod("Acquire", Flags).Invoke(null, new object[] { 0, owner.transform });
            reused.GetType().GetMethod("Play").Invoke(reused, new object[] { plantingSettings, null, null, null, null,
                Color.white, Color.white, Color.white, 0, .12f });
            Assert.AreSame(dust, reused.transform.Find("PlantDust").GetComponent<ParticleSystem>());
            Assert.AreEqual(4, dust.particleCount); Assert.AreEqual(2, dust.textureSheetAnimation.spriteCount);
            Assert.AreEqual(3, type.GetProperty("ActiveCount", Flags).GetValue(null));
        }
        finally { reset.Invoke(null, null); Object.Destroy(owner); }
        yield return null;
    }
}
