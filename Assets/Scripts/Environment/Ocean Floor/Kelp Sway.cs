using UnityEngine;

/// <summary>
/// Gentle current-driven sway. Tilts the whole kelp around its base using smooth noise.
/// Neighboring plants share a traveling wave, so patches ripple together like real kelp.
/// Added automatically by KelpForestSpawner.
/// </summary>
public class KelpSway : MonoBehaviour
{
    float maxAngle, speed, waveScale, phase, updateDistance;
    Vector3 currentDir;
    float along;
    Quaternion baseRot;

    static Transform camT;

    public void Init(Quaternion rot, float maxAngle, float speed, float waveScale, float currentAngleDeg, float updateDistance, float phase)
    {
        baseRot = rot;
        this.maxAngle = maxAngle;
        this.speed = speed;
        this.waveScale = waveScale;
        this.updateDistance = updateDistance;
        this.phase = phase;

        float a = currentAngleDeg * Mathf.Deg2Rad;
        currentDir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
        along = Vector3.Dot(transform.position, currentDir);   // position along the current, so waves travel through the forest
    }

    void Update()
    {
        // Skip kelp that is far from the camera.
        if (camT == null && Camera.main != null) camT = Camera.main.transform;
        if (camT != null && (camT.position - transform.position).sqrMagnitude > updateDistance * updateDistance) return;

        // Traveling sine wave along the current (clearly visible), plus a little noise so it isn't perfectly regular.
        float cycle = (Time.time * speed + along * waveScale) * Mathf.PI * 2f;
        float wave = Mathf.Sin(cycle);
        float noise = Mathf.PerlinNoise(along * waveScale * 3f + Time.time * speed * 0.5f, phase) * 2f - 1f;
        float side = Mathf.Sin(cycle * 0.7f + phase);

        // Constant lean with the current, with the wave swinging around it.
        float lean = maxAngle * (0.3f + 0.7f * (0.75f * wave + 0.25f * noise));
        float sideLean = maxAngle * 0.35f * side;

        Quaternion tilt = Quaternion.AngleAxis(lean, Vector3.Cross(Vector3.up, currentDir))
                        * Quaternion.AngleAxis(sideLean, currentDir);

        transform.rotation = tilt * baseRot;
    }
}