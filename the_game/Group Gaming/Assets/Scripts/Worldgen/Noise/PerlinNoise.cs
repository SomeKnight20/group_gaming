using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Worldgen/Noise/PerlinNoise")]
public class PerlinNoise : Noise
{
    [Tooltip("'Scale' of perlin noise, aka. how big the pixels are")]
    public float scale = 1f;

    public override float GetPureNoiseAt(int x, int y)
    {
        float noiseValue = Mathf.PerlinNoise(x / frequenzy + settings.globalSeed, y / frequenzy + settings.globalSeed) * scale;
        return noiseValue;
    }
}
