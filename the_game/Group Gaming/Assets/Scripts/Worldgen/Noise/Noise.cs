using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName ="Worldgen/Noise/Noise")]
public class Noise : ScriptableObject
{

    public int seed = 0;
    [Tooltip("'Frequenzy' of perlin noise, aka. how often there are pixels. NOTE: Some values don't work, for example 0.5 or 0")]
    public float frequenzy = 0.5f;
    [Tooltip("'Scale' of perlin noise, aka. how big the pixels are")]
    public float scale = 1f;

    [Range(0f, 1f)]
    [Tooltip("Chance to add a tile to a specific spot at x, y")]
    public float threshold = 0.5f;

    public virtual int GenerateNoiseAt(int x, int y)
    {
        float noiseValue = Mathf.PerlinNoise(x / frequenzy + seed, y / frequenzy + seed) * scale;
        return (noiseValue < threshold) ? 1 : 0;
    }
}
