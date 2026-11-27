using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Terrain))]
[RequireComponent(typeof(TerrainCollider))]
public class RandomOceanFloor : MonoBehaviour
{
    [Header("Terrain Size")]
    public float worldSize = 500f;
    public float minHeight = 0f;
    public float maxHeight = 30f;

    [Header("Random Seed")]
    public bool randomizeEveryStart = true;
    public int seed = 12345;

    [Header("Terrain Shape")]
    public float broadNoiseScale = 0.006f;
    public float ridgeNoiseScale = 0.018f;
    public float detailNoiseScale = 0.05f;

    [Range(0f, 1f)]
    public float broadNoiseStrength = 0.3f;

    [Range(0f, 1f)]
    public float ridgeStrength = 0.35f;

    [Range(0f, 1f)]
    public float detailStrength = 0.08f;

    [Header("Cliffs")]
    [Range(0f, 1f)]
    public float cliffThreshold = 0.62f;

    [Range(0f, 1f)]
    public float cliffStrength = 0.25f;

    [Header("Random Hills and Trenches")]
    public int featureCount = 12;
    public float smallestFeatureRadius = 10f;
    public float largestFeatureRadius = 45f;

    [Range(0f, 1f)]
    public float featureStrength = 0.25f;

    [Header("Terrain Openings")]
    public bool createOpenings = false;
    public int openingCount = 3;
    public float smallestOpeningRadius = 3f;
    public float largestOpeningRadius = 8f;

    private struct Feature
    {
        public Vector2 position;
        public float radius;
        public float strength;
    }

    private void Start()
    {
        GenerateTerrain();
    }

    private void GenerateTerrain()
    {
        Terrain terrain = GetComponent<Terrain>();
        TerrainCollider terrainCollider = GetComponent<TerrainCollider>();

        // Create runtime-only TerrainData.
        TerrainData runtimeData = Instantiate(terrain.terrainData);

        terrain.terrainData = runtimeData;
        terrainCollider.terrainData = runtimeData;

        runtimeData.size = new Vector3(
            worldSize,
            maxHeight - minHeight,
            worldSize
        );

        int actualSeed = randomizeEveryStart
            ? UnityEngine.Random.Range(0, 1000000)
            : seed;

        System.Random random = new System.Random(actualSeed);

        float offsetX = random.Next(-100000, 100000);
        float offsetZ = random.Next(-100000, 100000);

        List<Feature> features = CreateFeatures(random);

        int resolution = runtimeData.heightmapResolution;
        float[,] heights = new float[resolution, resolution];

        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float worldX =
                    x / (float)(resolution - 1) * worldSize;

                float worldZ =
                    z / (float)(resolution - 1) * worldSize;

                float broadNoise = FractalNoise(
                    worldX + offsetX,
                    worldZ + offsetZ,
                    broadNoiseScale,
                    4
                );

                float ridgeNoise = RidgedNoise(
                    worldX + offsetX,
                    worldZ + offsetZ,
                    ridgeNoiseScale,
                    3
                );

                float detailNoise = FractalNoise(
                    worldX + offsetX,
                    worldZ + offsetZ,
                    detailNoiseScale,
                    3
                );

                float height =
                    0.08f +
                    broadNoise * broadNoiseStrength +
                    ridgeNoise * ridgeStrength +
                    detailNoise * detailStrength;

                float cliffMask = SmoothStep(
                    cliffThreshold - 0.08f,
                    cliffThreshold + 0.08f,
                    broadNoise
                );

                height += cliffMask * cliffStrength;

                foreach (Feature feature in features)
                {
                    float distance = Vector2.Distance(
                        new Vector2(worldX, worldZ),
                        feature.position
                    );

                    if (distance < feature.radius)
                    {
                        float falloff = 1f - distance / feature.radius;
                        falloff *= falloff;

                        height += feature.strength * falloff;
                    }
                }

                heights[z, x] = Mathf.Clamp01(height);
            }
        }

        runtimeData.SetHeights(0, 0, heights);

        if (createOpenings)
        {
            CreateTerrainOpenings(runtimeData, random);
        }

        // Make sure the collider uses the generated terrain.
        terrainCollider.terrainData = runtimeData;
        Physics.SyncTransforms();
    }

    private List<Feature> CreateFeatures(System.Random random)
    {
        List<Feature> features = new List<Feature>();

        for (int i = 0; i < featureCount; i++)
        {
            Feature feature = new Feature();

            feature.position = new Vector2(
                (float)random.NextDouble() * worldSize,
                (float)random.NextDouble() * worldSize
            );

            feature.radius = Mathf.Lerp(
                smallestFeatureRadius,
                largestFeatureRadius,
                (float)random.NextDouble()
            );

            bool isHill = random.NextDouble() > 0.35f;

            feature.strength = isHill
                ? Mathf.Lerp(
                    0.05f,
                    featureStrength,
                    (float)random.NextDouble()
                )
                : -Mathf.Lerp(
                    0.05f,
                    featureStrength,
                    (float)random.NextDouble()
                );

            features.Add(feature);
        }

        return features;
    }

    private void CreateTerrainOpenings(
        TerrainData data,
        System.Random random
    )
    {
        int resolution = data.holesResolution;
        bool[,] holes = new bool[resolution, resolution];

        // True means solid terrain.
        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                holes[z, x] = true;
            }
        }

        for (int i = 0; i < openingCount; i++)
        {
            float centerX =
                (float)random.NextDouble() * resolution;

            float centerZ =
                (float)random.NextDouble() * resolution;

            float radius = Mathf.Lerp(
                smallestOpeningRadius,
                largestOpeningRadius,
                (float)random.NextDouble()
            );

            for (int z = 0; z < resolution; z++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float distance = Vector2.Distance(
                        new Vector2(x, z),
                        new Vector2(centerX, centerZ)
                    );

                    if (distance < radius)
                    {
                        holes[z, x] = false;
                    }
                }
            }
        }

        data.SetHoles(0, 0, holes);
    }

    private float FractalNoise(
        float x,
        float z,
        float scale,
        int octaves
    )
    {
        float value = 0f;
        float amplitude = 1f;
        float frequency = scale;
        float maximum = 0f;

        for (int i = 0; i < octaves; i++)
        {
            value += Mathf.PerlinNoise(
                x * frequency,
                z * frequency
            ) * amplitude;

            maximum += amplitude;
            amplitude *= 0.5f;
            frequency *= 2f;
        }

        return value / maximum;
    }

    private float RidgedNoise(
        float x,
        float z,
        float scale,
        int octaves
    )
    {
        float noise = FractalNoise(x, z, scale, octaves);
        return 1f - Mathf.Abs(noise * 2f - 1f);
    }

    private float SmoothStep(
        float edge0,
        float edge1,
        float value
    )
    {
        float t = Mathf.Clamp01(
            (value - edge0) / (edge1 - edge0)
        );

        return t * t * (3f - 2f * t);
    }
}