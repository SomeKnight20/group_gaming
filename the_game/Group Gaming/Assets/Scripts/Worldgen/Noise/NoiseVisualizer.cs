using UnityEngine;

public class NoiseVisualizer : MonoBehaviour
{
    public Noise noise;
    Texture2D texture;
    public int width = 256;
    public int height = 256;

    public bool isPixels = true;

    void Start()
    {
        Generate();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Generate();
        }
    }

    void Generate()
    {
        texture = new Texture2D(width, height);
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                float noiseValue;
                if (isPixels)
                {
                    noiseValue = noise.GenerateNoiseAt(x, y);

                }
                else
                {
                    noiseValue = noise.GetPureNoiseAt(x, y);
                }
                float colorValue = noiseValue;
                Color color = new Color(colorValue, colorValue, colorValue);
                texture.SetPixel(x, y, color);
            }
        }
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = texture;
    }
}
