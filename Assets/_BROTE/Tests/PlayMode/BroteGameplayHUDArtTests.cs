using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
public sealed class BroteGameplayHUDArtTests
{
    private const BindingFlags Flags=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
    private string path;
    private static Type T(string name)=>Type.GetType(name+", Assembly-CSharp",true);
    private static object Call(object owner,string name,params object[] args)=>owner.GetType().GetMethod(name,Flags).Invoke(owner,args);
    private static object Get(object owner,string name)=>owner.GetType().GetProperty(name,Flags).GetValue(owner);
    private static Component Find(string name)=>UnityEngine.Object.FindObjectsByType(T(name),FindObjectsSortMode.None).Cast<Component>().Single();
    private static VisualElement HUD()=>Find("BroteHUD").GetComponent<UIDocument>().rootVisualElement;
    private static object Kind(int index)=>Enum.ToObject(T("BroteUpgrade"),index);
    [SetUp] public void Setup()
    {
        path=Path.Combine(Path.GetTempPath(),"BROTE-HUD-"+Guid.NewGuid().ToString("N")+".json");
        T("BroteSaveSystem").GetField("editorTestSavePath",Flags).SetValue(null,path);Time.timeScale=1;
    }
    private static IEnumerator Load(){yield return SceneManager.LoadSceneAsync("Assets/_BROTE/Scenes/Acopalca/Acopalca.unity");yield return null;yield return null;}
    [UnityTearDown] public IEnumerator Cleanup()
    {
        yield return SceneManager.LoadSceneAsync("Assets/_BROTE/Scenes/UI/MainMenu.unity");yield return null;
        T("BroteSaveSystem").GetField("editorTestSavePath",Flags).SetValue(null,null);
        if(File.Exists(path))File.Delete(path);if(File.Exists(path+".tmp"))File.Delete(path+".tmp");Time.timeScale=1;
    }
    [UnityTest] public IEnumerator FreshGameHasOneOfficialHUDAndCanonicalCounters()
    {
        yield return Load();var root=HUD();
        Assert.AreEqual(1,root.Query<VisualElement>("gameplay-hud-panel").ToList().Count);
        Assert.AreEqual("GameplayHUD_MainPanel",root.Q("gameplay-hud-panel").style.backgroundImage.value.sprite.name);
        Assert.AreEqual("OBJETIVO 1/6",root.Q<Label>("objective-title").text);
        Assert.AreEqual("Produce tu primera plántula",root.Q<Label>("objective-description").text);
        Assert.AreEqual("Progreso: 0 / 1",root.Q<Label>("objective-progress").text);
        Assert.AreEqual("0 / 5",root.Q<Label>("hud-seedlings").text);Assert.AreEqual("0",root.Q<Label>("hud-minka").text);
        Assert.AreEqual("0%",root.Q<Label>("restoration-percentage").text);
        Assert.AreEqual(0,root.Q("ProgressClip").style.width.value.value);
        Assert.AreSame(root.Q<Label>("objective-description"),Find("BroteTutorialState").GetType().GetField("guide",Flags).GetValue(Find("BroteTutorialState")));
        foreach(var name in new[]{"minka","seedling","restoration","objective"})
            Assert.IsTrue(root.Q(name+"-icon").style.backgroundImage.value.sprite.name.StartsWith("GameplayHUD_Icon_"));
    }
    [UnityTest] public IEnumerator HealthClipMatchesExactManagerValueAtAllMilestones()
    {
        yield return Load();var root=HUD();var track=root.Q("RestorationProgressRoot");var clip=root.Q("ProgressClip");var fill=root.Q("GameplayHUD_ProgressFill");
        foreach(float value in new[]{0f,10f,25f,50f,75f,100f})
        {
            Call(Find("RestorationManager"),"RestoreSavedPoints",value);
            Assert.AreEqual(value,clip.style.width.value.value);Assert.AreEqual(value.ToString("0.#")+"%",root.Q<Label>("restoration-percentage").text);
            yield return null;yield return null;
            Assert.AreEqual(track.resolvedStyle.width*value/100,clip.resolvedStyle.width,2.1f);
            Assert.AreEqual(track.resolvedStyle.width,fill.resolvedStyle.width,.2f);
            Assert.AreEqual(track.worldBound.xMin,fill.worldBound.xMin,.1f);
            Assert.AreEqual(Overflow.Hidden,clip.style.overflow.value);
        }
    }
    [UnityTest] public IEnumerator RealProductionPlantPurchaseAndLoadUpdateHUDWithoutChangingRewards()
    {
        yield return Load();var session=Find("BroteIncrementalSession");
        for(int i=0;i<10;i++)Call(session,"Click");yield return null;
        Assert.AreEqual("1 / 5",HUD().Q<Label>("hud-seedlings").text);
        Assert.AreEqual("OBJETIVO 2/6",HUD().Q<Label>("objective-title").text);Assert.AreEqual("1",HUD().Q<Label>("hud-minka").text);
        var spot=UnityEngine.Object.FindObjectsByType(T("PlantingSpot"),FindObjectsSortMode.None).Cast<Component>().Single(x=>(string)Get(x,"StableId")=="01");
        Call(spot,"Interact",Find("PlayerInteraction"));yield return null;
        Assert.AreEqual("0 / 5",HUD().Q<Label>("hud-seedlings").text);Assert.AreEqual("OBJETIVO 3/6",HUD().Q<Label>("objective-title").text);
        Assert.AreEqual("2",HUD().Q<Label>("hud-minka").text);
        Assert.IsTrue((bool)Call(Find("BroteUpgradeManager"),"TryBuy",Kind(2)));yield return null;
        Assert.AreEqual("0 / 8",HUD().Q<Label>("hud-seedlings").text);Assert.AreEqual("0",HUD().Q<Label>("hud-minka").text);
        Call(Find("BroteMinkaManager"),"Restore",12,.63d);yield return null;
        Assert.AreEqual("12",HUD().Q<Label>("hud-minka").text); // Spendable integer, fraction untouched.
        // Save v2 requires health to match developed trees; reconstruct a coherent fixture.
        Call(spot,"RestoreSavedTree",true,2);spot.GetType().GetProperty("MinkaRewarded",Flags).SetValue(spot,true);
        Call(Find("RestorationManager"),"RestoreSavedPoints",10f);Assert.IsTrue((bool)Call(Find("BroteSaveSystem"),"Save"));
        yield return Load();
        Assert.AreEqual("0 / 8",HUD().Q<Label>("hud-seedlings").text);Assert.AreEqual("12",HUD().Q<Label>("hud-minka").text);
        Assert.AreEqual("10%",HUD().Q<Label>("restoration-percentage").text);Assert.AreEqual(10,HUD().Q("ProgressClip").style.width.value.value);
        Assert.AreEqual("OBJETIVO 3/6",HUD().Q<Label>("objective-title").text);
        for(int claimed=0;claimed<=6;claimed++)
        {
            Call(Find("BroteRestorationObjectives"),"Restore",(1<<claimed)-1,claimed);yield return null;
            Assert.AreEqual("OBJETIVO "+Math.Min(claimed+1,6)+"/6",HUD().Q<Label>("objective-title").text);
        }
    }
    [UnityTest] public IEnumerator ResponsiveHUDAndBuyTextFitFiveRequestedSizesAndModalDoesNotOverlap()
    {
        yield return Load();var hud=Find("BroteHUD");var root=HUD();
        Call(Find("NurserySeedlingSource"),"Interact",Find("PlayerInteraction"));yield return null;
        var panel=Find("NurseryManagementPanel");var nursery=panel.GetComponent<UIDocument>().rootVisualElement;
        Call(panel,"ToggleView");
        Assert.AreEqual(DisplayStyle.None,root.Q("gameplay-hud-panel").style.display.value);
        foreach(var size in new[]{new Vector2(1920,1080),new Vector2(1280,720),new Vector2(1440,900),new Vector2(1024,768),new Vector2(900,900)})
        {
            foreach(var r in new[]{root,nursery}){r.style.right=r.style.bottom=StyleKeyword.Auto;r.style.width=size.x;r.style.height=size.y;}
            Call(hud,"AdaptHUDLayout");Call(panel,"AdaptLayout");yield return null;yield return null;
            var scroll=nursery.Q<ScrollView>("upgrade-list");Assert.Greater(scroll.contentViewport.resolvedStyle.height,0);
            foreach(var card in scroll.contentContainer.Children())
            {
                var button=card.Q<Button>("upgrade-buy");Assert.IsNotNull(button);
                foreach(var text in new[]{"Mejorar","COMPRAR","SIN MINKA","MÁXIMO","Desbloquear"})
                {
                    var measured=button.MeasureTextSize(text,0,VisualElement.MeasureMode.Undefined,0,VisualElement.MeasureMode.Undefined);
                    Assert.LessOrEqual(measured.x,button.contentRect.width+.5f,text+" width at "+size);
                    Assert.LessOrEqual(measured.y,button.contentRect.height+.5f,text+" height at "+size);
                }
                Assert.AreEqual(BackgroundSizeType.Contain,button.style.backgroundSize.value.sizeType);
            }
            Call(panel,"Close");yield return null;yield return null;
            var frame=root.Q("gameplay-hud-panel");Assert.AreEqual(DisplayStyle.Flex,frame.style.display.value);
            Assert.GreaterOrEqual(frame.worldBound.yMin,root.worldBound.yMin);
            Assert.LessOrEqual(frame.resolvedStyle.height,171);
            Assert.AreEqual(12,frame.resolvedStyle.top+frame.resolvedStyle.height*(23f/343),1.1f);
            Assert.Less(frame.worldBound.xMax,root.worldBound.xMax);
            Assert.AreEqual(1657f/343,frame.resolvedStyle.width/frame.resolvedStyle.height,.02f);
            foreach(var label in frame.Query<Label>().ToList())
            {
                Assert.GreaterOrEqual(label.worldBound.xMin,frame.worldBound.xMin);
                Assert.LessOrEqual(label.worldBound.xMax,frame.worldBound.xMax);
                var measured=label.MeasureTextSize(label.text,label.contentRect.width,VisualElement.MeasureMode.AtMost,0,VisualElement.MeasureMode.Undefined);
                Assert.LessOrEqual(measured.y,label.contentRect.height+.5f,label.name+" at "+size);
            }
            Call(Find("NurserySeedlingSource"),"Interact",Find("PlayerInteraction"));Call(panel,"ToggleView");yield return null;
        }
        Call(Find("BroteMinkaManager"),"Add",50);yield return null;var buy=nursery.Q("upgrade-card-0").Q<Button>("upgrade-buy");
        Assert.AreEqual("Mejorar",buy.text);Assert.IsTrue(buy.enabledSelf);
        using(var down=PointerDownEvent.GetPooled(new Event{type=EventType.MouseDown,button=0,mousePosition=buy.worldBound.center})){down.target=buy;buy.SendEvent(down);}
        Assert.AreEqual(.9f,buy.style.unityBackgroundImageTintColor.value.r,.001f);
        using(var leave=PointerLeaveEvent.GetPooled()){leave.target=buy;buy.SendEvent(leave);}
        Assert.AreEqual(Color.white,buy.style.unityBackgroundImageTintColor.value);
        using(var submit=NavigationSubmitEvent.GetPooled()){submit.target=buy;buy.SendEvent(submit);}
        Assert.AreEqual(1,Call(Find("BroteUpgradeManager"),"Level",Kind(0)));
        for(int i=0;i<2;i++)Call(Find("BroteUpgradeManager"),"TryBuy",Kind(0));Call(panel,"Refresh");
        Assert.IsFalse(buy.enabledSelf);Assert.AreEqual("MÁXIMO",buy.text);
    }
}

