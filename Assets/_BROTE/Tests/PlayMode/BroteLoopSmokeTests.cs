using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

public sealed class BroteLoopSmokeTests
{
    private const BindingFlags Flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
    private readonly List<GameObject> roots=new List<GameObject>();
    private float previousTimeScale;
    private static Type Runtime(string name)=>Type.GetType(name+", Assembly-CSharp",true);
    private Component Create(string name){var root=new GameObject("Smoke "+name);roots.Add(root);var component=root.AddComponent(Runtime(name));if(name=="PlayerInteraction")((Behaviour)component).enabled=false;return component;}
    private static object Call(object owner,string method,params object[] args)=>owner.GetType().GetMethod(method,Flags).Invoke(owner,args);
    private static object Get(object owner,string name)=>owner.GetType().GetProperty(name,Flags).GetValue(owner);
    private static void Set(object owner,string name,object value)=>owner.GetType().GetField(name,Flags).SetValue(owner,value);
    [SetUp] public void Setup(){previousTimeScale=Time.timeScale;Time.timeScale=1f;}
    [UnityTearDown] public IEnumerator Cleanup(){foreach(var root in roots)if(root!=null)Object.Destroy(root);roots.Clear();Time.timeScale=previousTimeScale;yield return null;}
    private Component TreeTemplate()
    {
        // The real imported prefab, not a substitute growth implementation.
        Type assets=Type.GetType("UnityEditor.AssetDatabase, UnityEditor.CoreModule",true);
        var load=assets.GetMethod("LoadAssetAtPath",new[]{typeof(string),typeof(Type)});
        var prefab=(GameObject)load.Invoke(null,new object[]{"Assets/_BROTE/Prefabs/Plants/QuenualPlaceholder.prefab",typeof(GameObject)});
        Assert.IsNotNull(prefab);
        var root=Object.Instantiate(prefab,new Vector3(1000,1000,0),Quaternion.identity);roots.Add(root);
        var feedback=root.GetComponent(Runtime("QuenualGrowthFeedback"));if(feedback!=null)((Behaviour)feedback).enabled=false;
        var growth=root.GetComponent(Runtime("QuenualGrowth"));
        // Only the test instance is accelerated. Original prefab remains5+5s/+10.
        Set(growth,"secondsToYoung",.01f);Set(growth,"secondsToDeveloped",.01f);return growth;
    }
    private Component Spot(Component template,Component manager,Component inventory)
    {
        var spot=Create("PlantingSpot");Set(spot,"plantPrefab",template);Set(spot,"restorationManager",manager);Set(spot,"promptInventory",inventory);Set(spot,"plantingPoint",spot.transform);return spot;
    }
    // BROTE:122 BROTE:127
    [UnityTest] public IEnumerator PlantingCreatesOneTreeConsumesOneAndOccupiesSpot()
    {
        var manager=Create("RestorationManager");var player=Create("PlayerInteraction");var inventory=player.gameObject.AddComponent(Runtime("SeedlingInventory"));Call(inventory,"AddSeedlings",1);
        var spot=Spot(TreeTemplate(),manager,inventory);Call(spot,"Interact",player);Assert.AreEqual(0,Get(inventory,"CurrentSeedlings"));Call(inventory,"AddSeedlings",1);Call(spot,"Interact",player);
        Assert.AreEqual(1,Get(inventory,"CurrentSeedlings"));Assert.AreEqual("PLANTED",Get(spot,"State").ToString());
        var owned=(Component)Get(spot,"PlantedTree");Assert.AreEqual(1,owned.GetComponentsInChildren(Runtime("QuenualGrowth"),true).Length);Assert.AreSame(spot,Get(owned,"OwningSpot"));yield return null;
    }
    // BROTE:123 BROTE:124
    [UnityTest] public IEnumerator RealGrowthCoroutinePublishesStagesAndTenPointsOnce()
    {
        var manager=Create("RestorationManager");var tree=TreeTemplate();var stages=new List<int>();Action<int> listener=value=>stages.Add(value);
        tree.GetType().GetEvent("StageChanged").AddEventHandler(tree,listener);Call(tree,"Initialize",manager);
        float deadline=Time.realtimeSinceStartup+3f;
        while((int)Get(tree,"Stage")<2 && Time.realtimeSinceStartup<deadline)yield return null;
        CollectionAssert.AreEqual(new[]{0,1,2},stages);Assert.AreEqual(10f,Get(manager,"CurrentRestoration"));
        Call(tree,"Initialize",manager);yield return null;Assert.AreEqual(10f,Get(manager,"CurrentRestoration"));
    }
    // BROTE:125
    [UnityTest] public IEnumerator TenActualTreesReachOneHundred()
    {
        var manager=Create("RestorationManager");for(int i=0;i<10;i++){var tree=TreeTemplate();Call(tree,"Initialize",manager);}
        float deadline=Time.realtimeSinceStartup+3f;
        while((float)Get(manager,"CurrentRestoration")<100 && Time.realtimeSinceStartup<deadline)yield return null;
        Assert.AreEqual(100f,Get(manager,"RestorationPercentage"));
    }
    // BROTE:126
    [UnityTest] public IEnumerator EmptyInventoryLeavesSpotAndManagerUnchanged()
    {
        var manager=Create("RestorationManager");var player=Create("PlayerInteraction");var inventory=player.gameObject.AddComponent(Runtime("SeedlingInventory"));var spot=Spot(TreeTemplate(),manager,inventory);
        Call(spot,"Interact",player);Assert.AreEqual(0,Get(inventory,"CurrentSeedlings"));Assert.AreEqual("PREPARED",Get(spot,"State").ToString());
        Assert.AreEqual(0,spot.GetComponentsInChildren(Runtime("QuenualGrowth"),true).Length);Assert.AreEqual(0f,Get(manager,"CurrentRestoration"));yield return null;
    }
    // BROTE:128
    [UnityTest] public IEnumerator FullNurseryDoesNotOverflow()
    {
        var source=Create("NurserySeedlingSource");var player=Create("PlayerInteraction");var inventory=player.gameObject.AddComponent(Runtime("SeedlingInventory"));Call(inventory,"AddSeedlings",5);
        for(int i=0;i<20;i++){Call(source,"Interact",player);yield return null;}
        Assert.AreEqual(5,Get(inventory,"CurrentSeedlings"));
    }
    // BROTE:129
    [UnityTest] public IEnumerator RepeatedContributionsClampAtOneHundred()
    {
        var manager=Create("RestorationManager");for(int i=0;i<100;i++)Call(manager,"AddRestoration",10f);
        Assert.AreEqual(100f,Get(manager,"CurrentRestoration"));yield return null;
    }
}
