using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class BroteWave3SaveTests
{
    private static Type Runtime(string name)=>Type.GetType(name+", Assembly-CSharp",true);
    private static void Set(object owner,string name,object value)=>owner.GetType().GetField(name).SetValue(owner,value);
    private static object Get(object owner,string name)=>owner.GetType().GetField(name).GetValue(owner);
    private static object Data(int planted,int inventory,bool completed=false)
    {
        var data=Activator.CreateInstance(Runtime("BroteSaveData"));Set(data,"inventory",inventory);Set(data,"tutorialCompleted",completed);Set(data,"restoration",planted*10f);
        var spots=Array.CreateInstance(Runtime("BroteSavedSpot"),10);
        for(int i=0;i<10;i++){var spot=Activator.CreateInstance(Runtime("BroteSavedSpot"));Set(spot,"id",(i+1).ToString("00"));Set(spot,"planted",i<planted);Set(spot,"stage",i<planted?2:0);spots.SetValue(spot,i);}
        Set(data,"spots",spots);return data;
    }
    private static bool Valid(object data)=>(bool)data.GetType().GetMethod("IsValid").Invoke(data,null);
    [TestCase(0,0)] [TestCase(0,5)] [TestCase(5,0)] [TestCase(5,5)] [TestCase(10,0)] [TestCase(10,5)]
    public void JsonRoundTripPreservesSectorProgress(int planted,int inventory)
    {
        var data=Data(planted,inventory,true);string json=JsonUtility.ToJson(data);var loaded=JsonUtility.FromJson(json,Runtime("BroteSaveData"));
        Assert.IsTrue(Valid(loaded));Assert.AreEqual(planted*10f,Get(loaded,"restoration"));Assert.AreEqual(inventory,Get(loaded,"inventory"));Assert.IsTrue((bool)Get(loaded,"tutorialCompleted"));
    }
    [Test] public void RejectsDuplicateMissingAndUnstableIds()
    {
        var data=Data(5,5);var spots=(Array)Get(data,"spots");Set(spots.GetValue(9),"id","01");Assert.IsFalse(Valid(data));
        Set(spots.GetValue(9),"id","10");Set(spots.GetValue(0),"id","1");Assert.IsFalse(Valid(data));
        Set(data,"spots",Array.CreateInstance(Runtime("BroteSavedSpot"),0));Assert.IsFalse(Valid(data));
    }
    [Test] public void RejectsUnsupportedVersionAndSector()
    {
        var data=Data(0,0);Set(data,"saveVersion",3);Assert.IsFalse(Valid(data));Set(data,"saveVersion",1);Set(data,"sector","Sector_02");Assert.IsFalse(Valid(data));
    }
    [TestCase(-1)] [TestCase(6)] public void RejectsInvalidInventory(int count)=>Assert.IsFalse(Valid(Data(0,count)));
    [Test] public void RejectsDoubleRestorationAndInvalidStages()
    {
        var data=Data(5,0);Set(data,"restoration",100f);Assert.IsFalse(Valid(data));Set(data,"restoration",float.NaN);Assert.IsFalse(Valid(data));
        Set(data,"restoration",50f);var spots=(Array)Get(data,"spots");Set(spots.GetValue(0),"stage",3);Assert.IsFalse(Valid(data));Set(spots.GetValue(0),"stage",1);Assert.IsFalse(Valid(data));
    }
    [Test] public void RestoringDevelopedDoesNotContributeOrReplayStageFeedback()
    {
        var owner=new GameObject("Save test");var treeObject=new GameObject("Tree");
        try
        {
            var manager=owner.AddComponent(Runtime("RestorationManager"));manager.GetType().GetMethod("RestoreSavedPoints").Invoke(manager,new object[]{50f});
            var tree=treeObject.AddComponent(Runtime("QuenualGrowth"));
            tree.GetType().GetMethod("RestoreSavedStage").Invoke(tree,new object[]{manager,2});
            Assert.AreEqual(2,tree.GetType().GetProperty("Stage").GetValue(tree));Assert.AreEqual(50f,manager.GetType().GetProperty("CurrentRestoration").GetValue(manager));
            tree.GetType().GetMethod("Initialize").Invoke(tree,new object[]{manager});
            Assert.AreEqual(50f,manager.GetType().GetProperty("CurrentRestoration").GetValue(manager));
            Assert.IsTrue((bool)tree.GetType().GetField("contributed",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(tree));
        }
        finally { UnityEngine.Object.DestroyImmediate(treeObject);UnityEngine.Object.DestroyImmediate(owner); }
    }
}
