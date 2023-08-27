using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

[CreateAssetMenu(menuName = "Worldgen/Noise/PNoise")]
public class PNoise : Noise
{
    public float repX = 1;
    public float repY = 1;
    public override float GetPureNoiseAt(int x, int y)
    {
        return noise.pnoise(new float2((x + settings.globalSeed) / frequenzy, (y + settings.globalSeed) / frequenzy), new float2(repX, repY));
    }
}
