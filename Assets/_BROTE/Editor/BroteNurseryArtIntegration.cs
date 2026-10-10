using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class BroteNurseryArtIntegration
{
    public const string AtlasPath="Assets/_BROTE/Art/UI/Nursery/NurseryUI_Atlas.png";
    private static readonly string[] Names={"NurseryPanel_BG","Tab_Production_Active","Tab_Production_Inactive","Tab_Upgrades_Active","Tab_Upgrades_Inactive","ProgressBar_Back","ProgressBar_Fill","UpgradeCard_BG"};
    private static readonly string[] Fields={"panelBackground","productionTabActive","productionTabInactive","upgradesTabActive","upgradesTabInactive","progressBackground","progressFill","upgradeCardBackground"};
    [MenuItem("BROTE/UI/Connect Nursery Phase 1 Atlas")]
    public static void Connect()
    {
        var skin=AssetDatabase.LoadAssetAtPath<NurseryUISkin>("Assets/_BROTE/Resources/UI/NurseryUISkin.asset");
        if(skin==null)throw new InvalidOperationException("NurseryUISkin asset missing.");
        var sprites=AssetDatabase.LoadAllAssetsAtPath(AtlasPath).OfType<Sprite>().ToArray();
        var report=new System.Text.StringBuilder();
        for(int i=0;i<Names.Length;i++)
        {
            var matches=sprites.Where(x=>x.name==Names[i]).ToArray();
            if(matches.Length!=1){report.AppendLine("SKIPPED "+Names[i]+": exact matches="+matches.Length);continue;}
            typeof(NurseryUISkin).GetField(Fields[i]).SetValue(skin,matches[0]);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(matches[0],out string guid,out long fileId);
            report.AppendLine(Names[i]+" -> "+Fields[i]+" fileID="+fileId+" rect="+matches[0].rect+" sprite.border="+matches[0].border);
        }
        skin.panelSliceBorders=new Vector4(180,100,180,100);
        skin.cardSliceBorders=new Vector4(90,70,90,70);
        skin.panelSliceScale=.3f;skin.cardSliceScale=.5f;
        skin.progressInsets=new Vector4(14,8,14,8);
        EditorUtility.SetDirty(skin);AssetDatabase.SaveAssets();
        Debug.Log(report.ToString());
        var args=Environment.GetCommandLineArgs();int arg=Array.IndexOf(args,"-nurseryReport");
        if(arg>=0 && arg+1<args.Length)File.WriteAllText(args[arg+1],report.ToString());
    }
    [MenuItem("BROTE/UI/Connect Nursery Phase 2 Buttons and Icons")]
    public static void ConnectPhaseTwo()
    {
        var skin=AssetDatabase.LoadAssetAtPath<NurseryUISkin>("Assets/_BROTE/Resources/UI/NurseryUISkin.asset");
        if(skin==null)throw new InvalidOperationException("NurseryUISkin asset missing.");
        string[] names={"NurseryButton_Primary_Normal","NurseryButton_Primary_Pressed","NurseryButton_Primary_Disabled","NurseryButton_Close","BuyButton_Available","BuyButton_Disabled","BuyButton_Max","Icon_Minka","Icon_Seedling","Icon_Irrigation","Icon_Capacity","Icon_SoilPreparation","Icon_Expansion"};
        string[] fields={"primaryNormal","primaryPressed","primaryDisabled","closeButtonBackground","buyAvailable","buyDisabled","buyMax","iconMinka","iconSeedling","iconIrrigation","iconCapacity","iconSoilPreparation","iconExpansion"};
        var sprites=AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/_BROTE/Art/UI/Nursery"})
            .Select(AssetDatabase.GUIDToAssetPath).SelectMany(AssetDatabase.LoadAllAssetsAtPath).OfType<Sprite>().ToArray();
        var report=new System.Text.StringBuilder();
        for(int i=0;i<names.Length;i++)
        {
            var matches=sprites.Where(x=>x.name==names[i]).ToArray();
            if(matches.Length!=1){report.AppendLine("SKIPPED "+names[i]+": exact matches="+matches.Length);continue;}
            typeof(NurseryUISkin).GetField(fields[i]).SetValue(skin,matches[0]);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(matches[0],out string guid,out long fileId);
            report.AppendLine(names[i]+" -> "+fields[i]+" source="+AssetDatabase.GetAssetPath(matches[0])+" guid="+guid+" fileID="+fileId+" rect="+matches[0].rect);
        }
        EditorUtility.SetDirty(skin);AssetDatabase.SaveAssets();Debug.Log(report.ToString());
        var args=Environment.GetCommandLineArgs();int arg=Array.IndexOf(args,"-nurseryReport");
        if(arg>=0 && arg+1<args.Length)File.WriteAllText(args[arg+1],report.ToString());
    }

}
