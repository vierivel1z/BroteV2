using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
public sealed class BroteMainMenuArtEditorTests
{
    private static ScriptableObject Skin()=>AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/_BROTE/Resources/UI/MainMenuUISkin.asset");
    private static Sprite SpriteField(string name)
    {
        var skin=Skin();Assert.IsNotNull(skin);return (Sprite)skin.GetType().GetField(name).GetValue(skin);
    }
    [Test] public void MenuSkinUsesOfficialBackgroundAndExactButtonStates()
    {
        Assert.AreEqual("Assets/_BROTE/Art/MainMenu/MainMenu_Background.png",AssetDatabase.GetAssetPath(SpriteField("background")));
        foreach(var item in new[]{new[]{"buttonNormal","MainMenu_Button_Normal"},new[]{"buttonHover","MainMenu_Button_Hover"},
            new[]{"buttonPressed","MainMenu_Button_Pressed"},new[]{"buttonDisabled","MainMenu_Button_Disabled"},
            new[]{"buttonSmall","MainMenu_Button_Small"},new[]{"panelBackground","MainMenu_Panel_BG"}})
        {
            var sprite=SpriteField(item[0]);Assert.IsNotNull(sprite,item[0]);Assert.AreEqual(item[1],sprite.name);
            Assert.AreEqual("Assets/_BROTE/Art/UI/MainMenu/MainMenu_UI_Atlas.png",AssetDatabase.GetAssetPath(sprite));
        }
    }
    [Test] public void AmbientSelectionUsesSmallSubsetOfNamedOfficialSprites()
    {
        var skin=Skin();
        foreach(var field in new[]{"leaves","clusters","winds"})
        {
            var sprites=(Sprite[])skin.GetType().GetField(field).GetValue(skin);
            Assert.Greater(sprites.Length,0);Assert.LessOrEqual(sprites.Length,3);
            foreach(var sprite in sprites)
            {
                Assert.IsNotNull(sprite);
                Assert.AreEqual("Assets/_BROTE/Art/Effects/MainMenu_AmbientFX_Atlas.png",AssetDatabase.GetAssetPath(sprite));
                Assert.IsTrue(sprite.name.StartsWith(field=="leaves"?"MenuLeaf_":field=="clusters"?"MenuLeafCluster_":"MenuWind_"));
            }
        }
    }
}

