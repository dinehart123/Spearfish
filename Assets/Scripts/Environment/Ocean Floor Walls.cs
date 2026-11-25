using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(200)]
public class OceanFloorWalls : MonoBehaviour
{
    [Header("Wall Appearance")]
    public TerrainLayer wallTerrainLayer;

    // Optional. Leave empty to automatically create a material.
    public Material wallMaterial;

    public float textureTiling = 1f;

    [Header("Wall Size")]
    public float waterSurfaceY = 0f;
    public float wallTopAboveWater = 5f;
    public float wallDepthBelowTerrain = 100f;
    public float wallThickness = 20f;

    private void Start()
    {
        StartCoroutine(CreateWallsAfterTerrain());
    }

    private IEnumerator CreateWallsAfterTerrain()
    {
        yield return null;
        yield return new WaitForEndOfFrame();

        Terrain terrain = GetComponent<Terrain>();

        if (terrain == null || terrain.terrainData == null)
        {
            Debug.LogError("OceanFloorWalls needs a Terrain component.");
            yield break;
        }

        Material finalMaterial = GetWallMaterial();

        GameObject wallParent = new GameObject(
            "Ocean Floor Border Walls"
        );

        Vector3 terrainMin = terrain.transform.position;
        Vector3 terrainSize = terrain.terrainData.size;
        Vector3 terrainMax = terrainMin + terrainSize;

        float bottomY = terrainMin.y - wallDepthBelowTerrain;
        float topY = waterSurfaceY + wallTopAboveWater;
        float wallHeight = topY - bottomY;
        float centerY = bottomY + wallHeight / 2f;

        CreateWall(
            "North Wall",
            new Vector3(
                (terrainMin.x + terrainMax.x) / 2f,
                centerY,
                terrainMax.z + wallThickness / 2f
            ),
            new Vector3(
                terrainSize.x + wallThickness * 2f,
                wallHeight,
                wallThickness
            ),
            finalMaterial,
            wallParent.transform
        );

        CreateWall(
            "South Wall",
            new Vector3(
                (terrainMin.x + terrainMax.x) / 2f,
                centerY,
                terrainMin.z - wallThickness / 2f
            ),
            new Vector3(
                terrainSize.x + wallThickness * 2f,
                wallHeight,
                wallThickness
            ),
            finalMaterial,
            wallParent.transform
        );

        CreateWall(
            "East Wall",
            new Vector3(
                terrainMax.x + wallThickness / 2f,
                centerY,
                (terrainMin.z + terrainMax.z) / 2f
            ),
            new Vector3(
                wallThickness,
                wallHeight,
                terrainSize.z + wallThickness * 2f
            ),
            finalMaterial,
            wallParent.transform
        );

        CreateWall(
            "West Wall",
            new Vector3(
                terrainMin.x - wallThickness / 2f,
                centerY,
                (terrainMin.z + terrainMax.z) / 2f
            ),
            new Vector3(
                wallThickness,
                wallHeight,
                terrainSize.z + wallThickness * 2f
            ),
            finalMaterial,
            wallParent.transform
        );
    }

    private Material GetWallMaterial()
    {
        if (wallMaterial != null)
            return wallMaterial;

        Shader shader = Shader.Find(
            "Universal Render Pipeline/Lit"
        );

        if (shader == null)
            shader = Shader.Find("Standard");

        Material material = new Material(shader);
        material.name = "Ocean Floor Wall Material";

        if (wallTerrainLayer == null)
        {
            Debug.LogWarning(
                "No Terrain Layer assigned to OceanFloorWalls."
            );

            return material;
        }

        if (wallTerrainLayer.diffuseTexture != null)
        {
            if (material.HasProperty("_BaseMap"))
                material.SetTexture(
                    "_BaseMap",
                    wallTerrainLayer.diffuseTexture
                );

            if (material.HasProperty("_MainTex"))
                material.SetTexture(
                    "_MainTex",
                    wallTerrainLayer.diffuseTexture
                );
        }

        if (wallTerrainLayer.normalMapTexture != null)
        {
            if (material.HasProperty("_BumpMap"))
            {
                material.SetTexture(
                    "_BumpMap",
                    wallTerrainLayer.normalMapTexture
                );

                material.EnableKeyword("_NORMALMAP");
            }
        }

        if (material.HasProperty("_BaseMap"))
        {
            material.SetTextureScale(
                "_BaseMap",
                new Vector2(textureTiling, textureTiling)
            );
        }

        if (material.HasProperty("_MainTex"))
        {
            material.SetTextureScale(
                "_MainTex",
                new Vector2(textureTiling, textureTiling)
            );
        }

        if (material.HasProperty("_Smoothness"))
            material.SetFloat("_Smoothness", 0.15f);

        return material;
    }

    private void CreateWall(
        string wallName,
        Vector3 position,
        Vector3 scale,
        Material material,
        Transform parent
    )
    {
        GameObject wall = GameObject.CreatePrimitive(
            PrimitiveType.Cube
        );

        wall.name = wallName;
        wall.transform.position = position;
        wall.transform.localScale = scale;
        wall.transform.parent = parent;

        Renderer renderer = wall.GetComponent<Renderer>();
        renderer.material = material;
    }
}