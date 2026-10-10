using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class BroteRestorationPropPlacer : EditorWindow
{
    private const string Settings = "Assets/_BROTE/Settings/";
    private const string Sheet = "Assets/_BROTE/Art/Environment/Props/Nature/Restoration_Props.png";
    public const string ScenePath = "Assets/_BROTE/Scenes/Acopalca/Acopalca.unity";
    private int seed = 3102026;
    private bool preview = true;
    [Serializable] private sealed class Catalog { public string sourceSheet; public Groups groups; }
    [Serializable] private sealed class Groups { public string[] Degraded, Recovery, Restored; }
    [Serializable] private sealed class Layout { public Vector2[] clusters; public PathDefinition.Route[] routes; }
    [Serializable] private sealed class Slots { public Slot[] slots; }
    [Serializable] private sealed class Slot { public string kind; public Vector2 worldPosition; }
    private sealed class Placement { public Sprite sprite; public Vector2 position; public float scale; public bool flip; }
    [MenuItem("BROTE/Wave 3/Restoration Prop Placer")]
    private static void Open() => GetWindow<BroteRestorationPropPlacer>("BROTE Props");
    private void OnEnable() => SceneView.duringSceneGui += Draw;
    private void OnDisable() => SceneView.duringSceneGui -= Draw;
    private void OnGUI()
    {
        seed = EditorGUILayout.IntField("Seed",seed); preview = EditorGUILayout.Toggle("Preview",preview);
        EditorGUILayout.HelpBox("Generate reutiliza la decoración generada anteriormente. Props manuales se conservan. Snapshot antes de cada operación.",MessageType.Info);
        if(GUILayout.Button("Generate")) Generate(seed);
        if(GUILayout.Button("Regenerate")) Generate(seed);
        if(GUILayout.Button("Clear Generated")) Clear();
        SceneView.RepaintAll();
    }
    public static void Snapshot(string path)
    {
        string directory=".brote-work/wave-3/backups";Directory.CreateDirectory(directory);
        string target=Path.Combine(directory,Path.GetFileName(path)+"."+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fffffff")+".snapshot");
        if(File.Exists(path))File.Copy(path,target);else File.WriteAllText(target,"SCENE DID NOT EXIST BEFORE CREATION");
    }
    private static Transform Sector() => GameObject.Find("World/Gameplay/Sector_01")?.transform ?? UnityEngine.Object.FindObjectsByType<Transform>().First(x=>x.name=="Sector_01");
    private static Sprite[] Sprites() => AssetDatabase.LoadAllAssetsAtPath(Sheet).OfType<Sprite>().OrderBy(x=>x.name).ToArray();
    private static bool Safe(Vector2 p,float radius)
    {
        if(p.x-radius < -18.2f || p.x+radius > 18.2f || p.y-radius < -13.2f || p.y+radius > 12.5f)return false;
        foreach(var spot in UnityEngine.Object.FindObjectsByType<PlantingSpot>()) if(Vector2.Distance(p,spot.transform.position)<1.7f+radius)return false;
        foreach(var player in UnityEngine.Object.FindObjectsByType<PlayerInteraction>()) if(Vector2.Distance(p,player.transform.position)<1.7f+radius)return false;
        // Nursery building plus entrance: intentionally conservative footprint.
        if(p.x+radius>-12.8f && p.x-radius<-3.6f && p.y+radius>-2.5f && p.y-radius<5.5f)return false;
        var edge=UnityEngine.Object.FindObjectsByType<EdgeCollider2D>().First(x=>x.name=="ScenicBoundary");
        var points=edge.points.Select(x=>(Vector2)edge.transform.TransformPoint(x+edge.offset)).ToArray();
        for(int i=1;i<points.Length;i++) if(p.x>=points[i-1].x && p.x<=points[i].x)
        {float y=Mathf.Lerp(points[i-1].y,points[i].y,Mathf.InverseLerp(points[i-1].x,points[i].x,p.x));if(p.y+radius>y-.3f)return false;}
        var layout=JsonUtility.FromJson<Layout>(File.ReadAllText(Settings+"Sector01VisualLayout.json"));
        foreach(var route in layout.routes)for(int i=1;i<route.points.Length;i++) if(Distance(p,route.points[i-1],route.points[i])<route.width*.5f+radius+.2f)return false;
        return true;
    }
    public static float Distance(Vector2 p,Vector2 a,Vector2 b)
    {var d=b-a;float t=d.sqrMagnitude>0?Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude):0;return Vector2.Distance(p,a+t*d);}
    private static List<Placement> Plan(int seed)
    {
        var catalog=JsonUtility.FromJson<Catalog>(File.ReadAllText(Settings+"RestorationPropSets.json"));
        if(catalog.sourceSheet!=Sheet)throw new InvalidDataException("Prop sheet mismatch");
        var sprites=Sprites().Where(x=>catalog.groups.Degraded.Contains(x.name)).ToArray();
        var rng=new System.Random(seed);var result=new List<Placement>();int attempts=0;
        while(result.Count<26 && attempts++<20000)
        {
            // Small clusters, selected only inside the actual playable boundary.
            int cluster=result.Count/3;
            Vector2 center=new Vector2(-16+cluster%5*7.4f,-11+cluster/5*7.8f);
            var p=center+new Vector2((float)(rng.NextDouble()*2.6-1.3),(float)(rng.NextDouble()*2.6-1.3));
            var sprite=sprites[(result.Count+cluster)%sprites.Length];
            float scale=.6f/sprite.bounds.size.y*(.9f+(float)rng.NextDouble()*.2f);
            float radius=Mathf.Max(sprite.bounds.size.x,sprite.bounds.size.y)*scale;
            if(!Safe(p,radius)||result.Any(x=>Vector2.Distance(p,x.position)<.8f))
            {
                if(attempts%200==0) { // broad fallback keeps density low when a cluster is obstructed
                    p=new Vector2(-17+(float)rng.NextDouble()*34,-12+(float)rng.NextDouble()*14);
                    if(!Safe(p,radius)||result.Any(x=>Vector2.Distance(p,x.position)<1f))continue;
                } else continue;
            }
            result.Add(new Placement{sprite=sprite,position=p,scale=scale,flip=rng.Next(2)==1});
        }
        if(result.Count!=26)throw new InvalidDataException("Not enough clear prop positions");return result;
    }
    private void Draw(SceneView view)
    {
        if(!preview||UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!=ScenePath)return;
        Handles.color=new Color(.8f,.6f,.3f);foreach(var p in Plan(seed)){Handles.DrawWireDisc(p.position,Vector3.forward,.35f);Handles.Label(p.position,p.sprite.name);}
        var slots=JsonUtility.FromJson<Slots>(File.ReadAllText(Settings+"LandscapeTransitionSlots.json"));
        Handles.color=Color.cyan;foreach(var s in slots.slots){Handles.DrawWireDisc(s.worldPosition,Vector3.forward,.3f);Handles.Label(s.worldPosition,s.kind);}
    }
    private static GameObject Child(Transform parent,string name)
    {var found=parent.Find(name);if(found!=null)return found.gameObject;var go=new GameObject(name);Undo.RegisterCreatedObjectUndo(go,"BROTE props");go.transform.SetParent(parent,false);return go;}
    private static void Place(Transform parent,Placement p,string name)
    {
        var go=new GameObject(name);Undo.RegisterCreatedObjectUndo(go,"BROTE props");go.transform.SetParent(parent,false);go.transform.position=p.position;go.transform.localScale=Vector3.one*p.scale;
        var sr=go.AddComponent<SpriteRenderer>();sr.sprite=p.sprite;sr.flipX=p.flip;sr.sortingLayerName="Default";sr.sortingOrder=-3;
        // Existing shrubs are small ground decoration (<0.66u), so no per-frame YSort needed.
    }
    public static void Generate(int seed=3102026)
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Edit mode only");
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(scene.path!=ScenePath)throw new InvalidOperationException("Open Acopalca first");
        var plan=Plan(seed);Snapshot(scene.path);var sector=Sector();
        var generated=Child(sector,"GeneratedEnvironment");var degraded=Child(generated.transform,"Degraded");
        // Prior script-generated props have documented provenance; manual decoration is untouched.
        var legacy=sector.Find("DegradedProps");
        if(legacy!=null && legacy.Cast<Transform>().All(x=>x.name.StartsWith("DegradedProp_")))Undo.DestroyObjectImmediate(legacy.gameObject);
        foreach(Transform child in degraded.transform.Cast<Transform>().ToArray())if(child.name.StartsWith("Wave3_"))Undo.DestroyObjectImmediate(child.gameObject);
        for(int i=0;i<plan.Count;i++)Place(degraded.transform,plan[i],"Wave3_"+i.ToString("00")+"_"+plan[i].sprite.name);
        string[] groups={"Recovery20","Recovery40","Restored50"};string[] old={"Recovery_20_FirstShoots","Recovery_40_Vegetation","Recovery_50_Recovered"};
        for(int i=0;i<3;i++)
        {
            var target=Child(generated.transform,groups[i]);var source=sector.GetComponentsInChildren<Transform>(true).FirstOrDefault(x=>x.name==old[i]);
            if(source!=null) Undo.SetTransformParent(source,target.transform,"Preserve threshold references");
        }
        foreach(var sr in generated.GetComponentsInChildren<SpriteRenderer>(true))if(sr.sprite!=null)sr.enabled=true;
        foreach(var spot in UnityEngine.Object.FindObjectsByType<PlantingSpot>())
        {var so=new SerializedObject(spot);so.FindProperty("stableId").stringValue=spot.name.Substring(spot.name.Length-2);so.ApplyModifiedPropertiesWithoutUndo();}
        var owner=Child(sector,"Session");if(owner.GetComponent<BroteSaveSystem>()==null)owner.AddComponent<BroteSaveSystem>();if(owner.GetComponent<BroteTutorialState>()==null)owner.AddComponent<BroteTutorialState>();
        GenerateTransition();EditorSceneManager.MarkSceneDirty(scene);
    }
    private static void GenerateTransition()
    {
        var parent=GameObject.Find("World/Midground/LandscapeTransition").transform;var root=Child(parent,"LandscapeTransitionVisuals");
        foreach(Transform child in root.transform.Cast<Transform>().ToArray())if(child.name.StartsWith("Wave3_"))Undo.DestroyObjectImmediate(child.gameObject);
        var slots=JsonUtility.FromJson<Slots>(File.ReadAllText(Settings+"LandscapeTransitionSlots.json"));var all=Sprites();
        for(int i=0;i<slots.slots.Length;i++)
        {
            var slot=slots.slots[i];string prefix=slot.kind=="Rock"?"Deg_Rock_":slot.kind=="Ichu"?"Deg_Grass_":"Deg_Shrub_";
            var choices=all.Where(x=>x.name.StartsWith(prefix)).ToArray();var sprite=choices[i%choices.Length];
            // Slots lie just behind the boundary; small props leave visible gaps and add no collision.
            if(Vector2.Distance(slot.worldPosition,new Vector2(.47924554f,3.85f))<1.3f)continue;
            Place(root.transform,new Placement{sprite=sprite,position=slot.worldPosition,scale=(i%3==0?.65f:.5f)/sprite.bounds.size.y,flip=i%2==0},"Wave3_Transition_"+i);
            root.transform.GetChild(root.transform.childCount-1).GetComponent<SpriteRenderer>().sortingLayerName="Midground";
            root.transform.GetChild(root.transform.childCount-1).GetComponent<SpriteRenderer>().sortingOrder=1;
        }
    }
    private static void Clear()
    {
        Snapshot(ScenePath);var sector=Sector();var root=sector.Find("GeneratedEnvironment/Degraded");
        if(root!=null)foreach(Transform child in root.Cast<Transform>().ToArray())if(child.name.StartsWith("Wave3_"))Undo.DestroyObjectImmediate(child.gameObject);
        EditorSceneManager.MarkSceneDirty(sector.gameObject.scene);
    }
    public static void ExecuteWave3()
    {
        EditorSceneManager.OpenScene(ScenePath);Generate();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        const string menuPath="Assets/_BROTE/Scenes/UI/MainMenu.unity";Snapshot(menuPath);Directory.CreateDirectory(Path.GetDirectoryName(menuPath));
        var menu=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var go=new GameObject("MainMenu");var component=go.AddComponent<BroteMainMenu>();
        var sprite=AssetDatabase.FindAssets("Acopalca_DistantLandscape t:Sprite").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<Sprite>).FirstOrDefault(x=>x!=null);
        var so=new SerializedObject(component);so.FindProperty("landscape").objectReferenceValue=sprite;so.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.SaveScene(menu,menuPath);Snapshot("ProjectSettings/EditorBuildSettings.asset");
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(menuPath,true),new EditorBuildSettingsScene(ScenePath,true)};
        File.WriteAllText(".brote-work/wave-3/editor-generation.txt","Generated Acopalca props/transition/IDs/session and MainMenu; no Play claimed.");
    }
}
