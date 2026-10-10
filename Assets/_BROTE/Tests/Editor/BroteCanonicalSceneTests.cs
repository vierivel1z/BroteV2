using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public sealed class BroteCanonicalSceneTests
{
    [Test] public void AcopalcaBaseScene_HasZeroPlantedQuenualInstances()
    {
        const string path="Assets/_BROTE/Scenes/Acopalca/Acopalca.unity";
        var scene=SceneManager.GetSceneByPath(path);bool opened=!scene.isLoaded;
        if(opened)scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
        try
        {
            var roots=scene.GetRootGameObjects();var spotType=Type.GetType("PlantingSpot, Assembly-CSharp",true);
            var treeType=Type.GetType("QuenualGrowth, Assembly-CSharp",true);
            Assert.AreEqual(10,roots.Sum(x=>x.GetComponentsInChildren(spotType,true).Length));
            Assert.AreEqual(0,roots.Sum(x=>x.GetComponentsInChildren(treeType,true).Length));
        }
        finally{if(opened)EditorSceneManager.CloseScene(scene,true);}
    }
}
