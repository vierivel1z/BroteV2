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

public sealed class BroteMainMenuArtTests
{
    private const BindingFlags Flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
    private const string Menu="Assets/_BROTE/Scenes/UI/MainMenu.unity";
    private const string Game="Assets/_BROTE/Scenes/Acopalca/Acopalca.unity";
    private string path;
    private static Type T(string name)=>Type.GetType(name+", Assembly-CSharp",true);
    private static object Call(object owner,string name,params object[] args)=>owner.GetType().GetMethod(name,Flags).Invoke(owner,args);
    private static object Get(object owner,string name)=>owner.GetType().GetProperty(name,Flags).GetValue(owner);
    private static Component Find(string name)=>UnityEngine.Object.FindObjectsByType(T(name),FindObjectsSortMode.None).Cast<Component>().Single();
    private static IEnumerator Load(string scene){yield return SceneManager.LoadSceneAsync(scene);yield return null;yield return null;}
    [SetUp] public void Setup()
    {
        path=Path.Combine(Path.GetTempPath(),"BROTE-MenuArt-"+Guid.NewGuid().ToString("N")+".json");
        T("BroteSaveSystem").GetField("editorTestSavePath",Flags).SetValue(null,path);Time.timeScale=1;
    }
    [UnityTearDown] public IEnumerator Cleanup()
    {
        yield return Load(Menu);
        T("BroteSaveSystem").GetField("editorTestSavePath",Flags).SetValue(null,null);
        if(File.Exists(path))File.Delete(path);if(File.Exists(path+".tmp"))File.Delete(path+".tmp");
    }
    [UnityTest] public IEnumerator OfficialMenuLoadsAndContinueRejectsAbsentOrInvalidSave()
    {
        yield return Load(Menu);var menu=Find("BroteMainMenu");var root=menu.GetComponent<UIDocument>().rootVisualElement;
        CollectionAssert.AreEqual(new[]{"NUEVA PARTIDA","CONTINUAR","OPCIONES","SALIR"},root.Query<Button>().ToList().Select(x=>x.text).ToArray());
        Assert.IsNotNull(root.Q("MainMenuBackground").style.backgroundImage.value.sprite);
        Assert.AreEqual(BackgroundSizeType.Cover,root.Q("MainMenuBackground").style.backgroundSize.value.sizeType);
        var button=root.Query<Button>().ToList().Single(x=>x.text=="CONTINUAR");Assert.IsFalse(button.enabledInHierarchy);
        Assert.AreEqual("MainMenu_Button_Disabled",((Sprite)Get(button,"DisplayedSprite")).name);
        File.WriteAllText(path,"invalid JSON");Call(menu,"Home");
        Assert.IsFalse(root.Query<Button>().ToList().Single(x=>x.text=="CONTINUAR").enabledInHierarchy);
        Call(menu,"Continue");yield return null;Assert.AreEqual("MainMenu",SceneManager.GetActiveScene().name);
        Call(menu,"Options");Assert.AreEqual(2,root.Query<Slider>().ToList().Count);
        Assert.AreEqual(DisplayStyle.Flex,root.Q("MainMenuDialog").style.display.value);
        Assert.AreEqual("MainMenu_Panel_BG",root.Q("MainMenuDialog").style.backgroundImage.value.sprite.name);
        Call(menu,"Home");Assert.AreEqual(DisplayStyle.None,root.Q("MainMenuDialog").style.display.value);
    }
    [UnityTest] public IEnumerator ContinueRestoresV2AndMenuRoundTripDoesNotDuplicateFXOrDocuments()
    {
        yield return Load(Game);Call(Find("SeedlingInventory"),"AddSeedlings",2);Assert.IsTrue((bool)Call(Find("BroteSaveSystem"),"Save"));
        yield return Load(Menu);
        for(int i=0;i<2;i++)
        {
            var menu=Find("BroteMainMenu");var root=menu.GetComponent<UIDocument>().rootVisualElement;
            Assert.IsTrue(root.Query<Button>().ToList().Single(x=>x.text=="CONTINUAR").enabledInHierarchy);
            Assert.AreEqual(1,root.Query<VisualElement>("MainMenuAmbientFX").ToList().Count);
            Assert.AreEqual(8,root.Q("MainMenuAmbientFX").childCount);
            Assert.AreEqual(1,UnityEngine.Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None).Count(x=>x.gameObject.scene.name=="MainMenu"));
            var elements=root.Q("MainMenuAmbientFX").Children().ToArray();
            var ambient=menu.GetType().GetField("ambient",Flags).GetValue(menu);
            for(int frame=0;frame<500;frame++)Call(ambient,"Tick",.5f,1280f,720f);
            CollectionAssert.AreEqual(elements,root.Q("MainMenuAmbientFX").Children().ToArray());
            Assert.AreEqual(0,UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Count(x=>x.gameObject.scene.name=="MainMenu"));
            Call(menu,"Continue");for(int n=0;n<120&&SceneManager.GetActiveScene().name!="Acopalca";n++)yield return null;
            yield return null;yield return null;Assert.AreEqual(2,Get(Find("SeedlingInventory"),"CurrentSeedlings"));
            yield return Load(Menu);
        }
        var finalMenu=Find("BroteMainMenu");Call(finalMenu,"ConfirmNew");
        var doc=finalMenu.GetComponent<UIDocument>();Assert.IsTrue(doc.rootVisualElement.Query<Button>().ToList().Any(x=>x.text=="CONFIRMAR"));
        Call(finalMenu,"Home");Assert.IsTrue(File.Exists(path)); // Cancel never deletes progress.
    }
    [UnityTest] public IEnumerator ButtonPointerStatesUseOfficialSpritesAndResetAfterLeaving()
    {
        yield return Load(Menu);var root=Find("BroteMainMenu").GetComponent<UIDocument>().rootVisualElement;
        var skin=Resources.Load<ScriptableObject>("UI/MainMenuUISkin");
        var button=(Button)Activator.CreateInstance(T("MainMenuArtButton"),new object[]{"TEST",new Action(()=>{}),skin,false});
        root.Add(button);yield return null;
        Assert.AreEqual("MainMenu_Button_Normal",((Sprite)Get(button,"DisplayedSprite")).name);
        using(var e=PointerEnterEvent.GetPooled()){e.target=button;button.SendEvent(e);}
        Assert.AreEqual("MainMenu_Button_Hover",((Sprite)Get(button,"DisplayedSprite")).name);
        using(var e=PointerDownEvent.GetPooled(new Event{type=EventType.MouseDown,button=0,mousePosition=button.worldBound.center})){e.target=button;button.SendEvent(e);}
        Assert.AreEqual("MainMenu_Button_Pressed",((Sprite)Get(button,"DisplayedSprite")).name);
        using(var e=PointerUpEvent.GetPooled(new Event{type=EventType.MouseUp,button=0,mousePosition=button.worldBound.center})){e.target=button;button.SendEvent(e);}
        Assert.AreEqual("MainMenu_Button_Hover",((Sprite)Get(button,"DisplayedSprite")).name);
        using(var e=PointerLeaveEvent.GetPooled()){e.target=button;button.SendEvent(e);}
        Assert.AreEqual("MainMenu_Button_Normal",((Sprite)Get(button,"DisplayedSprite")).name);
        button.SetEnabled(false);Call(button,"RefreshState");
        using(var e=PointerEnterEvent.GetPooled()){e.target=button;button.SendEvent(e);}
        Assert.AreEqual("MainMenu_Button_Disabled",((Sprite)Get(button,"DisplayedSprite")).name);
    }
    [UnityTest] public IEnumerator ResponsiveLayoutKeepsLogoAndButtonsInsideViewport()
    {
        yield return Load(Menu);var menu=Find("BroteMainMenu");var root=menu.GetComponent<UIDocument>().rootVisualElement;
        root.style.right=root.style.bottom=StyleKeyword.Auto;
        foreach(var size in new[]{new Vector2(1920,1080),new Vector2(1280,720),new Vector2(1440,900),new Vector2(1024,768),new Vector2(900,900)})
        {
            root.style.width=size.x;root.style.height=size.y;Call(menu,"Layout",size.x,size.y);
            yield return null;yield return null;
            var viewport=root.worldBound;
            foreach(var item in root.Q("MainMenuHome").Children())
            {
                Assert.GreaterOrEqual(item.worldBound.xMin,viewport.xMin-.1f);
                Assert.GreaterOrEqual(item.worldBound.yMin,viewport.yMin-.1f);
                Assert.LessOrEqual(item.worldBound.xMax,viewport.xMax+.1f);
                Assert.LessOrEqual(item.worldBound.yMax,viewport.yMax+.1f);
            }
            Assert.AreEqual(viewport.width,root.Q("MainMenuBackground").worldBound.width,.1f);
            Assert.AreEqual(viewport.height,root.Q("MainMenuBackground").worldBound.height,.1f);
        }
    }
}

