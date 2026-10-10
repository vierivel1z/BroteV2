using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>Event-driven local terrain recovery. The editor paints the initial map.</summary>
public sealed class GroundRestorationController : MonoBehaviour
{
    public enum GroundState { Degraded, Recovery, Restored }
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private RestorationManager restorationManager;
    [SerializeField] private Transform sectorRoot;
    [SerializeField] private Tile[] degradedTiles;
    [SerializeField] private Tile[] recoveryTiles;
    [SerializeField] private Tile[] restoredTiles;
    [SerializeField] private int seed = 2026;
    [SerializeField, Range(2f, 3f)] private float initialRadius = 2.6f;
    [SerializeField, Min(0f)] private float additionalRadius = 4.5f;
    [SerializeField] private GroundTileSelection.Profile tileSelection = new GroundTileSelection.Profile();
    private HashSet<Vector3Int> cleanCells;
    private readonly Dictionary<Vector3Int, GroundState> states = new Dictionary<Vector3Int, GroundState>();
    private readonly Dictionary<QuenualGrowth, Vector3Int> matureTrees = new Dictionary<QuenualGrowth, Vector3Int>();

    private readonly Dictionary<Vector3Int, float> nearestDistances = new Dictionary<Vector3Int, float>();
    private readonly Dictionary<Vector3Int, float> cellNoise = new Dictionary<Vector3Int, float>();
    private readonly List<KeyValuePair<Vector3Int, GroundState>> changes = new List<KeyValuePair<Vector3Int, GroundState>>(128);
    private int treeVersion;
    private int lastTreeVersion = -1;
    private float lastRadius = float.NaN, lastRestoredRadius = float.NaN;
    private bool lastRestoreEnabled;
    public float InitialRadius => initialRadius;
    public float AdditionalRadius => additionalRadius;
    public float RadiusAt(float percentage) => initialRadius + additionalRadius * Mathf.InverseLerp(20f, 100f, percentage);
    public float RestoredRadiusAt(float percentage) => RadiusAt(percentage) * Mathf.Lerp(0.22f, 0.96f, Mathf.InverseLerp(40f, 100f, percentage));
    public IReadOnlyDictionary<Vector3Int, GroundState> CellStates => states;
    public int RecoveryCellCount => Count(GroundState.Recovery);
    public int RestoredCellCount => Count(GroundState.Restored);
    public int MatureTreeCount => matureTrees.Count;
    public GroundTileSelection.Profile SelectionProfile => tileSelection;
    public GroundState GetCellState(Vector3Int cell) => states.TryGetValue(cell, out GroundState state) ? state : GroundState.Degraded;

    private void OnEnable()
    {
        if (groundTilemap == null || restorationManager == null) return;
        CachePaintedCells();
        cleanCells = GroundTileSelection.BuildCleanCells(groundTilemap, sectorRoot);
        QuenualGrowth.Matured += OnTreeMatured;
        restorationManager.RestorationChanged += Refresh;
        foreach (QuenualGrowth plant in FindObjectsByType<QuenualGrowth>())
            if (plant.Stage == 2) Register(plant);
        Refresh(restorationManager.RestorationPercentage);
    }
    private void OnDisable()
    {
        QuenualGrowth.Matured -= OnTreeMatured;
        if (restorationManager != null) restorationManager.RestorationChanged -= Refresh;
    }
    private void CachePaintedCells()
    {
        if (states.Count > 0) return;
        var recovery = new HashSet<TileBase>(recoveryTiles ?? System.Array.Empty<Tile>());
        var restored = new HashSet<TileBase>(restoredTiles ?? System.Array.Empty<Tile>());
        foreach (Vector3Int cell in groundTilemap.cellBounds.allPositionsWithin)
        {
            TileBase tile = groundTilemap.GetTile(cell);
            if (tile == null) continue;
            states[cell] = restored.Contains(tile) ? GroundState.Restored :
                recovery.Contains(tile) ? GroundState.Recovery : GroundState.Degraded;
            cellNoise[cell] = (Mathf.PerlinNoise((cell.x + seed * 0.013f) * 0.24f,
                (cell.y + seed * 0.017f) * 0.24f) - 0.5f) * 1.8f;
        }
    }
    private bool Register(QuenualGrowth plant)
    {
        if (plant == null || (sectorRoot != null && !plant.transform.IsChildOf(sectorRoot))) return false;
        QuenualGrowth id = plant;
        if (matureTrees.ContainsKey(id)) return false;
        Vector3Int center = groundTilemap.WorldToCell(plant.transform.position);
        matureTrees[id] = center;
        foreach (var entry in states)
        {
            if (entry.Value == GroundState.Restored) continue;
            float distance = Vector2.Distance(new Vector2(entry.Key.x, entry.Key.y), new Vector2(center.x, center.y));
            if (!nearestDistances.TryGetValue(entry.Key, out float previous) || distance < previous) nearestDistances[entry.Key] = distance;
        }
        treeVersion++;
        return true;
    }
    private void OnTreeMatured(QuenualGrowth plant)
    {
        if (Register(plant)) Refresh(restorationManager.RestorationPercentage);
    }
    private void Refresh(float percentage)
    {
        if (matureTrees.Count == 0 || recoveryTiles == null || recoveryTiles.Length != 48 ||
            restoredTiles == null || restoredTiles.Length != 48) return;
        float radius = RadiusAt(percentage);
        float restoredRadius = RestoredRadiusAt(percentage);
        bool restoreEnabled = percentage >= 40f;
        if (lastTreeVersion == treeVersion && lastRadius == radius && lastRestoredRadius == restoredRadius && lastRestoreEnabled == restoreEnabled) return;
        changes.Clear();
        foreach (KeyValuePair<Vector3Int, GroundState> entry in states)
        {
            Vector3Int cell = entry.Key;
            if (entry.Value == GroundState.Restored || !nearestDistances.TryGetValue(cell, out float nearest)) continue;
            float noise = cellNoise[cell];
            if (nearest > radius + noise) continue;
            GroundState desired = GroundState.Recovery;
            if (restoreEnabled && nearest <= restoredRadius + noise &&
                GroundTileSelection.Hash01(cell.x, cell.y, seed, 91) > 0.045f)
                desired = GroundState.Restored;
            if (desired > entry.Value) changes.Add(new KeyValuePair<Vector3Int, GroundState>(cell, desired));
        }
        foreach (KeyValuePair<Vector3Int, GroundState> change in changes)
        {
            Tile[] bank = change.Value == GroundState.Restored ? restoredTiles : recoveryTiles;
            groundTilemap.SetTile(change.Key, bank[GroundTileSelection.SelectIndex(change.Key.x, change.Key.y, seed, (int)change.Value, cleanCells.Contains(change.Key), tileSelection)]);
            states[change.Key] = change.Value;
        }
        lastTreeVersion = treeVersion; lastRadius = radius; lastRestoredRadius = restoredRadius; lastRestoreEnabled = restoreEnabled;
    }
    private int Count(GroundState state)
    {
        int count = 0;
        foreach (GroundState current in states.Values) if (current == state) count++;
        return count;
    }
}

/// <summary>Shared edit/play selection. Uses compatible tones and coherent 2-4 cell patches.</summary>
public static class GroundTileSelection
{
    [System.Serializable]
    public sealed class Categories
    {
        public int[] baseTiles;
        public int[] variationTiles;
        public int[] specialTiles;
        public Categories(int[] bases, int[] variations, int[] specials)
        { baseTiles = bases; variationTiles = variations; specialTiles = specials; }
    }
    [System.Serializable]
    public sealed class Profile
    {
        [Range(0f, 1f)] public float baseWeight = 0.80f;
        [Range(0f, 1f)] public float variationWeight = 0.17f;
        [Range(2f, 4f)] public float patchSize = 3f;
        [Range(0.01f, 0.25f)] public float maxToneDifference = 0.08f;
        public Categories[] states = {
            new Categories(new[] {3,10,26,35,40,42}, new[] {1,17,33,44}, new[] {5,12,23,39}),
            new Categories(new[] {3,10,19,26,35,42}, new[] {1,8,12,17,33,44}, new[] {5,7,23,24}),
            new Categories(new[] {1,10,19,26,35,42}, new[] {8,14,17,33,44,46}, new[] {3,5,7,23,24,39})
        };
    }
    private static readonly Profile Defaults = new Profile();
    // Mean Rec.709 luminance of the existing 181px slices; PNG pixels are unchanged.
    private static readonly float[][] Luminance = {
        new[] { 0.5315f,0.2927f,0.4892f,0.3056f,0.4985f,0.2922f,0.4207f,0.4436f,0.3098f,0.5583f,0.3127f,0.5139f,0.3036f,0.4777f,0.3221f,0.4353f,0.4718f,0.3052f,0.511f,0.3043f,0.5276f,0.3729f,0.5152f,0.3077f,0.3049f,0.5013f,0.2955f,0.4593f,0.3292f,0.539f,0.3014f,0.4773f,0.5143f,0.3156f,0.5213f,0.3052f,0.4699f,0.3029f,0.5487f,0.3086f,0.3022f,0.4735f,0.3137f,0.4881f,0.3024f,0.4836f,0.2994f,0.51f },
        new[] { 0.5149f,0.2956f,0.5001f,0.3248f,0.5036f,0.3049f,0.4887f,0.3169f,0.3124f,0.5385f,0.3008f,0.5139f,0.2985f,0.5164f,0.3148f,0.484f,0.4831f,0.3152f,0.5227f,0.313f,0.5125f,0.3092f,0.4784f,0.3107f,0.2914f,0.4906f,0.3145f,0.5028f,0.3009f,0.5517f,0.3044f,0.5018f,0.4956f,0.3022f,0.505f,0.3032f,0.4997f,0.3096f,0.5072f,0.3132f,0.3161f,0.5011f,0.3085f,0.5026f,0.3109f,0.5251f,0.308f,0.5472f },
        new[] { 0.5123f,0.2941f,0.4995f,0.3158f,0.5137f,0.3026f,0.5037f,0.3253f,0.3178f,0.5394f,0.3087f,0.515f,0.3038f,0.526f,0.3143f,0.4873f,0.4963f,0.3267f,0.5329f,0.3096f,0.5294f,0.3199f,0.4808f,0.3173f,0.2939f,0.4886f,0.3194f,0.5051f,0.3028f,0.5471f,0.3125f,0.5097f,0.4991f,0.3078f,0.5005f,0.3075f,0.5095f,0.3138f,0.5194f,0.3224f,0.3142f,0.4967f,0.3108f,0.5089f,0.3137f,0.5256f,0.3129f,0.5634f },
    };
    public static int SelectIndex(int x, int y, int seed, int state, bool clean = false, Profile profile = null)
    {
        state = Mathf.Clamp(state, 0, 2);
        profile = profile ?? Defaults;
        Categories categories = profile.states != null && profile.states.Length > state && profile.states[state] != null
            ? profile.states[state] : Defaults.states[state];
        int[] bases = Valid(categories.baseTiles) ? categories.baseTiles : Defaults.states[state].baseTiles;
        float category = Hash01(x, y, seed, 100); // same category across restoration stages
        float baseWeight = Mathf.Clamp01(profile.baseWeight);
        float variationWeight = Mathf.Clamp(profile.variationWeight, 0f, 1f - baseWeight);
        int[] group = clean || category < baseWeight ? bases : category < baseWeight + variationWeight
            ? categories.variationTiles : categories.specialTiles;
        if (!Valid(group)) group = bases;
        float patch = Mathf.Clamp(profile.patchSize, 2f, 4f);
        float noise = CoherentNoise((x + seed % 997) / patch, (y + seed % 541) / patch, seed);
        float low = 1f, high = 0f;
        foreach (int index in bases) { low = Mathf.Min(low, Luminance[state][index]); high = Mathf.Max(high, Luminance[state][index]); }
        float target = Mathf.Lerp(low, high, noise);
        int regionX = Mathf.FloorToInt(x / patch), regionY = Mathf.FloorToInt(y / patch);
        int selected = Closest(group, state, target, regionX, regionY, seed);
        // If an authored category is too bright/dark, prefer a compatible base tile.
        if (Mathf.Abs(Luminance[state][selected] - target) > Mathf.Clamp(profile.maxToneDifference, 0.01f, 0.25f))
            selected = Closest(bases, state, target, regionX, regionY, seed);
        return selected;
    }
    // Smooth value noise: four stable anchors interpolated over patchSize cells.
    // Managed math also makes the saved editor layout reproducible outside Play.
    private static float CoherentNoise(float x, float y, int seed)
    {
        int ix = Mathf.FloorToInt(x), iy = Mathf.FloorToInt(y);
        float tx = x - ix, ty = y - iy;
        tx = tx * tx * (3f - 2f * tx); ty = ty * ty * (3f - 2f * ty);
        return Mathf.Lerp(Mathf.Lerp(Hash01(ix, iy, seed, 500), Hash01(ix + 1, iy, seed, 500), tx),
            Mathf.Lerp(Hash01(ix, iy + 1, seed, 500), Hash01(ix + 1, iy + 1, seed, 500), tx), ty);
    }
    private static bool Valid(int[] group)
    {
        if (group == null || group.Length == 0) return false;
        foreach (int index in group) if (index < 0 || index >= 48) return false;
        return true;
    }
    private static int Closest(int[] group, int state, float target, int x, int y, int seed)
    {
        int best = group[0]; float score = float.PositiveInfinity;
        foreach (int index in group)
        {
            float candidate = Mathf.Abs(Luminance[state][index] - target) + Hash01(x, y, seed, index + 200) * 0.025f;
            if (candidate < score) { score = candidate; best = index; }
        }
        return best;
    }
    public static HashSet<Vector3Int> BuildCleanCells(Tilemap map, Transform sector)
    {
        var result = new HashSet<Vector3Int>();
        if (map == null || sector == null) return result;
        foreach (PlantingSpot spot in sector.GetComponentsInChildren<PlantingSpot>(true))
        {
            Vector3Int center = map.WorldToCell(spot.transform.position);
            foreach (Vector3Int cell in map.cellBounds.allPositionsWithin)
                if (Vector2.Distance(new Vector2(center.x, center.y), new Vector2(cell.x, cell.y)) <= 1.75f) result.Add(cell);
        }
        foreach (NurserySeedlingSource source in sector.GetComponentsInChildren<NurserySeedlingSource>(true))
        {
            var visual = source.GetComponentInParent<UnityEngine.Rendering.SortingGroup>();
            var renderers = visual != null ? visual.GetComponentsInChildren<SpriteRenderer>(true) : source.GetComponentsInChildren<SpriteRenderer>(true);
            Bounds bounds = new Bounds(source.transform.position, Vector3.one * 2f);
            foreach (SpriteRenderer renderer in renderers) bounds.Encapsulate(renderer.bounds);
            bounds.Expand(new Vector3(2f, 2f, 0f));
            foreach (Vector3Int cell in map.cellBounds.allPositionsWithin)
            {
                Vector3 point = map.GetCellCenterWorld(cell);
                if (point.x >= bounds.min.x && point.x <= bounds.max.x && point.y >= bounds.min.y && point.y <= bounds.max.y) result.Add(cell);
            }
        }
        foreach (Tilemap paths in sector.gameObject.scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Tilemap>(true)))
        {
            if (paths.name != "PathsTilemap") continue;
            foreach (Vector3Int cell in paths.cellBounds.allPositionsWithin)
            {
                if (!paths.HasTile(cell)) continue;
                Vector3Int ground = map.WorldToCell(paths.GetCellCenterWorld(cell));
                for (int dy = -1; dy <= 1; dy++) for (int dx = -1; dx <= 1; dx++) result.Add(ground + new Vector3Int(dx, dy, 0));
            }
        }
        return result;
    }
    public static float Hash01(int x, int y, int seed, int salt)
    {
        unchecked
        {
            uint h = (uint)(x * 374761393 + y * 668265263 + seed * 1442695041 + salt * 1274126177);
            h = (h ^ (h >> 13)) * 1274126177u; h ^= h >> 16;
            return (h & 0x00ffffffu) / 16777216f;
        }
    }
}
