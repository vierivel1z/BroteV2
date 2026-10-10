using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static partial class BroteProjectValidator
{
    [MenuItem("BROTE/Validate/Art References")]
    public static void ArtMenu() => Publish(ValidateArt());
    public static Report ValidateArt()
    {
        var report=new Report();
        foreach(var renderer in SceneComponents<SpriteRenderer>(SceneManager.GetActiveScene()))
        {
            if(renderer.sprite==null) report.Warning(renderer.name+": no sprite assigned (may be an intentional backdrop slot).");
            else report.Info(renderer.name+": "+AssetDatabase.GetAssetPath(renderer.sprite));
        }
        foreach(string guid in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/_BROTE"}))
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            if(root==null)continue;
            foreach(var renderer in root.GetComponentsInChildren<SpriteRenderer>(true))
                if(renderer.sprite==null)report.Warning(root.name+"/"+renderer.name+": sprite absent.");
            Merge(report,ValidateMissingAndDuplicates(root));
        }
        foreach(string guid in AssetDatabase.FindAssets("t:Tile",new[]{"Assets/_BROTE"}))
        {
            string path=AssetDatabase.GUIDToAssetPath(guid);
            var tile=AssetDatabase.LoadAssetAtPath<Tile>(path);
            if(tile!=null&&tile.sprite==null)report.Error(path+": Tile sprite missing.");
        }
        report.Info("Only references inspected; original art, PPU and slicing unchanged.");
        return report;
    }
}
