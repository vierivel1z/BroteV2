using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static partial class BroteProjectValidator
{
    [MenuItem("BROTE/Validate/Scenic Area")]
    public static void ScenicMenu() => Publish(ValidateScenicArea(SceneManager.GetActiveScene()));
    public static Report ValidateScenicArea(Scene scene)
    {
        var report = new Report(); EdgeCollider2D boundary = null;
        foreach (var edge in SceneComponents<EdgeCollider2D>(scene)) if (edge.name == "ScenicBoundary") boundary = edge;
        if (boundary == null) { report.Error("ScenicBoundary missing in loaded scene."); return report; }
        if (!boundary.enabled || !boundary.gameObject.activeInHierarchy || boundary.isTrigger) report.Error("ScenicBoundary must be active and solid.");
        if (boundary.pointCount != 10) report.Warning("ScenicBoundary differs from the approved ten-point shape.");
        if (boundary.sharedMaterial == null || boundary.sharedMaterial.friction != 0 || boundary.sharedMaterial.bounciness != 0) report.Error("ScenicBoundary requires zero friction/bounce material.");
        var world = WorldPoints(boundary);
        foreach (var spot in SceneComponents<PlantingSpot>(scene))
        {
            if (!GameplaySide(spot.transform.position, world)) report.Error(spot.name + ": center outside scenic gameplay side.");
            var circle = spot.GetComponent<CircleCollider2D>();
            if (circle != null)
            {
                Vector2 center = circle.transform.TransformPoint(circle.offset);
                float radius = circle.radius * Mathf.Max(Mathf.Abs(circle.transform.lossyScale.x),Mathf.Abs(circle.transform.lossyScale.y));
                float clearance = PolylineDistance(center,world) - radius - boundary.edgeRadius;
                if (clearance < 0f) report.Warning(spot.name+": trigger crosses scenic boundary by approximately "+(-clearance).ToString("F3")+"u; PLAY_REQUIRED, do not move automatically.");
                else report.Info(spot.name+": trigger clearance "+clearance.ToString("F3")+"u.");
            }
        }
        foreach (var source in SceneComponents<NurserySeedlingSource>(scene))
            if (!GameplaySide(source.transform.position,world)) report.Error("Nursery interaction outside scenic gameplay side.");
        foreach (var player in SceneComponents<PlayerMovement>(scene))
            if (!GameplaySide(player.transform.position,world)) report.Error("Player initial position outside scenic gameplay side.");
        report.Info("ScenicBoundary world points: "+string.Join(", ",Array.ConvertAll(world,p=>p.ToString("F2"))));
        return report;
    }
    public static Vector2[] WorldPoints(EdgeCollider2D edge)
    {
        Vector2[] points=edge.points;
        for(int i=0;i<points.Length;i++) points[i]=edge.transform.TransformPoint(points[i]+edge.offset);
        return points;
    }
    public static float PolylineDistance(Vector2 point, Vector2[] line)
    {
        float nearest=float.PositiveInfinity;
        for(int i=1;i<line.Length;i++)
        {
            Vector2 delta=line[i]-line[i-1];float sqr=delta.sqrMagnitude;
            float t=sqr>0f?Mathf.Clamp01(Vector2.Dot(point-line[i-1],delta)/sqr):0f;
            nearest=Mathf.Min(nearest,Vector2.Distance(point,line[i-1]+delta*t));
        }
        return nearest;
    }
    public static bool GameplaySide(Vector2 point,Vector2[] line)
    {
        for(int i=1;i<line.Length;i++)
        {
            Vector2 a=line[i-1],b=line[i];float low=Mathf.Min(a.x,b.x),high=Mathf.Max(a.x,b.x);
            if(point.x<low||point.x>high||Mathf.Abs(b.x-a.x)<0.0001f)continue;
            return point.y <= Mathf.Lerp(a.y,b.y,(point.x-a.x)/(b.x-a.x));
        }
        return false;
    }
    [MenuItem("BROTE/Validate Sorting")]
    public static void SortingMenu() => Publish(ValidateSorting(SceneManager.GetActiveScene()));
    public static Report ValidateSorting(Scene scene)
    {
        var report=new Report();var layers=new HashSet<string>();
        foreach(var layer in SortingLayer.layers)layers.Add(layer.name);
        foreach(string name in new[]{"Background","Ground","Midground","Default","WorldObjects","UI"})
            if(!layers.Contains(name))report.Error("Sorting layer missing: "+name);
        foreach(var renderer in SceneComponents<Renderer>(scene))
        {
            var group=renderer.GetComponentInParent<SortingGroup>();
            report.Info(PathOf(renderer.transform)+": layer="+renderer.sortingLayerName+" order="+renderer.sortingOrder+" group="+(group!=null?group.name:"none"));
            if(!layers.Contains(renderer.sortingLayerName))report.Warning(renderer.name+": unrecognized sorting layer.");
        }
        foreach(var sorter in SceneComponents<YSort2D>(scene))
            report.Info("YSort2D: "+PathOf(sorter.transform)+" y="+sorter.transform.position.y.ToString("F3"));
        foreach(var group in SceneComponents<SortingGroup>(scene))
            report.Info("SortingGroup: "+PathOf(group.transform)+" layer="+group.sortingLayerName+" order="+group.sortingOrder);
        return report;
    }
    public static Report ValidateMissingAndDuplicates(GameObject root)
    {
        var report=new Report();
        foreach(var t in root.GetComponentsInChildren<Transform>(true))
        {
            int missing=GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
            if(missing>0)report.Error(PathOf(t)+": "+missing+" missing scripts.");
            var types=new Dictionary<Type,int>();
            foreach(var component in t.GetComponents<Component>())
            {
                if(component==null)continue;
                var type=component.GetType();types.TryGetValue(type,out int count);types[type]=count+1;
                var data=new SerializedObject(component);var field=data.GetIterator();
                while(field.NextVisible(true))
                    if(field.propertyType==SerializedPropertyType.ObjectReference&&field.objectReferenceValue==null&&!field.objectReferenceEntityIdValue.Equals(default(EntityId)))
                        report.Error(PathOf(t)+": missing reference "+field.propertyPath);
            }
            foreach(var entry in types)if(entry.Value>1)report.Warning(PathOf(t)+": duplicate "+entry.Key.Name+" x"+entry.Value+" (report only).");
        }
        return report;
    }
    [MenuItem("BROTE/Validate/Scene")]
    public static void SceneMenu() => Publish(ValidateScene(SceneManager.GetActiveScene()));
    public static Report ValidateScene(Scene scene)
    {
        var report=new Report();report.Info("Loaded scene: "+scene.path);
        if(!scene.IsValid()||!scene.isLoaded){report.Error("No loaded scene.");return report;}
        int players=0,spots=0,nurseries=0,managers=0,grounds=0;
        foreach(var player in SceneComponents<PlayerMovement>(scene))
        {
            players++;
            if(player.GetComponent<Rigidbody2D>()==null||player.GetComponent<Animator>()==null||player.GetComponent<SeedlingInventory>()==null||player.GetComponent<PlayerInteraction>()==null)
                report.Error("Player core component missing.");
        }
        foreach(var spot in SceneComponents<PlantingSpot>(scene)){spots++;Merge(report,ValidateSpot(spot.gameObject,true));}
        foreach(var source in SceneComponents<NurserySeedlingSource>(scene))
        {nurseries++;var group=source.GetComponentInParent<SortingGroup>();Merge(report,ValidateNursery(group!=null?group.gameObject:source.gameObject));}
        foreach(var manager in SceneComponents<RestorationManager>(scene)){managers++;if(manager.MaxRestoration!=100)report.Warning("Restoration max differs from 100.");}
        foreach(var controller in SceneComponents<GroundRestorationController>(scene))
        {
            grounds++;var data=new SerializedObject(controller);var map=data.FindProperty("groundTilemap").objectReferenceValue as Tilemap;
            if(map==null)report.Error("Ground reference missing.");
            else{int cells=0;foreach(var cell in map.cellBounds.allPositionsWithin)if(map.HasTile(cell))cells++;report.Info("Ground painted cells="+cells);if(cells!=1200)report.Warning("Ground differs from 40x30.");}
            foreach(string bank in new[]{"degradedTiles","recoveryTiles","restoredTiles"})
            {var array=data.FindProperty(bank);if(array==null||array.arraySize!=48)report.Error("Ground bank invalid: "+bank);else for(int i=0;i<array.arraySize;i++)if(array.GetArrayElementAtIndex(i).objectReferenceValue==null)report.Error(bank+" missing tile "+i);}
        }
        if(players!=1||spots!=10||nurseries!=1||managers!=1||grounds!=1)report.Error("Expected 1 player/10 spots/1 nursery/1 manager/1 ground; observed "+players+"/"+spots+"/"+nurseries+"/"+managers+"/"+grounds);
        foreach(var follow in SceneComponents<CameraFollow2D>(scene))
        {
            var cam=follow.GetComponent<Camera>();var data=new SerializedObject(follow);
            if(data.FindProperty("target").objectReferenceValue==null)report.Error("Camera target missing.");
            report.Info("Camera: size="+cam.orthographicSize+" offset="+data.FindProperty("framingOffset").vector2Value+" center bounds="+follow.CalculateCenterBounds(cam.aspect,cam.orthographicSize));
        }
        foreach(var root in scene.GetRootGameObjects())Merge(report,ValidateMissingAndDuplicates(root));
        Merge(report,ValidateScenicArea(scene));Merge(report,ValidateSorting(scene));Merge(report,ValidateSectorBounds(scene));
        return report;
    }
    public static Report ValidateSectorBounds(Scene scene)
    {
        var report=new Report(); float left=float.NegativeInfinity,right=float.PositiveInfinity,bottom=float.NegativeInfinity,top=float.PositiveInfinity; int count=0;
        foreach(var box in SceneComponents<BoxCollider2D>(scene))
        {
            if(box.transform.parent==null||box.transform.parent.name!="SectorBounds")continue;
            count++;var extent=box.bounds;
            switch(box.name){case "West":left=extent.max.x;break;case "East":right=extent.min.x;break;case "South":bottom=extent.max.y;break;case "North":top=extent.min.y;break;}
            if(!box.enabled||box.isTrigger||!box.gameObject.activeInHierarchy)report.Error(box.name+": perimeter must be active and solid.");
        }
        if(count!=4||float.IsInfinity(left)||float.IsInfinity(right)||float.IsInfinity(bottom)||float.IsInfinity(top)||left>=right||bottom>=top){report.Error("SectorBounds requires four named solid sides.");return report;}
        var inside=Rect.MinMaxRect(left,bottom,right,top);
        foreach(var spot in SceneComponents<PlantingSpot>(scene))if(!inside.Contains(spot.transform.position))report.Warning(spot.name+": outside sector inner bounds.");
        foreach(var source in SceneComponents<NurserySeedlingSource>(scene))if(!inside.Contains(source.transform.position))report.Warning(source.name+": outside sector inner bounds.");
        foreach(var player in SceneComponents<PlayerMovement>(scene))if(!inside.Contains(player.transform.position))report.Error("Player outside sector inner bounds.");
        foreach(var tree in SceneComponents<QuenualGrowth>(scene))if(!inside.Contains(tree.transform.position))report.Warning(tree.name+": outside sector inner bounds.");
        foreach(var node in SceneComponents<Transform>(scene))
        {
            bool gameplay=false;for(var ancestor=node;ancestor!=null;ancestor=ancestor.parent)if(ancestor.name=="Gameplay"){gameplay=true;break;}
            if(!gameplay||(node.GetComponent<Renderer>()==null&&node.GetComponent<Collider2D>()==null))continue;
            if(!inside.Contains(node.position))report.Warning(PathOf(node)+": rendered/collider object center outside sector inner bounds (report only).");
        }
        report.Info("Sector inner bounds="+inside+"; decorative landscape/background intentionally exempt.");
        return report;
    }
    [MenuItem("BROTE/Validate/Full Project")]
    public static void FullMenu()
    {
        var report=ValidateScene(SceneManager.GetActiveScene());
        foreach(string guid in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/_BROTE"}))
        {var root=AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));if(root!=null)Merge(report,ValidateMissingAndDuplicates(root));}
        Merge(report,ValidateTree(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_BROTE/Prefabs/Plants/QuenualPlaceholder.prefab")));
        Merge(report,ValidateSpot(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_BROTE/Prefabs/Interactables/PlantingSpot.prefab"),false));
        Merge(report,ValidateNursery(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_BROTE/Prefabs/Environment/Nursery/Nursery_Acopalpa.prefab")));
        Merge(report,ValidateArt());
        Publish(report);
    }
    [MenuItem("BROTE/Validate/Export Last Report")]
    public static void Export()
    {
        if(LastReport==null)FullMenu();
        const string path="Assets/_BROTE/Documentation/ValidationReport.md";
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));File.WriteAllText(path,LastReport.Markdown());AssetDatabase.ImportAsset(path);
    }
}
