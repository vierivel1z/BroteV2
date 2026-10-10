using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

// Also repairs the scene already open when the corrected assets are imported.
[InitializeOnLoad]
public static class BroteTilemapDiagnostics
{
    static BroteTilemapDiagnostics() => EditorApplication.delayCall += RepairOpenScene;

    public static void EnsureGroundLighting(UnityEngine.SceneManagement.Scene scene)
    {
        int groundLayer = SortingLayer.NameToID("Ground");
        foreach (var light in Object.FindObjectsByType<Light2D>(FindObjectsInactive.Include))
        {
            if (light.gameObject.scene != scene || light.lightType != Light2D.LightType.Global) continue;
            var data = new SerializedObject(light);
            var layers = data.FindProperty("m_ApplyToSortingLayers");
            if (layers == null || !layers.isArray) continue;
            bool includesGround = false;
            for (int i = 0; i < layers.arraySize; i++)
                includesGround |= layers.GetArrayElementAtIndex(i).intValue == groundLayer;
            if (includesGround) continue;
            Undo.RecordObject(light, "Illuminate BROTE ground");
            int index = layers.arraySize;
            layers.arraySize++;
            layers.GetArrayElementAtIndex(index).intValue = groundLayer;
            data.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(scene);
        }
    }

    [MenuItem("BROTE/Ground/Repair Tilemap Visibility")]
    public static void RepairOpenScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        foreach (var map in Object.FindObjectsByType<Tilemap>(FindObjectsInactive.Include))
        {
            if (map.name != "GroundTilemap" || !map.gameObject.scene.path.EndsWith("/Acopalca.unity")) continue;
            EnsureGroundLighting(map.gameObject.scene);
            map.RefreshAllTiles();
        }
        SceneView.RepaintAll();
        EditorApplication.QueuePlayerLoopUpdate();
        Report();
    }

    [MenuItem("BROTE/Ground/Diagnose Visible Tilemaps")]
    public static void Report()
    {
        var report = new StringBuilder();
        foreach (var map in Object.FindObjectsByType<Tilemap>(FindObjectsInactive.Include))
        {
            int tiles = 0, sprites = 0;
            foreach (var cell in map.cellBounds.allPositionsWithin)
            {
                if (map.HasTile(cell)) tiles++;
                if (map.GetSprite(cell) != null) sprites++;
            }
            var renderer = map.GetComponent<TilemapRenderer>();
            report.AppendLine($"{map.name}: tiles={tiles}, sprites={sprites}, active={map.gameObject.activeInHierarchy}, color={map.color}, renderer={renderer.enabled}, material={renderer.sharedMaterial}");
        }
        foreach (var light in Object.FindObjectsByType<Light2D>())
        {
            var layers = new SerializedObject(light).FindProperty("m_ApplyToSortingLayers");
            report.Append($"Light {light.name} layers:");
            for (int i = 0; i < layers.arraySize; i++) report.Append(" " + layers.GetArrayElementAtIndex(i).intValue);
            report.AppendLine();
        }
        Directory.CreateDirectory(".brote-work/tilemap-fix");
        File.WriteAllText(".brote-work/tilemap-fix/live-diagnostic.txt", report.ToString());
    }
}
