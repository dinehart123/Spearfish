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

    // Extra distance used to catch shader-displaced wave peaks.
    public float waveSafetyMargin = 1.5f;

    public float fullEffectDepth = 30f;
    public float transitionSpeed = 6f;

    [Header("Overlay")]
    [Range(0f, 1f)]
    public float minimumOpacity = 0.35f;

    [Range(0f, 1f)]
    public float maximumOpacity = 0.85f;

    public Color underwaterColor =
        new Color(0.02f, 0.35f, 0.45f, 1f);

    [Header("Visibility")]
    public float surfaceVisibilityDistance = 12f;
    public float deepVisibilityDistance = 6f;

    [Header("Fog")]
    public bool useFog = true;

    public Color fogColor =
        new Color(0.02f, 0.25f, 0.3f, 1f);

    public float fogStartDistance = 0f;

    private float underwaterBlend;

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

        float currentWaterHeight = waterSurfaceY;

        if (water != null)
        {
            currentWaterHeight =
                water.SampleHeight(playerCamera.position);
        }

        // The safety margin catches waves that are displaced
        // higher by the water shader than SampleHeight reports.
        float depth =
            currentWaterHeight +
            waveSafetyMargin -
            playerCamera.position.y;

        float depth01 =
            Mathf.Clamp01(depth / fullEffectDepth);

        float targetBlend = depth > 0f
            ? Mathf.Lerp(0.5f, 1f, depth01)
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
    }

    private void UpdateOverlay()
    {
        if (underwaterOverlay == null)
            return;

        float opacity = underwaterBlend <= 0.01f
            ? 0f
            : Mathf.Lerp(
                minimumOpacity,
                maximumOpacity,
                underwaterBlend
            );

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
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogStartDistance =
                fogStartDistance;

            RenderSettings.fogEndDistance = Mathf.Lerp(
                surfaceVisibilityDistance,
                deepVisibilityDistance,
                underwaterBlend
            );
        }
        else
        {
            RestoreFog();
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
        SetOverlayAlpha(0f);
    }
}