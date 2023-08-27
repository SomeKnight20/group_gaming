using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Worldgen/Noise/CNoise")]
public class CNoise : Noise
{
    public override float GetPureNoiseAt(int x, int y)
    {
        return Unity.Mathematics.noise.cnoise(new Unity.Mathematics.float2((x + settings.globalSeed) / frequenzy, (y + settings.globalSeed) / frequenzy));
    }
}
