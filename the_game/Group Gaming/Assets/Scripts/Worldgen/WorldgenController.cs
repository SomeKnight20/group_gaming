using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using UnityEngine.XR;

public class WorldgenController : MonoBehaviour
{
    [Tooltip("Biomes that can spawn")]
    public List<Biome> biomes = new List<Biome>();
    public Dictionary<Biome, float> biomeSpawnChances = new Dictionary<Biome, float>();
    public float allBiomesWeight = 0;

    public Noise biomeNoise;
    public int width = 300;
    public int height = 300;

    private Dictionary<Vector2, float> testNoiseMap = new Dictionary<Vector2, float>();

    private void Start()
    {
        RecalculateBiomeChances();
    }

    void RecalculateBiomeChances()
    {
        allBiomesWeight = 0;
        foreach (Biome biome in biomes)
        {
            allBiomesWeight += biome.spawnWeight;
        }

        biomes.Sort(
            delegate (Biome p1, Biome p2)
            {
                return p2.spawnWeight.CompareTo(p1.spawnWeight);
            }
        );

        float chance = 0;
        foreach (Biome biome in biomes)
        {
            chance += biome.spawnWeight / allBiomesWeight;
            biomeSpawnChances[biome] = chance;
            Debug.Log(chance);
        }
    }

    float BiomeNoiseAt(int x, int y)
    {
        float noiseValue = biomeNoise.GetPureNoiseAt(x, y);
        return noiseValue;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(2))
        {
            testNoiseMap.Clear();

            RecalculateBiomeChances();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float noiseValue = BiomeNoiseAt(x,y);

                    float color1 = 0f;
                    float color2 = 1f;
                    float color3 = 0.5f;

                    int i = 0;
                    foreach (KeyValuePair<Biome, float> kvp in biomeSpawnChances)
                    {
                        i++;
                        if(noiseValue < kvp.Value)
                        {
                            noiseValue = kvp.Value;

                            if (i == 1)
                            {
                                noiseValue = color1;
                            } else if (i == 2)
                            {
                                noiseValue = color2;
                            } else if ( i == 3)
                            {
                                noiseValue = color3;
                            }
                            testNoiseMap[new Vector2(x, y)] = noiseValue;
                            break;
                        }
                    }

                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        foreach (KeyValuePair<Vector2, float> kvp in testNoiseMap)
        {
            Gizmos.color = new Color(kvp.Value * 1, kvp.Value * 1, kvp.Value * 1);
            Vector3 pos = new Vector3(kvp.Key.x, kvp.Key.y, 0);
            Gizmos.DrawCube(pos, Vector3.one);
        }
    }
}
