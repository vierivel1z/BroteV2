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

public sealed class BroteWave4IntegrationTests
{
    private const BindingFlags Flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance;
    private string path;
    private static Type T(string name)=>Type.GetType(name+", Assembly-CSharp",true);
    private static object Call(object o,string n,params object[] args)=>o.GetType().GetMethod(n,Flags).Invoke(o,args);
    private static object Get(object o,string n)=>o.GetType().GetProperty(n,Flags).GetValue(o);
    private static Component Find(string n)=>UnityEngine.Object.FindObjectsByType(T(n)).Cast<Component>().Single();
    private static Component[] Spots()=>UnityEngine.Object.FindObjectsByType(T("PlantingSpot")).Cast<Component>().OrderBy(x=>(string)Get(x,"StableId")).ToArray();
    private static object Kind(int n)=>Enum.ToObject(T("BroteUpgrade"),n);
    [SetUp] public void Setup()
    {path=Path.Combine(Path.GetTempPath(),"BROTE-Wave4-"+Guid.NewGuid().ToString("N")+".json");T("BroteSaveSystem").GetField("editorTestSavePath",Flags).SetValue(null,path);Time.timeScale=1;}
    [UnityTearDown] public IEnumerator Cleanup()
    {
        Time.timeScale=1;yield return SceneManager.LoadSceneAsync("Assets/_BROTE/Scenes/UI/MainMenu.unity");yield return null;
        T("BroteSaveSystem").GetField("editorTestSavePath",Flags).SetValue(null,null);
        foreach(string suffix in new[]{"",".tmp",".v1.backup"})if(File.Exists(path+suffix))File.Delete(path+suffix);
    }
    private static IEnumerator Load()
    {yield return SceneManager.LoadSceneAsync("Assets/_BROTE/Scenes/Acopalca/Acopalca.unity");yield return null;yield return null;}


    [UnityTest] public IEnumerator NurseryContentIsClippedAndHeaderNeverScrolls()
    {
        yield return Load();T("NurseryManagementPanel").GetMethod("OpenFor",Flags).Invoke(null,new object[]{Find("PlayerInteraction")});
        var panel=Find("NurseryManagementPanel");var root=panel.GetComponent<UIDocument>().rootVisualElement;
        root.style.width=1280;root.style.height=720;root.style.right=root.style.bottom=StyleKeyword.Auto;
        yield return null;yield return null;yield return null;
        var viewport=root.Q("nursery-content-viewport");var heading=root.Q("nursery-heading");var tabs=root.Q("nursery-tabs");
        var production=root.Q<ScrollView>("production-view");var upgrades=root.Q<ScrollView>("upgrade-list");
        Assert.AreEqual(Overflow.Hidden,viewport.style.overflow.value);
        Assert.AreSame(viewport.parent,heading.parent);Assert.AreSame(heading.parent,tabs.parent);
        Assert.AreSame(heading,root.Q("nursery-close").parent);
        Assert.AreEqual(DisplayStyle.Flex,production.resolvedStyle.display);Assert.AreEqual(DisplayStyle.None,root.Q("upgrade-view").resolvedStyle.display);
        Assert.LessOrEqual(production.verticalScroller.highValue,.5f,"Production must fit at 1280x720");
        Call(panel,"HandleNavigation",true,false);yield return null;yield return null;yield return null;
        var headerBounds=heading.worldBound;var tabsBounds=tabs.worldBound;
        Assert.GreaterOrEqual(viewport.worldBound.yMin,tabsBounds.yMax-.5f);
        foreach(var scroll in new[]{production,upgrades})
        {Assert.AreEqual(ScrollView.TouchScrollBehavior.Clamped,scroll.touchScrollBehavior);Assert.AreEqual(Overflow.Hidden,scroll.contentViewport.style.overflow.value);}
        Assert.Greater(upgrades.verticalScroller.highValue,0);Assert.Greater(upgrades.contentViewport.worldBound.height,0);
        foreach(float delta in new[]{-10000f,-10000f,10000f,10000f})
        {
            using(var evt=WheelEvent.GetPooled(new Event{type=EventType.ScrollWheel,mousePosition=upgrades.contentViewport.worldBound.center,delta=new Vector2(0,delta)})){evt.target=upgrades.contentViewport;upgrades.contentViewport.SendEvent(evt);}
            yield return null;
            Assert.That(upgrades.scrollOffset.y,Is.InRange(0f,Mathf.Max(0,upgrades.verticalScroller.highValue)));
            Assert.AreEqual(delta<0?0:upgrades.verticalScroller.highValue,upgrades.scrollOffset.y,.5f);
            Assert.AreEqual(headerBounds,heading.worldBound);Assert.AreEqual(tabsBounds,tabs.worldBound);
        }
        Call(panel,"HandleNavigation",false,true);Assert.IsFalse((bool)T("NurseryManagementPanel").GetProperty("IsOpen",Flags).GetValue(null));
    }

    private static void AssertCleanInitialState()
    {
        Assert.AreEqual(0,Get(Find("SeedlingInventory"),"CurrentSeedlings"));Assert.AreEqual(5,Get(Find("SeedlingInventory"),"MaxSeedlings"));
        Assert.AreEqual(0,Get(Find("BroteMinkaManager"),"Minka"));Assert.AreEqual(0d,Get(Find("BroteMinkaManager"),"Fraction"));Assert.AreEqual(0d,Get(Find("NurseryProductionManager"),"Progress"));
        Assert.AreEqual(0,UnityEngine.Object.FindObjectsByType(T("QuenualGrowth")).Length);Assert.AreEqual(0f,Get(Find("RestorationManager"),"CurrentRestoration"));
        var objectives=Find("BroteRestorationObjectives");Assert.AreEqual(0,Get(objectives,"Observed"));Assert.AreEqual(0,Get(objectives,"Claimed"));
        StringAssert.Contains("Produce tu primera plántula",(string)Get(objectives,"CurrentText"));
        foreach(var spot in Spots())
        {
            string id=(string)Get(spot,"StableId");Assert.IsNull(Get(spot,"PlantedTree"));
            Assert.AreEqual(id=="01" || id=="06"?"PREPARED":id=="09" || id=="10"?"AVAILABLE":"LOCKED",Get(spot,"State").ToString());
        }
    }
    [UnityTest] public IEnumerator NewGameAfterContinue_ClearsEverything()
    {
        yield return Load();AssertCleanInitialState();
        var data=Call(Find("BroteSaveSystem"),"Capture");var dt=data.GetType();
        dt.GetField("inventory").SetValue(data,5);dt.GetField("restoration").SetValue(data,30f);dt.GetField("minka").SetValue(data,7);
        dt.GetField("nurseryProgress").SetValue(data,37.5d);dt.GetField("objectivesObserved").SetValue(data,7);dt.GetField("objectivesClaimed").SetValue(data,3);
        foreach(var record in (Array)dt.GetField("spots").GetValue(data))
        {
            var rt=record.GetType();string id=(string)rt.GetField("id").GetValue(record);
            if(id=="01" || id=="06" || id=="10")
            {rt.GetField("planted").SetValue(record,true);rt.GetField("stage").SetValue(record,2);rt.GetField("state").SetValue(record,1);rt.GetField("minkaRewarded").SetValue(record,true);}
        }
        Assert.IsTrue((bool)Call(data,"IsValid"));
        yield return SceneManager.LoadSceneAsync("Assets/_BROTE/Scenes/UI/MainMenu.unity");yield return null;
        File.WriteAllText(path,JsonUtility.ToJson(data));
        int audioRequests=0;Action<AudioClip,int,bool> observer=(clip,category,growth)=>audioRequests++;
        var audioEvent=T("BroteAudioFeedback").GetEvent("PlaybackRequested");audioEvent.AddEventHandler(null,observer);
        try
        {
            Call(Find("BroteMainMenu"),"Play");
            for(int i=0;i<120 && SceneManager.GetActiveScene().name!="Acopalca";i++)yield return null;
            yield return null;yield return null;
            Assert.AreEqual(5,Get(Find("SeedlingInventory"),"CurrentSeedlings"));Assert.AreEqual(3,Spots().Count(x=>Get(x,"PlantedTree")!=null));
            foreach(var spot in Spots().Where(x=>Get(x,"PlantedTree")!=null))Assert.AreEqual(2,Get(Get(spot,"PlantedTree"),"Stage"));
            Assert.AreEqual(30f,Get(Find("RestorationManager"),"CurrentRestoration"));Assert.AreEqual(7,Get(Find("BroteMinkaManager"),"Minka"));
            Assert.AreEqual(37.5d,Get(Find("NurseryProductionManager"),"Progress"));Assert.AreEqual(3,Get(Find("BroteRestorationObjectives"),"Claimed"));
            yield return null;Assert.AreEqual(0,audioRequests,"Load must not request gameplay SFX");
            foreach(var spot in Spots().Where(x=>Get(x,"PlantedTree")!=null))
            {
                var tree=(Component)Get(spot,"PlantedTree");var feedback=tree.GetComponent(T("QuenualGrowthFeedback"));
                Assert.IsNull(feedback.GetType().GetField("animation",Flags).GetValue(feedback));
                Assert.IsNull(feedback.GetType().GetField("pooledEffect",Flags).GetValue(feedback));
            }
            Assert.IsTrue((bool)Call(Find("BroteSaveSystem"),"Save"));
            yield return SceneManager.LoadSceneAsync("Assets/_BROTE/Scenes/UI/MainMenu.unity");yield return null;
            var menu=Find("BroteMainMenu");Call(menu,"ConfirmNew");
            var button=menu.GetComponent<UIDocument>().rootVisualElement.Query<Button>().ToList().Single(x=>x.text=="CONFIRMAR");
            Call(button.clickable,"Invoke",new object[]{null});
            for(int i=0;i<120 && SceneManager.GetActiveScene().name!="Acopalca";i++)yield return null;
            yield return null;yield return null;AssertCleanInitialState();
            var clean=Call(Find("BroteSaveSystem"),"Capture");Assert.AreEqual(0,clean.GetType().GetField("inventory").GetValue(clean));
            Assert.AreEqual(0,audioRequests);Assert.IsTrue((bool)Call(Find("BroteSaveSystem"),"Save"));yield return Load();AssertCleanInitialState();
        }
        finally{audioEvent.RemoveEventHandler(null,observer);}
        for(int i=0;i<10;i++)Call(Find("BroteIncrementalSession"),"Click");
        Assert.AreEqual(1,Get(Find("SeedlingInventory"),"CurrentSeedlings"));Assert.AreEqual(1,Get(Find("BroteRestorationObjectives"),"Claimed"));
    }

    [UnityTest] public IEnumerator OfficialAmbienceUsesExistingZonesLayerGainsMasterFocusAndPause()
    {
        yield return Load();var zones=UnityEngine.Object.FindObjectsByType(T("AmbientZone")).Cast<Component>().ToArray();Assert.AreEqual(2,zones.Length);
        var config=Resources.Load<ScriptableObject>("Audio/BroteAmbienceClips");Assert.IsNotNull(config);
        var global=zones.Single(x=>x.gameObject.name.StartsWith("Sector01Ambience"));var river=zones.Single(x=>x.gameObject.name.StartsWith("DistantRiverAudio"));
        Assert.AreEqual(new Vector3(-18,8,0),river.transform.localPosition);
        var globals=global.GetComponentsInChildren<AudioSource>();var localized=river.GetComponentsInChildren<AudioSource>();
        Assert.AreEqual(3,globals.Length);Assert.AreEqual(1,localized.Length);
        foreach(var source in globals){Assert.AreEqual(0,source.spatialBlend);Assert.IsTrue(source.loop);Assert.IsFalse(source.playOnAwake);}
        Assert.AreEqual(.85f,localized[0].spatialBlend);Assert.IsTrue(localized[0].loop);Assert.AreEqual(35,localized[0].maxDistance);
        Assert.AreSame(config.GetType().GetField("river").GetValue(config),localized[0].clip);
        foreach(string field in new[]{"wind","birds","leaves"})Assert.IsTrue(globals.Any(x=>x.clip==(AudioClip)config.GetType().GetField(field).GetValue(config)));
        var manager=Find("BroteAudioManager");var mt=T("BroteAudioManager");float previous=(float)mt.GetProperty("MasterVolume").GetValue(null);
        try
        {
            Call(manager,"OnApplicationFocus",true);mt.GetMethod("SetMasterVolume").Invoke(null,new object[]{1f});
            Call(global,"Update");Call(river,"Update");
            Assert.AreEqual(.22f,globals.Single(x=>x.clip.name=="Ambience_Wind").volume,.001f);
            Assert.AreEqual(.11f,globals.Single(x=>x.clip.name=="Ambience_Birds").volume,.001f);
            Assert.AreEqual(.077f,globals.Single(x=>x.clip.name=="Ambience_GrassRustle").volume,.001f);
            mt.GetMethod("SetMasterVolume").Invoke(null,new object[]{.5f});Call(global,"Update");Assert.AreEqual(.11f,globals.Single(x=>x.clip.name=="Ambience_Wind").volume,.001f);
            Call(manager,"OnApplicationFocus",false);Call(global,"Update");Assert.IsTrue(globals.All(x=>x.volume==0));
            Call(manager,"OnApplicationFocus",true);Time.timeScale=0;Call(global,"Update");Assert.IsTrue((bool)global.GetType().GetField("paused",Flags).GetValue(global));
            Time.timeScale=1;Call(global,"Update");Assert.IsFalse((bool)global.GetType().GetField("paused",Flags).GetValue(global));
            T("AmbientZone").GetMethod("AttachSector").Invoke(null,new object[]{Find("BroteHUD")});Assert.AreEqual(2,UnityEngine.Object.FindObjectsByType(T("AmbientZone")).Length);
        }
        finally{Time.timeScale=1;Call(manager,"OnApplicationFocus",true);mt.GetMethod("SetMasterVolume").Invoke(null,new object[]{previous});}
    }

    [UnityTest] public IEnumerator FreshNewGame_HasCanonicalInitialState()
    {
        yield return Load();AssertCleanInitialState();
        var session=Find("BroteIncrementalSession");var layout=session.GetType().GetField("layout",Flags).GetValue(session);
        var factory=T("BroteSaveData").GetMethod("CreateNewGameData").Invoke(null,new[]{layout});
        Assert.AreEqual(2,factory.GetType().GetField("saveVersion").GetValue(factory));Assert.IsTrue((bool)Call(factory,"IsValid"));
    }
    [UnityTest] public IEnumerator FreshNewGame_DoesNotContainRuntimeQuenuals()
    {yield return Load();Assert.AreEqual(0,UnityEngine.Object.FindObjectsByType(T("QuenualGrowth")).Length);Assert.IsTrue(Spots().All(x=>Get(x,"PlantedTree")==null));}
    [UnityTest] public IEnumerator FreshNewGame_FirstObjectiveIsProduceSeedling()
    {yield return Load();StringAssert.Contains("OBJETIVO 1/6",(string)Get(Find("BroteRestorationObjectives"),"CurrentText"));StringAssert.Contains("Produce tu primera plántula",(string)Get(Find("BroteRestorationObjectives"),"CurrentText"));}
    [UnityTest] public IEnumerator FreshNewGame_DoesNotCompleteObjectivesDuringBootstrap()
    {yield return Load();for(int i=0;i<5;i++)yield return null;AssertCleanInitialState();}
    [UnityTest] public IEnumerator Continue_RestoresExistingProgress()
    {
        yield return Load();var spots=Spots();
        foreach(string id in new[]{"01","06","10"})
        {
            var spot=spots.Single(x=>(string)Get(x,"StableId")==id);Call(spot,"RestoreSavedTree",true,2);
            spot.GetType().GetProperty("MinkaRewarded").SetValue(spot,true);
        }
        Call(Find("RestorationManager"),"RestoreSavedPoints",30f);Call(Find("SeedlingInventory"),"RestoreSavedCount",5);
        Call(Find("BroteMinkaManager"),"Restore",9,.25d);Call(Find("BroteRestorationObjectives"),"Restore",7,3);
        Assert.IsTrue((bool)Call(Find("BroteSaveSystem"),"Save"));yield return Load();
        Assert.AreEqual(5,Get(Find("SeedlingInventory"),"CurrentSeedlings"));Assert.AreEqual(3,UnityEngine.Object.FindObjectsByType(T("QuenualGrowth")).Length);
        Assert.AreEqual(9,Get(Find("BroteMinkaManager"),"Minka"));Assert.AreEqual(3,Get(Find("BroteRestorationObjectives"),"Claimed"));
        Assert.AreEqual(30f,Get(Find("RestorationManager"),"CurrentRestoration"));
    }
    [UnityTest] public IEnumerator ConfirmedNewGameIgnoresStaleSaveRecreatedDuringTransition()
    {
        yield return Load();Call(Find("SeedlingInventory"),"RestoreSavedCount",5);Assert.IsTrue((bool)Call(Find("BroteSaveSystem"),"Save"));
        string previous=File.ReadAllText(path);Assert.IsTrue((bool)T("BroteSaveSystem").GetMethod("DeleteConfirmedSave").Invoke(null,null));
        File.WriteAllText(path,previous);yield return Load();AssertCleanInitialState();
        yield return null;var saved=JsonUtility.FromJson(File.ReadAllText(path),T("BroteSaveData"));Assert.AreEqual(0,saved.GetType().GetField("inventory").GetValue(saved));
    }

    [UnityTest] public IEnumerator PlantedTreeIgnoresNonUniformSpotScaleAndLoadKeepsOnlyTreeVisible()
    {
        yield return Load();var spot=Spots().Single(x=>(string)Get(x,"StableId")=="01");
        var prefab=(Component)spot.GetType().GetField("plantPrefab",Flags).GetValue(spot);
        var beforeScale=spot.transform.localScale;
        spot.transform.localScale=new Vector3(2,.5f,1); // Synthetic stress case only; never saved to the scene.
        Call(Find("SeedlingInventory"),"AddSeedlings",1);Call(spot,"Interact",Find("PlayerInteraction"));yield return null;
        var tree=(Component)Get(spot,"PlantedTree");Assert.IsNotNull(tree);
        Assert.AreEqual(prefab.transform.localScale,tree.transform.localScale);Assert.AreEqual(Vector3.one,tree.transform.lossyScale);
        Assert.AreEqual(Vector3.zero,tree.transform.localPosition);Assert.AreEqual(Quaternion.identity,tree.transform.localRotation);
        Assert.AreEqual(spot.transform.position,tree.transform.position);Assert.IsFalse(tree.transform.IsChildOf(spot.transform));
        Assert.IsFalse(SpotVisual(spot).enabled);Assert.IsFalse(spot.GetComponent<SpriteRenderer>().enabled);
        Assert.AreEqual(1,tree.GetComponentsInChildren<SpriteRenderer>().Count(x=>x.enabled));
        yield return new WaitForSeconds(.5f);Assert.AreEqual(prefab.transform.localScale,tree.transform.localScale);
        spot.transform.localScale=beforeScale;
        Assert.IsTrue((bool)Call(Find("BroteSaveSystem"),"Save"));yield return Load();
        spot=Spots().Single(x=>(string)Get(x,"StableId")=="01");tree=(Component)Get(spot,"PlantedTree");
        Assert.IsNotNull(tree);Assert.AreEqual(Vector3.one,tree.transform.localScale);Assert.AreEqual(Vector3.one,tree.transform.lossyScale);
        Assert.IsFalse(SpotVisual(spot).enabled);Assert.IsFalse(spot.GetComponent<SpriteRenderer>().enabled);
        var prepared=Spots().Single(x=>(string)Get(x,"StableId")=="06");Assert.IsNull(Get(prepared,"PlantedTree"));
        Assert.IsTrue(SpotVisual(prepared).enabled);Assert.AreSame(OfficialSpot("prepared"),SpotVisual(prepared).sprite);
        var holder=tree.transform.parent.gameObject;UnityEngine.Object.Destroy(spot.gameObject);yield return null;yield return null;Assert.IsTrue(holder==null);
    }

    private static SpriteRenderer SpotVisual(Component spot)=>spot.GetComponentsInChildren<SpriteRenderer>().Single(x=>x.gameObject.name=="ProgressionSpotVisual");
    private static Sprite OfficialSpot(string name)
    {
        var art=Resources.Load<ScriptableObject>("Art/PlantingSpotArt");
        return (Sprite)art.GetType().GetField(name).GetValue(art);
    }
    [UnityTest] public IEnumerator OfficialSpotSpritesFollowPreparationExpansionAndSaveWithoutMovingPhysics()
    {
        yield return Load();var spots=Spots();Assert.AreEqual(10,spots.Length);
        var positions=spots.Select(x=>x.transform.position).ToArray();var scales=spots.Select(x=>x.transform.localScale).ToArray();
        var colliders=spots.Select(x=>x.GetComponent<CircleCollider2D>()).ToArray();
        var radii=colliders.Select(x=>x.radius).ToArray();var offsets=colliders.Select(x=>x.offset).ToArray();
        var ids=spots.Select(x=>(string)Get(x,"StableId")).ToArray();
        for(int i=0;i<spots.Length;i++)
        {
            var visual=SpotVisual(spots[i]);string field=Get(spots[i],"State").ToString().ToLowerInvariant();
            Assert.AreSame(OfficialSpot(field),visual.sprite);Assert.AreEqual(Color.white,visual.color);
            var source=spots[i].GetComponent<SpriteRenderer>();Assert.IsFalse(source.enabled);
            Assert.AreEqual(source.sortingLayerID,visual.sortingLayerID);Assert.AreEqual(source.sortingOrder,visual.sortingOrder);
            Assert.AreEqual(source.sprite.bounds.size.x*Mathf.Abs(spots[i].transform.lossyScale.x),visual.bounds.size.x,.02f);
            Assert.Less(SortingLayer.GetLayerValueFromID(visual.sortingLayerID),SortingLayer.GetLayerValueFromID(Find("PlayerInteraction").GetComponent<SpriteRenderer>().sortingLayerID));
        }
        var available=spots.Single(x=>(string)Get(x,"StableId")=="09");
        var center=SpotVisual(available).bounds.center;float width=SpotVisual(available).bounds.size.x;
        for(int i=0;i<3;i++){Call(available,"Interact",Find("PlayerInteraction"));yield return null;}
        yield return new WaitForSeconds(.3f);
        Assert.AreEqual("PREPARED",Get(available,"State").ToString());
        Assert.AreSame(OfficialSpot("prepared"),SpotVisual(available).sprite);
        Assert.That(Vector3.Distance(center,SpotVisual(available).bounds.center),Is.LessThan(.01f));
        Assert.AreEqual(width,SpotVisual(available).bounds.size.x,.02f);
        Call(Find("BroteMinkaManager"),"Add",4);Assert.IsTrue((bool)Call(Find("BroteUpgradeManager"),"TryBuy",Kind(4)));
        foreach(var spot in spots.Where(x=>(int)Get(x,"Zone")==1))Assert.AreSame(OfficialSpot("available"),SpotVisual(spot).sprite);
        foreach(var spot in spots.Where(x=>(int)Get(x,"Zone")==2))Assert.AreSame(OfficialSpot("locked"),SpotVisual(spot).sprite);
        for(int i=0;i<spots.Length;i++)
        {
            Assert.AreEqual(positions[i],spots[i].transform.position);Assert.AreEqual(scales[i],spots[i].transform.localScale);
            Assert.AreEqual(radii[i],colliders[i].radius);Assert.AreEqual(offsets[i],colliders[i].offset);Assert.IsTrue(colliders[i].isTrigger);
        }
        Assert.IsTrue((bool)Call(Find("BroteSaveSystem"),"Save"));yield return Load();
        CollectionAssert.AreEqual(ids,Spots().Select(x=>(string)Get(x,"StableId")).ToArray());
        foreach(var spot in Spots())Assert.AreSame(OfficialSpot(Get(spot,"State").ToString().ToLowerInvariant()),SpotVisual(spot).sprite);
        Assert.AreEqual("PREPARED",Get(Spots().Single(x=>(string)Get(x,"StableId")=="09"),"State").ToString());
    }
    [UnityTest] public IEnumerator OfficialSpotIntegrationHidesTerrainAndPreservesPlantedTreeStages()
    {
        yield return Load();var spot=Spots()[0];var source=spot.GetComponent<SpriteRenderer>();
        Call(Find("SeedlingInventory"),"AddSeedlings",1);Call(spot,"Interact",Find("PlayerInteraction"));yield return null;
        Assert.AreEqual("PLANTED",Get(spot,"State").ToString());Assert.IsFalse(SpotVisual(spot).enabled);Assert.IsFalse(source.enabled);
        Assert.AreEqual(0,Get(Find("SeedlingInventory"),"CurrentSeedlings"));
        for(int stage=0;stage<3;stage++)
        {
            Call(spot,"RestoreSavedTree",true,stage);yield return null;
            var tree=(Component)Get(spot,"PlantedTree");var renderer=(SpriteRenderer)Get(tree,"CurrentStageRenderer");
            var sprite=renderer.sprite;Assert.IsNotNull(sprite);Assert.IsTrue(renderer.enabled);
            Call(spot,"SetProgressState",Enum.Parse(T("PlantingSpot+SpotState"),"PLANTED"),0,false);
            Assert.AreSame(sprite,renderer.sprite);Assert.IsTrue(renderer.enabled);Assert.IsFalse(SpotVisual(spot).enabled);Assert.IsFalse(source.enabled);
            var group=tree.GetComponent<UnityEngine.Rendering.SortingGroup>();
            Assert.IsNotNull(group);
            Assert.Less(SortingLayer.GetLayerValueFromID(SpotVisual(spot).sortingLayerID),SortingLayer.GetLayerValueFromID(group.sortingLayerID));
            Assert.AreEqual(stage,Get(tree,"Stage"));
        }
        // Complete the synthetic Developed fixture according to Save v2's existing invariants.
        spot.GetType().GetProperty("MinkaRewarded").SetValue(spot,true);
        Call(Find("RestorationManager"),"RestoreSavedPoints",10f);
        Assert.IsTrue((bool)Call(Find("BroteSaveSystem"),"Save"));yield return Load();
        var restored=Spots()[0];Assert.AreEqual("PLANTED",Get(restored,"State").ToString());
        Assert.AreEqual(2,Get((Component)Get(restored,"PlantedTree"),"Stage"));
        Assert.IsFalse(SpotVisual(restored).enabled);
    }
    [UnityTest] public IEnumerator PhaseTwoButtonStatesIconsAndHeaderUseOfficialSprites()
    {
        yield return Load();Call(Find("NurserySeedlingSource"),"Interact",Find("PlayerInteraction"));yield return null;yield return null;
        var panel=Find("NurseryManagementPanel");var root=panel.GetComponent<UIDocument>().rootVisualElement;
        var skin=Resources.Load<ScriptableObject>("UI/NurseryUISkin");
        Func<string,Sprite> sprite=field=>(Sprite)skin.GetType().GetField(field).GetValue(skin);
        var primary=root.Q<Button>("prepare-seedling");
        Assert.AreSame(sprite("primaryNormal"),primary.style.backgroundImage.value.sprite);
        using(var evt=PointerDownEvent.GetPooled(new Event{type=EventType.MouseDown,button=0,mousePosition=primary.worldBound.center}))
        {evt.target=primary;primary.SendEvent(evt);}
        Assert.AreSame(sprite("primaryPressed"),primary.style.backgroundImage.value.sprite);
        using(var evt=PointerUpEvent.GetPooled(new Event{type=EventType.MouseUp,button=0,mousePosition=primary.worldBound.center}))
        {evt.target=primary;primary.SendEvent(evt);}
        Assert.AreSame(sprite("primaryNormal"),primary.style.backgroundImage.value.sprite);
        using(var evt=PointerCancelEvent.GetPooled()){evt.target=primary;primary.SendEvent(evt);}
        Assert.AreSame(sprite("primaryNormal"),primary.style.backgroundImage.value.sprite);
        Assert.AreSame(sprite("iconMinka"),root.Q<VisualElement>("minka-icon").style.backgroundImage.value.sprite);
        Assert.AreSame(sprite("iconSeedling"),root.Q<VisualElement>("seedling-icon").style.backgroundImage.value.sprite);
        foreach(string name in new[]{"minka-icon","seedling-icon"})
            Assert.AreEqual(BackgroundSizeType.Contain,root.Q<VisualElement>(name).style.backgroundSize.value.sizeType);
        var wallet=Find("BroteMinkaManager");Call(wallet,"Add",20);Call(panel,"Refresh");
        Assert.AreEqual(Get(wallet,"Minka").ToString(),root.Q<Label>("nursery-minka").text);
        Call(Find("SeedlingInventory"),"AddSeedlings",5);Call(panel,"Refresh");
        Assert.AreEqual("5 / 5",root.Q<Label>("nursery-inventory").text);
        Assert.IsFalse(primary.enabledSelf);Assert.AreSame(sprite("primaryDisabled"),primary.style.backgroundImage.value.sprite);
        Call(panel,"HandleNavigation",true,false);
        string[] icons={"iconSoilPreparation","iconIrrigation","iconCapacity","iconSoilPreparation","iconExpansion"};
        for(int i=0;i<5;i++)
        {
            var icon=root.Q<VisualElement>("upgrade-card-"+i).Q<VisualElement>("upgrade-icon");
            Assert.AreSame(sprite(icons[i]),icon.style.backgroundImage.value.sprite);
            Assert.AreEqual(36f,icon.style.width.value.value);Assert.AreEqual(36f,icon.style.height.value.value);
            Assert.AreEqual(BackgroundSizeType.Contain,icon.style.backgroundSize.value.sizeType);
        }
        var purchase=root.Q<VisualElement>("upgrade-card-0").Q<Button>("upgrade-buy");
        Assert.IsTrue(purchase.enabledSelf);Assert.AreSame(sprite("buyAvailable"),purchase.style.backgroundImage.value.sprite);
        for(int i=0;i<3;i++)Assert.IsTrue((bool)Call(Find("BroteUpgradeManager"),"TryBuy",Kind(0)));
        Call(panel,"Refresh");Assert.IsFalse(purchase.enabledSelf);Assert.AreEqual("M\u00c1XIMO",purchase.text);
        Assert.AreSame(sprite("buyMax"),purchase.style.backgroundImage.value.sprite);
        Call(panel,"HandleNavigation",true,false);Assert.AreSame(sprite("primaryDisabled"),primary.style.backgroundImage.value.sprite);
        var close=root.Q<Button>("nursery-close");Assert.AreSame(sprite("closeButtonBackground"),close.style.backgroundImage.value.sprite);
        Assert.IsEmpty(close.text);Assert.AreEqual(1,root.Query<Button>("nursery-close").ToList().Count);
        Assert.GreaterOrEqual(close.worldBound.yMin,root.Q<VisualElement>("nursery-safe-area").worldBound.yMin-.1f);
        Assert.LessOrEqual(close.worldBound.yMax,root.Q<VisualElement>("nursery-heading").worldBound.yMax+.1f);
        using(var evt=NavigationSubmitEvent.GetPooled()){evt.target=close;close.SendEvent(evt);}
        Assert.IsFalse((bool)T("NurseryManagementPanel").GetProperty("IsOpen").GetValue(null));
    }
    [UnityTest] public IEnumerator PhaseTwoPurchaseDisabledRecoversWhenMinkaBecomesSufficient()
    {
        yield return Load();Call(Find("NurserySeedlingSource"),"Interact",Find("PlayerInteraction"));yield return null;
        var panel=Find("NurseryManagementPanel");var root=panel.GetComponent<UIDocument>().rootVisualElement;
        var skin=Resources.Load<ScriptableObject>("UI/NurseryUISkin");Call(panel,"HandleNavigation",true,false);
        var purchase=root.Q<VisualElement>("upgrade-card-1").Q<Button>("upgrade-buy");
        Assert.IsFalse(purchase.enabledSelf);
        Assert.AreSame(skin.GetType().GetField("buyDisabled").GetValue(skin),purchase.style.backgroundImage.value.sprite);
        Call(Find("BroteMinkaManager"),"Add",2);yield return null;
        Assert.IsTrue(purchase.enabledSelf);
        Assert.AreSame(skin.GetType().GetField("buyAvailable").GetValue(skin),purchase.style.backgroundImage.value.sprite);
        Call(panel,"HandleNavigation",false,true);Assert.IsFalse((bool)T("NurseryManagementPanel").GetProperty("IsOpen").GetValue(null));
    }
    [UnityTest] public IEnumerator NewGameHasTwoPreparedTwoAvailableSixLockedAndNurseryEOnlyOpensPanel()
    {
        yield return Load();var spots=Spots();
        CollectionAssert.AreEquivalent(new[]{"01","06"},spots.Where(x=>Get(x,"State").ToString()=="PREPARED").Select(x=>(string)Get(x,"StableId")));
        CollectionAssert.AreEquivalent(new[]{"09","10"},spots.Where(x=>Get(x,"State").ToString()=="AVAILABLE").Select(x=>(string)Get(x,"StableId")));
        Assert.AreEqual(6,spots.Count(x=>Get(x,"State").ToString()=="LOCKED"));
        Call(Find("NurserySeedlingSource"),"Interact",Find("PlayerInteraction"));Assert.IsTrue((bool)T("NurseryManagementPanel").GetProperty("IsOpen").GetValue(null));
        Assert.AreEqual(0,Get(Find("SeedlingInventory"),"CurrentSeedlings"));
        for(int i=0;i<10;i++)Call(Find("BroteIncrementalSession"),"Click");
        Assert.AreEqual(1,Get(Find("SeedlingInventory"),"CurrentSeedlings"));Assert.AreEqual(1,Get(Find("BroteMinkaManager"),"Minka"));
        var doc=Find("NurseryManagementPanel").GetComponent<UIDocument>();Assert.AreEqual(1,doc.rootVisualElement.Query<ProgressBar>().ToList().Count);
        Assert.IsTrue(doc.rootVisualElement.Query<Button>().ToList().Any(x=>x.text=="PREPARAR PL\u00c1NTULA"));
        Call(Find("NurseryManagementPanel"),"Close");Assert.IsFalse((bool)T("NurseryManagementPanel").GetProperty("IsOpen").GetValue(null));
    }
    [UnityTest] public IEnumerator NurseryProductionFirstAndButtonClickNavigationAndFullInventoryStates()
    {
        yield return Load();Call(Find("NurserySeedlingSource"),"Interact",Find("PlayerInteraction"));yield return null;
        var panel=Find("NurseryManagementPanel");var root=panel.GetComponent<UIDocument>().rootVisualElement;
        var production=root.Q<VisualElement>("production-view");var upgrades=root.Q<VisualElement>("upgrade-view");var button=root.Q<Button>("prepare-seedling");
        Assert.AreEqual(DisplayStyle.Flex,production.style.display.value);Assert.AreEqual(DisplayStyle.None,upgrades.style.display.value);Assert.IsTrue(button.enabledInHierarchy);
        Assert.AreEqual("PREPARAR PL\u00c1NTULA",button.text);Assert.IsNotNull(button.clickable);
        // Send the UI click event through the same Button action used by the mouse.
        using(var click=NavigationSubmitEvent.GetPooled()){click.target=button;button.SendEvent(click);}yield return null;
        Assert.AreEqual(10d,Get(Find("NurseryProductionManager"),"Progress"));Assert.AreEqual(10f,root.Q<ProgressBar>("production-progress").value);
        Assert.AreEqual("10%",root.Q<Label>("production-percentage").text);
        Call(panel,"ToggleView");Assert.AreEqual(DisplayStyle.None,production.style.display.value);Assert.AreEqual(DisplayStyle.Flex,upgrades.style.display.value);
        Call(panel,"HandleNavigation",true,false);Assert.AreEqual(DisplayStyle.Flex,production.style.display.value);
        Call(Find("SeedlingInventory"),"AddSeedlings",5);yield return null;
        Assert.IsFalse(button.enabledInHierarchy);Assert.AreEqual(DisplayStyle.Flex,root.Q<Label>("inventory-full").style.display.value);
        Assert.AreEqual(10d,Get(Find("NurseryProductionManager"),"Progress"));
        Assert.AreEqual(10f,root.Q<VisualElement>("production-progress-clip").style.width.value.value);
        Call(Find("BroteIncrementalSession"),"Click");yield return null;
        Assert.AreEqual(5,Get(Find("SeedlingInventory"),"CurrentSeedlings"));
        Assert.AreEqual(10d,Get(Find("NurseryProductionManager"),"Progress"));
        Call(panel,"HandleNavigation",false,true);Assert.IsFalse((bool)T("NurseryManagementPanel").GetProperty("IsOpen").GetValue(null));
        Assert.AreEqual(Time.frameCount,T("NurseryManagementPanel").GetProperty("ClosedFrame").GetValue(null));
    }
    [UnityTest] public IEnumerator NurserySkinSlotsTabsAndProgressGrowFromFixedLeftEdge()
    {
        yield return Load();Call(Find("NurserySeedlingSource"),"Interact",Find("PlayerInteraction"));yield return null;
        var panel=Find("NurseryManagementPanel");var root=panel.GetComponent<UIDocument>().rootVisualElement;
        Assert.IsNotNull(Resources.Load<ScriptableObject>("UI/NurseryUISkin"));
        Assert.IsNotNull(Resources.Load<VisualTreeAsset>("UI/NurseryUpgradeCard"));
        var track=root.Q<VisualElement>("production-progress-interior");var clip=root.Q<VisualElement>("production-progress-clip");var fill=root.Q<VisualElement>("production-progress-fill");
        Assert.IsNotNull(track);Assert.IsNotNull(clip);Assert.IsNotNull(fill);
        foreach(float width in new[]{200f,400f})
        {
            track.style.right=StyleKeyword.Auto;track.style.width=width;
            foreach(float value in new[]{0f,10f,25f,50f,75f,90f,100f})
            {
                Call(Find("NurseryProductionManager"),"Restore",(double)value);Call(panel,"Refresh");
                yield return null;yield return null;
                Assert.AreEqual(LengthUnit.Percent,clip.style.width.value.unit);
                Assert.AreEqual(value,clip.style.width.value.value);
                Assert.AreEqual(Overflow.Hidden,clip.style.overflow.value);
                Assert.AreEqual(0f,clip.style.left.value.value);
                Assert.AreEqual(StyleKeyword.Auto,clip.style.right.keyword);
                Assert.AreEqual(track.worldBound.xMin,clip.worldBound.xMin,.1f);
                Assert.AreEqual(track.worldBound.xMin,fill.worldBound.xMin,.1f);
                Assert.AreEqual(width*value/100f,clip.resolvedStyle.width,.5f);
                Assert.AreEqual(width,fill.resolvedStyle.width,.5f);
            }
        }
        track.style.width=StyleKeyword.Auto;track.style.right=0;
        var improvements=root.Q<Button>("upgrades-tab");var production=root.Q<Button>("production-tab");
        using(var click=NavigationSubmitEvent.GetPooled()){click.target=improvements;improvements.SendEvent(click);}
        Assert.IsTrue(improvements.ClassListContains("nursery-tab-active"));
        Assert.AreEqual(DisplayStyle.Flex,root.Q<VisualElement>("upgrade-view").style.display.value);
        for(int i=0;i<5;i++)
        {
            var card=root.Q<VisualElement>("upgrade-card-"+i);Assert.IsNotNull(card.Q<Button>("upgrade-buy"));
            Assert.IsNotEmpty(card.Q<Label>("upgrade-title").text);
        }
        Call(Find("BroteMinkaManager"),"Add",10);
        Call(panel,"Refresh");var purchase=root.Q<VisualElement>("upgrade-card-0").Q<Button>("upgrade-buy");
        using(var click=NavigationSubmitEvent.GetPooled()){click.target=purchase;purchase.SendEvent(click);}
        Assert.AreEqual(1,Call(Find("BroteUpgradeManager"),"Level",Kind(0)));
        using(var click=NavigationSubmitEvent.GetPooled()){click.target=production;production.SendEvent(click);}
        Assert.IsTrue(production.ClassListContains("nursery-tab-active"));
        Call(Find("NurseryProductionManager"),"Restore",95d);Call(panel,"Refresh");
        var prepare=root.Q<Button>("prepare-seedling");
        using(var click=NavigationSubmitEvent.GetPooled()){click.target=prepare;prepare.SendEvent(click);}
        Assert.AreEqual(1,Get(Find("SeedlingInventory"),"CurrentSeedlings"));
        Assert.AreEqual(10d,Get(Find("NurseryProductionManager"),"Progress"));
        Assert.AreEqual(10f,clip.style.width.value.value);Assert.AreEqual("10%",root.Q<Label>("production-percentage").text);
        var close=root.Q<Button>("nursery-close");
        using(var click=NavigationSubmitEvent.GetPooled()){click.target=close;close.SendEvent(click);}
        Assert.IsFalse((bool)T("NurseryManagementPanel").GetProperty("IsOpen").GetValue(null));
    }
    [UnityTest] public IEnumerator NurseryRightSidebarFitsWideTallAndShortViewportsWithScrollableUpgrades()
    {
        yield return Load();Call(Find("NurserySeedlingSource"),"Interact",Find("PlayerInteraction"));yield return null;
        var panel=Find("NurseryManagementPanel");var doc=panel.GetComponent<UIDocument>();var root=doc.rootVisualElement;
        Assert.AreEqual(PanelScaleMode.ConstantPixelSize,doc.panelSettings.scaleMode);
        var shell=root.Q<VisualElement>("nursery-shell");
        foreach(var size in new[]{new Vector2(1920,1080),new Vector2(1280,720),new Vector2(1440,900),new Vector2(1024,768),new Vector2(1920,400),new Vector2(640,360)})
        {
            root.style.right=root.style.bottom=StyleKeyword.Auto;root.style.width=size.x;root.style.height=size.y;
            yield return null;yield return null;Call(panel,"AdaptLayout");yield return null;yield return null;
            float right=root.worldBound.xMax-shell.worldBound.xMax;
            Assert.That(right,Is.InRange(15.5f,24.5f));
            Assert.That(shell.resolvedStyle.height/size.y,Is.InRange(.8f,.91f));
            Assert.That(shell.worldBound.center.y,Is.EqualTo(root.worldBound.center.y).Within(1));
            Assert.Greater(shell.worldBound.xMin-root.worldBound.xMin,size.x*.4f);
            if(size.x>=1000)Assert.That(shell.resolvedStyle.width,Is.InRange(379f,431f));
            var close=root.Q<Button>("nursery-close");Assert.LessOrEqual(close.worldBound.yMax,shell.worldBound.yMax);
            Call(panel,"HandleNavigation",true,false);yield return null;yield return null;
            var scroll=root.Q<ScrollView>("upgrade-list");Assert.Greater(scroll.resolvedStyle.height,0);
            Assert.AreEqual(5,scroll.contentContainer.childCount);
            Assert.Greater(scroll.contentContainer.ElementAt(4).worldBound.yMax-scroll.contentContainer.ElementAt(0).worldBound.yMin,scroll.contentViewport.resolvedStyle.height);
            scroll.scrollOffset=new Vector2(0,100);yield return null;Assert.Greater(scroll.scrollOffset.y,0);
            Call(panel,"HandleNavigation",true,false);yield return null;
            Assert.AreEqual(DisplayStyle.Flex,root.Q<VisualElement>("production-view").style.display.value);
        }
        Call(panel,"HandleNavigation",false,true);Assert.IsFalse((bool)T("NurseryManagementPanel").GetProperty("IsOpen").GetValue(null));
    }
    [UnityTest] public IEnumerator NurseryTabsUseFourIndependentSpritesAndKeepTheirLabels()
    {
        yield return Load();Call(Find("NurserySeedlingSource"),"Interact",Find("PlayerInteraction"));yield return null;
        var panel=Find("NurseryManagementPanel");var root=panel.GetComponent<UIDocument>().rootVisualElement;
        var skin=Resources.Load<ScriptableObject>("UI/NurseryUISkin");
        string[] fields={"productionTabActive","productionTabInactive","upgradesTabActive","upgradesTabInactive"};
        var saved=fields.Select(x=>skin.GetType().GetField(x).GetValue(skin)).ToArray();
        var texture=new Texture2D(8,8);var sprites=Enumerable.Range(0,4).Select(_=>Sprite.Create(texture,new Rect(0,0,8,8),Vector2.one*.5f)).ToArray();
        try
        {
            for(int i=0;i<4;i++)skin.GetType().GetField(fields[i]).SetValue(skin,sprites[i]);
            Call(panel,"Refresh");var production=root.Q<Button>("production-tab");var upgrades=root.Q<Button>("upgrades-tab");
            Assert.AreSame(sprites[0],production.style.backgroundImage.value.sprite);Assert.AreSame(sprites[3],upgrades.style.backgroundImage.value.sprite);
            Call(panel,"HandleNavigation",true,false);
            Assert.AreSame(sprites[1],production.style.backgroundImage.value.sprite);Assert.AreSame(sprites[2],upgrades.style.backgroundImage.value.sprite);
            Assert.AreEqual("PL\u00c1NTULAS",production.text);Assert.AreEqual("MEJORAS",upgrades.text);
        }
        finally
        {
            for(int i=0;i<4;i++)skin.GetType().GetField(fields[i]).SetValue(skin,saved[i]);
            Call(panel,"Refresh");foreach(var sprite in sprites)UnityEngine.Object.Destroy(sprite);UnityEngine.Object.Destroy(texture);
        }
    }
    [UnityTest] public IEnumerator LockedCannotPlantAvailableNeedsThreeActionsAndSoilUpgradeReducesActions()
    {
        yield return Load();var inv=Find("SeedlingInventory");var player=Find("PlayerInteraction");var spots=Spots();Call(inv,"AddSeedlings",5);
        var locked=spots[1];Call(locked,"Interact",player);Assert.AreEqual(5,Get(inv,"CurrentSeedlings"));Assert.IsNull(Get(locked,"PlantedTree"));
        var available=spots[8];
        for(int i=0;i<3;i++) { Call(available,"Interact",player);Call(available,"Interact",player);Assert.AreEqual(i+1,Get(available,"PreparationActions"));yield return null; }
        Assert.AreEqual("PREPARED",Get(available,"State").ToString());Assert.AreEqual(5,Get(inv,"CurrentSeedlings"));
        Call(Find("BroteMinkaManager"),"Add",7);Call(Find("BroteUpgradeManager"),"TryBuy",Kind(3));Call(Find("BroteUpgradeManager"),"TryBuy",Kind(3));
        Call(spots[9],"Interact",player);Assert.AreEqual("PREPARED",Get(spots[9],"State").ToString());
        Call(available,"Interact",player);Assert.AreEqual("PLANTED",Get(available,"State").ToString());Assert.AreEqual(4,Get(inv,"CurrentSeedlings"));
    }
    [UnityTest] public IEnumerator ExpansionsUnlockExactlyThreeEachWithoutMovingSpots()
    {
        yield return Load();var spots=Spots();var positions=spots.Select(x=>x.transform.position).ToArray();
        Call(Find("BroteMinkaManager"),"Add",12);Assert.IsTrue((bool)Call(Find("BroteUpgradeManager"),"TryBuy",Kind(4)));
        CollectionAssert.AreEquivalent(new[]{"04","05","08","09","10"},spots.Where(x=>Get(x,"State").ToString()=="AVAILABLE").Select(x=>(string)Get(x,"StableId")));
        Assert.IsTrue((bool)Call(Find("BroteUpgradeManager"),"TryBuy",Kind(4)));Assert.AreEqual(0,spots.Count(x=>Get(x,"State").ToString()=="LOCKED"));
        CollectionAssert.AreEqual(positions,spots.Select(x=>x.transform.position).ToArray());
    }
    [UnityTest] public IEnumerator DevelopedRewardsOnceAndV2RoundTripPreservesEconomyObjectivesAndPartialPreparation()
    {
        yield return Load();var session=Find("BroteIncrementalSession");for(int i=0;i<10;i++)Call(session,"Click");Call(Spots()[0],"Interact",Find("PlayerInteraction"));
        yield return new WaitForSeconds(10.2f);
        var wallet=Find("BroteMinkaManager");Assert.AreEqual(5,Get(wallet,"Minka")); // produce1+plant1+tree1+objectiveDeveloped2
        Call(Find("BroteUpgradeManager"),"TryBuy",Kind(0));Call(session,"Click");Call(Spots()[8],"Interact",Find("PlayerInteraction"));
        Assert.IsTrue((bool)Call(Find("BroteSaveSystem"),"Save"));
        string json=File.ReadAllText(path);var saved=JsonUtility.FromJson(json,T("BroteSaveData"));Assert.AreEqual(2,saved.GetType().GetField("saveVersion").GetValue(saved));
        int before=(int)Get(wallet,"Minka");double fraction=(double)Get(wallet,"Fraction");
        yield return Load();Assert.AreEqual(before,Get(Find("BroteMinkaManager"),"Minka"));Assert.AreEqual(15d,Get(Find("NurseryProductionManager"),"Progress"));
        Assert.AreEqual(1,Get(Spots()[8],"PreparationActions"));Assert.AreEqual(10f,Get(Find("RestorationManager"),"CurrentRestoration"));
        Assert.IsFalse((bool)Call(Find("BroteSaveSystem"),"Load"),"Cannot load a second time into planted scene");
        Call(Get(Spots()[0],"PlantedTree"),"PublishRestoredMaturity");Assert.AreEqual(before,Get(Find("BroteMinkaManager"),"Minka"));
        Assert.AreEqual(10f,Get(Find("RestorationManager"),"CurrentRestoration"));Assert.GreaterOrEqual((double)Get(Find("BroteMinkaManager"),"Fraction"),fraction);
    }
    [UnityTest] public IEnumerator PauseStopsMinkaAndNurseryAndResumeRestoresProduction()
    {
        yield return Load();var wallet=Find("BroteMinkaManager");Call(wallet,"Add",2);Call(Find("BroteUpgradeManager"),"TryBuy",Kind(1));
        Call(Spots()[0],"RestoreSavedTree",true,2);Call(Find("RestorationManager"),"RestoreSavedPoints",10f);
        Call(Find("BrotePauseMenu"),"SetPaused",true);double fraction=(double)Get(wallet,"Fraction"),progress=(double)Get(Find("NurseryProductionManager"),"Progress");
        yield return new WaitForSecondsRealtime(.25f);Assert.AreEqual(fraction,Get(wallet,"Fraction"));Assert.AreEqual(progress,Get(Find("NurseryProductionManager"),"Progress"));
        Call(Find("BrotePauseMenu"),"SetPaused",false);yield return new WaitForSeconds(.25f);Assert.Greater((double)Get(wallet,"Fraction"),fraction);Assert.Greater((double)Get(Find("NurseryProductionManager"),"Progress"),progress);
    }
    [UnityTest] public IEnumerator V2RestoresTwentySeedlingsAllLevelsAndFullInventoryProgressWithoutClampingToFive()
    {
        yield return Load();var wallet=Find("BroteMinkaManager");var upgrades=Find("BroteUpgradeManager");Call(wallet,"Add",100);
        for(int kind=0;kind<5;kind++) for(int level=0;level<(kind<3?3:2);level++)Assert.IsTrue((bool)Call(upgrades,"TryBuy",Kind(kind)));
        Call(Find("SeedlingInventory"),"RestoreSavedCount",20);Call(Find("NurseryProductionManager"),"Restore",95d);Call(wallet,"Restore",42,.37d);
        Assert.IsTrue((bool)Call(Find("BroteSaveSystem"),"Save"));yield return Load();
        Assert.AreEqual(20,Get(Find("SeedlingInventory"),"MaxSeedlings"));Assert.AreEqual(20,Get(Find("SeedlingInventory"),"CurrentSeedlings"));
        for(int kind=0;kind<5;kind++)Assert.AreEqual(kind<3?3:2,Call(Find("BroteUpgradeManager"),"Level",Kind(kind)));
        Assert.AreEqual(95d,Get(Find("NurseryProductionManager"),"Progress"));Assert.AreEqual(42,Get(Find("BroteMinkaManager"),"Minka"));Assert.AreEqual(.37d,(double)Get(Find("BroteMinkaManager"),"Fraction"),1e-8);
        Assert.AreEqual(0,Spots().Count(x=>Get(x,"State").ToString()=="LOCKED"));
        yield return new WaitForSeconds(.2f);Assert.AreEqual(95d,Get(Find("NurseryProductionManager"),"Progress"));Assert.AreEqual(20,Get(Find("SeedlingInventory"),"CurrentSeedlings"));
    }
    [UnityTest] public IEnumerator ConfirmedNewGameCannotRecreateOldSaveDuringSceneReload()
    {
        yield return Load();for(int i=0;i<10;i++)Call(Find("BroteIncrementalSession"),"Click");
        var save=Find("BroteSaveSystem");Assert.IsTrue((bool)Call(save,"Save"));
        Assert.IsTrue((bool)T("BroteSaveSystem").GetMethod("DeleteConfirmedSave").Invoke(null,null));
        Call(save,"RequestSave");Assert.IsFalse((bool)Call(save,"Save"));yield return null;Assert.IsFalse(File.Exists(path));
        yield return Load();Assert.AreEqual(0,Get(Find("SeedlingInventory"),"CurrentSeedlings"));Assert.AreEqual(0,Get(Find("BroteMinkaManager"),"Minka"));
        Assert.AreEqual(6,Spots().Count(x=>Get(x,"State").ToString()=="LOCKED"));
        // Diagnose the actual menu confirmation using the existing isolated save fixture.
        Call(Find("SeedlingInventory"),"AddSeedlings",1);Call(Spots()[0],"Interact",Find("PlayerInteraction"));
        Assert.IsNotNull(Get(Spots()[0],"PlantedTree"));Assert.IsTrue((bool)Call(Find("BroteSaveSystem"),"Save"));
        yield return SceneManager.LoadSceneAsync("Assets/_BROTE/Scenes/UI/MainMenu.unity");yield return null;
        var menu=Find("BroteMainMenu");Call(menu,"ConfirmNew");
        var confirm=menu.GetComponent<UIDocument>().rootVisualElement.Query<Button>().ToList().Single(x=>x.text=="CONFIRMAR");
        Call(confirm.clickable,"Invoke",new object[]{null});
        for(int i=0;i<120 && SceneManager.GetActiveScene().name!="Acopalca";i++)yield return null;
        yield return null;yield return null;
        Assert.AreEqual("Acopalca",SceneManager.GetActiveScene().name);
        Assert.IsTrue(Spots().All(x=>Get(x,"PlantedTree")==null));
        Assert.AreEqual(2,Spots().Count(x=>Get(x,"State").ToString()=="PREPARED"));
        Assert.AreEqual(2,Spots().Count(x=>Get(x,"State").ToString()=="AVAILABLE"));
        Assert.AreEqual(6,Spots().Count(x=>Get(x,"State").ToString()=="LOCKED"));
    }
    [UnityTest] public IEnumerator RealV1SaveMigratesAndRetainsLegacyProgressWithoutRetroactiveMinka()
    {
        var data=Activator.CreateInstance(T("BroteSaveData"));var records=Array.CreateInstance(T("BroteSavedSpot"),10);
        for(int i=0;i<10;i++){var r=Activator.CreateInstance(T("BroteSavedSpot"));r.GetType().GetField("id").SetValue(r,(i+1).ToString("00"));r.GetType().GetField("planted").SetValue(r,i<5);r.GetType().GetField("stage").SetValue(r,i<5?2:0);records.SetValue(r,i);}
        data.GetType().GetField("spots").SetValue(data,records);data.GetType().GetField("restoration").SetValue(data,50f);data.GetType().GetField("inventory").SetValue(data,5);data.GetType().GetField("tutorialCompleted").SetValue(data,true);
        File.WriteAllText(path,JsonUtility.ToJson(data));yield return Load();
        Assert.AreEqual(50f,Get(Find("RestorationManager"),"CurrentRestoration"));Assert.AreEqual(5,Get(Find("SeedlingInventory"),"CurrentSeedlings"));Assert.AreEqual(0,Get(Find("BroteMinkaManager"),"Minka"));Assert.AreEqual(2,Call(Find("BroteUpgradeManager"),"Level",Kind(4)));
        Assert.IsTrue(File.Exists(path+".v1.backup"));Assert.IsTrue((bool)Get(Find("BroteTutorialState"),"Completed"));Assert.IsTrue((bool)Call(Find("BroteSaveSystem"),"Save"));
        yield return Load();Assert.AreEqual(50f,Get(Find("RestorationManager"),"CurrentRestoration"));Assert.AreEqual(0,Get(Find("BroteMinkaManager"),"Minka"));
    }
}

