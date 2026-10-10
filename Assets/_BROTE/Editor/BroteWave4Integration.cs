using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BroteWave4Integration
{
    public static void Execute()
    {
        const string path="Assets/_BROTE/Scenes/Acopalca/Acopalca.unity";
        Directory.CreateDirectory(".brote-work/wave-4");
        File.Copy(path,".brote-work/wave-4/Acopalca-before-native.unity",true);
        var scene=EditorSceneManager.OpenScene(path);
        var save=BroteProjectValidator.SceneComponents<BroteSaveSystem>(scene).Single();
        var go=save.gameObject;
        if(go.GetComponent<BroteMinkaManager>()==null)go.AddComponent<BroteMinkaManager>();
        if(go.GetComponent<BroteUpgradeManager>()==null)go.AddComponent<BroteUpgradeManager>();
        if(go.GetComponent<NurseryProductionManager>()==null)go.AddComponent<NurseryProductionManager>();
        if(go.GetComponent<BroteRestorationObjectives>()==null)go.AddComponent<BroteRestorationObjectives>();
        var session=go.GetComponent<BroteIncrementalSession>();if(session==null)session=go.AddComponent<BroteIncrementalSession>();
        var so=new SerializedObject(session);so.FindProperty("zoneLayout").objectReferenceValue=AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/_BROTE/Settings/IncrementalZoneLayout.json");so.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        Validate();
    }
    public static void Validate()
    {
        var scene=EditorSceneManager.OpenScene("Assets/_BROTE/Scenes/Acopalca/Acopalca.unity");
        var report=BroteProjectValidator.ValidateScene(scene);
        var spots=BroteProjectValidator.SceneComponents<PlantingSpot>(scene).OrderBy(x=>x.StableId).ToArray();
        if(spots.Length!=10 || !spots.Select(x=>x.StableId).SequenceEqual(Enumerable.Range(1,10).Select(x=>x.ToString("00")))) report.Error("Invalid ten stable IDs");
        foreach(var type in new[]{typeof(BroteMinkaManager),typeof(BroteUpgradeManager),typeof(NurseryProductionManager),typeof(BroteRestorationObjectives),typeof(BroteIncrementalSession)})
            if(scene.GetRootGameObjects().SelectMany(x=>x.GetComponentsInChildren(type,true)).Count()!=1)report.Error("Missing/duplicate "+type.Name);
        var session=BroteProjectValidator.SceneComponents<BroteIncrementalSession>(scene).Single();
        var data=new SerializedObject(session).FindProperty("zoneLayout").objectReferenceValue as TextAsset;
        if(data==null)report.Error("Missing layout");
        else
        {
            var layout=JsonUtility.FromJson<BroteZoneLayout>(data.text);
            if(!layout.zones.Select(x=>x.name).SequenceEqual(new[]{"Initial","Expansion01","Expansion02"}) || !layout.zones.Select(x=>x.stableIds.Length).SequenceEqual(new[]{4,3,3}) || layout.zones.SelectMany(x=>x.stableIds).Distinct().Count()!=10 || layout.prepared.Length!=2)report.Error("Invalid spatial grouping");
        }
        File.WriteAllText(".brote-work/wave-4/native-validator.md",report.Markdown());
        if(report.Errors>0)throw new InvalidOperationException("Wave4 validator errors: "+report.Errors);
        Debug.Log("BROTE WAVE4 VALIDATOR PASS; visual/input review PLAY_REQUIRED");
    }
}
