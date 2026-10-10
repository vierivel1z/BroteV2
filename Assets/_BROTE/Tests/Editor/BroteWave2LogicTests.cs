using System;
using System.Reflection;
using NUnit.Framework;

public sealed class BroteWave2LogicTests
{
    private const BindingFlags Flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance;
    private static Type Runtime(string type)=>Type.GetType(type+", Assembly-CSharp",true);
    private static object Call(object owner,string name,params object[] args)=>owner.GetType().GetMethod(name,Flags).Invoke(owner,args);
    [Test] public void SimultaneousIncreasesAggregateWithoutRepeatingMilestone()
    {
        var state=Activator.CreateInstance(Runtime("BroteRestorationFeedbackState"));Call(state,"Sync",0f);
        Assert.AreEqual(0,Call(state,"Observe",10f,1f));Assert.AreEqual(20,Call(state,"Observe",20f,1.02f));Assert.AreEqual(0,Call(state,"Observe",30f,1.04f));
        object[] before={1.1f,null};Assert.IsFalse((bool)Call(state,"TryTake",before));
        object[] after={1.2f,null};Assert.IsTrue((bool)Call(state,"TryTake",after));Assert.AreEqual(30f,after[1]);
        Assert.AreEqual(40,Call(state,"Observe",40f,2f));Assert.AreEqual(50,Call(state,"Observe",50f,3f));Assert.AreEqual(100,Call(state,"Observe",100f,4f));
        Call(state,"Sync",100f);Assert.AreEqual(0,Call(state,"Observe",100f,5f));
    }
    [TestCase(1f,.97f,false)] [TestCase(1f,.8f,true)]
    public void InteractionSelectionHasSmallStableMargin(float previous,float candidate,bool expected)
    {
        Assert.AreEqual(expected,Runtime("PlayerInteraction").GetMethod("ShouldSwitch",Flags).Invoke(null,new object[]{previous,candidate,.08f}));
    }
}
