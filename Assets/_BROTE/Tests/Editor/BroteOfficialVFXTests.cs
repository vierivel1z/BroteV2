using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public sealed class BroteOfficialVFXTests
{
    private const string Art = "Assets/_BROTE/Art/Environment/Effects/Growth/";
    private GameObject root;
    private Component effect;
    private object settings;
    private static Type Runtime(string name) => Type.GetType(name + ", Assembly-CSharp", true);
    private void Set(string name, object value) => settings.GetType().GetField(name).SetValue(settings, value);
    [SetUp] public void Setup()
    {
        root = new GameObject("Official VFX test");
        effect = root.AddComponent(Runtime("BroteGrowthVFX"));
        settings = Activator.CreateInstance(Runtime("BroteVFXSettings"));
    }
    [TearDown] public void Cleanup()
    {
        var materials = (Material[])effect.GetType().GetField("ownedMaterials", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(effect);
        foreach (var material in materials) if (material != null) Object.DestroyImmediate(material);
        Object.DestroyImmediate(root);
    }
    private Sprite[] DustSprites() => AssetDatabase.LoadAllAssetsAtPath(Art + "PlantDustAtlas.png")
        .OfType<Sprite>().OrderBy(s => s.name).ToArray();
    private Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Art + name + ".png");
    private void Play() => effect.GetType().GetMethod("Play").Invoke(effect, new object[] {
        settings, null, null, null, null, Color.white, Color.white, Color.white, 0, .6f });
    private ParticleSystem System(string name) => root.transform.Find(name).GetComponent<ParticleSystem>();
    private static ParticleSystem.Particle[] Particles(ParticleSystem system)
    {
        var particles = new ParticleSystem.Particle[system.particleCount]; system.GetParticles(particles); return particles;
    }
    [TestCase(2)] [TestCase(1)] [TestCase(0)]
    public void DustSupportsVariantsAndFallbackWithoutChangingBurst(int count)
    {
        var sprites = DustSprites(); Assert.AreEqual(2, sprites.Length);
        Set("dustSprites", sprites.Take(count).ToArray()); Play();
        var dust = System("PlantDust"); var sheet = dust.textureSheetAnimation;
        Assert.AreEqual(4, dust.particleCount);
        Assert.AreEqual(1, root.GetComponentsInChildren<ParticleSystem>().Count(s => s.name == "PlantDust"));
        Assert.AreEqual(3, root.GetComponentsInChildren<ParticleSystem>().Length);
        Assert.AreEqual(Math.Max(1, count), sheet.spriteCount);
        Assert.AreEqual(0f, sheet.frameOverTime.constant);
        Assert.AreEqual(ParticleSystemCurveMode.TwoConstants, sheet.startFrame.mode);
        Assert.AreEqual(1f, sheet.startFrame.constantMax, .00011f);
        Assert.IsFalse(dust.collision.enabled);
        foreach (var particle in Particles(dust))
        {
            Assert.AreEqual(.3f, particle.startLifetime, .0001f);
            Assert.GreaterOrEqual(particle.startSize, .1f); Assert.LessOrEqual(particle.startSize, .22f);
        }
        if (count == 0) Assert.AreEqual("BroteDust", sheet.GetSprite(0).texture.name);
        else Assert.AreSame(sprites[0], sheet.GetSprite(0));
    }
    [TestCase(false)] [TestCase(true)]
    public void PulseUsesOfficialOrProceduralWithExistingExpansion(bool official)
    {
        Set("emitPulse", true); Set("pulseSprite", official ? Sprite("RestorationPulse") : null); Play();
        var pulse = System("RestorationPulse"); var sheet = pulse.textureSheetAnimation;
        Assert.AreEqual(1, pulse.particleCount); Assert.AreEqual(1, sheet.spriteCount);
        if (official) Assert.AreSame(Sprite("RestorationPulse"), sheet.GetSprite(0));
        else Assert.AreEqual("BrotePulse", sheet.GetSprite(0).texture.name);
        Assert.AreEqual(ParticleSystemCurveMode.Constant, sheet.startFrame.mode);
        Assert.AreEqual(0f, sheet.frameOverTime.constant);
        Assert.AreEqual(0, pulse.GetComponent<ParticleSystemRenderer>().sortingOrder);
        var particle = Particles(pulse)[0]; Assert.AreEqual(.7f, particle.startSize, .0001f);
        Assert.AreEqual(.38f, particle.startLifetime, .0001f);
        Assert.AreEqual(.6f, pulse.sizeOverLifetime.size.curve.Evaluate(0f), .0001f);
        Assert.AreEqual(1.5f, pulse.sizeOverLifetime.size.curve.Evaluate(1f), .0001f);
        Assert.AreEqual(0f, pulse.colorOverLifetime.color.gradient.Evaluate(1f).a);
    }
    [Test] public void MatureSparkReusesDustAndNextPlantingRestoresDustVariants()
    {
        Set("dustSprites", DustSprites()); Set("dustCount", 0); Set("leafCount", 8);
        Set("sparkSprite", Sprite("NaturalSpark")); Set("sparkCount", 1); Play();
        var dust = System("PlantDust");
        Assert.AreEqual(1, dust.particleCount); Assert.AreEqual(8, System("GrowthLeaves").particleCount);
        Assert.AreSame(Sprite("NaturalSpark"), dust.textureSheetAnimation.GetSprite(0));
        Assert.AreEqual(0, dust.GetComponent<ParticleSystemRenderer>().sortingOrder);
        var spark = Particles(dust)[0];
        Assert.AreEqual(.24f, spark.startLifetime, .0001f); Assert.AreEqual(.075f, spark.startSize, .0001f);
        Assert.LessOrEqual(spark.startColor.a / 255f, .29f);
        Assert.AreEqual(.6f, spark.position.y, .0001f);
        Set("sparkCount", 100); Play(); Assert.AreEqual(2, dust.particleCount);
        Set("dustCount", 4); Play();
        Assert.AreSame(dust, System("PlantDust")); Assert.AreEqual(4, dust.particleCount);
        Assert.AreEqual(2, dust.textureSheetAnimation.spriteCount);
        Assert.AreEqual(2, dust.GetComponent<ParticleSystemRenderer>().sortingOrder);
        Set("dustSprites", new Sprite[] { null, null }); Set("sparkSprite", null);
        Set("pulseSprite", null); Set("emitPulse", true); Play();
        Assert.AreEqual("BroteDust", dust.textureSheetAnimation.GetSprite(0).texture.name);
        Assert.AreEqual("BrotePulse", System("RestorationPulse").textureSheetAnimation.GetSprite(0).texture.name);
        Set("dustCount", 0); Play(); Assert.AreEqual(0, dust.particleCount);
        Assert.AreEqual(3, root.GetComponentsInChildren<ParticleSystem>().Length);
    }
    [TestCase("PlantingVFX", 3, 4, 0)] [TestCase("GrowthVFX", 5, 0, 0)] [TestCase("MatureVFX", 8, 0, 1)]
    public void PresetsConnectAllOfficialAssetsAndPreserveCounts(string name, int leaves, int dust, int sparks)
    {
        var component = Resources.Load<GameObject>("VFX/" + name).GetComponent(Runtime("BroteGrowthVFX"));
        var preset = component.GetType().GetField("settings").GetValue(component);
        object Get(string field) => preset.GetType().GetField(field).GetValue(preset);
        Assert.AreEqual(leaves, Get("leafCount")); Assert.AreEqual(dust, Get("dustCount"));
        Assert.AreEqual(sparks, Get("sparkCount"));
        var sprites = (Sprite[])Get("dustSprites"); Assert.AreEqual(2, sprites.Length);
        Assert.IsTrue(sprites.All(s => s != null && s.texture == sprites[0].texture));
        Assert.AreSame(Sprite("RestorationPulse"), Get("pulseSprite"));
        Assert.AreEqual(name == "MatureVFX" ? Sprite("NaturalSpark") : null, Get("sparkSprite"));
    }
    [TestCase("PlantDust_01")] [TestCase("PlantDust_02")] [TestCase("NaturalSpark")] [TestCase("RestorationPulse")]
    public void OfficialSourceImportMatchesRequirements(string name)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(Art + name + ".png");
        Assert.AreEqual(TextureImporterType.Sprite, importer.textureType);
        Assert.AreEqual(SpriteImportMode.Single, importer.spriteImportMode);
        Assert.AreEqual(128f, importer.spritePixelsPerUnit);
        Assert.AreEqual(FilterMode.Bilinear, importer.filterMode);
        Assert.AreEqual(TextureImporterAlphaSource.FromInput, importer.alphaSource);
        Assert.IsTrue(importer.alphaIsTransparency); Assert.IsFalse(importer.mipmapEnabled);
        Assert.AreEqual(TextureImporterCompression.Uncompressed, importer.textureCompression);
    }
}
