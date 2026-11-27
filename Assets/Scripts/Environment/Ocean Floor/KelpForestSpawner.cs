using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns patches ("forests") of kelp on a runtime-generated Terrain.
/// Field names match your existing scene data, so your current values carry over.
/// Waits for the terrain generator to finish before spawning.
/// </summary>
public class KelpForestSpawner : MonoBehaviour
{
    public enum SizeMode { Multiplier, TopDepthBelowSurface }
    public enum UprightMode { Auto, None, PlusX, MinusX, PlusY, MinusY, PlusZ, MinusZ }

    [System.Serializable]
    public class KelpEntry
    {
        public GameObject prefab;
        public float spawnWeight = 1f;
        public Vector3 rotationOffset;   // extra manual rotation; leave at 0 when using Auto/axis modes
        [Tooltip("Auto = detect the stalk axis. If a kelp lies sideways, force which of the mesh's local axes points up.")]
        public UprightMode upright = UprightMode.Auto;
        [Tooltip("Multiplies the spacing around this kelp. Use > 1 for big kelp that needs more room (e.g. 2 = double the gap).")]
        public float spacingMultiplier = 1f;
    }

    [Header("References")]
    public Terrain oceanFloor;
    public List<KelpEntry> kelpPrefabs = new List<KelpEntry>();
    public float waterSurfaceY = 0f;

    [Header("Patches")]
    [Tooltip("Number of forests is rolled once per spawn between these (inclusive)")]
    public int minimumForestCount = 6;
    public int maximumForestCount = 12;
    [Tooltip("Each forest rolls a random kelp count between these (inclusive)")]
    public int minimumKelpPerForest = 15;
    public int maximumKelpPerForest = 45;
    public float forestRadius = 25f;
    [Tooltip("Each forest rolls its own spacing (min distance between kelp) between these. Low = dense, high = sparse")]
    public float minimumSpacing = 1f;
    public float maximumSpacing = 3f;
    [Tooltip("0 = perfectly round patches, 1 = very ragged edges")]
    [Range(0f, 1f)] public float edgeRoughness = 0.5f;
    [Tooltip("Higher = kelp bunches toward the center of a patch. 0.5 = even spread")]
    [Range(0.3f, 1.5f)] public float centerBias = 0.7f;
    [Tooltip("Keep patches this far from world origin (your boat)")]
    public float clearRadiusAroundOrigin = 40f;
    [Tooltip("Min distance between a new forest and existing ones, as a multiple of forestRadius. Rolled per forest between these.")]
    public float minimumPatchSeparation = 1f;
    public float maximumPatchSeparation = 2.5f;

    [Header("Filters")]
    public bool useDepthFilter = false;
    public float minimumDepth = 10f;
    public float maximumDepth = 100f;
    public bool useSlopeFilter = false;
    public float maximumTerrainSlope = 60f;

    [Header("Size")]
    [Tooltip("Multiplier = random scale (min/max Scale below). TopDepthBelowSurface = each kelp grows until its tip is a random number of meters below the surface.")]
    public SizeMode sizeMode = SizeMode.TopDepthBelowSurface;
    [Tooltip("Multiplier mode: multiplier applied to the prefab's own scale (NOT an absolute size)")]
    public float minimumScale = 0.8f;
    public float maximumScale = 1.4f;
    [Tooltip("Meters below the surface the kelp tip may reach. In TopDepth mode each kelp rolls between min and max. In both modes, kelp is never allowed above 'minimum'.")]
    public float minimumTopDepth = 2f;
    public float maximumTopDepth = 10f;

    [Header("Look")]
    [Tooltip("Sink the base slightly into the sand so it never floats")]
    public float verticalOffset = -0.2f;
    [Range(0f, 1f)] public float alignToSlope = 0.3f;
    [Tooltip("Detects each mesh's longest axis (the stalk) and rotates it to point up. Set rotationOffsets to 0 when this is on.")]
    public bool autoUpright = true;

    [Header("Sway")]
    public bool addSway = true;
    [Tooltip("Max lean in degrees, rolled per kelp")]
    public float minimumSwayAngle = 4f;
    public float maximumSwayAngle = 10f;
    [Tooltip("Sway speed, rolled per kelp")]
    public float minimumSwaySpeed = 0.3f;
    public float maximumSwaySpeed = 0.7f;
    [Tooltip("Smaller = bigger, slower waves rolling through a forest")]
    public float swayWaveScale = 0.05f;
    [Tooltip("Direction of the current in degrees (0 = +X, 90 = +Z)")]
    public float currentAngle = 0f;
    [Tooltip("Kelp farther than this from the camera stops animating (saves performance)")]
    public float swayUpdateDistance = 90f;

    [Header("Randomness")]
    public bool useRandomSeed = true;
    public int seed = 12345;

    [Header("Timing")]
    public bool spawnOnStart = true;
    public float terrainWaitTimeout = 10f;

    Transform root;
    readonly List<Vector3> centers = new List<Vector3>();
    System.Random rng;
    readonly Dictionary<GameObject, Quaternion> uprightCache = new Dictionary<GameObject, Quaternion>();
    readonly Dictionary<GameObject, Bounds?> boundsCache = new Dictionary<GameObject, Bounds?>();

    IEnumerator Start()
    {
        if (!spawnOnStart) yield break;
        yield return WaitForTerrain();
        Spawn();
    }

    // The terrain is generated at runtime, so make sure it's actually shaped before sampling it.
    IEnumerator WaitForTerrain()
    {
        yield return null;
        yield return null;
        float waited = 0f;
        while (waited < terrainWaitTimeout)
        {
            if (TerrainLooksGenerated()) yield break;
            waited += Time.unscaledDeltaTime;
            yield return null;
        }
        Debug.LogWarning("KelpForestSpawner: terrain still looks flat after waiting; spawning anyway.");
    }

    bool TerrainLooksGenerated()
    {
        if (oceanFloor == null) return false;
        Vector3 size = oceanFloor.terrainData.size;
        Vector3 o = oceanFloor.transform.position;
        float min = float.MaxValue, max = float.MinValue;
        for (int ix = 1; ix < 6; ix++)
            for (int iz = 1; iz < 6; iz++)
            {
                float h = oceanFloor.SampleHeight(new Vector3(o.x + size.x * ix / 6f, 0, o.z + size.z * iz / 6f));
                min = Mathf.Min(min, h);
                max = Mathf.Max(max, h);
            }
        return max - min > 0.5f;
    }

    [ContextMenu("Respawn")]
    public void Spawn()
    {
        if (oceanFloor == null) { Debug.LogError("KelpForestSpawner: assign Ocean Floor (Terrain)."); return; }

        Clear();
        uprightCache.Clear();
        boundsCache.Clear();
        rng = new System.Random(useRandomSeed ? System.Environment.TickCount : seed);

        var valid = kelpPrefabs.FindAll(k => k != null && k.prefab != null && k.spawnWeight > 0f);
        if (valid.Count == 0) { Debug.LogError("KelpForestSpawner: no valid kelp prefabs assigned."); return; }

        foreach (var k in valid)
            if (k.prefab.scene.IsValid())
                Debug.LogWarning($"KelpForestSpawner: '{k.prefab.name}' is a scene object, not a prefab asset. Drag the asset from the Project window instead.", this);

        root = new GameObject("KelpForests").transform;
        root.SetParent(transform, false);
        root.position = Vector3.zero;   // spawner's own Y position is irrelevant

        PickCenters();

        int total = 0;
        for (int f = 0; f < centers.Count; f++)
            total += SpawnForest(f, centers[f], valid);

        Debug.Log($"KelpForestSpawner: {centers.Count} forests, {total} kelp.");
    }

    void PickCenters()
    {
        centers.Clear();
        Vector3 size = oceanFloor.terrainData.size;
        Vector3 o = oceanFloor.transform.position;
        float margin = forestRadius;

        int fLo = Mathf.Max(0, Mathf.Min(minimumForestCount, maximumForestCount));
        int fHi = Mathf.Max(minimumForestCount, maximumForestCount);
        int forestCount = rng.Next(fLo, fHi + 1);

        float sepLo = Mathf.Min(minimumPatchSeparation, maximumPatchSeparation);
        float sepHi = Mathf.Max(minimumPatchSeparation, maximumPatchSeparation);
        float separation = Lerp(sepLo, sepHi);   // re-rolled each time a forest is placed

        int attempts = Mathf.Max(1, forestCount) * 60;
        while (centers.Count < forestCount && attempts-- > 0)
        {
            float x = o.x + Lerp(margin, size.x - margin);
            float z = o.z + Lerp(margin, size.z - margin);

            if (!TryGetFloor(x, z, out Vector3 p, out _)) continue;
            if (new Vector2(x, z).magnitude < clearRadiusAroundOrigin + forestRadius) continue;

            bool tooClose = false;
            foreach (var c in centers)
                if ((new Vector2(c.x, c.z) - new Vector2(x, z)).magnitude < forestRadius * separation) { tooClose = true; break; }
            if (tooClose) continue;

            centers.Add(p);
            separation = Lerp(sepLo, sepHi);
        }

        if (centers.Count < forestCount)
            Debug.LogWarning($"KelpForestSpawner: only found room for {centers.Count}/{forestCount} forests (filters too strict?).");
    }

    int SpawnForest(int index, Vector3 center, List<KelpEntry> entries)
    {
        var parent = new GameObject($"KelpForest_{index}").transform;
        parent.SetParent(root, false);

        float noiseOffset = (float)rng.NextDouble() * 1000f;
        var placed = new List<Vector2>();
        var placedMul = new List<float>();
        float totalWeight = 0f;
        foreach (var e in entries) totalWeight += e.spawnWeight;

        float spacing = Lerp(Mathf.Min(minimumSpacing, maximumSpacing), Mathf.Max(minimumSpacing, maximumSpacing));
        int lo = Mathf.Max(0, Mathf.Min(minimumKelpPerForest, maximumKelpPerForest));
        int hi = Mathf.Max(minimumKelpPerForest, maximumKelpPerForest);
        int target = rng.Next(lo, hi + 1);

        int attempts = target * 12;
        while (placed.Count < target && attempts-- > 0)
        {
            // Ragged patch edge: radius varies with angle using Perlin noise.
            float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
            float n = Mathf.PerlinNoise(noiseOffset + Mathf.Cos(angle) * 1.5f, noiseOffset + Mathf.Sin(angle) * 1.5f);
            float edgeRadius = forestRadius * Mathf.Lerp(1f - edgeRoughness, 1f, n);
            float r = edgeRadius * Mathf.Pow((float)rng.NextDouble(), centerBias);

            float x = center.x + Mathf.Cos(angle) * r;
            float z = center.z + Mathf.Sin(angle) * r;

            var entry = PickEntry(entries, totalWeight);
            float myMul = Mathf.Max(0.01f, entry.spacingMultiplier);

            // The gap between two kelp uses the larger of their multipliers, so big kelp pushes neighbors away.
            var xz = new Vector2(x, z);
            bool tooClose = false;
            for (int i = 0; i < placed.Count; i++)
            {
                float need = spacing * Mathf.Max(myMul, placedMul[i]);
                if ((placed[i] - xz).sqrMagnitude < need * need) { tooClose = true; break; }
            }
            if (tooClose) continue;

            if (!TryGetFloor(x, z, out Vector3 pos, out Vector3 normal)) continue;

            Quaternion yaw = Quaternion.AngleAxis((float)rng.NextDouble() * 360f, Vector3.up);
            Quaternion tilt = Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(Vector3.up, normal), alignToSlope);
            Quaternion rot = tilt * yaw * Quaternion.Euler(entry.rotationOffset) * GetUprightFix(entry);

            Vector3 spawnPos = pos + Vector3.up * verticalOffset;

            // Height available between the sand and the highest allowed tip position.
            float ceilingY = waterSurfaceY - Mathf.Min(minimumTopDepth, maximumTopDepth);
            float available = ceilingY - spawnPos.y;
            if (available < 0.5f) continue;   // water too shallow for kelp here

            float unitHeight = TopHeightAtUnitScale(entry.prefab, rot);   // tip height above pivot at multiplier 1
            float scaleMul;
            if (sizeMode == SizeMode.TopDepthBelowSurface && unitHeight > 0.001f)
            {
                float topDepth = Lerp(Mathf.Min(minimumTopDepth, maximumTopDepth), Mathf.Max(minimumTopDepth, maximumTopDepth));
                float desired = Mathf.Min((waterSurfaceY - topDepth) - spawnPos.y, available);
                if (desired < 0.5f) continue;
                scaleMul = desired / unitHeight;
            }
            else
            {
                scaleMul = Lerp(minimumScale, maximumScale);
                if (unitHeight > 0.001f) scaleMul = Mathf.Min(scaleMul, available / unitHeight);   // never poke above the cap
            }

            var go = Instantiate(entry.prefab, spawnPos, rot, parent);
            go.SetActive(true); // in case the source object lived under an inactive parent
            go.transform.localScale = entry.prefab.transform.localScale * scaleMul;

            if (addSway)
            {
                var sway = go.AddComponent<KelpSway>();
                sway.Init(rot,
                    Lerp(Mathf.Min(minimumSwayAngle, maximumSwayAngle), Mathf.Max(minimumSwayAngle, maximumSwayAngle)),
                    Lerp(Mathf.Min(minimumSwaySpeed, maximumSwaySpeed), Mathf.Max(minimumSwaySpeed, maximumSwaySpeed)),
                    swayWaveScale, currentAngle, swayUpdateDistance, (float)rng.NextDouble() * 100f);
            }

            placed.Add(xz);
            placedMul.Add(myMul);
        }
        return placed.Count;
    }

    // Terrain height + normal at a world XZ, with depth/slope filters applied.
    bool TryGetFloor(float x, float z, out Vector3 point, out Vector3 normal)
    {
        point = default; normal = Vector3.up;

        var t = oceanFloor.transform.position;
        var size = oceanFloor.terrainData.size;
        if (x < t.x || z < t.z || x > t.x + size.x || z > t.z + size.z) return false;

        float y = oceanFloor.SampleHeight(new Vector3(x, 0, z)) + t.y;   // SampleHeight is relative to terrain Y
        float depth = waterSurfaceY - y;
        if (useDepthFilter && (depth < minimumDepth || depth > maximumDepth)) return false;

        float nx = (x - t.x) / size.x, nz = (z - t.z) / size.z;
        if (useSlopeFilter && oceanFloor.terrainData.GetSteepness(nx, nz) > maximumTerrainSlope) return false;

        normal = oceanFloor.terrainData.GetInterpolatedNormal(nx, nz);
        point = new Vector3(x, y, z);
        return true;
    }

    KelpEntry PickEntry(List<KelpEntry> entries, float totalWeight)
    {
        float roll = (float)rng.NextDouble() * totalWeight;
        foreach (var e in entries)
        {
            roll -= e.spawnWeight;
            if (roll <= 0f) return e;
        }
        return entries[entries.Count - 1];
    }

    // Rotation that turns the mesh's longest local axis into "up", so sideways-exported meshes stand correctly.
    Quaternion GetUprightFix(KelpEntry entry)
    {
        GameObject prefab = entry.prefab;
        if (!autoUpright || entry.upright == UprightMode.None) return Quaternion.identity;

        switch (entry.upright)
        {
            case UprightMode.PlusX:  return Quaternion.FromToRotation(Vector3.right, Vector3.up);
            case UprightMode.MinusX: return Quaternion.FromToRotation(Vector3.left, Vector3.up);
            case UprightMode.PlusY:  return Quaternion.identity;
            case UprightMode.MinusY: return Quaternion.FromToRotation(Vector3.down, Vector3.up);
            case UprightMode.PlusZ:  return Quaternion.FromToRotation(Vector3.forward, Vector3.up);
            case UprightMode.MinusZ: return Quaternion.FromToRotation(Vector3.back, Vector3.up);
        }

        if (uprightCache.TryGetValue(prefab, out var cached)) return cached;

        Quaternion fix = Quaternion.identity;
        if (GetLocalBounds(prefab, out Bounds b))
        {
            Vector3 s = b.size;
            int axis = (s.x >= s.y && s.x >= s.z) ? 0 : (s.y >= s.z ? 1 : 2);
            Vector3 dir = Vector3.zero;
            dir[axis] = b.center[axis] >= 0f ? 1f : -1f;   // mesh extends from the pivot toward the tip
            fix = Quaternion.FromToRotation(dir, Vector3.up);
        }

        uprightCache[prefab] = fix;
        return fix;
    }

    // Combined renderer bounds of the prefab, in the prefab root's local space (ignores the root's own scale/rotation).
    bool GetLocalBounds(GameObject prefab, out Bounds result)
    {
        if (boundsCache.TryGetValue(prefab, out var cached)) { result = cached.GetValueOrDefault(); return cached.HasValue; }

        Bounds? found = null;
        Matrix4x4 toRoot = prefab.transform.worldToLocalMatrix;
        foreach (var r in prefab.GetComponentsInChildren<Renderer>(true))
        {
            Bounds lb = r.localBounds;
            Matrix4x4 m = toRoot * r.transform.localToWorldMatrix;
            for (int i = 0; i < 8; i++)
            {
                Vector3 c = lb.center + Vector3.Scale(lb.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                Vector3 pt = m.MultiplyPoint3x4(c);
                if (found == null) found = new Bounds(pt, Vector3.zero);
                else { var f = found.Value; f.Encapsulate(pt); found = f; }
            }
        }
        boundsCache[prefab] = found;
        result = found.GetValueOrDefault();
        return found.HasValue;
    }

    // How far the highest point of the mesh sits above its pivot (world units) at multiplier 1, given the final rotation.
    float TopHeightAtUnitScale(GameObject prefab, Quaternion rot)
    {
        if (!GetLocalBounds(prefab, out Bounds b)) return 0f;
        Vector3 ls = prefab.transform.localScale;
        float maxY = 0f;
        for (int i = 0; i < 8; i++)
        {
            Vector3 c = b.center + Vector3.Scale(b.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
            maxY = Mathf.Max(maxY, (rot * Vector3.Scale(c, ls)).y);
        }
        return maxY;
    }

    float Lerp(float a, float b) => a + (b - a) * (float)rng.NextDouble();

    public void Clear()
    {
        if (root != null) Destroy(root.gameObject);
        centers.Clear();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        foreach (var c in centers)
            Gizmos.DrawWireSphere(c, forestRadius);
    }
}