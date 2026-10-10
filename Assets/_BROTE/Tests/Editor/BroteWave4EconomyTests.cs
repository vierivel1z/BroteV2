using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class BroteWave4EconomyTests
{
    private GameObject owner,player;
    private Component wallet,upgrades,production,inventory,objectives;
    private static Type T(string name)=>Type.GetType(name+", Assembly-CSharp",true);
    private static object Call(object o,string n,params object[] args)=>o.GetType().GetMethod(n).Invoke(o,args);
    private static object Get(object o,string n)=>o.GetType().GetProperty(n).GetValue(o);
    private static object Kind(int n)=>Enum.ToObject(T("BroteUpgrade"),n);
    [SetUp] public void Setup()
    {
        Time.timeScale=1;owner=new GameObject("Economy test");player=new GameObject("Inventory test");
        wallet=owner.AddComponent(T("BroteMinkaManager"));upgrades=owner.AddComponent(T("BroteUpgradeManager"));
        production=owner.AddComponent(T("NurseryProductionManager"));objectives=owner.AddComponent(T("BroteRestorationObjectives"));
        inventory=player.AddComponent(T("SeedlingInventory"));Call(production,"Initialize",inventory);
    }
    [TearDown] public void Cleanup(){UnityEngine.Object.DestroyImmediate(owner);UnityEngine.Object.DestroyImmediate(player);Time.timeScale=1;}
    [Test] public void BaseClickNeedsTenClicksAndProducesExactlyOne()
    { for(int i=0;i<9;i++)Call(production,"Click");Assert.AreEqual(0,Get(inventory,"CurrentSeedlings"));Call(production,"Click");Assert.AreEqual(1,Get(inventory,"CurrentSeedlings"));Assert.AreEqual(0d,Get(production,"Progress")); }
    [Test] public void OverflowIsPreservedAndFullInventoryDoesNotLoseProgress()
    {
        Call(production,"Restore",95d);Call(production,"Advance",15d);Assert.AreEqual(1,Get(inventory,"CurrentSeedlings"));Assert.AreEqual(10d,Get(production,"Progress"));
        Call(inventory,"AddSeedlings",4);Call(production,"Click");Call(production,"Tick",1000d);Assert.AreEqual(10d,Get(production,"Progress"));Assert.AreEqual(5,Get(inventory,"CurrentSeedlings"));
        Call(inventory,"TryConsumeSeedling");Call(production,"Advance",90d);Assert.AreEqual(5,Get(inventory,"CurrentSeedlings"));
    }
    [TestCase(0,new[]{1,2,4})] [TestCase(1,new[]{2,4,7})] [TestCase(2,new[]{2,4,7})] [TestCase(3,new[]{2,5})] [TestCase(4,new[]{4,8})]
    public void ExactCostsAndMaximumLevels(int kind,int[] costs)
    {
        Call(wallet,"Add",100);int remaining=100;
        for(int i=0;i<costs.Length;i++){Assert.AreEqual(costs[i],Call(upgrades,"NextCost",Kind(kind)));Assert.IsTrue((bool)Call(upgrades,"TryBuy",Kind(kind)));remaining-=costs[i];Assert.AreEqual(remaining,Get(wallet,"Minka"));}
        Assert.IsFalse((bool)Call(upgrades,"TryBuy",Kind(kind)));Assert.AreEqual(remaining,Get(wallet,"Minka"));
    }
    [Test] public void InsufficientMinkaDoesNotChangeLevelOrWallet()
    {Assert.IsFalse((bool)Call(upgrades,"TryBuy",Kind(4)));Assert.AreEqual(0,Call(upgrades,"Level",Kind(4)));Assert.AreEqual(0,Get(wallet,"Minka"));}
    [Test] public void PassiveFractionsAccumulateAndTreeCountScalesIncome()
    {Call(wallet,"Tick",25d,2);Assert.AreEqual(0,Get(wallet,"Minka"));Assert.AreEqual(.5d,(double)Get(wallet,"Fraction"),1e-8);Call(wallet,"Tick",25d,2);Assert.AreEqual(1,Get(wallet,"Minka"));Assert.AreEqual(0d,(double)Get(wallet,"Fraction"),1e-8);}
    [Test] public void SpendingUpgradesBeforeExpansionCannotPermanentlySoftLock()
    {
        Call(wallet,"Add",1);Assert.IsTrue((bool)Call(upgrades,"TryBuy",Kind(0)));Assert.AreEqual(0,Get(wallet,"Minka"));
        Call(wallet,"Tick",400d,1);Assert.IsTrue((bool)Call(upgrades,"TryBuy",Kind(4)));
        Call(wallet,"Tick",800d,1);Assert.IsTrue((bool)Call(upgrades,"TryBuy",Kind(4)));Assert.AreEqual(2,Call(upgrades,"Level",Kind(4)));
    }
    [Test] public void AutomaticProductionAndManualClicksCoexistAndDoNotBankOfflineInventory()
    {
        Call(wallet,"Add",2);Call(upgrades,"TryBuy",Kind(1));Call(production,"Tick",45d);Call(production,"Click");Assert.AreEqual(1,Get(inventory,"CurrentSeedlings"));
        Call(production,"Tick",100000d);Assert.AreEqual(5,Get(inventory,"CurrentSeedlings"));Assert.AreEqual(0d,Get(production,"Progress"));
    }
    [Test] public void DynamicCapacityClampsAndExpandsInventorySafely()
    {Call(inventory,"SetCapacity",20);Call(inventory,"AddSeedlings",30);Assert.AreEqual(20,Get(inventory,"CurrentSeedlings"));Call(inventory,"SetCapacity",8);Assert.AreEqual(8,Get(inventory,"CurrentSeedlings"));}
    [Test] public void ObjectivesPayOnlyOnceIncludingOutOfOrderObservationsAndRestore()
    {
        Call(objectives,"Observe",5);Call(objectives,"Observe",4);Assert.AreEqual(0,Get(wallet,"Minka"));
        for(int i=0;i<4;i++)Call(objectives,"Observe",i);
        Assert.AreEqual(8,Get(wallet,"Minka"));Assert.AreEqual(6,Get(objectives,"Claimed"));
        Call(objectives,"Restore",63,6);for(int i=0;i<6;i++)Call(objectives,"Observe",i);Assert.AreEqual(8,Get(wallet,"Minka"));
    }
    [Test] public void PausedClickAndPurchasesAreRejected()
    {Call(wallet,"Add",10);Time.timeScale=0;Call(wallet,"Tick",1000d,10);Assert.AreEqual(10,Get(wallet,"Minka"));Call(production,"Tick",1000d);Call(production,"Click");Assert.AreEqual(0d,Get(production,"Progress"));Assert.IsFalse((bool)Call(upgrades,"TryBuy",Kind(0)));}
    [Test] public void LegacyExpansionObservationLetsRemainingObjectivesFinishWithoutBuyingMaximumExpansion()
    {
        // Migrated partial v1: first three milestones claimed, expansion already open.
        Call(objectives,"Restore",39,3);Call(objectives,"Observe",3);Assert.AreEqual(4,Get(objectives,"Claimed"));
        Call(objectives,"Observe",4);Assert.AreEqual(6,Get(objectives,"Claimed"));Assert.AreEqual(4,Get(wallet,"Minka"));
        Call(objectives,"Observe",5);Assert.AreEqual(4,Get(wallet,"Minka"));
    }
    [Test] public void SaveV1MigratesWithoutLosingTenLegacyTreesAndWithoutRewardReplay()
    {
        var data=Activator.CreateInstance(T("BroteSaveData"));var spots=Array.CreateInstance(T("BroteSavedSpot"),10);
        for(int i=0;i<10;i++){var s=Activator.CreateInstance(T("BroteSavedSpot"));s.GetType().GetField("id").SetValue(s,(i+1).ToString("00"));s.GetType().GetField("planted").SetValue(s,true);s.GetType().GetField("stage").SetValue(s,2);spots.SetValue(s,i);}
        data.GetType().GetField("spots").SetValue(data,spots);data.GetType().GetField("restoration").SetValue(data,100f);data.GetType().GetField("inventory").SetValue(data,5);
        Assert.IsTrue((bool)Call(data,"IsValid"));Call(data,"MigrateV1");Assert.IsTrue((bool)Call(data,"IsValid"));
        Assert.AreEqual(2,data.GetType().GetField("saveVersion").GetValue(data));Assert.AreEqual(2,data.GetType().GetField("expansionLevel").GetValue(data));Assert.AreEqual(0,data.GetType().GetField("minka").GetValue(data));
        var clone=JsonUtility.FromJson(JsonUtility.ToJson(data),T("BroteSaveData"));Assert.IsTrue((bool)Call(clone,"IsValid"));Assert.AreEqual(100f,clone.GetType().GetField("restoration").GetValue(clone));
        data.GetType().GetField("minkaFraction").SetValue(data,double.NaN);Assert.IsFalse((bool)Call(data,"IsValid"));
    }
}
