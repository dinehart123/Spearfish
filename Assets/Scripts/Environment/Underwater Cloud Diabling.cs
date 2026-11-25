using UnityEngine;
using HardArtcore.ShoreLine;

public class UnderwaterCloudVisibility : MonoBehaviour
{
    public Transform playerCamera;
    public GameObject shapedCloudsRoot;
    public WaterSurface water;

    [Header("Fallback Water Height")]
    public float waterSurfaceY = 0f;

    [Header("Wave Detection")]
    public float enterDepth = 0.05f;
    public float surfaceRevealHeight = 0.1f;

    private bool underwater;

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main.transform;

        underwater = IsBelowWaveSurface();
        ApplyCloudVisibility();
    }

    private void Update()
    {
        if (playerCamera == null ||
            shapedCloudsRoot == null)
        {
            return;
        }

        float waveHeight = GetWaveHeight();
        float cameraY = playerCamera.position.y;

        // Hide clouds when the wave is above the camera.
        if (!underwater &&
            cameraY < waveHeight - enterDepth)
        {
            underwater = true;
            ApplyCloudVisibility();
        }

        // Show clouds shortly after the camera rises above the wave.
        else if (underwater &&
                 cameraY > waveHeight + surfaceRevealHeight)
        {
            underwater = false;
            ApplyCloudVisibility();
        }
    }

    private bool IsBelowWaveSurface()
    {
        return playerCamera.position.y <
               GetWaveHeight() - enterDepth;
    }

    private float GetWaveHeight()
    {
        if (water != null)
        {
            return water.SampleHeight(
                playerCamera.position
            );
        }

        return waterSurfaceY;
    }

    private void ApplyCloudVisibility()
    {
        shapedCloudsRoot.SetActive(!underwater);
    }
}