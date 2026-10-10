using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class BroteSpotArtIntegration
{
    [MenuItem("BROTE/Art/Connect Official Planting Spot States")]
    public static void Connect()
    {
        const string atlas="Assets/_BROTE/Art/Environment/PlantingSpots/SpotState.png";
        const string path="Assets/_BROTE/Resources/Art/PlantingSpotArt.asset";
        var sprites=AssetDatabase.LoadAllAssetsAtPath(atlas).OfType<Sprite>().ToArray();
        string[] names={"SpotState_Locked","SpotState_Available","SpotState_Prepared"};
        // Validate all exact names before changing any references.
        var exact=names.Select(n=>sprites.Single(x=>x.name==n)).ToArray();
        if(!(exact[0].rect.x<exact[1].rect.x && exact[1].rect.x<exact[2].rect.x))
            throw new InvalidOperationException("SpotState atlas order does not match Locked/Available/Prepared.");
        if(!AssetDatabase.IsValidFolder("Assets/_BROTE/Resources/Art"))AssetDatabase.CreateFolder("Assets/_BROTE/Resources","Art");
        var art=AssetDatabase.LoadAssetAtPath<PlantingSpotArt>(path);
        if(art==null){art=ScriptableObject.CreateInstance<PlantingSpotArt>();AssetDatabase.CreateAsset(art,path);}
        art.locked=exact[0];art.available=exact[1];art.prepared=exact[2];
        EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
        var report=new System.Text.StringBuilder();
        foreach(var sprite in exact)
        {
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprite,out string guid,out long fileId);
            report.AppendLine(sprite.name+" guid="+guid+" fileID="+fileId+" rect="+sprite.rect+" PPU="+sprite.pixelsPerUnit+" pivot="+sprite.pivot);
        }
        Debug.Log(report.ToString());
        var args=Environment.GetCommandLineArgs();int arg=Array.IndexOf(args,"-spotArtReport");
        if(arg>=0 && arg+1<args.Length)File.WriteAllText(args[arg+1],report.ToString());
    }
}
