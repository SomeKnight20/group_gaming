using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Worldgen/Noise/Noise")]
public class Noise : ScriptableObject
{

    public GlobalSettings settings;
    [Tooltip("'Frequenzy' of perlin noise, aka. how often there are pixels. NOTE: Some values don't work, for example 0.5 or 0")]
    public float frequenzy = 0.5f;
    [Tooltip("'Scale' of perlin noise, aka. how big the pixels are")]
    public float scale = 1f;

    [Header("More complex noise options")]
    [Tooltip("How many times the noise loops")]
    public int octaves = 1;
    [Tooltip("Calculates a 'second frequency'")]
    public float lacunarity = 2.0f;
    [Tooltip("Used to control amplitude")]
    public float persistence = 0.5f;

    [Range(0f, 1f)]
    [Tooltip("Chance to add a tile to a specific spot at x, y")]
    public float threshold = 0.5f;

    public virtual int GenerateNoiseAt(int x, int y)
    {
        float noiseValue = Mathf.PerlinNoise(x / frequenzy + settings.globalSeed, y / frequenzy + settings.globalSeed) * scale;
        return (noiseValue < threshold) ? 1 : 0;
    }

    public virtual float GetPureNoiseAt(int x, int y)
    {
        float noiseValue = Mathf.PerlinNoise(x / frequenzy + settings.globalSeed, y / frequenzy + settings.globalSeed) * scale;
        return noiseValue;
    }

    public virtual float GenerateOctavePerlinAt(float x, float y)
    {
        x /= frequenzy;
        y /= frequenzy;

        float amplitude = 1.0f;
        float totalAmplitude = 0.0f;
        float noiseValue = 0.0f;

        for (int i = 0; i < octaves; i++)
        {
            float freq = Mathf.Pow(lacunarity, i);
            float perlinValue = Mathf.PerlinNoise(x * freq, y * freq);
            noiseValue += perlinValue * amplitude;
            totalAmplitude += amplitude;

            amplitude *= persistence;
        }

        return noiseValue / totalAmplitude;
    }
}
