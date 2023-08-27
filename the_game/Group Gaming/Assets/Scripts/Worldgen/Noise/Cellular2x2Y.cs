using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Worldgen/Noise/Cellular2x2Y")]
public class Cellular2x2Y : Noise
{
    public override float GetPureNoiseAt(int x, int y)
    {
        return Unity.Mathematics.noise.cellular2x2(new Unity.Mathematics.float2((x + settings.globalSeed) / frequenzy, (y + settings.globalSeed) / frequenzy)).y;
    }
}
