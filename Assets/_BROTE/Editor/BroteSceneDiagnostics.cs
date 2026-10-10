using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Scene View overlays only. No tile edits, runtime components or generated scene objects.</summary>
public sealed class BroteSceneDiagnostics : EditorWindow
{
    private bool radii, groundStates, scenic, cameras, transition, interactions, nurseryBounds, sectorBounds;
    private float previewPercentage = 50f;
    private GroundRestorationController.GroundState filter;
    [Serializable] private sealed class Slots { public Slot[] slots; }
    [Serializable] private sealed class Slot { public string kind; public Vector2 worldPosition; }
    [MenuItem("BROTE/Debug/Scene Overlays")]
    private static void Open() => GetWindow<BroteSceneDiagnostics>("BROTE Overlays");
    private void OnEnable() { SceneView.duringSceneGui -= Draw; SceneView.duringSceneGui += Draw; }
    private void OnDisable() => SceneView.duringSceneGui -= Draw;
    private void OnGUI()
    {
        EditorGUILayout.HelpBox("Solo Scene View. No pinta tiles ni cambia el estado de restauración. Los radios en Edit Mode son vistas previas.", MessageType.Info);
        EditorGUI.BeginChangeCheck();
        interactions=EditorGUILayout.Toggle("Interaction radii",interactions);
        nurseryBounds=EditorGUILayout.Toggle("Nursery solid/trigger bounds",nurseryBounds);
        sectorBounds=EditorGUILayout.Toggle("SectorBounds boxes",sectorBounds);
        radii = EditorGUILayout.Toggle("Radios de restauración", radii);
        previewPercentage = EditorGUILayout.Slider("% de vista previa", previewPercentage, 0f, 100f);
        groundStates = EditorGUILayout.Toggle("Estados actuales del Ground", groundStates);
        filter = (GroundRestorationController.GroundState)EditorGUILayout.EnumPopup("Estado a destacar", filter);
        scenic = EditorGUILayout.Toggle("ScenicBoundary", scenic);
        cameras = EditorGUILayout.Toggle("Cámara y bounds", cameras);
        transition = EditorGUILayout.Toggle("Slots de transición (datos)", transition);
        if (EditorGUI.EndChangeCheck()) SceneView.RepaintAll();
    }
    private void Draw(SceneView view)
    {
        Scene scene = SceneManager.GetActiveScene();
        if(interactions)
        {
            Handles.color=Color.yellow;
            foreach(var player in BroteProjectValidator.SceneComponents<PlayerInteraction>(scene))
                Handles.DrawWireDisc(player.transform.position,Vector3.forward,new SerializedObject(player).FindProperty("interactionRadius").floatValue);
            Handles.color=new Color(.5f,.9f,.4f);
            foreach(var spot in BroteProjectValidator.SceneComponents<PlantingSpot>(scene))DrawTrigger(spot.GetComponent<CircleCollider2D>());
            foreach(var source in BroteProjectValidator.SceneComponents<NurserySeedlingSource>(scene))DrawTrigger(source.GetComponent<CircleCollider2D>());
        }
        if(nurseryBounds||sectorBounds)
        foreach(var collider in BroteProjectValidator.SceneComponents<Collider2D>(scene))
        {
            bool sector=collider.transform.parent!=null&&collider.transform.parent.name=="SectorBounds";
            bool nursery=collider.GetComponentInParent<UnityEngine.Rendering.SortingGroup>()?.GetComponentInChildren<NurserySeedlingSource>(true)!=null;
            if(!((sector&&sectorBounds)||(nursery&&nurseryBounds)))continue;
            Handles.color=collider.isTrigger?Color.yellow:sector?Color.magenta:Color.cyan;
            if(collider is BoxCollider2D box){var old=Handles.matrix;Handles.matrix=box.transform.localToWorldMatrix;Handles.DrawWireCube(box.offset,box.size);Handles.matrix=old;}
            else DrawTrigger(collider as CircleCollider2D);
        }

        if (radii || groundStates)
        foreach (var controller in BroteProjectValidator.SceneComponents<GroundRestorationController>(scene))
        {
            if (radii)
            {
                float percentage = Application.isPlaying ? GetPercentage(scene) : previewPercentage;
                if (Application.isPlaying)
                {
                    foreach (var tree in BroteProjectValidator.SceneComponents<QuenualGrowth>(scene))
                        if (tree.Stage == 2) DrawRadii(tree.transform.position, controller, percentage);
                }
                else foreach (var spot in BroteProjectValidator.SceneComponents<PlantingSpot>(scene))
                    DrawRadii(spot.transform.position, controller, percentage);
            }
            if (groundStates)
            {
                var data = new SerializedObject(controller);
                var map = data.FindProperty("groundTilemap").objectReferenceValue as UnityEngine.Tilemaps.Tilemap;
                if (map == null) continue;
                Handles.color = filter == GroundRestorationController.GroundState.Degraded ? new Color(0.8f, 0.45f, 0.2f, 0.5f)
                    : filter == GroundRestorationController.GroundState.Recovery ? new Color(0.8f, 0.8f, 0.2f, 0.5f) : new Color(0.3f, 0.85f, 0.4f, 0.5f);
                foreach (Vector3Int cell in map.cellBounds.allPositionsWithin)
                {
                    if (!map.HasTile(cell)) continue;
                    var state = controller.CellStates.Count > 0 ? controller.GetCellState(cell) : SerializedState(map.GetTile(cell), data);
                    if (state == filter) Handles.DrawWireCube(map.GetCellCenterWorld(cell), map.cellSize * 0.9f);
                }
            }
        }
        if (scenic)
        foreach (var edge in BroteProjectValidator.SceneComponents<EdgeCollider2D>(scene))
        {
            if (edge.name != "ScenicBoundary") continue;
            var points = edge.points; var world = new Vector3[points.Length];
            for (int i = 0; i < points.Length; i++) world[i] = edge.transform.TransformPoint(points[i] + edge.offset);
            Handles.color = edge.enabled ? new Color(0.1f, 0.95f, 0.75f) : Color.red;
            Handles.DrawAAPolyLine(3f, world);
            foreach (var point in world) Handles.Label(point, point.ToString("F2"));
        }
        if (cameras)
        foreach (var follow in BroteProjectValidator.SceneComponents<CameraFollow2D>(scene))
        {
            var cam = follow.GetComponent<Camera>(); var d = new SerializedObject(follow);
            Rect bounds = follow.CalculateCenterBounds(cam.aspect, cam.orthographicSize, cam.orthographic);
            float minX = bounds.xMin, maxX = bounds.xMax, minY = bounds.yMin, maxY = bounds.yMax;
            Handles.color = Color.cyan;
            Handles.DrawWireCube(new Vector3((minX+maxX)*0.5f,(minY+maxY)*0.5f,0),new Vector3(maxX-minX,maxY-minY,0));
            Handles.color = Color.white;
            Handles.DrawWireCube(new Vector3(cam.transform.position.x,cam.transform.position.y,0),new Vector3(cam.orthographicSize*cam.aspect*2,cam.orthographicSize*2,0));
            var target = d.FindProperty("target").objectReferenceValue as Transform;
            if (target != null)
            {
                Vector2 offset = d.FindProperty("framingOffset").vector2Value;
                Handles.color = Color.yellow; Handles.DrawLine(target.position,target.position+(Vector3)offset);
                Handles.Label(target.position+(Vector3)offset,"framingOffset " + offset);
            }
        }
        if (transition)
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/_BROTE/Settings/LandscapeTransitionSlots.json");
            var slots = asset != null ? JsonUtility.FromJson<Slots>(asset.text) : null;
            if (slots?.slots != null) foreach (var slot in slots.slots)
            {
                Handles.color = new Color(0.6f,0.8f,0.95f);
                Handles.DrawWireDisc(slot.worldPosition,Vector3.forward,0.2f);
                Handles.Label(slot.worldPosition,slot.kind+" (proposal)");
            }
        }
    }
    private static void DrawTrigger(CircleCollider2D circle)
    {
        if(circle==null)return;
        Handles.DrawWireDisc(circle.transform.TransformPoint(circle.offset),Vector3.forward,circle.radius*Mathf.Max(Mathf.Abs(circle.transform.lossyScale.x),Mathf.Abs(circle.transform.lossyScale.y)));
    }
    private static void DrawRadii(Vector3 center, GroundRestorationController controller, float percentage)
    {
        Handles.color = new Color(0.85f, 0.74f, 0.3f, 0.6f);
        Handles.DrawWireDisc(center, Vector3.forward, controller.RadiusAt(percentage));
        Handles.color = new Color(0.3f, 0.85f, 0.45f, 0.55f);
        Handles.DrawWireDisc(center, Vector3.forward, controller.RestoredRadiusAt(percentage));
    }
    private static float GetPercentage(Scene scene)
    {
        foreach (var manager in BroteProjectValidator.SceneComponents<RestorationManager>(scene)) return manager.RestorationPercentage;
        return 0f;
    }
    private static GroundRestorationController.GroundState SerializedState(UnityEngine.Tilemaps.TileBase tile, SerializedObject controller)
    {
        foreach (string field in new[] { "restoredTiles", "recoveryTiles" })
        {
            var bank = controller.FindProperty(field);
            if (bank == null || !bank.isArray) continue;
            for (int i = 0; i < bank.arraySize; i++)
                if (bank.GetArrayElementAtIndex(i).objectReferenceValue == tile)
                    return field == "restoredTiles" ? GroundRestorationController.GroundState.Restored : GroundRestorationController.GroundState.Recovery;
        }
        return GroundRestorationController.GroundState.Degraded;
    }
}
