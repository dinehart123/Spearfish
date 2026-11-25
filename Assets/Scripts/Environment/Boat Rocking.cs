using UnityEngine;
using HardArtcore.ShoreLine;

public class BoatWaveMotion : MonoBehaviour
{
    public WaterSurface water;

    [Header("Movement")]
    public float heightOffset = 0.5f;
    [Range(0f, 1f)] public float verticalFollow = 0.25f;
    public float movementSmoothness = 3f;

    [Header("Rocking")]
    [Range(0f, 1f)] public float rockingStrength = 0.2f;
    public float maxTilt = 3f;
    public float forwardDistance = 2f;
    public float sideDistance = 0.7f;

    private float startingY;
    private float waterLevel;
    private Quaternion startingRotation;

    private void Start()
    {
        startingY = transform.position.y;
        waterLevel = water.transform.position.y;
        startingRotation = transform.localRotation;
    }

    private void LateUpdate()
    {
        if (water == null)
            return;

        // Sample the wave from the middle of the boat.
        float centerWaveHeight = water.SampleHeight(transform.position);

        float targetY = startingY +
            (centerWaveHeight - waterLevel) * verticalFollow +
            heightOffset;

        Vector3 position = transform.position;
        position.y = Mathf.Lerp(
            position.y,
            targetY,
            movementSmoothness * Time.deltaTime
        );

        transform.position = position;

        // Sample the ends only for gentle rocking.
        Vector3 bow = transform.TransformPoint(
            new Vector3(forwardDistance, 0f, 0f));

        Vector3 stern = transform.TransformPoint(
            new Vector3(-forwardDistance, 0f, 0f));

        Vector3 left = transform.TransformPoint(
            new Vector3(0f, 0f, -sideDistance));

        Vector3 right = transform.TransformPoint(
            new Vector3(0f, 0f, sideDistance));

        float pitch = Mathf.Atan2(
            water.SampleHeight(bow) - water.SampleHeight(stern),
            forwardDistance * 2f
        ) * Mathf.Rad2Deg * rockingStrength;

        float roll = Mathf.Atan2(
            water.SampleHeight(right) - water.SampleHeight(left),
            sideDistance * 2f
        ) * Mathf.Rad2Deg * rockingStrength;

        pitch = Mathf.Clamp(pitch, -maxTilt, maxTilt);
        roll = Mathf.Clamp(roll, -maxTilt, maxTilt);

        Quaternion targetRotation =
            Quaternion.Euler(pitch, 0f, -roll) * startingRotation;

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            movementSmoothness * Time.deltaTime
        );
    }
}