using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.XR;

public class WorldgenController : MonoBehaviour
{
    [Header("Settings")]
    public GlobalSettings settings;

    [Header("Biomes")]
    [Tooltip("Biomes that can spawn")]
    public List<Biome> biomes = new List<Biome>();
    public Dictionary<Biome, float> biomeSpawnChances = new Dictionary<Biome, float>();
    Dictionary<Generator.Coord, Biome> biomeMap = new Dictionary<Generator.Coord, Biome>();
    HashSet<Generator.Coord> generatedBiomes = new HashSet<Generator.Coord>();
    protected Dictionary<Generator.Coord, TileAtlasTile> allTilemapData = new Dictionary<Generator.Coord, TileAtlasTile>(); // Contains all generated tiles

    float allBiomesWeight = 0; // All biome weights added together
    [Tooltip("How big 1 pixel is on the biome noise map")]
    public int biomeNoisePixelSizeWidth = 50;
    public int biomeNoisePixelSizeHeight = 50;
    System.Random biomeNoiseRandomizer = new System.Random();

    public Noise biomeNoise;
    public int generateBiomesX = 4;
    public int generateBiomesY = 4;

    [Header("Generator Settings")]
    public bool connectToClosestBiome = true;
    public bool blendBiomes = true;

    public Tilemap tilemap;

    // Debugging
    public List<Color> colors = new List<Color>();
    public Dictionary<Generator.Coord, float> noises = new Dictionary<Generator.Coord, float>();

    // private Dictionary<Vector2, float> testNoiseMap = new Dictionary<Vector2, float>();

    private void Start()
    {
        biomeNoiseRandomizer = new System.Random(settings.globalSeed);

        foreach (Biome biome in biomes)
        {
            biome.SetDefaultTilemap(tilemap);
            biome.SetWorldGenerator(this);
            biome.SetGlobalSettings(settings);
            biome.OnStart();
        }

        RecalculateBiomeChances();
        GenerateBiomeMapAreaAt(0, 0, generateBiomesX * biomeNoisePixelSizeWidth, generateBiomesY * biomeNoisePixelSizeHeight);
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
        return new Generator.Coord(Mathf.FloorToInt(x / biomeNoisePixelSizeWidth), Mathf.FloorToInt(y / biomeNoisePixelSizeHeight));
    }

    float BiomeNoiseAt(int x, int y)
    {
        float noiseValue = biomeNoise.GenerateOctavePerlinAt(x, y);
        // Perlin noise is bad at making random biome maps, since the more biomes the more the last ones are?
        //float noiseValue = (float) biomeNoiseRandomizer.NextDouble();
        return noiseValue;
    }

    public Biome BiomeAt(int x, int y)
    {
        Generator.Coord biomePos = PositionToBiomePos(x, y);
        return biomeMap[biomePos];
    }
    public Biome BiomeAtBiomeCoords(int biomeX, int biomeY)
    {
        return biomeMap[new Generator.Coord(biomeX, biomeY)];
    }
    void GenerateBiomeMapAreaAt(int startX, int startY, int width, int height)
    {
        // Generates a biome map for a certain area
        noises.Clear();
        // Loop through each coordinate
        for (int x = startX; x < width + startX; x++)
        {
            for (int y = startY; y < height + startY; y++)
            {
                float noiseValue = BiomeNoiseAt(x, y);
                noises.Add(new Generator.Coord(x, y), noiseValue);

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
    void AddBiomeAt(int biomeX, int biomeY)
    {
        this.generatedBiomes.Add(new Generator.Coord(biomeX, biomeY));
    }
    public bool IsBiomeGeneratedAt(int biomeX, int biomeY)
    {
        return this.generatedBiomes.Contains(new Generator.Coord(biomeX, biomeY));
    }
    public Dictionary<Generator.Coord, TileAtlasTile> GetAllTilemapData()
    {
        return this.allTilemapData;
    }

    public TileAtlasTile GetTileAt(int x, int y)
    {
        return allTilemapData[new Generator.Coord(x, y)];
    }

    public void SetTileAt(int x, int y, TileAtlasTile tile)
    {
        SetTileAt(new Generator.Coord(x, y), tile);
    }

    public void SetTileAt(Generator.Coord coord, TileAtlasTile tile)
    {
        TileBase tileBase = null;
        if (tile == null)
        {
            this.allTilemapData.Remove(coord);
        }
        else
        {
            this.allTilemapData[coord] = tile;
            tileBase = tile.tile;
        }
        this.tilemap.SetTile(new Vector3Int(coord.tileX, coord.tileY, 0), tileBase);
    }


    void GenerateWorld(int biomeX, int biomeY, int amountX, int amountY)
    {
        // Generates the world

        ResetRandomizers();

        // Loop through each biome amount we want to generate and make the biomes and tiles
        for (int x = biomeX; x < amountX + biomeX; x++)
        {
            for (int y = biomeY; y < amountY + biomeY; y++)
            {
                Biome biome = BiomeAtBiomeCoords(x, y);
                biome.CreateMapFromArea(x * biomeNoisePixelSizeWidth, y * biomeNoisePixelSizeHeight, biomeNoisePixelSizeWidth, biomeNoisePixelSizeHeight);

                if (this.connectToClosestBiome)
                {
                    biome.ConnectToClosestBiome(x, y);
                }

                biome.ProcessMap(x * biomeNoisePixelSizeWidth, y * biomeNoisePixelSizeHeight, biomeNoisePixelSizeWidth, biomeNoisePixelSizeHeight);
                biome.FillTilemap(x * biomeNoisePixelSizeWidth, y * biomeNoisePixelSizeHeight, biomeNoisePixelSizeWidth, biomeNoisePixelSizeHeight);
                biome.FillBiomePassages();


                if (this.blendBiomes)
                {
                    biome.BlendIntoSurroundedBiomes(x, y);
                }

                AddBiomeAt(x, y);
            }
        }
    }

    void ResetRandomizers()
    {
        biomeNoiseRandomizer = new System.Random(settings.globalSeed);
    }

    void ResetMap()
    {
        generatedBiomes.Clear();
        allTilemapData.Clear();
        foreach (Biome biome in biomes)
        {
            biome.OnStart();
        }
    }

    public bool TileExistsAt(int x, int y)
    {
        return this.allTilemapData.ContainsKey(new Generator.Coord(x, y));
    }

    public bool TileExistsAt(Generator.Coord coord)
    {
        return this.allTilemapData.ContainsKey(coord);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ResetMap();
            ResetRandomizers();
            foreach (Biome biome in biomes)
            {
                biome.ResetMap();
            }
            RecalculateBiomeChances();
            GenerateBiomeMapAreaAt(0, 0, generateBiomesX * biomeNoisePixelSizeWidth, generateBiomesY * biomeNoisePixelSizeHeight);
            GenerateWorld(0, 0, generateBiomesX, generateBiomesY);
        }
    }

    private void OnDrawGizmos()
    {
        //return;
        // Draw biome map, Causes big lag but good for changing biome noise
        /*foreach (KeyValuePair<Generator.Coord, Biome> kvp in biomeMap)
        {
            for(int i = 0; i < biomes.Count; i++)
            {
                if (kvp.Value == biomes[i])
                {
                    Gizmos.color = colors[i];
                    break;
                }
            }
            Vector3 pos = new Vector3(kvp.Key.tileX, kvp.Key.tileY, 0);
            Gizmos.DrawCube(pos, Vector3.one);
        }*/
        /*foreach (KeyValuePair<Generator.Coord, float> kvp in noises)
        {
            float color = 1 * kvp.Value;
            Gizmos.color = new Color(color, color, color, 255);
            Vector3 pos = new Vector3(kvp.Key.tileX, kvp.Key.tileY, 0);
            Gizmos.DrawCube(pos, Vector3.one);
        }*/
    }
}
