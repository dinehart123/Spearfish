using UnityEngine;
using HardArtcore.ShoreLine;

public class SurfaceBobbingMotion : MonoBehaviour
{
    public WaterSurface water;
    public Transform playerCamera;
    public Transform bobPivot;

    [Header("Surface Detection")]
    public float surfaceBand = 1f;

    [Header("Wave Sampling")]
    public float sampleDistance = 1.5f;
    public float sampleSideDistance = 0.8f;

    [Header("Bobbing")]
    public float maxTilt = 3f;
    public float smoothness = 4f;

    private void LateUpdate()
    {
        if (water == null ||
            playerCamera == null ||
            bobPivot == null)
        {
            return;
        }

        float waterHeight =
            water.SampleHeight(playerCamera.position);

        float distanceFromSurface =
            Mathf.Abs(
                playerCamera.position.y - waterHeight
            );

        float targetPitch = 0f;
        float targetRoll = 0f;

        if (distanceFromSurface <= surfaceBand)
        {
            Vector3 forward =
                Vector3.ProjectOnPlane(
                    playerCamera.forward,
                    Vector3.up
                ).normalized;

            Vector3 right =
                playerCamera.right;

            Vector3 frontPoint =
                playerCamera.position +
                forward * sampleDistance;

            Vector3 backPoint =
                playerCamera.position -
                forward * sampleDistance;

            Vector3 leftPoint =
                playerCamera.position -
                right * sampleSideDistance;

            Vector3 rightPoint =
                playerCamera.position +
                right * sampleSideDistance;

            float frontHeight =
                water.SampleHeight(frontPoint);

            float backHeight =
                water.SampleHeight(backPoint);

            float leftHeight =
                water.SampleHeight(leftPoint);

            float rightHeight =
                water.SampleHeight(rightPoint);

            targetPitch = Mathf.Clamp(
                Mathf.Atan2(
                    frontHeight - backHeight,
                    sampleDistance * 2f
                ) * Mathf.Rad2Deg,
                -maxTilt,
                maxTilt
            );

            targetRoll = Mathf.Clamp(
                Mathf.Atan2(
                    rightHeight - leftHeight,
                    sampleSideDistance * 2f
                ) * Mathf.Rad2Deg,
                -maxTilt,
                maxTilt
            );
        }

        Quaternion targetRotation =
            Quaternion.Euler(
                targetPitch,
                0f,
                -targetRoll
            );

        bobPivot.localRotation =
            Quaternion.Slerp(
                bobPivot.localRotation,
                targetRotation,
                smoothness * Time.deltaTime
            );
    }
}