using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>Read-only checks. Never repairs, places, paints or saves scene objects.</summary>
public static partial class BroteProjectValidator
{
    public sealed class Report
    {
        public readonly List<string> Lines = new List<string>();
        public int Errors, Warnings;
        public void Info(string text) => Lines.Add("INFO: " + text);
        public void Error(string text) { Errors++; Lines.Add("ERROR: " + text); }
        public void Warning(string text) { Warnings++; Lines.Add("WARNING: " + text); }
        public string Markdown() => "# BROTE — Validation Report\n\nSTATICALLY_VERIFIED / Editor inspection; PLAY_REQUIRED for navigation, input and rendering.\n\n"
            + "Errors: " + Errors + "; warnings: " + Warnings + ".\n\n" + string.Join("\n", Lines.ConvertAll(x => "- " + x)) + "\n";
    }
    public static Report LastReport { get; private set; }
    public static IEnumerable<T> SceneComponents<T>(Scene scene) where T : Component
    {
        if (!scene.IsValid() || !scene.isLoaded) yield break;
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (T component in root.GetComponentsInChildren<T>(true)) yield return component;
    }
    private static UnityEngine.Object Reference(UnityEngine.Object owner, string field)
    {
        var property = new SerializedObject(owner).FindProperty(field);
        return property != null ? property.objectReferenceValue : null;
    }
    private static string PathOf(Transform t)
    {
        string path = t.name;
        while (t.parent != null) { t = t.parent; path = t.name + "/" + path; }
        return path;
    }
    public static Report ValidateTree(GameObject root)
    {
        var report = new Report();
        if (root == null) { report.Error("Quenual prefab missing."); return report; }
        var growth = root.GetComponent<QuenualGrowth>();
        if (growth == null) { report.Error(root.name + ": QuenualGrowth missing."); return report; }
        var distinct = new HashSet<UnityEngine.Object>();
        foreach (string stage in new[] { "seedling", "young", "developed" })
        {
            var renderer = Reference(growth, stage) as SpriteRenderer;
            if (renderer == null || renderer.sprite == null) report.Error(root.name + ": invalid " + stage + " renderer/sprite.");
            else if (!renderer.transform.IsChildOf(root.transform) || !distinct.Add(renderer)) report.Error(root.name + ": stages must be distinct descendants.");
        }
        if (root.GetComponent<SortingGroup>() == null) report.Error(root.name + ": SortingGroup missing.");
        if (root.GetComponent<YSort2D>() == null) report.Error(root.name + ": YSort2D missing.");
        if (root.GetComponent<QuenualGrowthFeedback>() == null) report.Warning(root.name + ": visual feedback missing (growth still works).");
        var data = new SerializedObject(growth);
        if (data.FindProperty("secondsToYoung").floatValue != 5f || data.FindProperty("secondsToDeveloped").floatValue != 5f || data.FindProperty("restorationPoints").floatValue != 10f)
            report.Warning(root.name + ": timings/points differ from DECISIONS 5+5s/+10; no automatic repair.");
        report.Info(root.name + ": inspected 3 stages, growth, sorting and references.");
        return report;
    }
    public static Report ValidateSpot(GameObject root, bool sceneInstance)
    {
        var report = new Report();
        if (root == null) { report.Error("PlantingSpot prefab missing."); return report; }
        var spot = root.GetComponent<PlantingSpot>();
        if (spot == null) { report.Error(root.name + ": PlantingSpot missing."); return report; }
        var collider = root.GetComponent<Collider2D>();
        if (collider == null || !collider.isTrigger || !collider.enabled) report.Error(root.name + ": enabled trigger required.");
        var visual = root.GetComponent<SpriteRenderer>();
        if (visual == null || visual.sprite == null) report.Error(root.name + ": visual sprite missing.");
        if (!(Reference(spot, "plantPrefab") is QuenualGrowth)) report.Error(root.name + ": Quenual prefab reference missing.");
        var point = Reference(spot, "plantingPoint") as Transform;
        if (point == null || (point != root.transform && !point.IsChildOf(root.transform))) report.Error(root.name + ": spawn point missing/outside instance.");
        if (sceneInstance)
        {
            if (!(Reference(spot, "restorationManager") is RestorationManager)) report.Error(root.name + ": scene manager missing.");
            if (!(Reference(spot, "promptInventory") is SeedlingInventory)) report.Error(root.name + ": scene inventory missing.");
        }
        report.Info(root.name + ": trigger, visual, spawn and prefab checked; external scene refs " + (sceneInstance ? "required" : "supplied by scene"));
        return report;
    }
    public static Report ValidateNursery(GameObject root)
    {
        var report = new Report();
        if (root == null) { report.Error("Nursery prefab missing."); return report; }
        var source = root.GetComponentInChildren<NurserySeedlingSource>(true);
        if (source == null) { report.Error(root.name + ": NurserySeedlingSource missing."); return report; }
        var trigger = source.GetComponent<Collider2D>();
        if (trigger == null || !trigger.isTrigger || !trigger.enabled) report.Error(root.name + ": interaction trigger missing.");
        int solids = 0; bool blocked = false;
        Vector2 approach = (Vector2)source.transform.position + Vector2.down * 0.8f;
        foreach (Collider2D collider in root.GetComponentsInChildren<Collider2D>(true))
        {
            if (!collider.enabled || collider.isTrigger) continue;
            solids++;
            // Serialized geometry is valid even when a prefab is not in a physics scene.
            if (collider is BoxCollider2D box)
            {
                Vector2 local = box.transform.InverseTransformPoint(approach);
                Vector2 half = box.size * 0.5f;
                blocked |= Mathf.Abs(local.x - box.offset.x) <= half.x + 0.28f && Mathf.Abs(local.y - box.offset.y) <= half.y + 0.4f;
            }
        }
        if (solids == 0) report.Error(root.name + ": no solid building colliders.");
        if (blocked) report.Warning(root.name + ": front approach intersects a solid box; navigation PLAY_REQUIRED.");
        bool hasSprite = false;
        foreach (SpriteRenderer renderer in root.GetComponentsInChildren<SpriteRenderer>(true)) hasSprite |= renderer.sprite != null;
        if (!hasSprite) report.Error(root.name + ": nursery sprite missing.");
        report.Info(root.name + ": " + solids + " solids; front sample " + approach + ", structurally " + (blocked ? "obstructed" : "open") + "; Play still required.");
        return report;
    }
    private static void Merge(Report into, Report other)
    {
        into.Errors += other.Errors; into.Warnings += other.Warnings; into.Lines.AddRange(other.Lines);
    }
    [MenuItem("BROTE/Validate/Quenual Prefab")]
    public static void TreeMenu() => Publish(ValidateTree(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_BROTE/Prefabs/Plants/QuenualPlaceholder.prefab")));
    [MenuItem("BROTE/Validate/PlantingSpot Prefab")]
    public static void SpotMenu() => Publish(ValidateSpot(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_BROTE/Prefabs/Interactables/PlantingSpot.prefab"), false));
    [MenuItem("BROTE/Validate/Nursery Prefab")]
    public static void NurseryMenu() => Publish(ValidateNursery(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_BROTE/Prefabs/Environment/Nursery/Nursery_Acopalpa.prefab")));
    [MenuItem("BROTE/Validate/Prefabs")]
    public static void PrefabsMenu()
    {
        var report = new Report();
        Merge(report, ValidateTree(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_BROTE/Prefabs/Plants/QuenualPlaceholder.prefab")));
        Merge(report, ValidateSpot(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_BROTE/Prefabs/Interactables/PlantingSpot.prefab"), false));
        Merge(report, ValidateNursery(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_BROTE/Prefabs/Environment/Nursery/Nursery_Acopalpa.prefab")));
        Publish(report);
    }
    public static void Publish(Report report)
    {
        LastReport = report;
        Debug.Log(report.Markdown());
    }
}
