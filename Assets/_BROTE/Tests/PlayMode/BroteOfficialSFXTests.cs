using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class BroteOfficialSFXTests
{
    private const BindingFlags Flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
    private string path;
    private readonly List<string> requests=new List<string>();
    private readonly List<int> categories=new List<int>();
    private Action<AudioClip,int,bool> observer;
    private static Type T(string n)=>Type.GetType(n+", Assembly-CSharp",true);
    private static object Call(object o,string n,params object[] args)=>o.GetType().GetMethod(n,Flags).Invoke(o,args);
    private static object Get(object o,string n)=>o.GetType().GetProperty(n,Flags).GetValue(o);
    private static Component Find(string n)=>UnityEngine.Object.FindObjectsByType(T(n)).Cast<Component>().Single();
    private static Component[] Spots()=>UnityEngine.Object.FindObjectsByType(T("PlantingSpot")).Cast<Component>().OrderBy(x=>(string)Get(x,"StableId")).ToArray();
    private static object Kind(int i)=>Enum.ToObject(T("BroteUpgrade"),i);
    private int Count(string name)=>requests.Count(x=>x=="SFX_"+name);
    private void Clear(){requests.Clear();categories.Clear();}
    [SetUp] public void Setup()
    {
        path=Path.Combine(Path.GetTempPath(),"BROTE-OfficialSFX-"+Guid.NewGuid().ToString("N")+".json");
        T("BroteSaveSystem").GetField("editorTestSavePath",Flags).SetValue(null,path);Time.timeScale=1;
        observer=(clip,category,growth)=>{requests.Add(clip.name);categories.Add(category);};
        T("BroteAudioFeedback").GetEvent("PlaybackRequested").AddEventHandler(null,observer);
    }
    [UnityTearDown] public IEnumerator Cleanup()
    {
        T("BroteAudioFeedback").GetEvent("PlaybackRequested").RemoveEventHandler(null,observer);
        Time.timeScale=1;yield return SceneManager.LoadSceneAsync("Assets/_BROTE/Scenes/UI/MainMenu.unity");yield return null;
        T("BroteSaveSystem").GetField("editorTestSavePath",Flags).SetValue(null,null);
        foreach(string suffix in new[]{"",".tmp",".v1.backup"})if(File.Exists(path+suffix))File.Delete(path+suffix);
    }
    private IEnumerator Load(){yield return SceneManager.LoadSceneAsync("Assets/_BROTE/Scenes/Acopalca/Acopalca.unity");yield return null;yield return null;Clear();}
    [UnityTest] public IEnumerator PlantRequestsOnlyAfterSuccessfulInventoryConsumptionAndNeverTwice()
    {
        yield return Load();var spot=Spots()[0];var player=Find("PlayerInteraction");
        Call(spot,"Interact",player);yield return null;Assert.AreEqual(0,Count("Plant"));
        Call(Find("SeedlingInventory"),"AddSeedlings",1);
        Call(Spots().First(x=>Get(x,"State").ToString()=="LOCKED"),"Interact",player);
        Call(Spots().First(x=>Get(x,"State").ToString()=="AVAILABLE"),"Interact",player);
        yield return null;Assert.AreEqual(0,Count("Plant"));
        Call(spot,"Interact",player);yield return null;yield return null;
        Assert.AreEqual("PLANTED",Get(spot,"State").ToString());Assert.AreEqual(0,Get(Find("SeedlingInventory"),"CurrentSeedlings"));
        Assert.AreEqual(1,Count("Plant"));
        Call(spot,"Interact",player);
        var tree=(Component)Get(spot,"PlantedTree");Call(tree.GetComponent(T("QuenualGrowthFeedback")),"OnStageChanged",0);
        yield return null;Assert.AreEqual(1,Count("Plant"));
        Assert.AreEqual(0,tree.GetComponentsInChildren<AudioSource>().Length);
        Assert.IsTrue(categories.All(x=>x==1));
        yield return new WaitForSeconds(.6f);
    }
    [UnityTest] public IEnumerator SeedlingCompletionOverflowAutomaticAndFullInventoryUseRealDeliveredEvent()
    {
        yield return Load();var session=Find("BroteIncrementalSession");var source=Find("NurserySeedlingSource");
        Call(source,"Interact",Find("PlayerInteraction"));yield return null;Assert.AreEqual(0,Count("SeedlingReady"));
        Call(session,"Click");Assert.AreEqual(0,Count("SeedlingReady"));
        for(int i=0;i<9;i++)Call(session,"Click");Assert.AreEqual(1,Count("SeedlingReady"));
        Call(Find("BroteMinkaManager"),"Add",10);Call(Find("BroteUpgradeManager"),"TryBuy",Kind(0));Clear();
        Call(Find("NurseryProductionManager"),"Restore",95d);Call(session,"Click");
        Assert.AreEqual(1,Count("SeedlingReady"));Assert.AreEqual(10d,Get(Find("NurseryProductionManager"),"Progress"));
        Call(Find("SeedlingInventory"),"RestoreSavedCount",5);Clear();Call(session,"Click");
        Assert.AreEqual(0,Count("SeedlingReady"));Assert.AreEqual(10d,Get(Find("NurseryProductionManager"),"Progress"));
        Call(Find("SeedlingInventory"),"RestoreSavedCount",0);Call(Find("BroteUpgradeManager"),"TryBuy",Kind(1));
        Call(Find("NurseryProductionManager"),"Restore",90d);Clear();Call(Find("NurseryProductionManager"),"Tick",5d);
        Assert.AreEqual(1,Count("SeedlingReady"));Assert.AreEqual(1,Get(Find("SeedlingInventory"),"CurrentSeedlings"));
    }
    [UnityTest] public IEnumerator AllFiveUpgradeKindsSoundOnceOnlyWhenPurchaseSucceeds()
    {
        yield return Load();var up=Find("BroteUpgradeManager");Assert.IsFalse((bool)Call(up,"TryBuy",Kind(0)));Assert.AreEqual(0,Count("Upgrade"));
        Call(Find("BroteMinkaManager"),"Add",100);
        for(int i=0;i<5;i++)Assert.IsTrue((bool)Call(up,"TryBuy",Kind(i)));
        Assert.AreEqual(5,Count("Upgrade"));
        Assert.IsTrue((bool)Call(up,"TryBuy",Kind(0)));Assert.IsTrue((bool)Call(up,"TryBuy",Kind(0)));Clear();
        Assert.IsFalse((bool)Call(up,"TryBuy",Kind(0)));Assert.AreEqual(0,Count("Upgrade"));
        Call(Find("NurserySeedlingSource"),"Interact",Find("PlayerInteraction"));Call(Find("NurseryManagementPanel"),"ToggleView");
        yield return null;Assert.AreEqual(0,Count("Upgrade"));
        Assert.IsTrue((bool)Call(Find("BroteSaveSystem"),"Save"));yield return Load();yield return null;
        Assert.AreEqual(0,Count("Upgrade"));Assert.AreEqual(0,requests.Count);
    }
    [UnityTest] public IEnumerator RealYoungAndDevelopedTransitionsSoundOnceAndDuplicateCallbackStaysSilent()
    {
        yield return Load();var spot=Spots()[0];Call(Find("SeedlingInventory"),"AddSeedlings",1);Call(spot,"Interact",Find("PlayerInteraction"));yield return null;
        var tree=(Component)Get(spot,"PlantedTree");var camera=Camera.main;
        var follow=camera.GetComponent(T("CameraFollow2D"));if(follow!=null)((Behaviour)follow).enabled=false;
        camera.transform.position=tree.transform.position+new Vector3(0,0,-10);
        float deadline=Time.realtimeSinceStartup+8;
        while((int)Get(tree,"Stage")<1 && Time.realtimeSinceStartup<deadline)yield return null;
        yield return null;Assert.AreEqual(1,Get(tree,"Stage"));Assert.AreEqual(1,Count("Growth"));
        Call(tree.GetComponent(T("QuenualGrowthFeedback")),"OnStageChanged",1);yield return null;
        Assert.AreEqual(1,Count("Growth"));deadline=Time.realtimeSinceStartup+8;
        while((int)Get(tree,"Stage")<2 && Time.realtimeSinceStartup<deadline)yield return null;
        yield return null;Assert.AreEqual(2,Get(tree,"Stage"));Assert.AreEqual(2,Count("Growth"));
        Call(tree.GetComponent(T("QuenualGrowthFeedback")),"OnStageChanged",2);yield return null;Assert.AreEqual(2,Count("Growth"));
        Assert.AreEqual(0,tree.GetComponentsInChildren<AudioSource>().Length);Assert.IsTrue(categories.All(x=>x==1));
        yield return new WaitForSeconds(.6f); // Allow existing VFX to release before scene teardown.
    }
    [UnityTest] public IEnumerator GrowthCooldownAndSharedVoicePoolRemainBounded()
    {
        yield return Load();T("BroteAudioFeedback").GetMethod("Reset",Flags).Invoke(null,null);
        var config=Resources.Load<ScriptableObject>("Audio/BroteSFXClips");var clip=(AudioClip)config.GetType().GetField("growth").GetValue(config);
        var play=T("BroteAudioFeedback").GetMethod("PlayOptional");var category=Enum.ToObject(T("BroteAudioCategory"),1);
        Assert.IsTrue((bool)play.Invoke(null,new object[]{clip,Vector3.zero,true,category}));
        Assert.IsFalse((bool)play.Invoke(null,new object[]{clip,Vector3.zero,true,category}));
        yield return new WaitForSecondsRealtime(.2f);
        Assert.IsTrue((bool)play.Invoke(null,new object[]{clip,Vector3.zero,true,category}));
        var voices=(AudioSource[])T("BroteAudioFeedback").GetField("voices",Flags).GetValue(null);Assert.AreEqual(3,voices.Length);
        Assert.LessOrEqual(voices.Count(x=>x!=null && x.isPlaying),3);
        if(voices.Count(x=>x!=null && x.isPlaying)>=2)
        {yield return new WaitForSecondsRealtime(.2f);Assert.IsFalse((bool)play.Invoke(null,new object[]{clip,Vector3.zero,true,category}));}
        Assert.LessOrEqual(voices.Count(x=>x!=null),3);
    }
    [UnityTest] public IEnumerator SectorCrossesOneHundredOnceAndLoadingCompletedSaveStaysSilent()
    {
        yield return Load();var manager=Find("RestorationManager");Call(manager,"RestoreSavedPoints",90f);
        yield return new WaitForSecondsRealtime(.25f);Clear();Call(manager,"AddRestoration",10f);
        yield return new WaitForSecondsRealtime(.25f);Assert.AreEqual(1,Count("SectorRestored"));Assert.IsTrue(categories.All(x=>x==1));
        Call(manager,"AddRestoration",10f);yield return new WaitForSecondsRealtime(.25f);Assert.AreEqual(1,Count("SectorRestored"));
        Assert.IsTrue((bool)T("BrotePauseMenu").GetProperty("IsSectorRestoredVisible").GetValue(null));
        Call(Find("BrotePauseMenu"),"ContinueAfterRestoration");
        // A completed sector fixture must obey Save v2's existing zone invariants.
        Call(Find("BroteMinkaManager"),"Add",20);
        Assert.IsTrue((bool)Call(Find("BroteUpgradeManager"),"TryBuy",Kind(4)));
        Assert.IsTrue((bool)Call(Find("BroteUpgradeManager"),"TryBuy",Kind(4)));
        foreach(var spot in Spots()){Call(spot,"RestoreSavedTree",true,2);spot.GetType().GetProperty("MinkaRewarded").SetValue(spot,true);}
        Call(manager,"RestoreSavedPoints",100f);Assert.IsTrue((bool)Call(Find("BroteSaveSystem"),"Save"));yield return Load();
        yield return new WaitForSecondsRealtime(.25f);Assert.AreEqual(0,Count("SectorRestored"));Assert.AreEqual(0,Count("Growth"));Assert.AreEqual(0,Count("Plant"));
    }
    [UnityTest] public IEnumerator AllSFXRespectMasterSfxAndExistingFocusMute()
    {
        yield return Load();var manager=Find("BroteAudioManager");var type=T("BroteAudioManager");
        var master=type.GetField("master",Flags);var sfx=type.GetField("sfx",Flags);
        var focused=type.GetField("focused",Flags);object oldMaster=master.GetValue(null),oldSfx=sfx.GetValue(null),oldFocus=focused.GetValue(null);
        try
        {
            master.SetValue(null,.5f);sfx.SetValue(null,.4f);Call(manager,"OnApplicationFocus",true);
            var category=Enum.ToObject(T("BroteAudioCategory"),1);
            Assert.AreEqual(.2f,(float)type.GetMethod("VolumeFor").Invoke(null,new[]{category}),.0001f);
            var config=Resources.Load<ScriptableObject>("Audio/BroteSFXClips");var clip=(AudioClip)config.GetType().GetField("upgrade").GetValue(config);
            T("BroteAudioFeedback").GetMethod("PlayOptional").Invoke(null,new object[]{clip,Vector3.zero,false,category});
            T("BroteAudioFeedback").GetMethod("RefreshVolumes").Invoke(null,null);
            var voices=(AudioSource[])T("BroteAudioFeedback").GetField("voices",Flags).GetValue(null);
            foreach(var voice in voices.Where(x=>x!=null))Assert.AreEqual(.28f*.2f,voice.volume,.0001f);
            Call(manager,"OnApplicationFocus",false);
            Assert.AreEqual(0f,type.GetMethod("VolumeFor").Invoke(null,new[]{category}));
            T("BroteAudioFeedback").GetMethod("RefreshVolumes").Invoke(null,null);
            foreach(var voice in voices.Where(x=>x!=null))Assert.AreEqual(0f,voice.volume);
            Assert.LessOrEqual(UnityEngine.Object.FindObjectsByType<AudioListener>().Length,1);
        }
        finally{master.SetValue(null,oldMaster);sfx.SetValue(null,oldSfx);focused.SetValue(null,oldFocus);}
    }
}
