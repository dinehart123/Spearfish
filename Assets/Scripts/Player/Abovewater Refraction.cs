using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class AboveWaterSurfaceOpacity : MonoBehaviour
{
    public Transform playerCamera;

    [Header("Water Height")]
    public float waterSurfaceY = 0f;

    [Header("Opacity")]
    [Range(0f, 1f)]
    public float aboveWaterOpacity = 0.95f;

    [Range(0f, 1f)]
    public float belowWaterOpacity = 1f;

    public float transitionSpeed = 3f;

    [Header("Refraction")]
    public bool reduceRefractionAboveWater = true;

    [Range(0f, 1f)]
    public float aboveWaterRefraction = 0.1f;

    private Material waterMaterial;
    private float currentOpacity;

    private Color shallowColor;
    private Color deepColor;
    private Color horizonColor;

    private float originalRefraction;

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main.transform;

        Renderer waterRenderer = GetComponent<Renderer>();

        // Creates an instance so the original material asset is not permanently changed.
        waterMaterial = waterRenderer.material;

        if (waterMaterial.HasProperty("_ShallowColor"))
            shallowColor = waterMaterial.GetColor("_ShallowColor");

        if (waterMaterial.HasProperty("_DeepColor"))
            deepColor = waterMaterial.GetColor("_DeepColor");

        if (waterMaterial.HasProperty("_HorizonColor"))
            horizonColor = waterMaterial.GetColor("_HorizonColor");

        if (waterMaterial.HasProperty("_RefractionStrength"))
            originalRefraction =
                waterMaterial.GetFloat("_RefractionStrength");

        currentOpacity = aboveWaterOpacity;
    }

    private void Update()
    {
        if (playerCamera == null || waterMaterial == null)
            return;

        bool aboveWater =
            playerCamera.position.y >= waterSurfaceY;

        float targetOpacity = aboveWater
            ? aboveWaterOpacity
            : belowWaterOpacity;

        currentOpacity = Mathf.MoveTowards(
            currentOpacity,
            targetOpacity,
            transitionSpeed * Time.deltaTime
        );

        ApplyOpacity(currentOpacity);

        if (reduceRefractionAboveWater &&
            waterMaterial.HasProperty("_RefractionStrength"))
        {
            float targetRefraction = aboveWater
                ? aboveWaterRefraction
                : originalRefraction;

            float currentRefraction =
                waterMaterial.GetFloat("_RefractionStrength");

            waterMaterial.SetFloat(
                "_RefractionStrength",
                Mathf.MoveTowards(
                    currentRefraction,
                    targetRefraction,
                    transitionSpeed * Time.deltaTime
                )
            );
        }
    }

    private void ApplyOpacity(float opacity)
    {
        if (waterMaterial.HasProperty("_ShallowColor"))
        {
            Color color = shallowColor;
            color.a = opacity;
            waterMaterial.SetColor("_ShallowColor", color);
        }

        if (waterMaterial.HasProperty("_DeepColor"))
        {
            Color color = deepColor;
            color.a = opacity;
            waterMaterial.SetColor("_DeepColor", color);
        }

        if (waterMaterial.HasProperty("_HorizonColor"))
        {
            Color color = horizonColor;
            color.a = opacity;
            waterMaterial.SetColor("_HorizonColor", color);
        }
    }
}