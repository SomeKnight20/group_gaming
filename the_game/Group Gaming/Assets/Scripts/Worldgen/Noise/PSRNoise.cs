using UnityEngine;
using Unity.Mathematics;

[CreateAssetMenu(menuName = "Worldgen/Noise/PSRNoise")]
public class PSRNoise : Noise
{
    public float perX = 1;
    public float perY = 1;
    public override float GetPureNoiseAt(int x, int y)
    {
        return noise.psrnoise(new float2((x + settings.globalSeed) / frequenzy, (y + settings.globalSeed) / frequenzy), new float2(perX, perY));
    }
}
