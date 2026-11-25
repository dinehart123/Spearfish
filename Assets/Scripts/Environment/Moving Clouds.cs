using UnityEngine;

public class MovingClouds : MonoBehaviour
{
    public Vector2 windDirection = new Vector2(0.01f, 0.003f);
    public float speed = 1f;

    private Material cloudMaterial;
    private Vector2 offset;

    private void Start()
    {
        cloudMaterial = GetComponent<Renderer>().material;
    }

    //Move clouds
    private void Update()
    {
        offset += windDirection * speed * Time.deltaTime;
        cloudMaterial.SetTextureOffset("_BaseMap", offset);
    }
}