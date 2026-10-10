using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public sealed class BroteQuickStatus : EditorWindow
{
    private Vector2 scroll;
    [MenuItem("BROTE/Quick Status")]
    public static void Open() => GetWindow<BroteQuickStatus>("BROTE Status");
    private void OnGUI()
    {
        var scene = SceneManager.GetActiveScene();
        EditorGUILayout.LabelField("Loaded scene", scene.path);
        int spots=0, players=0, nurseries=0, cells=0;
        foreach(var item in BroteProjectValidator.SceneComponents<PlantingSpot>(scene)) spots++;
        foreach(var item in BroteProjectValidator.SceneComponents<PlayerMovement>(scene)) players++;
        foreach(var item in BroteProjectValidator.SceneComponents<NurserySeedlingSource>(scene)) nurseries++;
        foreach(var map in BroteProjectValidator.SceneComponents<Tilemap>(scene))
            if(map.name=="GroundTilemap") foreach(var cell in map.cellBounds.allPositionsWithin) if(map.HasTile(cell)) cells++;
        EditorGUILayout.LabelField("Spots (expected 10)",spots.ToString());
        EditorGUILayout.LabelField("Player / nursery",players+" / "+nurseries);
        EditorGUILayout.LabelField("Ground painted cells",cells.ToString());
        foreach(var manager in BroteProjectValidator.SceneComponents<RestorationManager>(scene)) EditorGUILayout.LabelField("Restoration maximum",manager.MaxRestoration.ToString());
        foreach(var camera in BroteProjectValidator.SceneComponents<CameraFollow2D>(scene)) EditorGUILayout.LabelField("Camera",camera.name);
        if(GUILayout.Button("Validate Scene")) BroteProjectValidator.SceneMenu();
        if(GUILayout.Button("Validate Prefabs")) BroteProjectValidator.PrefabsMenu();
        if(GUILayout.Button("Validate Art References")) BroteProjectValidator.ArtMenu();
        if(GUILayout.Button("Validate Full Project")) BroteProjectValidator.FullMenu();
        if(GUILayout.Button("Export Last Report")) BroteProjectValidator.Export();
        var report=BroteProjectValidator.LastReport;
        if(report==null) return;
        EditorGUILayout.LabelField("Last validation",report.Errors+" errors / "+report.Warnings+" warnings");
        scroll=EditorGUILayout.BeginScrollView(scroll);
        foreach(string line in report.Lines) EditorGUILayout.LabelField(line,EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndScrollView();
    }
}
