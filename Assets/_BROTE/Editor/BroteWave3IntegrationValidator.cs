using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BroteWave3IntegrationValidator
{
    public static void Execute()
    {
        var scene=EditorSceneManager.OpenScene(BroteRestorationPropPlacer.ScenePath);
        var report=BroteProjectValidator.ValidateScene(scene);
        var sector=BroteProjectValidator.SceneComponents<Transform>(scene).Single(x=>x.name=="Sector_01");
        foreach(string group in new[]{"Degraded","Recovery20","Recovery40","Restored50"})
            if(sector.Find("GeneratedEnvironment/"+group)==null)report.Error("Missing generated group "+group);
        var props=sector.Find("GeneratedEnvironment/Degraded").GetComponentsInChildren<SpriteRenderer>(true);
        if(props.Length!=26)report.Error("Expected26 degraded props");
        var transition=GameObject.Find("World/Midground/LandscapeTransition/LandscapeTransitionVisuals");
        if(transition==null||transition.GetComponentsInChildren<SpriteRenderer>().Length!=8)report.Error("Expected8 transition visuals");
        foreach(var sr in props.Concat(transition.GetComponentsInChildren<SpriteRenderer>()))
            if(sr.sprite==null||sr.GetComponent<Collider2D>()!=null)report.Error("Prop missing sprite or contains collider");
        var spots=BroteProjectValidator.SceneComponents<PlantingSpot>(scene).OrderBy(x=>x.StableId).ToArray();
        if(!spots.Select(x=>x.StableId).SequenceEqual(Enumerable.Range(1,10).Select(x=>x.ToString("00"))))report.Error("Invalid spot IDs");
        if(BroteProjectValidator.SceneComponents<BroteSaveSystem>(scene).Count()!=1||BroteProjectValidator.SceneComponents<BroteTutorialState>(scene).Count()!=1)report.Error("Missing/duplicate session components");
        var planMethod=typeof(BroteRestorationPropPlacer).GetMethod("Plan",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static);
        string Signature()
        {
            var placements=(System.Collections.IEnumerable)planMethod.Invoke(null,new object[]{3102026});
            var signature=new System.Text.StringBuilder();
            foreach(var placement in placements)
            {
                var type=placement.GetType();
                foreach(string field in new[]{"sprite","position","scale","flip"})signature.Append(type.GetField(field).GetValue(placement)).Append('|');
            }
            return signature.ToString();
        }
        if(Signature()!=Signature())report.Error("Same seed produces different plans");
        report.Info("Seed3102026: two native read-only plans match.");
        File.WriteAllText(".brote-work/wave-3/integration/native-scene-validation.md",report.Markdown());
        var menu=EditorSceneManager.OpenScene("Assets/_BROTE/Scenes/UI/MainMenu.unity");
        foreach(var root in menu.GetRootGameObjects())
        {var refs=BroteProjectValidator.ValidateMissingAndDuplicates(root);if(refs.Errors>0)report.Error(refs.Markdown());}
        var settings=new SerializedObject(Object.FindObjectsByType<BroteMainMenu>().Single());
        if(settings.FindProperty("landscape").objectReferenceValue==null)report.Error("Missing menu landscape");
        if(report.Errors>0)throw new System.InvalidOperationException("Scene integration validator errors: "+report.Errors);
        File.WriteAllText(".brote-work/wave-3/integration/native-integration-validation.txt","PASS:26 degraded/8 transition/10 IDs/session/menu GUID refs, existing scene validator errors0.\n");
    }
}
