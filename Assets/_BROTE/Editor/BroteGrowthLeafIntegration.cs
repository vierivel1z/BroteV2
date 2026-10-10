using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Explicit setup only; never runs on import or changes the original PNG pixels.
public static class BroteGrowthLeafIntegration
{
    public const string ArtPath = "Assets/_BROTE/Art/Environment/Effects/Growth/";
    public const string AtlasPath = ArtPath + "GrowthLeavesAtlas.png";
    private const string ClosureBackup = ".brote-work/art-request-001/backups/";

    [MenuItem("BROTE/VFX/Integrate Official Growth Leaves")]
    public static void Integrate()
    {
        var sprites = BuildAtlas(new[] { "GrowthLeaf_01", "GrowthLeaf_02", "GrowthLeaf_03", "GrowthLeaf_04" },
            AtlasPath, ".brote-work/growth-leaves/backups/");
        foreach (string name in new[] { "PlantingVFX", "GrowthVFX", "MatureVFX" })
            UpdatePreset(name, ".brote-work/growth-leaves/backups/", settings => settings.leafSprites = sprites);
        AssetDatabase.SaveAssets();
        Debug.Log("BROTE GrowthLeaf INTEGRATED: four exact-pixel atlas slices; originals preserved.");
    }

    [MenuItem("BROTE/VFX/Integrate Dust Spark and Pulse")]
    public static void IntegrateRemaining()
    {
        var dustSprites = BuildAtlas(new[] { "PlantDust_01", "PlantDust_02" }, ArtPath + "PlantDustAtlas.png", ClosureBackup);
        var spark = ImportSingle("NaturalSpark");
        var pulse = ImportSingle("RestorationPulse");
        foreach (string name in new[] { "PlantingVFX", "GrowthVFX", "MatureVFX" })
            UpdatePreset(name, ClosureBackup, settings =>
            {
                settings.dustSprites = dustSprites;
                settings.pulseSprite = pulse;
                settings.sparkSprite = name == "MatureVFX" ? spark : null;
                settings.sparkCount = name == "MatureVFX" ? 1 : 0;
            });
        AssetDatabase.SaveAssets();
        Debug.Log("BROTE ART REQUEST 001 INTEGRATED: Dust x2, Pulse and one subtle Mature Spark; source PNGs preserved.");
    }

    private static void Backup(string path, string directory)
    {
        Directory.CreateDirectory(directory);
        if (File.Exists(path) && !File.Exists(directory + Path.GetFileName(path)))
            File.Copy(path, directory + Path.GetFileName(path));
        if (File.Exists(path + ".meta") && !File.Exists(directory + Path.GetFileName(path) + ".meta"))
            File.Copy(path + ".meta", directory + Path.GetFileName(path) + ".meta");
    }

    private static void UpdatePreset(string name, string backups, Action<BroteVFXSettings> configure)
    {
        string path = "Assets/_BROTE/Resources/VFX/" + name + ".prefab";
        Backup(path, backups);
        var prefab = PrefabUtility.LoadPrefabContents(path);
        try
        {
            configure(prefab.GetComponent<BroteGrowthVFX>().settings);
            PrefabUtility.SaveAsPrefabAsset(prefab, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(prefab); }
    }

    private static Sprite ImportSingle(string name)
    {
        string path = ArtPath + name + ".png";
        Backup(path + ".meta", ClosureBackup);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        Configure(importer, SpriteImportMode.Single);
        importer.SaveAndReimport();
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null) throw new InvalidOperationException("Sprite missing: " + path);
        return sprite;
    }

    private static Sprite[] BuildAtlas(string[] names, string path, string backups)
    {
        var originals = new Texture2D[names.Length];
        Texture2D atlas = null;
        try
        {
            for (int i = 0; i < names.Length; i++)
            {
                ImportSingle(names[i]);
                originals[i] = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!originals[i].LoadImage(File.ReadAllBytes(ArtPath + names[i] + ".png")))
                    throw new InvalidOperationException(names[i]);
            }
            const int gutter = 4, columns = 2;
            int rows = (names.Length + columns - 1) / columns;
            int cellWidth = originals.Max(t => t.width) + gutter * 2;
            int cellHeight = originals.Max(t => t.height) + gutter * 2;
            atlas = new Texture2D(cellWidth * columns, cellHeight * rows, TextureFormat.RGBA32, false);
            atlas.SetPixels(new Color[atlas.width * atlas.height]);
            var slices = new SpriteMetaData[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                int x = (i % columns) * cellWidth + gutter, y = (i / columns) * cellHeight + gutter;
                atlas.SetPixels(x, y, originals[i].width, originals[i].height, originals[i].GetPixels());
                slices[i] = new SpriteMetaData { name = names[i],
                    rect = new Rect(x, y, originals[i].width, originals[i].height),
                    alignment = (int)SpriteAlignment.Center, pivot = Vector2.one * .5f };
            }
            atlas.Apply();
            Backup(path, backups);
            File.WriteAllBytes(path, atlas.EncodeToPNG());
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            Configure(importer, SpriteImportMode.Multiple);
            importer.maxTextureSize = Mathf.NextPowerOfTwo(Mathf.Max(atlas.width, atlas.height));
#pragma warning disable CS0618
            importer.spritesheet = slices;
#pragma warning restore CS0618
            importer.SaveAndReimport();
            var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToArray();
            if (sprites.Length != names.Length) throw new InvalidOperationException("Atlas slice count mismatch.");
            return sprites;
        }
        finally
        {
            foreach (var texture in originals) if (texture != null) UnityEngine.Object.DestroyImmediate(texture);
            if (atlas != null) UnityEngine.Object.DestroyImmediate(atlas);
        }
    }

    private static void Configure(TextureImporter importer, SpriteImportMode mode)
    {
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = mode;
        importer.spritePixelsPerUnit = 128;
        importer.alphaSource = TextureImporterAlphaSource.FromInput;
        importer.alphaIsTransparency = true;
        importer.filterMode = FilterMode.Bilinear;
        var textureSettings = new TextureImporterSettings();
        importer.ReadTextureSettings(textureSettings);
        textureSettings.spriteMeshType = SpriteMeshType.FullRect;
        textureSettings.spriteGenerateFallbackPhysicsShape = false;
        importer.SetTextureSettings(textureSettings);
        importer.mipmapEnabled = false;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
    }
}
