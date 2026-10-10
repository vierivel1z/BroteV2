using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public sealed class BrotePropCatalog : EditorWindow
{
    private const string Sheet = "Assets/_BROTE/Art/Environment/Props/Nature/Restoration_Props.png";
    private readonly List<Sprite> sprites = new List<Sprite>();
    private Vector2 scroll;
    [MenuItem("BROTE/Art/Restoration Props Catalog")]
    private static void Open() => GetWindow<BrotePropCatalog>("Restoration Props");
    private void OnEnable() => Reload();
    private void Reload()
    {
        sprites.Clear();
        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(Sheet)) if (asset is Sprite sprite) sprites.Add(sprite);
        sprites.Sort((a,b)=>string.CompareOrdinal(a.name,b.name));
    }
    private void OnGUI()
    {
        EditorGUILayout.HelpBox("Catálogo de sprites importados. No coloca props ni modifica el slicing. Configuración: RestorationPropSets.json.", MessageType.Info);
        if (GUILayout.Button("Refresh")) Reload();
        scroll = EditorGUILayout.BeginScrollView(scroll);
        foreach (string category in new[] { "Deg_", "Rec_", "Res_" })
        {
            EditorGUILayout.LabelField(category,EditorStyles.boldLabel);
            foreach (Sprite sprite in sprites)
            {
                if (!sprite.name.StartsWith(category,StringComparison.Ordinal)) continue;
                using (new EditorGUI.DisabledScope(true)) EditorGUILayout.ObjectField(sprite.name,sprite,typeof(Sprite),false);
            }
        }
        EditorGUILayout.EndScrollView();
    }
}
