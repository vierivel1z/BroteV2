using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class BroteWave3GrowthTests
{
    [UnityTest] public IEnumerator LoadedYoungResumesOnlyPendingStageAndContributesOnce()
    {
        var owner=new GameObject("Loaded manager");var treeObject=new GameObject("Loaded young");
        var managerType=Type.GetType("RestorationManager, Assembly-CSharp",true);var growthType=Type.GetType("QuenualGrowth, Assembly-CSharp",true);
        var manager=owner.AddComponent(managerType);var tree=treeObject.AddComponent(growthType);
        const BindingFlags flags=BindingFlags.NonPublic|BindingFlags.Instance;
        growthType.GetField("secondsToYoung",flags).SetValue(tree,100f);growthType.GetField("secondsToDeveloped",flags).SetValue(tree,.01f);
        managerType.GetMethod("RestoreSavedPoints").Invoke(manager,new object[]{50f});
        growthType.GetMethod("RestoreSavedStage").Invoke(tree,new object[]{manager,1});
        Assert.AreEqual(1,growthType.GetProperty("Stage").GetValue(tree));
        yield return new WaitForSeconds(.08f);
        Assert.AreEqual(2,growthType.GetProperty("Stage").GetValue(tree));Assert.AreEqual(60f,managerType.GetProperty("CurrentRestoration").GetValue(manager));
        growthType.GetMethod("Initialize").Invoke(tree,new object[]{manager});yield return new WaitForSeconds(.02f);
        Assert.AreEqual(60f,managerType.GetProperty("CurrentRestoration").GetValue(manager));
        UnityEngine.Object.Destroy(treeObject);UnityEngine.Object.Destroy(owner);yield return null;
    }
}
