using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Generator;
using System.Threading;

public class WorldgenController : MonoBehaviour
{
    [Header("Settings")]
    public GlobalSettings settings;

    [Header("Biomes")]
    [Tooltip("Biomes that can spawn")]
    public List<Biome> biomes = new List<Biome>();

    [Header("Biome Generation Chances")]
    [Tooltip("If biome is trying to copy a surrounding biome, this chance is tested")]
    public float copyChanceOfABiome = 0.5f;

    Dictionary<Biome, float> biomeSpawnChances;
    Dictionary<Coord, Biome> biomeMap;
    HashSet<Coord> generatedBiomes;
    float allBiomesWeight = 0; // All biome weights added together
    System.Random biomeNoiseRandomizer;
    System.Random biomeNoiseCopyRandomizer;
    System.Random biomeNoiseExpansionRandomizer;

    // Tilemap stuff
    protected Dictionary<Coord, TileAtlasTile> allTilemapData; // Contains all generated tiles
    [Header("Tilemap")]
    public Tilemap tilemap;

    [Header("Biome Generation")]
    [Tooltip("How big 1 pixel is on the biome noise map")]
    public int biomeNoisePixelSizeWidth = 50;
    [Tooltip("How big 1 pixel is on the biome noise map")]
    public int biomeNoisePixelSizeHeight = 50;
    public int generateBiomesX = 4;
    public int generateBiomesY = 4;

    [Header("Generator Settings")]
    public bool connectToClosestBiome = true;
    public bool blendBiomes = true;

    // private Dictionary<Vector2, float> testNoiseMap = new Dictionary<Vector2, float>();

    private void Start()
    {
        ResetRandomizers();

        biomeMap = new Dictionary<Coord, Biome>();
        generatedBiomes = new HashSet<Coord>();
        allTilemapData = new Dictionary<Coord, TileAtlasTile>();

        foreach (Biome biome in biomes)
        {
            biome.SetDefaultTilemap(tilemap);
            biome.SetWorldGenerator(this);
            biome.SetGlobalSettings(settings);
            biome.OnStart();
        }

        CalculateBiomeChances();
        GenerateBiomeMapAreaAt(0, 0, generateBiomesX * biomeNoisePixelSizeWidth, generateBiomesY * biomeNoisePixelSizeHeight);
        GenerateWorld(0, 0, generateBiomesX, generateBiomesY);
    }

    void CalculateBiomeChances()
    {
        biomeSpawnChances = new Dictionary<Biome, float>();

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

    public Coord PositionToBiomePos(int x, int y)
    {
        return new Coord(Mathf.FloorToInt(x / biomeNoisePixelSizeWidth), Mathf.FloorToInt(y / biomeNoisePixelSizeHeight));
    }

    float BiomeNoiseAt(int x, int y)
    {
        //float noiseValue = biomeNoise.GenerateNoiseAt(x, y);
        // Perlin noise is bad at making random biome maps, since the more biomes the more the last ones are?
        float noiseValue = (float)biomeNoiseRandomizer.NextDouble();
        return noiseValue;
    }

    public Biome BiomeAt(int x, int y)
    {
        Coord biomePos = PositionToBiomePos(x, y);
        return biomeMap[biomePos];
    }
    public Biome BiomeAtBiomeCoords(int biomeX, int biomeY)
    {
        return biomeMap[new Coord(biomeX, biomeY)];
    }
    void GenerateBiomeMapAreaAt(int startX, int startY, int width, int height)
    {
        // Loop through each coordinate

        for (int x = startX; x < width + startX; x++)
        {
            for (int y = startY; y < height + startY; y++)
            {
                CalculateBiomeMapAt(x, y);
            }
        }
    }

    void CalculateBiomeMapAt(int x, int y)
    {
        float noiseValue = BiomeNoiseAt(x, y);

        foreach (KeyValuePair<Biome, float> kvp in biomeSpawnChances)
        {
            if (noiseValue <= kvp.Value)
            {
                Biome selectedBiome = kvp.Key;
                Coord coord = new Coord(x, y);

                selectedBiome = AttemptToCopyBiomeAt(x, y, selectedBiome);
                AttemptToExpandAt(x, y, selectedBiome.expansionChance, selectedBiome.expansionChanceDecayAdditional, selectedBiome);

                biomeMap[coord] = selectedBiome;
                break;
            }
        }
    }

    void AttemptToExpandAt(int x, int y, float chance, float decayAdditional, Biome biomeToExpand)
    {
        float minExpandChance = (float)biomeNoiseExpansionRandomizer.NextDouble();

        // If biome should expand
        if (minExpandChance > chance)
        {
            return;
        }

        for (int expandX = x - 1; expandX <= x + 1; expandX++)
        {
            for (int expandY = y - 1; expandY <= y + 1; expandY++)
            {
                Coord expandCoord = new Coord(expandX, expandY);
                // Skip itself and don't expand into the same biome
                if (biomeToExpand == biomeMap.GetValueOrDefault(expandCoord, null) || (expandX == x && expandY == y))
                {
                    continue;
                }

                float randomValue = (float)biomeNoiseExpansionRandomizer.NextDouble();

                // If we should expand
                if (randomValue <= chance)
                {
                    biomeMap[expandCoord] = biomeToExpand;
                    AttemptToExpandAt(expandX, expandY, chance - decayAdditional, decayAdditional, biomeToExpand);
                }
            }
        }
    }

    Biome AttemptToCopyBiomeAt(int x, int y, Biome selectedBiome)
    {
        // Attempt to copy surrounding biomes
        bool doCopy = (float)biomeNoiseCopyRandomizer.NextDouble() <= selectedBiome.copyChance ? true : false;
        if (doCopy)
        {
            for (int copyX = x - 1; copyX <= x + 1; copyX++)
            {
                for (int copyY = y - 1; copyY <= y + 1; copyY++)
                {
                    // If biome is generated at position, attempt to copy it
                    if (biomeMap.ContainsKey(new Coord(copyX, copyY)))
                    {
                        float randomValue = (float)biomeNoiseCopyRandomizer.NextDouble();
                        if (randomValue <= copyChanceOfABiome)
                        {
                            return BiomeAt(copyX, copyY);
                        }
                    }
                }
            }
        }

        return selectedBiome;
    }

    void AddBiomeAt(int biomeX, int biomeY)
    {
        this.generatedBiomes.Add(new Coord(biomeX, biomeY));
    }
    public bool IsBiomeGeneratedAt(int biomeX, int biomeY)
    {
        return this.generatedBiomes.Contains(new Coord(biomeX, biomeY));
    }
    public Dictionary<Coord, TileAtlasTile> GetAllTilemapData()
    {
        return this.allTilemapData;
    }

    public TileAtlasTile GetTileAt(int x, int y)
    {
        return allTilemapData[new Coord(x, y)];
    }

    public void SetTileAt(int x, int y, TileAtlasTile tile)
    {
        SetTileAt(new Coord(x, y), tile);
    }

    public void SetTileAt(Coord coord, TileAtlasTile tile)
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

    public void SetTiles(Dictionary<Coord, TileAtlasTile> tiles1, TileBase[] tiles2, Vector3Int[] tilePositions)
    {
        // Create a thread to improve performance
        Thread t = new Thread(delegate ()
        {
            foreach (KeyValuePair<Coord, TileAtlasTile> tileKVP in tiles1)
            {
                if (tileKVP.Value.tile == null)
                {
                    this.allTilemapData.Remove(tileKVP.Key);
                }
                else
                {
                    this.allTilemapData[tileKVP.Key] = tileKVP.Value;
                }
            }
        });
        t.Start();

        tilemap.SetTiles(tilePositions, tiles2);

        t.Join();
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
        biomeNoiseCopyRandomizer = new System.Random(settings.globalSeed);
        biomeNoiseExpansionRandomizer = new System.Random(settings.globalSeed);
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
        return this.allTilemapData.ContainsKey(new Coord(x, y));
    }

    public bool TileExistsAt(Coord coord)
    {
        return this.allTilemapData.ContainsKey(coord);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ResetMap();
            ResetRandomizers();

            biomeMap = new Dictionary<Coord, Biome>();
            generatedBiomes = new HashSet<Coord>();
            allTilemapData = new Dictionary<Coord, TileAtlasTile>();

            foreach (Biome biome in biomes)
            {
                biome.SetDefaultTilemap(tilemap);
                biome.SetWorldGenerator(this);
                biome.SetGlobalSettings(settings);
                biome.OnStart();
            }

            CalculateBiomeChances();
            GenerateBiomeMapAreaAt(0, 0, generateBiomesX * biomeNoisePixelSizeWidth, generateBiomesY * biomeNoisePixelSizeHeight);
            GenerateWorld(0, 0, generateBiomesX, generateBiomesY);
        }
    }

    private void OnDrawGizmos()
    {
        //return;
        // Draw biome map, Causes big lag but good for changing biome noise
        /*foreach (KeyValuePair<Coord, Biome> kvp in biomeMap)
        {
            for (int i = 0; i < biomes.Count; i++)
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
        /*foreach (KeyValuePair<Coord, float> kvp in noises)
        {
            float color = 1 * kvp.Value;
            Gizmos.color = new Color(color, color, color, 255);
            Vector3 pos = new Vector3(kvp.Key.tileX, kvp.Key.tileY, 0);
            Gizmos.DrawCube(pos, Vector3.one);
        }*/
    }
}
