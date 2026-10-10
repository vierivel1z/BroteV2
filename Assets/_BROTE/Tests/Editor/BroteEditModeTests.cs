using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

// Reflection keeps the existing Assembly-CSharp layout; no runtime assembly migration.
public sealed class BroteEditModeTests
{
    private readonly List<GameObject> roots = new List<GameObject>();
    private const BindingFlags Flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
    private static Type Runtime(string name) => Type.GetType(name+", Assembly-CSharp",true);
    private Component Create(string name)
    {
        var root=new GameObject("Test "+name);roots.Add(root);return root.AddComponent(Runtime(name));
    }
    private static object Invoke(object owner,string method,params object[] args) => owner.GetType().GetMethod(method,Flags).Invoke(owner,args);
    private static object Property(object owner,string name) => owner.GetType().GetProperty(name,Flags).GetValue(owner);
    private static void Set(object owner,string name,object value) => owner.GetType().GetField(name,Flags).SetValue(owner,value);
    [TearDown] public void Cleanup(){foreach(var root in roots) if(root!=null)Object.DestroyImmediate(root);roots.Clear();}

    [TestCase(0,0)] [TestCase(1,1)] [TestCase(5,5)] [TestCase(6,5)] [TestCase(int.MaxValue,5)] [TestCase(-1,0)]
    public void InventoryClamps(int added,int expected)
    {
        var inventory=Create("SeedlingInventory");Invoke(inventory,"AddSeedlings",added);
        Assert.AreEqual(expected,Property(inventory,"CurrentSeedlings"));
    }
    [Test] public void InventoryConsumesExactlyOneAndNotBelowZero()
    {
        var inventory=Create("SeedlingInventory");Assert.IsFalse((bool)Invoke(inventory,"TryConsumeSeedling"));
        Invoke(inventory,"AddSeedlings",1);Assert.IsTrue((bool)Invoke(inventory,"TryConsumeSeedling"));
        Assert.AreEqual(0,Property(inventory,"CurrentSeedlings"));Assert.IsFalse((bool)Invoke(inventory,"TryConsumeSeedling"));
    }
    [Test] public void RestorationStartsAtZeroAndClamps()
    {
        var manager=Create("RestorationManager");Assert.AreEqual(0f,Property(manager,"CurrentRestoration"));
        Invoke(manager,"AddRestoration",10f);Assert.AreEqual(10f,Property(manager,"CurrentRestoration"));
        Invoke(manager,"AddRestoration",1000f);Assert.AreEqual(100f,Property(manager,"CurrentRestoration"));
        Invoke(manager,"AddRestoration",-100f);Assert.AreEqual(100f,Property(manager,"CurrentRestoration"));
    }
    [Test] public void GrowthAdvancesWithTwoFiveSecondWaitsAndContributesOnce()
    {
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_BROTE/Prefabs/Plants/QuenualPlaceholder.prefab");
        Assert.IsNotNull(prefab);
        var root=(GameObject)PrefabUtility.InstantiatePrefab(prefab);roots.Add(root);
        var growth=root.GetComponent(Runtime("QuenualGrowth"));var manager=Create("RestorationManager");
        Set(growth,"manager",manager);Set(growth,"initialized",true);
        int events=0;Action<int> listener=stage=>events++;
        growth.GetType().GetEvent("StageChanged").AddEventHandler(growth,listener);
        var sequence=(IEnumerator)Invoke(growth,"Grow");
        Assert.IsTrue(sequence.MoveNext());Assert.IsInstanceOf<WaitForSeconds>(sequence.Current);
        Assert.AreEqual(5f,typeof(WaitForSeconds).GetField("m_Seconds",Flags).GetValue(sequence.Current));
        Assert.AreEqual(0,Property(growth,"Stage"));
        Assert.IsTrue(sequence.MoveNext());Assert.AreEqual(1,Property(growth,"Stage"));
        Assert.AreEqual(5f,typeof(WaitForSeconds).GetField("m_Seconds",Flags).GetValue(sequence.Current));
        Assert.IsFalse(sequence.MoveNext());Assert.AreEqual(2,Property(growth,"Stage"));
        Assert.AreEqual(10f,Property(manager,"CurrentRestoration"));Assert.AreEqual(2,events);
        var repeat=(IEnumerator)Invoke(growth,"Grow");while(repeat.MoveNext()){}
        Assert.AreEqual(10f,Property(manager,"CurrentRestoration"));
        Assert.IsTrue(((SpriteRenderer)Property(growth,"CurrentStageRenderer")).enabled);
    }
    [Test] public void PlantingGuardsRejectMissingSetupAndReentry()
    {
        var spot=Create("PlantingSpot");Assert.IsFalse((bool)Property(spot,"CanInteract"));
        Invoke(spot,"Interact",new object[]{null});
        Set(spot,"plantPrefab",Create("QuenualGrowth"));Set(spot,"restorationManager",Create("RestorationManager"));
        Assert.IsTrue((bool)Property(spot,"CanInteract"));Set(spot,"planting",true);
        Assert.IsFalse((bool)Property(spot,"CanInteract"));Set(spot,"planting",false);
        Set(spot,"state",Enum.Parse(Runtime("PlantingSpot").GetNestedType("SpotState"),"PLANTED"));
        Assert.IsFalse((bool)Property(spot,"CanInteract"));
    }
    [Test] public void TileSelectionIsDeterministicAndInBankBounds()
    {
        var select=Runtime("GroundTileSelection").GetMethod("SelectIndex",Flags);
        for(int state=0;state<3;state++)for(int x=-20;x<20;x++)for(int y=-15;y<15;y++)
        {
            object[] args={x,y,2026,state,false,null};int a=(int)select.Invoke(null,args);
            Assert.That(a,Is.InRange(0,47));Assert.AreEqual(a,select.Invoke(null,args));
        }
    }
    [Test] public void TileSelectionAcceptsInvalidProfileAndCleanCells()
    {
        var profileType=Runtime("GroundTileSelection").GetNestedType("Profile");
        var profile=Activator.CreateInstance(profileType);Set(profile,"states",null);
        var select=Runtime("GroundTileSelection").GetMethod("SelectIndex",Flags);
        foreach(int state in new[]{-1,0,1,2,3})
            Assert.That((int)select.Invoke(null,new object[]{-19,14,2026,state,true,profile}),Is.InRange(0,47));
    }
    [TestCase(-100000f,32767)] [TestCase(100000f,-32768)] [TestCase(0f,0)]
    public void YSortingClampsToRendererRange(float y,int expected)
    {
        Assert.AreEqual(expected,Runtime("YSort2D").GetMethod("CalculateOrder",Flags).Invoke(null,new object[]{y,100f,0}));
    }
    [TestCase(1.333333f)] [TestCase(1.777778f)] [TestCase(2.333333f)] [TestCase(3.555556f)]
    public void CameraViewportStaysInsideGround(float aspect)
    {
        var follow=Create("CameraFollow2D");Set(follow,"constrainGroundViewport",true);Set(follow,"minX",-12.35f);Set(follow,"maxX",12.35f);Set(follow,"minY",-10.7f);Set(follow,"maxY",10f);Rect bounds=(Rect)Invoke(follow,"CalculateCenterBounds",aspect,4.3f,true);
        Assert.GreaterOrEqual(bounds.xMin-4.3f*aspect,-20.0001f);
        Assert.LessOrEqual(bounds.xMax+4.3f*aspect,20.0001f);
        Assert.GreaterOrEqual(bounds.yMin-4.3f,-15.0001f);
        Assert.LessOrEqual(bounds.yMax+4.3f,15.0001f);
    }
    [Test] public void GrowthBudgetExpiresAndStaleReleaseDoesNotReleaseReplacement()
    {
        var limiter=Activator.CreateInstance(Runtime("GrowthVFXLimiter"),3);
        object first=null;
        for(int i=0;i<3;i++){object[] args={0f,.5f,null};Assert.IsTrue((bool)Invoke(limiter,"TryAcquire",args));if(i==0)first=args[2];}
        object[] denied={0f,.5f,null};Assert.IsFalse((bool)Invoke(limiter,"TryAcquire",denied));
        Assert.AreEqual(3,Invoke(limiter,"ActiveCount",0f));
        object[] replacement={.6f,.5f,null};Assert.IsTrue((bool)Invoke(limiter,"TryAcquire",replacement));
        Invoke(limiter,"Release",first);Assert.AreEqual(1,Invoke(limiter,"ActiveCount",.6f));
        Invoke(limiter,"Release",replacement[2]);Assert.AreEqual(0,Invoke(limiter,"ActiveCount",.6f));
    }
}
