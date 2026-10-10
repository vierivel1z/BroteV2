using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class BrotePathPainter : EditorWindow
{
    private PathDefinition definition;
    private bool preview=true;
    private Sprite overlaySprite;
    private string report="";
    [MenuItem("BROTE/Wave 3/Path Preview and Painter")]
    private static void Open()=>GetWindow<BrotePathPainter>("BROTE Path");
    private void OnEnable(){definition=JsonUtility.FromJson<PathDefinition>(File.ReadAllText("Assets/_BROTE/Settings/Sector01PathDefinition.json"));SceneView.duringSceneGui+=Draw;}
    private void OnDisable()=>SceneView.duringSceneGui-=Draw;
    private void OnGUI()
    {
        preview=EditorGUILayout.Toggle("Preview",preview);overlaySprite=(Sprite)EditorGUILayout.ObjectField("Path_Acopalca overlay",overlaySprite,typeof(Sprite),false);
        EditorGUILayout.HelpBox("WAITING_ART. No Ground tiles. Original route proposals retain conflicts until reviewed.",MessageType.Info);
        if(GUILayout.Button("Validate clearance"))report=string.Join("\n",Validate(definition));
        EditorGUILayout.HelpBox(report,MessageType.Info);
        if(GUILayout.Button("Paint"))Paint();if(GUILayout.Button("Clear Generated Path"))Clear();SceneView.RepaintAll();
    }
    public static List<string> Validate(PathDefinition data)
    {
        var report=new List<string>();if(data?.routes==null){report.Add("Missing routes");return report;}
        foreach(var route in data.routes)
        {
            if(route.points==null||route.points.Length<2||route.width<=0){report.Add(route.name+": invalid route");continue;}
            float half=route.width*.5f;
            for(int i=1;i<route.points.Length;i++)
            {
                var a=route.points[i-1];var b=route.points[i];
                foreach(var spot in UnityEngine.Object.FindObjectsByType<PlantingSpot>())if(BroteRestorationPropPlacer.Distance(spot.transform.position,a,b)<half+.7f)report.Add(route.name+" crosses "+spot.name);
                foreach(var collider in UnityEngine.Object.FindObjectsByType<Collider2D>())
                {
                    if(collider.isTrigger||collider.GetComponent<PlayerMovement>()!=null)continue;
                    if(collider is EdgeCollider2D edge && edge.name=="ScenicBoundary")
                    {
                        var p=edge.points.Select(x=>(Vector2)edge.transform.TransformPoint(x+edge.offset)).ToArray();
                        for(int k=1;k<p.Length;k++)if(SegmentsNear(a,b,p[k-1],p[k],half+edge.edgeRadius))report.Add(route.name+" crosses ScenicBoundary");
                    }
                    else if(collider is BoxCollider2D)
                    {
                        var bounds=collider.bounds;bounds.Expand(half*2);for(int sample=0;sample<=40;sample++)if(bounds.Contains(Vector2.Lerp(a,b,sample/40f))){report.Add(route.name+" crosses "+collider.name);break;}
                    }
                }
            }
            if(route.points.Any(p=>p.x-half<-19f||p.x+half>19f||p.y-half<-14f||p.y+half>14f))report.Add(route.name+" outside SectorBounds");
        }
        return report.Distinct().ToList();
    }
    private static bool SegmentsNear(Vector2 a,Vector2 b,Vector2 c,Vector2 d,float radius)
    {
        float Cross(Vector2 x,Vector2 y)=>x.x*y.y-x.y*y.x;
        float ca=Cross(b-a,c-a),da=Cross(b-a,d-a),ac=Cross(d-c,a-c),bc=Cross(d-c,b-c);
        return (ca*da<=0&&ac*bc<=0 && Mathf.Max(Mathf.Min(a.x,b.x),Mathf.Min(c.x,d.x))<=Mathf.Min(Mathf.Max(a.x,b.x),Mathf.Max(c.x,d.x)) && Mathf.Max(Mathf.Min(a.y,b.y),Mathf.Min(c.y,d.y))<=Mathf.Min(Mathf.Max(a.y,b.y),Mathf.Max(c.y,d.y)))||Mathf.Min(Mathf.Min(BroteRestorationPropPlacer.Distance(a,c,d),BroteRestorationPropPlacer.Distance(b,c,d)),Mathf.Min(BroteRestorationPropPlacer.Distance(c,a,b),BroteRestorationPropPlacer.Distance(d,a,b)))<radius;
    }
    private void Draw(SceneView view)
    {
        if(!preview||definition?.routes==null)return;Handles.color=new Color(.85f,.65f,.3f,.8f);
        foreach(var route in definition.routes)for(int i=1;i<route.points.Length;i++)
        {Vector2 a=route.points[i-1],b=route.points[i];Vector2 n=new Vector2(-(b-a).y,(b-a).x).normalized*route.width*.5f;Handles.DrawLine(a+n,b+n);Handles.DrawLine(a-n,b-n);Handles.DrawLine(a,b);}
    }
    private void Paint()
    {
        if(EditorApplication.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != BroteRestorationPropPlacer.ScenePath) { report="Open Acopalca in Edit Mode";return; }
        if(overlaySprite==null||!AssetDatabase.GetAssetPath(overlaySprite).Contains("/Paths/Path_Acopalca")){report="WAITING_ART: assign approved Path_Acopalca sprite";return;}
        var errors=Validate(definition);if(errors.Count>0){report=string.Join("\n",errors);return;}
        BroteRestorationPropPlacer.Snapshot(BroteRestorationPropPlacer.ScenePath);Clear();var sector=UnityEngine.Object.FindObjectsByType<Transform>().First(x=>x.name=="Sector_01");
        var root=new GameObject("GeneratedPath");root.transform.SetParent(sector,false);Undo.RegisterCreatedObjectUndo(root,"Path paint");
        foreach(var route in definition.routes)for(int i=1;i<route.points.Length;i++)
        {
            Vector2 a=route.points[i-1],b=route.points[i];var go=new GameObject("PathOverlay");go.transform.SetParent(root.transform,false);go.transform.position=(a+b)*.5f;
            go.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2((b-a).y,(b-a).x)*Mathf.Rad2Deg);
            go.transform.localScale=new Vector3(Vector2.Distance(a,b)/overlaySprite.bounds.size.x,route.width/overlaySprite.bounds.size.y,1);
            var sr=go.AddComponent<SpriteRenderer>();sr.sprite=overlaySprite;sr.sortingLayerName="Ground";sr.sortingOrder=5;
        }
        EditorSceneManager.MarkSceneDirty(sector.gameObject.scene);
    }
    private static void Clear()
    {
        if(EditorApplication.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!=BroteRestorationPropPlacer.ScenePath)return;
        BroteRestorationPropPlacer.Snapshot(BroteRestorationPropPlacer.ScenePath);
        var root=UnityEngine.Object.FindObjectsByType<Transform>().FirstOrDefault(x=>x.name=="GeneratedPath"&&x.parent!=null&&x.parent.name=="Sector_01");
        if(root!=null)Undo.DestroyObjectImmediate(root.gameObject);
    }
}
