using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(100)]
[RequireComponent(typeof(Terrain))]
public class RandomOceanFloorTextures : MonoBehaviour
{
    public TerrainLayer sandLayer;
    public TerrainLayer rockLayer;

    [Range(0f, 1f)]
    public float rockPercentage = 0.3f;

    public float rockPatchScale = 0.025f;
    public bool randomizeEveryStart = true;
    public int seed = 12345;

    private void Start()
    {
        Terrain terrain = GetComponent<Terrain>();
        TerrainData data = terrain.terrainData;

        data.terrainLayers = new TerrainLayer[]
        {
            sandLayer,
            rockLayer
        };

        int resolution = data.alphamapResolution;
        float[,,] terrainLayers =
            new float[resolution, resolution, 2];

        float[,] noise =
            new float[resolution, resolution];

        int randomSeed = randomizeEveryStart
            ? Random.Range(0, 1000000)
            : seed;

        float offsetX = randomSeed * 0.37f;
        float offsetZ = randomSeed * 0.71f;

        List<float> values = new List<float>();

        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float value = Mathf.PerlinNoise(
                    offsetX + x * rockPatchScale,
                    offsetZ + z * rockPatchScale
                );

                noise[x, z] = value;
                values.Add(value);
            }
        }

        values.Sort();

        int cutoffIndex = Mathf.FloorToInt(
            values.Count * (1f - rockPercentage)
        );

        float rockCutoff = values[cutoffIndex];

        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                bool isRock = noise[x, z] >= rockCutoff;

                if (isRock)
                {
                    terrainLayers[x, z, 0] = 0f;
                    terrainLayers[x, z, 1] = 1f;
                }
                else
                {
                    terrainLayers[x, z, 0] = 1f;
                    terrainLayers[x, z, 1] = 0f;
                }
            }
        }

        data.SetAlphamaps(0, 0, terrainLayers);
    }
}