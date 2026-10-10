using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>Composition guide for missing art. Editor only; never paints or spawns objects.</summary>
public sealed class BroteSectorVisualGuide : EditorWindow
{
    private const string PlanPath = "Assets/_BROTE/Settings/Sector01VisualLayout.json";
    [Serializable] private sealed class Route { public string name; public float width; public Vector2[] points; }
    [Serializable] private sealed class Clearing { public string name; public Vector2 center; public float radius; }
    [Serializable] private sealed class Layout { public string scenePath; public Route[] routes; public Clearing[] clearings; public Vector2[] clusters; }
    private Layout layout;
    private bool showRoutes = true, showClearings = true, showClusters = true;

    [MenuItem("BROTE/Visuals/Sector 01 Composition Guide")]
    private static void Open() => GetWindow<BroteSectorVisualGuide>("Sector 01 Layout");
    private void OnEnable()
    {
        Reload();
        SceneView.duringSceneGui += DrawGuide;
        SceneView.RepaintAll();
    }
    private void OnDisable() { SceneView.duringSceneGui -= DrawGuide; SceneView.RepaintAll(); }
    private void Reload()
    {
        var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(PlanPath);
        layout = asset != null ? JsonUtility.FromJson<Layout>(asset.text) : null;
    }
    private void OnGUI()
    {
        EditorGUILayout.HelpBox("Guia solo para Scene View. No aparece en Game y no pinta tiles. Faltan sprites de camino y props naturales.", MessageType.Info);
        if (layout == null) { EditorGUILayout.HelpBox("No se encontro el plan visual.", MessageType.Warning); return; }
        EditorGUI.BeginChangeCheck();
        showRoutes = EditorGUILayout.Toggle("Recorrido propuesto (1.5 u)", showRoutes);
        showClearings = EditorGUILayout.Toggle("Espacios limpios", showClearings);
        showClusters = EditorGUILayout.Toggle("Grupos de props futuros", showClusters);
        if (EditorGUI.EndChangeCheck()) SceneView.RepaintAll();
        if (GUILayout.Button("Seleccionar PathsTilemap")) SelectMap("PathsTilemap");
        if (GUILayout.Button("Seleccionar DecorationTilemap")) SelectMap("DecorationTilemap");
        if (GUILayout.Button("Encuadrar Sector 01") && SceneView.lastActiveSceneView != null)
            SceneView.lastActiveSceneView.Frame(new Bounds(Vector3.zero, new Vector3(40, 30, 1)), false);
        if (GUILayout.Button("Recargar plan")) { Reload(); SceneView.RepaintAll(); }
        EditorGUILayout.HelpBox("Cerrar esta ventana retira la guia. Pending_20/40/50 conserva las referencias de SectorVisualState; sus SpriteRenderer estan ocultos hasta sustituir el arte geometrico.", MessageType.None);
    }
    private void SelectMap(string name)
    {
        var map = UnityEngine.Object.FindObjectsByType<Tilemap>()
            .FirstOrDefault(m => m.name == name && m.gameObject.scene.path == layout.scenePath);
        if (map != null) Selection.activeGameObject = map.gameObject;
    }
    private void DrawGuide(SceneView view)
    {
        if (layout == null || SceneManager.GetActiveScene().path != layout.scenePath || Event.current.type != EventType.Repaint) return;
        using (new Handles.DrawingScope())
        {
            if (showRoutes && layout.routes != null)
            foreach (Route route in layout.routes)
            {
                if (route.points == null || route.points.Length < 2) continue;
                for (int i = 0; i < route.points.Length - 1; i++)
                {
                    Vector3 a = route.points[i], b = route.points[i + 1];
                    Vector3 before = route.points[Mathf.Max(0, i - 1)];
                    Vector3 after = route.points[Mathf.Min(route.points.Length - 1, i + 2)];
                    // Catmull-Rom tangents keep the guide curved without authored path sprites.
                    Handles.DrawBezier(a, b, a + (b - before) / 6f, b - (after - a) / 6f,
                        new Color(0.95f, 0.72f, 0.35f, 0.9f), null, 3f);
                }
                Handles.Label(route.points[route.points.Length / 2], route.name + " [planned]");
            }
            if (showClearings && layout.clearings != null)
            {
                Handles.color = new Color(0.4f, 0.75f, 1f, 0.7f);
                foreach (Clearing clearing in layout.clearings)
                {
                    Handles.DrawWireDisc(clearing.center, Vector3.forward, clearing.radius);
                    Handles.Label(clearing.center, clearing.name);
                }
            }
            if (showClusters && layout.clusters != null)
            {
                Handles.color = new Color(0.6f, 0.85f, 0.45f, 0.85f);
                foreach (Vector2 center in layout.clusters)
                    Handles.DrawWireDisc(center, Vector3.forward, 0.8f);
            }
        }
    }
}
