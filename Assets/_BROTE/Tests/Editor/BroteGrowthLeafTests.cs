using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public sealed class BroteGrowthLeafTests
{
    private GameObject root;
    private Component effect;
    private static Type Runtime(string name) => Type.GetType(name + ", Assembly-CSharp", true);
    [SetUp] public void Setup()
    {
        root = new GameObject("Growth leaf test");
        effect = root.AddComponent(Runtime("BroteGrowthVFX"));
    }
    [TearDown] public void Cleanup()
    {
        // Runtime material cleanup is deferred; dispose explicitly in EditMode.
        var materials = (Material[])effect.GetType().GetField("ownedMaterials", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(effect);
        foreach (var material in materials) if (material != null) Object.DestroyImmediate(material);
        Object.DestroyImmediate(root);
    }
    private Sprite[] Official() => AssetDatabase.LoadAllAssetsAtPath("Assets/_BROTE/Art/Environment/Effects/Growth/GrowthLeavesAtlas.png")
        .OfType<Sprite>().OrderBy(s => s.name).ToArray();
    private void Play(Sprite[] sprites, int count = 3)
    {
        var settings = Activator.CreateInstance(Runtime("BroteVFXSettings"));
        settings.GetType().GetField("leafSprites").SetValue(settings, sprites);
        settings.GetType().GetField("leafCount").SetValue(settings, count);
        effect.GetType().GetMethod("Play").Invoke(effect, new object[] { settings, null, null, null, null,
            Color.white, Color.white, Color.white, 0, .12f });
    }
    [TestCase(4)] [TestCase(1)] [TestCase(0)] [TestCase(6)]
    public void VariantsKeepOneLeafSystemAndStaticFrames(int count)
    {
        var sprites = Official(); Assert.AreEqual(4, sprites.Length);
        Play(Enumerable.Range(0, count).Select(i => sprites[i % 4]).ToArray());
        var leaves = root.transform.Find("GrowthLeaves").GetComponent<ParticleSystem>();
        Assert.AreEqual(3, root.GetComponentsInChildren<ParticleSystem>().Length);
        Assert.AreEqual(1, root.GetComponentsInChildren<ParticleSystem>().Count(s => s.name == "GrowthLeaves"));
        var sheet = leaves.textureSheetAnimation;
        Assert.AreEqual(Math.Max(1, count), sheet.spriteCount);
        Assert.AreEqual(ParticleSystemAnimationMode.Sprites, sheet.mode);
        Assert.AreEqual(ParticleSystemCurveMode.TwoConstants, sheet.startFrame.mode);
        Assert.AreEqual(0f, sheet.startFrame.constantMin); Assert.AreEqual(1f, sheet.startFrame.constantMax, .00011f); // Unity clamps the upper boundary below one.
        Assert.AreEqual(0f, sheet.frameOverTime.constant);
        Assert.AreEqual(3, leaves.particleCount);
        if (count == 0) Assert.AreEqual("BroteLeaf", sheet.GetSprite(0).texture.name);
        else for (int i = 0; i < count; i++) Assert.AreSame(sprites[i % 4], sheet.GetSprite(i));
    }
    [Test] public void MissingReferencesAndReuseRefreshTheSheetWithoutAddingSystems()
    {
        Play(Official(), 8);
        var leaves = root.transform.Find("GrowthLeaves").GetComponent<ParticleSystem>();
        Assert.AreEqual(8, leaves.particleCount);
        Play(new Sprite[] { null, null }, 5);
        Assert.AreSame(leaves, root.transform.Find("GrowthLeaves").GetComponent<ParticleSystem>());
        Assert.AreEqual(1, leaves.textureSheetAnimation.spriteCount);
        Assert.AreEqual("BroteLeaf", leaves.textureSheetAnimation.GetSprite(0).texture.name);
        Assert.AreEqual(5, leaves.particleCount);
        Play(null);
        Assert.AreEqual(1, leaves.textureSheetAnimation.spriteCount);
        Assert.AreEqual(3, root.GetComponentsInChildren<ParticleSystem>().Length);
    }
    [TestCase("PlantingVFX", 3)] [TestCase("GrowthVFX", 5)] [TestCase("MatureVFX", 8)]
    public void ResourcePresetsHaveOfficialVariantsAndUnchangedCounts(string name, int count)
    {
        var prefab = Resources.Load<GameObject>("VFX/" + name);
        var component = prefab.GetComponent(Runtime("BroteGrowthVFX"));
        var settings = component.GetType().GetField("settings").GetValue(component);
        Assert.AreEqual(count, settings.GetType().GetField("leafCount").GetValue(settings));
        var sprites = (Sprite[])settings.GetType().GetField("leafSprites").GetValue(settings);
        Assert.AreEqual(4, sprites.Length);
        Assert.IsTrue(sprites.All(s => s != null && s.texture == sprites[0].texture));
        Assert.AreEqual(0, prefab.GetComponentsInChildren<ParticleSystem>().Length);
    }
    [Test] public void OfficialSourceImportsRemainSingleBilinear128WithAlpha()
    {
        for (int i = 1; i <= 4; i++)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath("Assets/_BROTE/Art/Environment/Effects/Growth/GrowthLeaf_0" + i + ".png");
            Assert.AreEqual(TextureImporterType.Sprite, importer.textureType);
            Assert.AreEqual(SpriteImportMode.Single, importer.spriteImportMode);
            Assert.AreEqual(128f, importer.spritePixelsPerUnit);
            Assert.AreEqual(FilterMode.Bilinear, importer.filterMode);
            Assert.IsTrue(importer.alphaIsTransparency);
            Assert.AreEqual(TextureImporterAlphaSource.FromInput, importer.alphaSource);
            Assert.AreEqual(TextureImporterCompression.Uncompressed, importer.textureCompression);
        }
    }
}
