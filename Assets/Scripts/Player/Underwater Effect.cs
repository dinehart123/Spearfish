using UnityEngine;
using UnityEngine.UI;
using HardArtcore.ShoreLine;

public class UnderwaterEffect : MonoBehaviour
{
    public Transform playerCamera;
    public Image underwaterOverlay;

    [Header("Water")]
    public WaterSurface water;
    public float waterSurfaceY = 0f;
    public float waveSafetyMargin = 1f;
    public float fullEffectDepth = 30f;
    public float transitionSpeed = 4f;

    [Header("Overlay")]
    [Range(0f, 1f)]
    public float minimumOpacity = 0.1f;

    [Range(0f, 1f)]
    public float maximumOpacity = 0.65f;

    public Color underwaterColor =
        new Color(0.01f, 0.28f, 0.38f, 1f);

    [Header("Fog")]
    public bool useFog = true;

    public Color fogColor =
        new Color(0.01f, 0.20f, 0.27f, 1f);

    public float shallowFogDensity = 0.012f;
    public float deepFogDensity = 0.08f;

    [Header("Underwater Sky")]
    public Color underwaterBackground =
        new Color(0.01f, 0.12f, 0.16f, 1f);

    private Camera cameraComponent;
    private Skybox cameraSkybox;

    private float underwaterBlend;
    private bool skyHidden;

    private Material originalSkybox;
    private bool originalCameraSkyboxEnabled;
    private CameraClearFlags originalClearFlags;
    private Color originalBackgroundColor;

    private bool originalFog;
    private Color originalFogColor;
    private FogMode originalFogMode;
    private float originalFogDensity;
    private float originalFogStartDistance;
    private float originalFogEndDistance;

    private void Start()
    {
        if (playerCamera == null)
        {
            Camera mainCamera = Camera.main;

            if (mainCamera != null)
                playerCamera = mainCamera.transform;
        }

        if (water == null)
            water = FindObjectOfType<WaterSurface>();

        if (playerCamera != null)
        {
            cameraComponent =
                playerCamera.GetComponent<Camera>();

            cameraSkybox =
                playerCamera.GetComponent<Skybox>();
        }

        if (cameraComponent != null)
        {
            originalClearFlags =
                cameraComponent.clearFlags;

            originalBackgroundColor =
                cameraComponent.backgroundColor;
        }

        if (cameraSkybox != null)
        {
            originalCameraSkyboxEnabled =
                cameraSkybox.enabled;
        }

        originalSkybox = RenderSettings.skybox;

        originalFog = RenderSettings.fog;
        originalFogColor = RenderSettings.fogColor;
        originalFogMode = RenderSettings.fogMode;
        originalFogDensity = RenderSettings.fogDensity;
        originalFogStartDistance =
            RenderSettings.fogStartDistance;
        originalFogEndDistance =
            RenderSettings.fogEndDistance;

        SetOverlayAlpha(0f);
    }

    private void Update()
    {
        if (playerCamera == null)
            return;

        float currentWaterHeight =
            waterSurfaceY;

        if (water != null)
        {
            currentWaterHeight =
                water.SampleHeight(
                    playerCamera.position
                );
        }

        // Actual depth does not include the safety margin.
        float actualDepth =
            currentWaterHeight -
            playerCamera.position.y;

        // The safety margin helps fog begin before shader
        // displaced wave peaks become visually obvious.
        float fogDepth =
            actualDepth + waveSafetyMargin;

        float depth01 =
            Mathf.Clamp01(
                fogDepth / fullEffectDepth
            );

        float targetBlend = fogDepth > 0f
            ? Mathf.Lerp(0.15f, 1f, depth01)
            : 0f;

        targetBlend = Mathf.SmoothStep(
            0f,
            1f,
            targetBlend
        );

        underwaterBlend = Mathf.MoveTowards(
            underwaterBlend,
            targetBlend,
            transitionSpeed * Time.deltaTime
        );

        UpdateOverlay();
        UpdateFog();

        // Skybox uses actual depth, not the fog safety margin.
        UpdateSky(actualDepth);
    }

    private void UpdateOverlay()
    {
        if (underwaterOverlay == null)
            return;

        float opacity = Mathf.Lerp(
            0f,
            maximumOpacity,
            underwaterBlend
        );

        if (underwaterBlend > 0.01f)
        {
            opacity = Mathf.Max(
                opacity,
                minimumOpacity
            );
        }

        Color color = underwaterColor;
        color.a = opacity;

        underwaterOverlay.color = color;
    }

    private void UpdateFog()
    {
        if (!useFog)
            return;

        if (underwaterBlend > 0.01f)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode =
                FogMode.ExponentialSquared;

            RenderSettings.fogColor = fogColor;

            RenderSettings.fogDensity =
                Mathf.Lerp(
                    shallowFogDensity,
                    deepFogDensity,
                    underwaterBlend
                );
        }
        else
        {
            RestoreFog();
        }
    }

    private void UpdateSky(float actualDepth)
    {
        // Hide the sky only when actually below the wave.
        if (!skyHidden && actualDepth > 0.05f)
        {
            skyHidden = true;

            RenderSettings.skybox = null;

            if (cameraSkybox != null)
                cameraSkybox.enabled = false;

            if (cameraComponent != null)
            {
                cameraComponent.clearFlags =
                    CameraClearFlags.SolidColor;

                cameraComponent.backgroundColor =
                    underwaterBackground;
            }
        }

        // Restore the sky once slightly above the wave.
        else if (skyHidden && actualDepth < -0.05f)
        {
            skyHidden = false;
            RestoreSky();
        }
    }

    private void SetOverlayAlpha(float alpha)
    {
        if (underwaterOverlay == null)
            return;

        Color color = underwaterColor;
        color.a = alpha;

        underwaterOverlay.color = color;
    }

    private void RestoreSky()
    {
        RenderSettings.skybox = originalSkybox;

        if (cameraSkybox != null)
        {
            cameraSkybox.enabled =
                originalCameraSkyboxEnabled;
        }

        if (cameraComponent != null)
        {
            cameraComponent.clearFlags =
                originalClearFlags;

            cameraComponent.backgroundColor =
                originalBackgroundColor;
        }
    }

    private void RestoreFog()
    {
        RenderSettings.fog = originalFog;
        RenderSettings.fogColor = originalFogColor;
        RenderSettings.fogMode = originalFogMode;
        RenderSettings.fogDensity = originalFogDensity;
        RenderSettings.fogStartDistance =
            originalFogStartDistance;
        RenderSettings.fogEndDistance =
            originalFogEndDistance;
    }

    private void OnDisable()
    {
        RestoreFog();
        RestoreSky();
        SetOverlayAlpha(0f);
    }
}