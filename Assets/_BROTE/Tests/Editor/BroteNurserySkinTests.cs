using System;
using System.Linq;
using UnityEditor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class BroteNurserySkinTests
{
    [Test] public void PhaseTwoReferencesAllThirteenExactManualSprites()
    {
        var skin=Resources.Load<ScriptableObject>("UI/NurseryUISkin");
        string[] names={"NurseryButton_Primary_Normal","NurseryButton_Primary_Pressed","NurseryButton_Primary_Disabled","NurseryButton_Close","BuyButton_Available","BuyButton_Disabled","BuyButton_Max","Icon_Minka","Icon_Seedling","Icon_Irrigation","Icon_Capacity","Icon_SoilPreparation","Icon_Expansion"};
        string[] fields={"primaryNormal","primaryPressed","primaryDisabled","closeButtonBackground","buyAvailable","buyDisabled","buyMax","iconMinka","iconSeedling","iconIrrigation","iconCapacity","iconSoilPreparation","iconExpansion"};
        var sprites=AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/_BROTE/Art/UI/Nursery"}).Select(AssetDatabase.GUIDToAssetPath)
            .SelectMany(AssetDatabase.LoadAllAssetsAtPath).OfType<Sprite>().ToArray();
        for(int i=0;i<names.Length;i++)
            Assert.AreSame(sprites.Single(x=>x.name==names[i]),skin.GetType().GetField(fields[i]).GetValue(skin));
    }
    [Test] public void PhaseOneReferencesExactOfficialSlicesAndConservativeBorders()
    {
        var skin=Resources.Load<ScriptableObject>("UI/NurseryUISkin");
        string[] names={"NurseryPanel_BG","Tab_Production_Active","Tab_Production_Inactive","Tab_Upgrades_Active","Tab_Upgrades_Inactive","ProgressBar_Back","ProgressBar_Fill","UpgradeCard_BG"};
        string[] fields={"panelBackground","productionTabActive","productionTabInactive","upgradesTabActive","upgradesTabInactive","progressBackground","progressFill","upgradeCardBackground"};
        var sprites=AssetDatabase.LoadAllAssetsAtPath("Assets/_BROTE/Art/UI/Nursery/NurseryUI_Atlas.png").OfType<Sprite>().ToArray();
        for(int i=0;i<names.Length;i++)
        {
            var exact=sprites.Single(x=>x.name==names[i]);
            Assert.AreSame(exact,skin.GetType().GetField(fields[i]).GetValue(skin));
        }
        foreach(string kind in new[]{"panel","card"})
        {
            var border=(Vector4)skin.GetType().GetField(kind+"SliceBorders").GetValue(skin);
            var sprite=(Sprite)skin.GetType().GetField(kind=="panel"?"panelBackground":"upgradeCardBackground").GetValue(skin);
            Assert.Greater(border.x,0);Assert.Greater(border.y,0);
            Assert.Less(border.x+border.z,sprite.rect.width);Assert.Less(border.y+border.w,sprite.rect.height);
        }
        foreach(string field in new[]{"primaryNormal","primaryPressed","primaryDisabled","buyAvailable","buyDisabled","buyMax","iconMinka","iconSeedling","iconIrrigation","iconCapacity","iconSoilPreparation","iconExpansion"})
            Assert.AreEqual(typeof(Sprite),skin.GetType().GetField(field).FieldType);
        Assert.IsNotNull(Resources.Load<VisualTreeAsset>("UI/NurseryUpgradeCard").CloneTree().Q<VisualElement>("upgrade-icon"));
    }
    [Test] public void PersistentSkinAndCardTemplateResolveAllRequiredSlots()
    {
        var skin=Resources.Load<ScriptableObject>("UI/NurseryUISkin");Assert.IsNotNull(skin);
        foreach(string slot in new[]{"panelBackground","productionTabActive","productionTabInactive","upgradesTabActive","upgradesTabInactive","produceButtonBackground","buyButtonBackground","closeButtonBackground","progressBackground","progressFill","upgradeCardBackground"})
            Assert.AreEqual(typeof(Sprite),skin.GetType().GetField(slot).FieldType);
        Assert.IsNotNull(Resources.Load<StyleSheet>("UI/NurseryPanel"));
        var template=Resources.Load<VisualTreeAsset>("UI/NurseryUpgradeCard");Assert.IsNotNull(template);
        var first=template.CloneTree();var second=template.CloneTree();
        foreach(string slot in new[]{"upgrade-title","upgrade-level","upgrade-effect","upgrade-cost"})Assert.IsNotNull(first.Q<Label>(slot));
        Assert.IsNotNull(first.Q<Button>("upgrade-buy"));
        Assert.AreNotSame(first.Q<Button>("upgrade-buy"),second.Q<Button>("upgrade-buy"));
    }
    [Test] public void SpriteUsesOriginalColorsAndClearingSlotRestoresPlaceholder()
    {
        var element=new VisualElement();var texture=new Texture2D(4,4);
        var sprite=Sprite.Create(texture,new Rect(0,0,4,4),Vector2.one*.5f);
        try
        {
            var apply=Type.GetType("NurseryUISkin, Assembly-CSharp",true).GetMethod("Apply");
            apply.Invoke(null,new object[]{element,sprite,Color.green});
            Assert.AreSame(sprite,element.style.backgroundImage.value.sprite);
            Assert.AreEqual(Color.clear,element.style.backgroundColor.value);
            Assert.AreEqual(Color.white,element.style.unityBackgroundImageTintColor.value);
            apply.Invoke(null,new object[]{element,null,Color.green});
            Assert.IsNull(element.style.backgroundImage.value.sprite);
            Assert.AreEqual(Color.green,element.style.backgroundColor.value);
        }
        finally {UnityEngine.Object.DestroyImmediate(sprite);UnityEngine.Object.DestroyImmediate(texture);}
    }
}
