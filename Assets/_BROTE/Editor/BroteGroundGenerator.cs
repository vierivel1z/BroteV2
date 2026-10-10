using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class BroteGroundGenerator
{
    public const int Seed = 2026;
    const string Root = "Assets/_BROTE/";
    const string SpotPrefab = Root + "Prefabs/Interactables/PlantingSpot.prefab";
    static readonly string[] States = { "Degraded", "Recovery", "Restored" };
    static string Texture(int i) => Root + "Art/Environment/Tiles/Ground/" + States[i] + "/" +
        (i == 0 ? "Ground_Degraded_Tileset" : "Ground_" + States[i]) + ".png";
    public static void Folder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        Folder(Path.GetDirectoryName(path).Replace('\\', '/'));
        AssetDatabase.CreateFolder(Path.GetDirectoryName(path).Replace('\\', '/'), Path.GetFileName(path));
    }
    [MenuItem("BROTE/Ground/Generate Sector 01")]
    public static void Generate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Generate only in Edit Mode.");
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (!scene.path.EndsWith("/Acopalca.unity")) throw new InvalidOperationException("Open Acopalca first.");
        var sector = GameObject.Find("Sector_01").transform;
        var map = UnityEngine.Object.FindObjectsByType<Tilemap>().Single(m => m.name == "GroundTilemap");
        var manager = UnityEngine.Object.FindAnyObjectByType<RestorationManager>();
        var inventory = UnityEngine.Object.FindAnyObjectByType<SeedlingInventory>();
        var spotRoot = sector.Find("PlantingSpots");
        var spots = spotRoot.GetComponentsInChildren<PlantingSpot>().OrderBy(s => s.name).ToList();
        var positions = Positions(sector, map);
        Tile[][] tiles = ImportTiles();
        if (AssetDatabase.LoadAssetAtPath<GameObject>(SpotPrefab) == null)
        {
            if (spots.Count == 0) throw new InvalidOperationException("PlantingSpot template missing.");
            Folder(Path.GetDirectoryName(SpotPrefab).Replace('\\', '/'));
            var copy = UnityEngine.Object.Instantiate(spots[0].gameObject);
            copy.name = "PlantingSpot";
            var data = new SerializedObject(copy.GetComponent<PlantingSpot>());
            data.FindProperty("restorationManager").objectReferenceValue = null;
            data.FindProperty("promptInventory").objectReferenceValue = null;
            data.FindProperty("plantingPoint").objectReferenceValue = copy.transform;
            data.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(copy, SpotPrefab);
            UnityEngine.Object.DestroyImmediate(copy);
        }
        Undo.RegisterCompleteObjectUndo(map, "Generate BROTE ground");
        map.ClearAllTiles();
        Vector3Int center = map.WorldToCell(sector.position);
        for (int y = -15; y < 15; y++)
        for (int x = -20; x < 20; x++)
        {
            Vector3Int cell = center + new Vector3Int(x, y, 0);
            map.SetTile(cell, tiles[0][GroundTileSelection.SelectIndex(cell.x, cell.y, Seed, 0)]);
        }
        map.CompressBounds();
        var renderer = map.GetComponent<TilemapRenderer>();
        Undo.RecordObject(renderer, "Ground render order");
        renderer.sortingLayerName = "Ground"; renderer.sortingOrder = -50;
        renderer.mode = TilemapRenderer.Mode.Chunk;
        BroteTilemapDiagnostics.EnsureGroundLighting(scene);
        map.RefreshAllTiles();
        var variants = AssetDatabase.LoadAllAssetsAtPath(Root + "Art/Environment/PlantingSpots/PlantingSpots_Empty.png")
            .OfType<Sprite>().OrderBy(s => s.name).ToArray();
        if (variants.Length != 6) throw new InvalidOperationException("Expected six spot sprites.");
        while (spots.Count > 10) { Undo.DestroyObjectImmediate(spots.Last().gameObject); spots.RemoveAt(spots.Count - 1); }
        while (spots.Count < 10)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(SpotPrefab), spotRoot);
            Undo.RegisterCreatedObjectUndo(go, "Create planting spot");
            spots.Add(go.GetComponent<PlantingSpot>());
        }
        for (int i = 0; i < 10; i++)
        {
            var spot = spots[i];
            Undo.RecordObject(spot.transform, "Position planting spot");
            spot.name = "PlantingSpot_" + (i + 1).ToString("D2");
            spot.transform.position = positions[i];
            var sr = spot.GetComponent<SpriteRenderer>();
            Undo.RecordObject(sr, "Spot visual");
            sr.sprite = variants[(i * 5 + 2) % 6]; sr.sortingLayerName = "Default"; sr.sortingOrder = -1;
            foreach (var sort in spot.GetComponents<YSort2D>()) Undo.DestroyObjectImmediate(sort);
            var data = new SerializedObject(spot);
            data.FindProperty("restorationManager").objectReferenceValue = manager;
            data.FindProperty("promptInventory").objectReferenceValue = inventory;
            data.FindProperty("plantingPoint").objectReferenceValue = spot.transform;
            data.ApplyModifiedProperties();
        }
        var host = sector.Find("GroundRestoration");
        if (host == null)
        {
            var go = new GameObject("GroundRestoration"); Undo.RegisterCreatedObjectUndo(go, "Ground restoration");
            go.transform.SetParent(sector, false); host = go.transform;
        }
        var controller = host.GetComponent<GroundRestorationController>();
        if (controller == null) controller = Undo.AddComponent<GroundRestorationController>(host.gameObject);
        var settings = new SerializedObject(controller);
        settings.FindProperty("groundTilemap").objectReferenceValue = map;
        settings.FindProperty("restorationManager").objectReferenceValue = manager;
        settings.FindProperty("sectorRoot").objectReferenceValue = sector;
        settings.FindProperty("seed").intValue = Seed;
        string[] names = { "degradedTiles", "recoveryTiles", "restoredTiles" };
        for (int state = 0; state < 3; state++)
        {
            var array = settings.FindProperty(names[state]); array.arraySize = 48;
            for (int i = 0; i < 48; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = tiles[state][i];
        }
        settings.ApplyModifiedProperties();
        RepaintGround();
        var player = inventory.gameObject;
        if (player.GetComponent<YSort2D>() == null) Undo.AddComponent<YSort2D>(player);
        Undo.RecordObject(player.GetComponent<SpriteRenderer>(), "Player render layer");
        player.GetComponent<SpriteRenderer>().sortingLayerName = "WorldObjects";
        var camera = UnityEngine.Object.FindAnyObjectByType<CameraFollow2D>();
        var cameraData = new SerializedObject(camera);
        cameraData.FindProperty("useBounds").boolValue = true;
        cameraData.FindProperty("minX").floatValue = sector.position.x - 11;
        cameraData.FindProperty("maxX").floatValue = sector.position.x + 11;
        cameraData.FindProperty("minY").floatValue = sector.position.y - 10;
        cameraData.FindProperty("maxY").floatValue = sector.position.y + 10;
        cameraData.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
    }
    static List<Vector3> Positions(Transform sector, Tilemap map)
    {
        var result = new List<Vector3>();
        var random = new System.Random(Seed);
        var nursery = sector.Find("NurseryArea");
        var bounds = nursery.GetComponentInChildren<SpriteRenderer>().bounds;
        bounds.Expand(new Vector3(2, 3, 0));
        Physics2D.SyncTransforms();
        for (int attempt = 0; result.Count < 10 && attempt < 10000; attempt++)
        {
            var p = sector.position + new Vector3((float)random.NextDouble() * 22 - 11, (float)random.NextDouble() * 16 - 8, 0);
            if (bounds.Contains(new Vector3(p.x, p.y, bounds.center.z))) continue;
            if (result.Any(q => Vector2.Distance(p, q) < 3.1f)) continue;
            if (Physics2D.OverlapCircleAll(p, .8f).Any(c => !c.isTrigger && c.GetComponentInParent<PlayerMovement>() == null)) continue;
            if (UnityEngine.Object.FindObjectsByType<QuenualGrowth>().Any(t => Vector2.Distance(p, t.transform.position) < 2.5f)) continue;
            result.Add(p);
        }
        if (result.Count != 10) throw new InvalidOperationException("Cannot place ten safe spots.");
        return result;
    }
    public static Tile[][] ImportTiles()
    {
        foreach (int i in Enumerable.Range(0, 3))
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(Texture(i));
            if (importer == null) throw new InvalidOperationException("Missing terrain: " + Texture(i));
            importer.GetSourceTextureWidthAndHeight(out int w, out int h);
            if (w != 1448 || h != 1086) throw new InvalidOperationException("Expected 1448 x 1086: " + Texture(i));
        }
        var banks = new Tile[3][];
        for (int state = 0; state < 3; state++)
        {
            string path = Texture(state);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 181; importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp; importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.crunchedCompression = false;
            importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 2048;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; settings.spriteGenerateFallbackPhysicsShape = false;
            settings.spriteAlignment = (int)SpriteAlignment.Center; settings.spritePivot = Vector2.one * .5f;
            importer.SetTextureSettings(settings);
            foreach (string platform in new[] { "Standalone", "WebGL", "Android", "iPhone" }) importer.ClearPlatformTextureSettings(platform);
            importer.SaveAndReimport();
            var factories = new SpriteDataProviderFactories(); factories.Init();
            var provider = factories.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var previous = provider.GetSpriteRects();
            var rects = new SpriteRect[48];
            for (int i = 0; i < 48; i++)
            {
                var rect = new Rect((i % 8) * 181, (5 - i / 8) * 181, 181, 181);
                var old = previous.FirstOrDefault(s => s.rect == rect);
                rects[i] = new SpriteRect { name = old != null ? old.name : Path.GetFileNameWithoutExtension(path) + "_" + i,
                    rect = rect, alignment = SpriteAlignment.Center, pivot = Vector2.one * .5f,
                    spriteID = old != null ? old.spriteID : GUID.Generate() };
            }
            provider.SetSpriteRects(rects);
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
            var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
            string folder = Root + "Tiles/Acopalca/Ground/" + States[state]; Folder(folder);
            banks[state] = new Tile[48];
            for (int i = 0; i < 48; i++)
            {
                string name = "Ground_" + States[state] + "_" + i.ToString("D2");
                string target = folder + "/" + name + ".asset";
                var matches = AssetDatabase.FindAssets(name + " t:Tile", new[] { Root + "Tiles" })
                    .Select(AssetDatabase.GUIDToAssetPath).Where(p => Path.GetFileNameWithoutExtension(p) == name).ToArray();
                if (matches.Length > 1) throw new InvalidOperationException("Duplicate tile: " + name);
                if (matches.Length == 1 && matches[0] != target)
                {
                    string error = AssetDatabase.MoveAsset(matches[0], target);
                    if (!string.IsNullOrEmpty(error)) throw new InvalidOperationException(error);
                }
                var tile = AssetDatabase.LoadAssetAtPath<Tile>(target);
                if (tile == null) { tile = ScriptableObject.CreateInstance<Tile>(); AssetDatabase.CreateAsset(tile, target); }
                tile.name = name; tile.sprite = sprites.Single(s => s.rect == rects[i].rect);
                tile.color = Color.white; tile.transform = Matrix4x4.identity; tile.flags = TileFlags.LockColor;
                tile.colliderType = Tile.ColliderType.None; EditorUtility.SetDirty(tile); banks[state][i] = tile;
            }
        }
        return banks;
    }
    [MenuItem("BROTE/Ground/Clear Sector 01")]
    public static void Clear()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        var sector = GameObject.Find("Sector_01");
        if (sector == null || !sector.scene.path.EndsWith("/Acopalca.unity")) return;
        var map = UnityEngine.Object.FindObjectsByType<Tilemap>().Single(m => m.name == "GroundTilemap");
        Undo.RegisterCompleteObjectUndo(map, "Clear BROTE ground"); map.ClearAllTiles();
        foreach (var spot in sector.transform.Find("PlantingSpots").GetComponentsInChildren<PlantingSpot>())
            Undo.DestroyObjectImmediate(spot.gameObject);
        EditorSceneManager.MarkSceneDirty(sector.scene);
    }
    [MenuItem("BROTE/Ground/Regenerate Sector 01")]
    public static void Regenerate() => Generate();

    // Only repaints existing cells: keeps art imports, object positions and ground states.
    [MenuItem("BROTE/Ground/Repaint Existing Ground (Keep Objects)")]
    public static void RepaintGround()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Repaint only in Edit Mode.");
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.path != Root + "Scenes/Acopalca/Acopalca.unity") throw new InvalidOperationException("Open Acopalca first.");
        var sector = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true)).Single(t => t.name == "Sector_01");
        var map = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Tilemap>(true)).Single(t => t.name == "GroundTilemap");
        var controller = sector.GetComponentInChildren<GroundRestorationController>(true);
        var data = controller != null ? new SerializedObject(controller) : null;
        int seed = data != null ? data.FindProperty("seed").intValue : Seed;
        var tiles = new Dictionary<TileBase, int>();
        var banks = new Tile[3][];
        for (int state = 0; state < 3; state++)
        {
            banks[state] = new Tile[48];
            for (int index = 0; index < 48; index++)
            {
                string path = Root + "Tiles/Acopalca/Ground/" + States[state] + "/Ground_" + States[state] + "_" + index.ToString("D2") + ".asset";
                var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
                if (tile == null || tile.sprite == null) throw new InvalidOperationException("Missing tile/sprite: " + path);
                banks[state][index] = tile; tiles.Add(tile, state);
            }
        }
        var clean = GroundTileSelection.BuildCleanCells(map, sector);
        Undo.RegisterCompleteObjectUndo(map, "Repaint BROTE ground without moving objects");
        foreach (Vector3Int cell in map.cellBounds.allPositionsWithin)
        {
            var tile = map.GetTile(cell);
            if (tile == null || !tiles.TryGetValue(tile, out int state)) continue;
            int index = GroundTileSelection.SelectIndex(cell.x, cell.y, seed, state, clean.Contains(cell), controller != null ? controller.SelectionProfile : null);
            map.SetTile(cell, banks[state][index]);
        }
        map.RefreshAllTiles(); EditorSceneManager.MarkSceneDirty(scene);
    }
}


