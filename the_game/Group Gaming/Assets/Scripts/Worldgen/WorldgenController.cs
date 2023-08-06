using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.XR;

public class WorldgenController : MonoBehaviour
{
    [Header("Biomes")]
    [Tooltip("Biomes that can spawn")]
    public List<Biome> biomes = new List<Biome>();
    public Dictionary<Biome, float> biomeSpawnChances = new Dictionary<Biome, float>();
    Dictionary<Generator.Coord, Biome> biomeMap = new Dictionary<Generator.Coord, Biome>();

    float allBiomesWeight = 0; // All biome weights added together
    [Tooltip("How big 1 pixel is on the biome noise map")]
    public int biomeNoisePixelSize = 50;

    public Noise biomeNoise;
    public int generateBiomesX = 4;
    public int generateBiomesY = 4;

    public Tilemap tilemap;

    // private Dictionary<Vector2, float> testNoiseMap = new Dictionary<Vector2, float>();

    private void Start()
    {
        foreach (Biome biome in biomes)
        {
            biome.SetDefaultTilemap(tilemap);
        }

        RecalculateBiomeChances();
        GenerateBiomeMapAreaAt(0, 0, generateBiomesX * biomeNoisePixelSize, generateBiomesY * biomeNoisePixelSize);
        GenerateWorld(0, 0, generateBiomesX, generateBiomesY);
    }

    void RecalculateBiomeChances()
    {
        // Add biome weights together
        allBiomesWeight = 0;
        foreach (Biome biome in biomes)
        {
            allBiomesWeight += biome.spawnWeight;
        }

        // Sort biomes so in theory they take less computing power
        biomes.Sort(
            delegate (Biome p1, Biome p2)
            {
                return p2.spawnWeight.CompareTo(p1.spawnWeight);
            }
        );

        // Calculate true biome chances
        float chance = 0;
        foreach (Biome biome in biomes)
        {
            chance += biome.spawnWeight / allBiomesWeight;
            biomeSpawnChances[biome] = chance;
        }
    }

    public Generator.Coord PositionToBiomePos(int x, int y)
    {
        return new Generator.Coord(Mathf.FloorToInt(x / biomeNoisePixelSize), Mathf.FloorToInt(y / biomeNoisePixelSize));
    }

    float BiomeNoiseAt(int x, int y)
    {
        float noiseValue = biomeNoise.GetPureNoiseAt(x, y);
        return noiseValue;
    }

    Biome BiomeAt(int x, int y)
    {
        Generator.Coord biomePos = PositionToBiomePos(x, y);
        return biomeMap[biomePos];
    }
    Biome BiomeAtBiomeCoords(int biomeX, int biomeY)
    {
        return biomeMap[new Generator.Coord(biomeX, biomeY)];
    }
    void GenerateBiomeMapAreaAt(int startX, int startY, int width, int height)
    {
        // Generates a biome map for a certain area

        // Loop through each coordinate
        for (int x = startX; x < width + startX; x++)
        {
            for (int y = startY; y < height + startY; y++)
            {
                float noiseValue = BiomeNoiseAt(x, y);

                foreach (KeyValuePair<Biome, float> kvp in biomeSpawnChances)
                {
                    if (noiseValue < kvp.Value)
                    {
                        biomeMap[new Generator.Coord(x, y)] = kvp.Key;
                        break;
                    }
                }
            }
        }
    }

    void GenerateWorld(int biomeX, int biomeY, int amountX, int amountY)
    {
        for (int x = biomeX; x < amountX + biomeX; x++)
        {
            for (int y = biomeY; y < amountY + biomeY; y++)
            {
                Biome biome = BiomeAtBiomeCoords(x, y);
                biome.CreateMapFromArea(x * biomeNoisePixelSize, y * biomeNoisePixelSize, biomeNoisePixelSize, biomeNoisePixelSize);
                biome.ProcessMap(x * biomeNoisePixelSize, y * biomeNoisePixelSize, biomeNoisePixelSize, biomeNoisePixelSize);
                biome.FillTilemap(x * biomeNoisePixelSize, y * biomeNoisePixelSize, biomeNoisePixelSize, biomeNoisePixelSize);
            } 
        }
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            foreach (Biome biome in biomes)
            {
                biome.ResetMap();
            }
            RecalculateBiomeChances();
            GenerateBiomeMapAreaAt(0, 0, generateBiomesX * biomeNoisePixelSize, generateBiomesY * biomeNoisePixelSize);
            GenerateWorld(0, 0, generateBiomesX, generateBiomesY);
        }
        if (Input.GetMouseButtonDown(2))
        {
            /*testNoiseMap.Clear();

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
            }*/
        }
    }

    private void OnDrawGizmos()
    {
        // Draw biome map
        foreach (KeyValuePair<Generator.Coord, Biome> kvp in biomeMap)
        {
            if (kvp.Value == biomes[0])
            {
                Gizmos.color = new Color(0,0,255);
            } else if (kvp.Value == biomes[1])
            {
                Gizmos.color = new Color(255, 0, 0);
            }
            else
            {
                Gizmos.color = new Color(0, 255, 0);
            }
            Vector3 pos = new Vector3(kvp.Key.tileX, kvp.Key.tileY, 0);
            Gizmos.DrawCube(pos, Vector3.one);
        }
    }
}
