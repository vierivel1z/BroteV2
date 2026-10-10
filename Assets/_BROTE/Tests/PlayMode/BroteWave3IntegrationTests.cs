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

public sealed class BroteWave3IntegrationTests
{
    private const BindingFlags Flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
    private const string ScenePath="Assets/_BROTE/Scenes/Acopalca/Acopalca.unity";
    private string testPath;
    private static Type Runtime(string name)=>Type.GetType(name+", Assembly-CSharp",true);
    private static object Call(object owner,string name,params object[] args)=>owner.GetType().GetMethod(name,Flags).Invoke(owner,args);
    private static object Get(object owner,string name)=>owner.GetType().GetProperty(name,Flags).GetValue(owner);
    private static Component Find(string name)=>UnityEngine.Object.FindObjectsByType(Runtime(name),FindObjectsSortMode.None).Cast<Component>().Single();
    private static Component[] Spots()=>UnityEngine.Object.FindObjectsByType(Runtime("PlantingSpot"),FindObjectsSortMode.None).Cast<Component>().OrderBy(x=>(string)Get(x,"StableId")).ToArray();
    [SetUp] public void Setup()
    {
        testPath=Path.Combine(Path.GetTempPath(),"BROTE-Wave3-"+Guid.NewGuid().ToString("N")+".json");
        Runtime("BroteSaveSystem").GetField("editorTestSavePath",Flags).SetValue(null,testPath);Time.timeScale=1f;
    }
    [UnityTearDown] public IEnumerator Cleanup()
    {
        // Destroy the session before clearing the isolated path, so quit/autosave can't touch user data.
        yield return SceneManager.LoadSceneAsync("Assets/_BROTE/Scenes/UI/MainMenu.unity",LoadSceneMode.Single);
        yield return null;
        Runtime("BroteSaveSystem").GetField("editorTestSavePath",Flags).SetValue(null,null);
        if(File.Exists(testPath))File.Delete(testPath);if(File.Exists(testPath+".tmp"))File.Delete(testPath+".tmp");Time.timeScale=1f;
    }
    private static IEnumerator LoadGame()
    {
        yield return SceneManager.LoadSceneAsync(ScenePath,LoadSceneMode.Single);yield return null;yield return null;
    }
    private static void UnlockLegacyFixture()
    {
        Call(Find("BroteMinkaManager"),"Add",12);
        var kind=Enum.ToObject(Runtime("BroteUpgrade"),4);
        Call(Find("BroteUpgradeManager"),"TryBuy",kind);Call(Find("BroteUpgradeManager"),"TryBuy",kind);
    }
    [UnityTest] public IEnumerator ActualSceneRoundTripsZeroFiveTenTreesInventoryZeroFiveAndTutorial()
    {
        foreach(int mature in new[]{0,5,10})foreach(int count in new[]{0,5})
        {
            if(File.Exists(testPath))File.Delete(testPath);
            yield return LoadGame();var save=Find("BroteSaveSystem");var manager=Find("RestorationManager");var inventory=Find("SeedlingInventory");var spots=Spots();
            CollectionAssert.AreEqual(Enumerable.Range(1,10).Select(x=>x.ToString("00")),spots.Select(x=>(string)Get(x,"StableId")));
            Assert.IsNotNull(Find("BroteTutorialState"));Assert.IsNotNull(Find("BrotePauseMenu"));
            UnlockLegacyFixture();
            for(int i=0;i<mature;i++)Call(spots[i],"RestoreSavedTree",true,2);
            Call(manager,"RestoreSavedPoints",mature*10f);Call(inventory,"RestoreSavedCount",count);Call(Find("BroteTutorialState"),"RestoreCompleted",true);
            Assert.IsTrue((bool)Call(save,"Save"));Assert.IsTrue(File.Exists(testPath));
            yield return LoadGame();save=Find("BroteSaveSystem");manager=Find("RestorationManager");inventory=Find("SeedlingInventory");spots=Spots();
            Assert.AreEqual(mature*10f,Get(manager,"CurrentRestoration"));Assert.AreEqual(count,Get(inventory,"CurrentSeedlings"));
            Assert.AreEqual(mature,spots.Count(x=>Get(x,"State").ToString()=="PLANTED"));
            Assert.AreEqual(mature,Get(Find("GroundRestorationController"),"MatureTreeCount"));
            Assert.IsTrue((bool)Get(Find("BroteTutorialState"),"Completed"));
            foreach(var spot in spots.Take(mature))Assert.AreEqual(2,Get(Get(spot,"PlantedTree"),"Stage"));
            yield return new WaitForSeconds(.1f);
            Assert.AreEqual(mature*10f,Get(manager,"CurrentRestoration"),"Loaded Developed must never grant +10 again");
            Assert.IsTrue((bool)Call(save,"Save"));
        }
    }
    [UnityTest] public IEnumerator ActualSceneLoadsYoungAndSeedlingWithoutRestartingDevelopedContribution()
    {
        yield return LoadGame();var spots=Spots();var save=Find("BroteSaveSystem");
        UnlockLegacyFixture();
        Call(spots[0],"RestoreSavedTree",true,2);Call(spots[1],"RestoreSavedTree",true,1);Call(spots[2],"RestoreSavedTree",true,0);
        Call(Find("RestorationManager"),"RestoreSavedPoints",10f);Assert.IsTrue((bool)Call(save,"Save"));
        yield return LoadGame();spots=Spots();
        Assert.AreEqual(2,Get(Get(spots[0],"PlantedTree"),"Stage"));Assert.AreEqual(1,Get(Get(spots[1],"PlantedTree"),"Stage"));Assert.AreEqual(0,Get(Get(spots[2],"PlantedTree"),"Stage"));
        yield return new WaitForSeconds(5.15f);
        Assert.AreEqual(2,Get(Get(spots[1],"PlantedTree"),"Stage"));Assert.AreEqual(1,Get(Get(spots[2],"PlantedTree"),"Stage"));Assert.AreEqual(20f,Get(Find("RestorationManager"),"CurrentRestoration"));
        yield return new WaitForSeconds(5.15f);
        Assert.AreEqual(2,Get(Get(spots[2],"PlantedTree"),"Stage"));Assert.AreEqual(30f,Get(Find("RestorationManager"),"CurrentRestoration"));
        Assert.IsTrue((bool)Get(Find("BroteTutorialState"),"Completed"));
    }
    private object ReadIsolatedSave()
    {
        Assert.IsTrue(File.Exists(testPath), "Autosave must create its isolated file");
        var data=JsonUtility.FromJson(File.ReadAllText(testPath),Runtime("BroteSaveData"));
        Assert.IsTrue((bool)Call(data,"IsValid"));return data;
    }
    [UnityTest] public IEnumerator ActualNurseryPlantYoungAndDevelopedAutosaveCoherentProgress()
    {
        yield return LoadGame();var player=Find("PlayerInteraction");var source=Find("NurserySeedlingSource");var spot=Spots()[0];
        Call(source,"Interact",player);
        Assert.AreEqual(0,Get(Find("SeedlingInventory"),"CurrentSeedlings"),"E must only open nursery");
        for(int click=0;click<10;click++)Call(Find("BroteIncrementalSession"),"Click");yield return null;yield return null;
        var data=ReadIsolatedSave();Assert.AreEqual(1,data.GetType().GetField("inventory").GetValue(data));
        Call(Find("NurseryManagementPanel"),"Close");
        Call(spot,"Interact",player);yield return null;yield return null;
        data=ReadIsolatedSave();Assert.AreEqual(0,data.GetType().GetField("inventory").GetValue(data));
        var record=((Array)data.GetType().GetField("spots").GetValue(data)).GetValue(0);
        Assert.IsTrue((bool)record.GetType().GetField("planted").GetValue(record));Assert.AreEqual(0,record.GetType().GetField("stage").GetValue(record));
        yield return new WaitForSeconds(5.15f);yield return null;
        data=ReadIsolatedSave();record=((Array)data.GetType().GetField("spots").GetValue(data)).GetValue(0);
        Assert.AreEqual(1,record.GetType().GetField("stage").GetValue(record));Assert.AreEqual(0f,data.GetType().GetField("restoration").GetValue(data));
        yield return new WaitForSeconds(5.15f);yield return null;
        data=ReadIsolatedSave();record=((Array)data.GetType().GetField("spots").GetValue(data)).GetValue(0);
        Assert.AreEqual(2,record.GetType().GetField("stage").GetValue(record));Assert.AreEqual(10f,data.GetType().GetField("restoration").GetValue(data));
        Assert.IsTrue((bool)data.GetType().GetField("tutorialCompleted").GetValue(data));
    }
    [UnityTest] public IEnumerator ActualMainMenuBuildsButtonsControlsOptionsAndLandscape()
    {
        yield return SceneManager.LoadSceneAsync("Assets/_BROTE/Scenes/UI/MainMenu.unity",LoadSceneMode.Single);yield return null;
        var menu=Find("BroteMainMenu");var doc=menu.GetComponent<UIDocument>();Assert.IsNotNull(doc);
        var buttons=doc.rootVisualElement.Query<Button>().ToList().Select(x=>x.text).ToArray();
        CollectionAssert.IsSubsetOf(new[]{"NUEVA PARTIDA","CONTINUAR","OPCIONES","SALIR"},buttons);
        Assert.IsNotNull(menu.GetType().GetField("landscape",Flags).GetValue(menu));
        Call(menu,"Controls");Assert.IsTrue(doc.rootVisualElement.Query<Label>().ToList().Any(x=>x.text.Contains("WASD / flechas")));
        Call(menu,"Options");Assert.AreEqual(2,doc.rootVisualElement.Query<Slider>().ToList().Count);
        Call(menu,"Home");Call(menu,"Play");while(SceneManager.GetActiveScene().name!="Acopalca")yield return null;yield return null;
        Assert.IsNotNull(Find("BroteSaveSystem"));
    }
}
