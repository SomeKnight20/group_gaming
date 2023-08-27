using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Noise : ScriptableObject
{

    [Range(0f, 1f)]
    [Tooltip("Chance to add a tile to a specific spot at x, y")]
    public float threshold = 0.5f;

    [Tooltip("'Frequenzy' of perlin noise, aka. how often there are pixels. NOTE: Some values don't work, for example 0.5 or 0")]
    public float frequenzy = 0.5f;

    public GlobalSettings settings;

    public virtual int GenerateNoiseAt(int x, int y)
    {
        float noiseValue = GetPureNoiseAt(x, y);
        return (noiseValue < threshold) ? 1 : 0;
    }

    public virtual float GetPureNoiseAt(int x, int y)
    {
        return 0;
    }
}
