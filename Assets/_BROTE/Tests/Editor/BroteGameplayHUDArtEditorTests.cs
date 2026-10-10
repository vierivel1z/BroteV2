using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
public sealed class BroteGameplayHUDArtEditorTests
{
    [Test] public void NormalBuildStartsInMainMenuAndEditorHelperExists()
    {
        var scenes=EditorBuildSettings.scenes.Where(x=>x.enabled).Select(x=>x.path).ToArray();
        CollectionAssert.AreEqual(new[]{"Assets/_BROTE/Scenes/UI/MainMenu.unity","Assets/_BROTE/Scenes/Acopalca/Acopalca.unity"},scenes);
        Assert.IsNotNull(System.Type.GetType("BroteLocalSaveDebug, Assembly-CSharp-Editor").GetMethod("PlayFromMainMenu"));
    }
    [Test] public void HUDSkinReferencesSevenExactOfficialSprites()
    {
        var skin=AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/_BROTE/Resources/UI/GameplayHUDSkin.asset");
        Assert.IsNotNull(skin);
        foreach(var pair in new[]{new[]{"mainPanel","GameplayHUD_MainPanel"},new[]{"progressFill","GameplayHUD_ProgressFill"},
            new[]{"accentDiamond","GameplayHUD_AccentDiamond"},new[]{"iconMinka","GameplayHUD_Icon_Minka"},
            new[]{"iconSeedling","GameplayHUD_Icon_Seedling"},new[]{"iconRestoration","GameplayHUD_Icon_Restoration"},new[]{"iconObjective","GameplayHUD_Icon_Objective"}})
        {
            var sprite=(Sprite)skin.GetType().GetField(pair[0]).GetValue(skin);
            Assert.IsNotNull(sprite,pair[0]);Assert.AreEqual(pair[1],sprite.name);
            Assert.IsTrue(AssetDatabase.GetAssetPath(sprite).StartsWith("Assets/_BROTE/Art/UI/GameplayHUD/"));
        }
    }
    [Test] public void HUDAtlasImportRetainsApprovedSettingsAndNamedSlices()
    {
        foreach(var file in new[]{"GameplayHUD_Atlas.png","GameplayHUD_Icons_Atlas.png"})
        {
            string path="Assets/_BROTE/Art/UI/GameplayHUD/"+file;
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            Assert.AreEqual(TextureImporterType.Sprite,importer.textureType);Assert.AreEqual(SpriteImportMode.Multiple,importer.spriteImportMode);
            Assert.AreEqual(128,importer.spritePixelsPerUnit);Assert.IsFalse(importer.mipmapEnabled);
            Assert.IsTrue(importer.alphaIsTransparency);Assert.AreEqual(FilterMode.Bilinear,importer.filterMode);
            Assert.AreEqual(TextureWrapMode.Clamp,importer.wrapMode);Assert.AreEqual(TextureImporterCompression.Uncompressed,importer.textureCompression);
            Assert.IsTrue(AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Any(x=>x.name.StartsWith("GameplayHUD_")));
        }
    }
}

