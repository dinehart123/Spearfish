using UnityEngine;

public class WaterWaves : MonoBehaviour
{
    public Vector2 waveSpeed = new Vector2(0.02f, 0.01f);

    private Material waterMaterial;
    private Vector2 offset;

    private void Start()
    {
        waterMaterial = GetComponent<Renderer>().material;
    }

    private void Update()
    {
        offset += waveSpeed * Time.deltaTime;
        waterMaterial.SetTextureOffset("_BumpMap", offset);
    }
}