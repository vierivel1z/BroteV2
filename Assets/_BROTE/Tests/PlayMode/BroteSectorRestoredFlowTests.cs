using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
public sealed class BroteSectorRestoredFlowTests
{
    private const BindingFlags Flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
    private string path;
    private int sounds;
    private InputSettings.BackgroundBehavior previousBackground;
    private InputSettings.EditorInputBehaviorInPlayMode previousEditorInput;
    private Action<AudioClip,int,bool> observer;
    private static Type T(string n)=>Type.GetType(n+", Assembly-CSharp",true);
    private static Component Find(string n)=>UnityEngine.Object.FindObjectsByType(T(n),FindObjectsSortMode.None).Cast<Component>().Single();
    private static object Call(object o,string n,params object[] args)=>o.GetType().GetMethod(n,Flags).Invoke(o,args);
    private static object Get(object o,string n)=>o.GetType().GetProperty(n,Flags).GetValue(o);
    [SetUp] public void Setup()
    {
        previousEditorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        previousBackground=InputSystem.settings.backgroundBehavior;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
        path=Path.Combine(Path.GetTempPath(),"BROTE-Final-"+Guid.NewGuid().ToString("N")+".json");
        T("BroteSaveSystem").GetField("editorTestSavePath",Flags).SetValue(null,path);Time.timeScale=1;sounds=0;
        observer=(clip,c,g)=>{if(clip.name=="SFX_SectorRestored")sounds++;};
        T("BroteAudioFeedback").GetEvent("PlaybackRequested").AddEventHandler(null,observer);
    }
    private static IEnumerator Load(){yield return SceneManager.LoadSceneAsync("Assets/_BROTE/Scenes/Acopalca/Acopalca.unity");yield return null;yield return null;}
    [UnityTearDown] public IEnumerator Cleanup()
    {
        T("BroteAudioFeedback").GetEvent("PlaybackRequested").RemoveEventHandler(null,observer);
        if(Keyboard.current!=null)InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());
        yield return SceneManager.LoadSceneAsync("Assets/_BROTE/Scenes/UI/MainMenu.unity");yield return null;
        T("BroteSaveSystem").GetField("editorTestSavePath",Flags).SetValue(null,null);
        InputSystem.settings.backgroundBehavior=previousBackground;InputSystem.settings.editorInputBehaviorInPlayMode=previousEditorInput;
        foreach(var suffix in new[]{"",".tmp"})if(File.Exists(path+suffix))File.Delete(path+suffix);Time.timeScale=1;
    }
    [UnityTest] public IEnumerator KeyboardKPreservesEconomySpotsAndSaveAndFinalControlsWork()
    {
        yield return Load();var save=Find("BroteSaveSystem");Assert.IsTrue((bool)Call(save,"Save"));
        string disk=File.ReadAllText(path);var before=Call(save,"Capture");
        var keyboard=Keyboard.current??InputSystem.AddDevice<Keyboard>();
        InputSystem.EnableDevice(keyboard);
        InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.K));yield return null;yield return null;
        InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
        var manager=Find("RestorationManager");var pause=Find("BrotePauseMenu");
        Assert.AreEqual(100f,Get(manager,"RestorationPercentage"));Assert.AreEqual(0f,Time.timeScale);
        Assert.IsTrue((bool)T("BrotePauseMenu").GetProperty("IsSectorRestoredVisible").GetValue(null));
        var after=Call(save,"Capture");after.GetType().GetField("restoration").SetValue(after,0f);
        Assert.AreEqual(JsonUtility.ToJson(before),JsonUtility.ToJson(after));
        Assert.AreEqual(disk,File.ReadAllText(path));Assert.IsFalse((bool)Call(save,"Save"));
        var player=Find("PlayerInteraction");var position=player.transform.position;
        InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W,Key.E));yield return new WaitForSecondsRealtime(.1f);
        Assert.AreEqual(position,player.transform.position);
        InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
        yield return new WaitForSecondsRealtime(.3f);Assert.AreEqual(1,sounds);
        InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));yield return null;yield return null;
        InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
        Assert.IsTrue((bool)T("BrotePauseMenu").GetProperty("IsSectorRestoredVisible").GetValue(null));
        var root=pause.GetComponent<UIDocument>().rootVisualElement;
        foreach(var size in new[]{new Vector2(1920,1080),new Vector2(1280,720),new Vector2(1440,900),new Vector2(1024,768),new Vector2(900,900)})
        {
            root.style.right=root.style.bottom=StyleKeyword.Auto;root.style.width=size.x;root.style.height=size.y;
            yield return null;yield return null;var panel=root.Q("sector-restored-panel");
            Assert.LessOrEqual(panel.worldBound.xMax,root.worldBound.xMax);Assert.LessOrEqual(panel.worldBound.yMax,root.worldBound.yMax);
            foreach(var label in panel.Query<Label>().ToList())
            {
                var m=label.MeasureTextSize(label.text,label.contentRect.width,VisualElement.MeasureMode.AtMost,0,VisualElement.MeasureMode.Undefined);
                Assert.LessOrEqual(m.y,label.contentRect.height+.5f,label.text);
            }
        }
        var hud=Find("BroteHUD");Call(hud,"ShowContext","Prueba de contexto");Call(hud,"LateUpdate");
        Assert.AreEqual(DisplayStyle.None,hud.GetComponent<UIDocument>().rootVisualElement.Q<Label>("hud-interaction-prompt").style.display.value);
        using(var submit=NavigationSubmitEvent.GetPooled()){submit.target=root.Q<Button>("sector-continue");root.Q<Button>("sector-continue").SendEvent(submit);}
        Assert.AreEqual(1f,Time.timeScale);Assert.AreEqual(100f,Get(manager,"RestorationPercentage"));
        Call(hud,"LateUpdate");Assert.AreEqual(DisplayStyle.Flex,hud.GetComponent<UIDocument>().rootVisualElement.Q<Label>("hud-interaction-prompt").style.display.value);
        Assert.IsFalse((bool)Call(pause,"TryDebugRestoration"));yield return new WaitForSecondsRealtime(.2f);Assert.AreEqual(1,sounds);
        // Return uses the same action as its button, never New Game or save deletion.
        Call(pause,"ReturnToMainMenu");while(SceneManager.GetActiveScene().name!="MainMenu")yield return null;
        Assert.AreEqual(disk,File.ReadAllText(path));yield return Load();Assert.AreEqual(0f,Get(Find("RestorationManager"),"RestorationPercentage"));
    }
    [UnityTest] public IEnumerator OfficialRestorationEventShowsFinalOnceAndMenuSavesNormalProgress()
    {
        yield return Load();var pause=Find("BrotePauseMenu");var manager=Find("RestorationManager");
        // No trees/economy needed to validate event routing; save guard avoids an incoherent test fixture.
        Call(Find("BroteSaveSystem"),"BeginRestorationDebugPreview");
        Call(manager,"AddRestoration",50f);Assert.IsFalse((bool)T("BrotePauseMenu").GetProperty("IsSectorRestoredVisible").GetValue(null));
        Call(manager,"AddRestoration",50f);yield return new WaitForSecondsRealtime(.3f);
        Assert.IsTrue((bool)T("BrotePauseMenu").GetProperty("IsSectorRestoredVisible").GetValue(null));Assert.AreEqual(1,sounds);
        Call(pause,"ContinueAfterRestoration");Call(manager,"AddRestoration",10f);Call(pause,"ShowSectorRestored");yield return null;
        Assert.IsFalse((bool)T("BrotePauseMenu").GetProperty("IsPaused").GetValue(null));Assert.AreEqual(1,sounds);
        yield return Load();var session=Find("BroteIncrementalSession");for(int i=0;i<10;i++)Call(session,"Click");
        var final=Find("BrotePauseMenu").GetComponent<UIDocument>().rootVisualElement.Q<Button>("sector-main-menu");
        // Hidden button action is tested directly through the public shared handler.
        Call(Find("BrotePauseMenu"),"ReturnToMainMenu");while(SceneManager.GetActiveScene().name!="MainMenu")yield return null;
        Assert.IsTrue(File.Exists(path));yield return Load();Assert.AreEqual(1,Get(Find("SeedlingInventory"),"CurrentSeedlings"));
    }
    [UnityTest] public IEnumerator LastRealTreeCompletesSectorKeepsRewardAndSavesBeforeReturningToMenu()
    {
        yield return Load();var wallet=Find("BroteMinkaManager");Call(wallet,"Add",20);
        var upgrade=Find("BroteUpgradeManager");var kind=Enum.ToObject(T("BroteUpgrade"),4);
        Assert.IsTrue((bool)Call(upgrade,"TryBuy",kind));Assert.IsTrue((bool)Call(upgrade,"TryBuy",kind));
        var spots=UnityEngine.Object.FindObjectsByType(T("PlantingSpot"),FindObjectsSortMode.None).Cast<Component>().OrderBy(x=>(string)Get(x,"StableId")).ToArray();
        foreach(var spot in spots.Take(9)){Call(spot,"RestoreSavedTree",true,2);spot.GetType().GetProperty("MinkaRewarded").SetValue(spot,true);}
        Call(spots[9],"RestoreSavedTree",true,1);Call(Find("RestorationManager"),"RestoreSavedPoints",90f);
        int before=(int)Get(wallet,"Minka");sounds=0;float deadline=Time.realtimeSinceStartup+8;
        while((float)Get(Find("RestorationManager"),"RestorationPercentage")<100&&Time.realtimeSinceStartup<deadline)yield return null;
        Assert.AreEqual(100f,Get(Find("RestorationManager"),"RestorationPercentage"));
        Assert.AreEqual(before+1,Get(wallet,"Minka"));Assert.AreEqual(0f,Time.timeScale);
        yield return new WaitForSecondsRealtime(.3f);Assert.AreEqual(1,sounds);
        Assert.IsTrue((bool)Call(Find("BroteSaveSystem"),"Save"));
        var button=Find("BrotePauseMenu").GetComponent<UIDocument>().rootVisualElement.Q<Button>("sector-main-menu");
        using(var submit=NavigationSubmitEvent.GetPooled()){submit.target=button;button.SendEvent(submit);}
        while(SceneManager.GetActiveScene().name!="MainMenu")yield return null;
        sounds=0;yield return Load();yield return new WaitForSecondsRealtime(.3f);
        Assert.AreEqual(100f,Get(Find("RestorationManager"),"RestorationPercentage"));Assert.AreEqual(0,sounds);
        Assert.IsFalse((bool)T("BrotePauseMenu").GetProperty("IsSectorRestoredVisible").GetValue(null));
    }

}
