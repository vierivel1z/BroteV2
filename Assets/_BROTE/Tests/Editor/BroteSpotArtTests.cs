using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class BroteSpotArtTests
{
    [Test] public void SharedArtResolvesExactThreeSubSpritesInConfirmedAtlasOrder()
    {
        var art=Resources.Load<ScriptableObject>("Art/PlantingSpotArt");Assert.IsNotNull(art);
        const string path="Assets/_BROTE/Art/Environment/PlantingSpots/SpotState.png";
        var sprites=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
        string[] fields={"locked","available","prepared"};string[] names={"SpotState_Locked","SpotState_Available","SpotState_Prepared"};
        Sprite previous=null;
        for(int i=0;i<3;i++)
        {
            var exact=sprites.Single(x=>x.name==names[i]);
            Assert.AreSame(exact,art.GetType().GetField(fields[i]).GetValue(art));
            if(previous!=null){Assert.AreSame(previous.texture,exact.texture);Assert.Less(previous.rect.x,exact.rect.x);}
            Assert.Greater(exact.rect.width,0);Assert.AreEqual(128f,exact.pixelsPerUnit);previous=exact;
        }
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        Assert.AreEqual(SpriteImportMode.Multiple,importer.spriteImportMode);Assert.IsTrue(importer.alphaIsTransparency);
        var state=Type.GetType("PlantingSpot+SpotState, Assembly-CSharp",true);
        Assert.IsNull(art.GetType().GetMethod("ForState").Invoke(art,new[]{Enum.Parse(state,"PLANTED")}));
    }
}
