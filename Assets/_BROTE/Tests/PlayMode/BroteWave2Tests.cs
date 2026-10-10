using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

public sealed class BroteWave2Tests
{
    private const BindingFlags Flags=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
    private readonly List<GameObject> roots=new List<GameObject>();
    private float savedScale;
    private static Type Runtime(string name)=>Type.GetType(name+", Assembly-CSharp",true);
    private static object Call(object owner,string name,params object[] args)=>owner.GetType().GetMethod(name,Flags).Invoke(owner,args);
    private static object Static(string type,string name,params object[] args)=>Runtime(type).GetMethod(name,Flags).Invoke(null,args);
    [SetUp] public void Setup(){savedScale=Time.timeScale;Time.timeScale=1f;}
    [UnityTearDown] public IEnumerator Cleanup(){foreach(var root in roots)if(root!=null)Object.Destroy(root);roots.Clear();Time.timeScale=savedScale;yield return null;}
    [UnityTest] public IEnumerator PauseRestoresPreviousSpeedAndDisableCannotLeaveTimeFrozen()
    {
        var root=new GameObject("Wave2 HUD test");roots.Add(root);root.AddComponent(Runtime("BroteHUD"));
        var pause=root.GetComponentInChildren(Runtime("BrotePauseMenu"));Assert.IsNotNull(pause);
        Time.timeScale=.65f;Call(pause,"SetPaused",true);Assert.AreEqual(0f,Time.timeScale);
        Assert.IsTrue((bool)Runtime("BrotePauseMenu").GetProperty("IsPaused").GetValue(null));
        Call(pause,"SetPaused",false);Assert.AreEqual(.65f,Time.timeScale);
        Call(pause,"SetPaused",true);((Behaviour)pause).enabled=false;Assert.AreEqual(.65f,Time.timeScale);
        Assert.IsFalse((bool)Runtime("BrotePauseMenu").GetProperty("IsPaused").GetValue(null));yield return null;
    }
    [UnityTest] public IEnumerator ResourcePresetsAndOfficialAmbienceAreConnectedAndEmptyClipsStaySilent()
    {
        for(int stage=0;stage<3;stage++)
        {
            var preset=(Component)Static("BroteVFXPool","Preset",stage);Assert.IsNotNull(preset);
            var settings=preset.GetType().GetField("settings").GetValue(preset);
            Assert.AreEqual(new[]{3,5,8}[stage],settings.GetType().GetField("leafCount").GetValue(settings));
        }
        foreach(string name in new[]{"Sector01Ambience","DistantRiverAudio"})
        {
            var prefab=Resources.Load<GameObject>("Audio/"+name);Assert.IsNotNull(prefab);
            var instance=Object.Instantiate(prefab);roots.Add(instance);yield return null;
            Assert.AreEqual(name=="Sector01Ambience"?3:1,instance.GetComponentsInChildren<AudioSource>().Length);
        }
    }
    [UnityTest] public IEnumerator EmptyAmbientZoneRefreshAllocatesNoSources()
    {
        var empty=new GameObject("Empty ambience");roots.Add(empty);empty.SetActive(false);
        var zone=empty.AddComponent(Runtime("AmbientZone"));Call(zone,"Refresh");
        Assert.AreEqual(0,empty.GetComponentsInChildren<AudioSource>(true).Length);yield return null;
    }
    [UnityTest] public IEnumerator NativeOverlapSelectionIsStableAndClearsOutOfRange()
    {
        var root=new GameObject("Wave2 interaction test");roots.Add(root);var player=root.AddComponent(Runtime("PlayerInteraction"));((Behaviour)player).enabled=false;
        for(int i=0;i<2;i++)
        {
            var target=new GameObject("Target "+i);roots.Add(target);target.transform.position=new Vector3(1000+i*.1f,1000,0);target.AddComponent(Runtime("NurserySeedlingSource"));var circle=target.AddComponent<CircleCollider2D>();circle.isTrigger=true;
        }
        root.transform.position=new Vector3(999.8f,1000,0);Physics2D.SyncTransforms();Call(player,"RefreshTarget");var first=Runtime("PlayerInteraction").GetProperty("CurrentInteractable").GetValue(player);
        Assert.IsNotNull(first);root.transform.position=new Vector3(1000.06f,1000,0);Physics2D.SyncTransforms();Call(player,"RefreshTarget");Assert.AreSame(first,Runtime("PlayerInteraction").GetProperty("CurrentInteractable").GetValue(player));
        root.transform.position=new Vector3(1000.2f,1000,0);Physics2D.SyncTransforms();Call(player,"RefreshTarget");Assert.AreNotSame(first,Runtime("PlayerInteraction").GetProperty("CurrentInteractable").GetValue(player));
        root.transform.position=new Vector3(100,100,0);Physics2D.SyncTransforms();Call(player,"RefreshTarget");Assert.IsNull(Runtime("PlayerInteraction").GetProperty("CurrentInteractable").GetValue(player));yield return null;
    }
    [UnityTest] public IEnumerator PoolReusesActualParticleSystemsAndRejectsFourthEffect()
    {
        var root=new GameObject("Wave2 VFX owner");roots.Add(root);var effects=new Component[3];
        try
        {
            for(int stage=0;stage<3;stage++)
            {
                var preset=(Component)Static("BroteVFXPool","Preset",stage);Assert.IsNotNull(preset);
                var effect=(Component)Static("BroteVFXPool","Acquire",stage,root.transform);Assert.IsNotNull(effect);effects[stage]=effect;
                var settings=preset.GetType().GetField("settings").GetValue(preset);
                Call(effect,"Play",settings,null,null,null,null,Color.white,Color.green,Color.green,0,.12f);
                Assert.AreEqual(3,effect.GetComponentsInChildren<ParticleSystem>().Length);
            }
            Assert.IsNull(Static("BroteVFXPool","Acquire",0,root.transform));Static("BroteVFXPool","Release",effects[0]);
            var reused=(Component)Static("BroteVFXPool","Acquire",2,root.transform);Assert.AreSame(effects[0],reused);
            Assert.AreEqual(3,reused.GetComponentsInChildren<ParticleSystem>().Length);yield return null;
        }
        finally {foreach(var effect in effects)if(effect!=null)Static("BroteVFXPool","Release",effect);}
    }
    [UnityTest] public IEnumerator MasterSfxAndFocusAreIndependentAndPreferencesAreRestored()
    {
        const string masterKey="BROTE.MasterVolume",sfxKey="BROTE.SFXVolume";
        bool hasMaster=PlayerPrefs.HasKey(masterKey),hasSfx=PlayerPrefs.HasKey(sfxKey);
        float master=PlayerPrefs.GetFloat(masterKey,1),sfx=PlayerPrefs.GetFloat(sfxKey,1);
        try
        {
            var root=new GameObject("Wave2 Audio test");roots.Add(root);var manager=root.AddComponent(Runtime("BroteAudioManager"));
            Static("BroteAudioManager","SetMasterVolume",.4f);Static("BroteAudioManager","SetSFXVolume",.25f);
            var category=Runtime("BroteAudioCategory");
            Call(manager,"OnApplicationFocus",true);
            Assert.AreEqual(.1f,(float)Static("BroteAudioManager","VolumeFor",Enum.Parse(category,"SFX")),.0001f);
            Assert.AreEqual(.4f,(float)Static("BroteAudioManager","VolumeFor",Enum.Parse(category,"UI")),.0001f);
            Assert.AreEqual(.4f,(float)Static("BroteAudioManager","VolumeFor",Enum.Parse(category,"Ambience")),.0001f);
            Call(manager,"OnApplicationFocus",false);Assert.AreEqual(0f,(float)Static("BroteAudioManager","VolumeFor",Enum.Parse(category,"UI")));
            Call(manager,"OnApplicationFocus",true);Assert.AreEqual(.4f,(float)Static("BroteAudioManager","VolumeFor",Enum.Parse(category,"UI")),.0001f);
            yield return null;
        }
        finally
        {
            Static("BroteAudioManager","SetMasterVolume",master);Static("BroteAudioManager","SetSFXVolume",sfx);
            if(!hasMaster)PlayerPrefs.DeleteKey(masterKey);if(!hasSfx)PlayerPrefs.DeleteKey(sfxKey);PlayerPrefs.Save();Static("BroteAudioManager","Reset");
        }
    }
}
