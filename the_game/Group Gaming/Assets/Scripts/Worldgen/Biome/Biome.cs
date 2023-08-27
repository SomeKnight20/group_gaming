using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Generator;

[CreateAssetMenu(menuName = "Worldgen/Biome/Biome")]
public class Biome : ScriptableObject
{
    [Tooltip("What generator system this biome uses")]
    [SerializeField]
    protected Generator generator;
    protected WorldgenController worldGenerator; // Controls Worldgeneration

    [Header("Generation")]
    [Tooltip("Chance to spawn this biome. 1 = default")]
    public float spawnWeight = 1;
    [Tooltip("%-Chance that this biome copies the biome next to it instead of being itself")]
    [Range(0f, 1f)]
    public float copyChance = 0.5f;
    [Range(0f, 1f)]
    [Tooltip("Chance to do an expansion next to it")]
    public float expansionChance = 0.5f;
    [Range(0f, 1f)]
    [Tooltip("Decreases the chance to further expand the biome")]
    public float expansionChanceDecayAdditional = 0.1f;

    [Header("Connections")]
    [Tooltip("How wide are the tunnels that connect two biomes together")]
    public int biomeConnectionRadiusMin = 3;
    [Tooltip("How wide are the tunnels that connect two biomes together")]
    public int biomeConnectionRadiusMax = 3;
    [Tooltip("Minimum scan area for tiles when connecting two biomes together (in tiles)")]
    public int biomeConnectionScanSize = 5;

    [Tooltip("Should biome try to force connect to top biome (Only if the biome which it tries to connect to is generated)")]
    public bool forceConnectToTopBiome = false;
    [Tooltip("Should biome try to force connect to bottom biome (Only if the biome which it tries to connect to is generated)")]
    public bool forceConnectToBottomBiome = false;
    [Tooltip("Should biome try to force connect to left biome (Only if the biome which it tries to connect to is generated)")]
    public bool forceConnectToLeftBiome = false;
    [Tooltip("Should biome try to force connect to right biome (Only if the biome which it tries to connect to is generated)")]
    public bool forceConnectToRightBiome = false;
    [Tooltip("Should biome try to force connect to a random biome (Only if any biome around this one is generated)")]
    public bool connectToRandomBiome = true;

    [Header("Biome Blending")]
    [Tooltip("Minimum radius for blending circle")]
    public int minBlendRadius = 0;
    [Tooltip("Maximum radius for blending circle")]
    public int maxBlendRadius = 8;
    [Tooltip("If there is an empty tile to blend, should we blend with it or ignore this position")]
    public bool allowEmptyBlending = false;
    [Tooltip("If there is an empty tile to blend, should we blend with the last tile we used instead. NOTE: 'allowEmptyBlending' must be false")]
    public bool useLastTileIfEmptyBlend = true;

    [Header("Tiles")]
    public TileAtlas tileAtlas;
    protected Tilemap tilemap;

    // Global settings
    protected GlobalSettings settings;

    // Connected to biomes
    public HashSet<Biome> connectedBiomes = new HashSet<Biome>();

    protected Dictionary<Coord, TileAtlasTile> tilemapData = new Dictionary<Coord, TileAtlasTile>();

    // Biome passages
    protected List<Coord> biomePassageCoords = new List<Coord>();

    public virtual void OnStart()
    {
        generator.OnStart();
    }

    public void SetGlobalSettings(GlobalSettings globalSettings)
    {
        this.settings = globalSettings;
        generator.SetGlobalSettings(globalSettings);
    }

    public virtual void SetDefaultTilemap(Tilemap tilemap)
    {
        this.tilemap = tilemap;
    }

    public Dictionary<Coord, TileAtlasTile> GetTilemapData()
    {
        return this.tilemapData;
    }

    public void SetWorldGenerator(WorldgenController controller)
    {
        worldGenerator = controller;
    }

    public virtual void ResetMap()
    {
        generator.ResetMap();
        tilemapData.Clear();
        connectedBiomes.Clear();
        biomePassageCoords.Clear();

        if (this.tilemap == null)
        {
            return;
        }

        // Clear the area first
        this.tilemap.ClearAllTiles();
    }

    public virtual void CreateMapFromArea(int startX, int startY, int width, int height)
    {
        // Generates the base layout of the map
        generator.CreateMapFromArea(startX, startY, width, height);
    }
    public virtual void ProcessMap(int startX, int startY, int width, int height)
    {
        // This basically makes the map data generated from "generator" better
        this.tilemapData = new Dictionary<Coord, TileAtlasTile>();
    }

    public virtual void FillTilemap(int startX, int startY, int width, int height)
    {
        // This fills the tilemap with tiles
        if (this.tilemap == null)
        {
            return;
        }

        TileBase[] tilesToAdd = new TileBase[width * height];
        Vector3Int[] tilePositions = new Vector3Int[width * height];

        for (int i = 0; i < tilemapData.Count; i++)
        {
            KeyValuePair<Coord, TileAtlasTile> tile = this.tilemapData.ElementAt(i);

            //Coord coord = tile.Key;
            //TileAtlasTile tileData = tile.Value;
            //worldGenerator.SetTileAt(coord, tileData);

            tilesToAdd[i] = tile.Value.tile;
            tilePositions[i] = tile.Key.Vector3Int(0);
        }

        worldGenerator.SetTiles(this.tilemapData, tilesToAdd, tilePositions);

        generator.ResetMap();
    }

    public virtual void BlendIntoSurroundedBiomes(int biomeX, int biomeY)
    {
        int x;
        int y;
        System.Random rand = new System.Random(settings.globalSeed); // Random number generator
        int radius = rand.Next(minBlendRadius, maxBlendRadius);
        TileAtlasTile blendingTile = null; // What tile to blend with

        // If there is biome below and it's not the same as this biome
        if (worldGenerator.IsBiomeGeneratedAt(biomeX, biomeY - 1) && worldGenerator.BiomeAtBiomeCoords(biomeX, biomeY - 1) != this)
        {
            // Blending into bottom biome
            y = biomeY * worldGenerator.biomeNoisePixelSizeHeight;
            for (x = biomeX * worldGenerator.biomeNoisePixelSizeWidth; x < (biomeX + 1) * worldGenerator.biomeNoisePixelSizeWidth; x += radius)
            {
                DoBlending(x, y, ref blendingTile, radius);
                radius = rand.Next(minBlendRadius, maxBlendRadius); // Generate a new random radius to make the blending better
            }
        }

        // If there is biome above and it's not the same as this biome
        if (worldGenerator.IsBiomeGeneratedAt(biomeX, biomeY + 1) && worldGenerator.BiomeAtBiomeCoords(biomeX, biomeY + 1) != this)
        {
            // Blending into above biome
            y = (biomeY + 1) * worldGenerator.biomeNoisePixelSizeHeight - 1;
            for (x = biomeX * worldGenerator.biomeNoisePixelSizeWidth; x < (biomeX + 1) * worldGenerator.biomeNoisePixelSizeWidth; x += radius)
            {
                DoBlending(x, y, ref blendingTile, radius);
                radius = rand.Next(minBlendRadius, maxBlendRadius); // Generate a new random radius to make the blending better
            }
        }

        // If there is biome to the left and it's not the same as this biome
        if (worldGenerator.IsBiomeGeneratedAt(biomeX - 1, biomeY) && worldGenerator.BiomeAtBiomeCoords(biomeX - 1, biomeY) != this)
        {
            // Blending into left biome
            x = biomeX * worldGenerator.biomeNoisePixelSizeWidth;
            for (y = biomeY * worldGenerator.biomeNoisePixelSizeHeight; y < (biomeY + 1) * worldGenerator.biomeNoisePixelSizeHeight; y += radius)
            {
                DoBlending(x, y, ref blendingTile, radius);
                radius = rand.Next(minBlendRadius, maxBlendRadius); // Generate a new random radius to make the blending better
            }
        }

        // If there is biome to the right and it's not the same as this biome
        if (worldGenerator.IsBiomeGeneratedAt(biomeX + 1, biomeY) && worldGenerator.BiomeAtBiomeCoords(biomeX + 1, biomeY) != this)
        {
            // Blending into right biome
            x = (biomeX + 1) * worldGenerator.biomeNoisePixelSizeWidth - 1;
            for (y = biomeY * worldGenerator.biomeNoisePixelSizeHeight; y < (biomeY + 1) * worldGenerator.biomeNoisePixelSizeHeight; y += radius)
            {
                DoBlending(x, y, ref blendingTile, radius);
                radius = rand.Next(minBlendRadius, maxBlendRadius); // Generate a new random radius to make the blending better
            }
        }
    }
    protected void DoBlending(int x, int y, ref TileAtlasTile blendingTile, int radius)
    {
        // If there is no tile or air at a position
        if (!worldGenerator.TileExistsAt(x, y))
        {
            // If we want to blend with air
            if (allowEmptyBlending)
            {
                blendingTile = null;

            }
            // If we want to use the last tile we blended with
            else if (!useLastTileIfEmptyBlend && blendingTile != null)
            {
                return;
            }
        }
        else // If a tile exists at (x,y)
        {
            blendingTile = worldGenerator.GetTileAt(x, y);
        }

        List<Coord> circleCoords = DrawCircle(new Coord(x, y), radius); // List of blending tiles

        // Blend tiles
        foreach (Coord coord in circleCoords)
        {
            // We don't want to blend into air
            if (!worldGenerator.TileExistsAt(coord))
            {
                continue;
            }
            worldGenerator.SetTileAt(coord, blendingTile);
        }

    }
    public void ConnectToClosestBiome(int biomeX, int biomeY)
    {
        // Connects this biome to the closes one nearby

        Dictionary<Coord, Biome> nearbyBiomes = new Dictionary<Coord, Biome>();

        // Loop through biomes around this one and find one
        for (int x = biomeX - 1; x <= biomeX + 1; x++)
        {
            for (int y = biomeY - 1; y <= biomeY + 1; y++)
            {
                if (x == biomeX || y == biomeY && !(x == biomeX && y == biomeY))
                {
                    if (worldGenerator.IsBiomeGeneratedAt(x, y))
                    {
                        Biome closestBiome = worldGenerator.BiomeAt(x, y);
                        nearbyBiomes.Add(new Coord(x, y), closestBiome);
                        break;
                    }
                }
            }
        }

        // If there is a biome nearby
        if (nearbyBiomes.Count > 0)
        {
            List<Coord> coords;

            // Pick random biome
            System.Random rand = new System.Random(settings.globalSeed);
            int index = rand.Next(0, nearbyBiomes.Count - 1);
            KeyValuePair<Coord, Biome> closestBiomePair = nearbyBiomes.ElementAt(index);
            int closestBiomeX = closestBiomePair.Key.tileX;
            int closestBiomeY = closestBiomePair.Key.tileY;
            Biome closestBiome = closestBiomePair.Value;

            // If closest biome is to the right
            if (closestBiomeX > biomeX && connectToRandomBiome || (forceConnectToRightBiome && worldGenerator.IsBiomeGeneratedAt(biomeX + 1, biomeY)))
            {
                coords = ClosestBiomeTileFromRight(biomeX, biomeY, biomeX + 1, biomeY);

                if (coords.Count > 1)
                {
                    CreatePassageToBiome(closestBiome, coords[0], coords[1], biomeX, biomeY, biomeX + 1, biomeY);
                }
            }
            // If closest biome is to the left
            if (closestBiomeX < biomeX && connectToRandomBiome || (forceConnectToLeftBiome && worldGenerator.IsBiomeGeneratedAt(biomeX - 1, biomeY)))
            {
                coords = ClosestBiomeTileFromLeft(biomeX, biomeY, biomeX - 1, biomeY);
                if (coords.Count > 1)
                {
                    CreatePassageToBiome(closestBiome, coords[0], coords[1], biomeX, biomeY, biomeX - 1, biomeY);
                }
            }
            // If closest biome is above
            if (closestBiomeY > biomeY && connectToRandomBiome || (forceConnectToTopBiome && worldGenerator.IsBiomeGeneratedAt(biomeX, biomeY + 1)))
            {
                coords = ClosestBiomeTileFromTop(biomeX, biomeY, biomeX, biomeY + 1);
                if (coords.Count > 1)
                {
                    CreatePassageToBiome(closestBiome, coords[0], coords[1], biomeX, biomeY, biomeX, biomeY + 1);
                }
            }
            // If closest biome is below
            if (closestBiomeY < biomeY && connectToRandomBiome || (forceConnectToBottomBiome && worldGenerator.IsBiomeGeneratedAt(biomeX, biomeY - 1)))
            {
                coords = ClosestBiomeTileFromBottom(biomeX, biomeY, biomeX, biomeY - 1);
                if (coords.Count > 1)
                {
                    CreatePassageToBiome(closestBiome, coords[0], coords[1], biomeX, biomeY, biomeX, biomeY - 1);
                }
            }
        }
    }

    List<Coord> ClosestBiomeTileFromTop(int biomeX, int biomeY, int closestBiomeX, int closestBiomeY)
    {
        List<Coord> emptyTilesA = new List<Coord>();
        List<Coord> emptyTilesB = new List<Coord>();

        int countA = 0;
        int countB = 0;

        Thread A = new Thread(delegate ()
        {
            // Loop through each biome tile
            for (int yA = (biomeY + 1) * worldGenerator.biomeNoisePixelSizeHeight - 1; yA > (biomeY) * worldGenerator.biomeNoisePixelSizeHeight; yA--)
            {
                for (int xA = biomeX * worldGenerator.biomeNoisePixelSizeWidth; xA < (biomeX + 1) * worldGenerator.biomeNoisePixelSizeWidth; xA++)
                {
                    // If there is not air at a position
                    if (generator.TileExistsAt(xA, yA))
                    {
                        continue;
                    }

                    emptyTilesA.Add(new Coord(xA, yA));
                }

                if (countA > biomeConnectionScanSize && emptyTilesA.Count > 0)
                {
                    break;
                }
                countA++;
            }
        });

        Thread B = new Thread(delegate ()
        {
            // Find closest tile for this tile
            for (int yB = (closestBiomeY) * worldGenerator.biomeNoisePixelSizeHeight; yB < (closestBiomeY + 1) * worldGenerator.biomeNoisePixelSizeHeight; yB++)
            {
                for (int xB = closestBiomeX * worldGenerator.biomeNoisePixelSizeWidth; xB < (closestBiomeX + 1) * worldGenerator.biomeNoisePixelSizeWidth; xB++)
                {
                    // If there is not air at a position
                    if (worldGenerator.TileExistsAt(xB, yB))
                    {
                        continue;
                    }
                    emptyTilesB.Add(new Coord(xB, yB));
                }

                if (countB > biomeConnectionScanSize && emptyTilesB.Count > 0)
                {
                    break;
                }
                countB++;
            }
        });

        // Wait for threads to finnish
        A.Start();
        B.Start();
        A.Join();
        B.Join();

        return FindClosestPoint(emptyTilesA, emptyTilesB);
    }

    List<Coord> ClosestBiomeTileFromBottom(int biomeX, int biomeY, int closestBiomeX, int closestBiomeY)
    {
        List<Coord> emptyTilesA = new List<Coord>();
        List<Coord> emptyTilesB = new List<Coord>();

        int countA = 0;
        int countB = 0;

        Thread A = new Thread(delegate ()
        {
            // find all empty tiles in both biomes' connection areas (Biome A)
            for (int yA = biomeY * worldGenerator.biomeNoisePixelSizeHeight; yA < (biomeY + 1) * worldGenerator.biomeNoisePixelSizeHeight; yA++)
            {
                for (int xA = biomeX * worldGenerator.biomeNoisePixelSizeWidth; xA < (biomeX + 1) * worldGenerator.biomeNoisePixelSizeWidth; xA++)
                {
                    // If there is not air at a position
                    if (generator.TileExistsAt(xA, yA))
                    {
                        continue;
                    }

                    emptyTilesA.Add(new Coord(xA, yA));
                }

                if (countA > biomeConnectionScanSize && emptyTilesA.Count > 0)
                {
                    break;
                }
                countA++;
            }
        });
        Thread B = new Thread(delegate ()
        {
            // find all empty tiles in both biomes' connection areas (Biome B)
            for (int yB = (closestBiomeY + 1) * worldGenerator.biomeNoisePixelSizeHeight - 1; yB > (closestBiomeY) * worldGenerator.biomeNoisePixelSizeHeight; yB--)
            {
                for (int xB = closestBiomeX * worldGenerator.biomeNoisePixelSizeWidth; xB < (closestBiomeX + 1) * worldGenerator.biomeNoisePixelSizeWidth; xB++)
                {
                    // If there is not air at a position
                    if (worldGenerator.TileExistsAt(xB, yB))
                    {
                        continue;
                    }
                    emptyTilesB.Add(new Coord(xB, yB));
                }

                if (countB > biomeConnectionScanSize && emptyTilesB.Count > 0)
                {
                    break;
                }
                countB++;
            }
        });

        // Wait for threads to finnish
        A.Start();
        B.Start();
        A.Join();
        B.Join();

        return FindClosestPoint(emptyTilesA, emptyTilesB);
    }
    List<Coord> ClosestBiomeTileFromLeft(int biomeX, int biomeY, int closestBiomeX, int closestBiomeY)
    {
        List<Coord> emptyTilesA = new List<Coord>();
        List<Coord> emptyTilesB = new List<Coord>();

        int countA = 0;
        int countB = 0;

        Thread A = new Thread(delegate ()
        {
            // Loop through each biome tile
            for (int xA = (biomeX) * worldGenerator.biomeNoisePixelSizeWidth; xA < (biomeX + 1) * worldGenerator.biomeNoisePixelSizeWidth; xA++)
            {
                for (int yA = biomeY * worldGenerator.biomeNoisePixelSizeHeight; yA < (biomeY + 1) * worldGenerator.biomeNoisePixelSizeHeight; yA++)
                {
                    // If there is not air at a position
                    if (generator.TileExistsAt(xA, yA))
                    {
                        continue;
                    }

                    emptyTilesA.Add(new Coord(xA, yA));
                }

                if (countA > biomeConnectionScanSize && emptyTilesA.Count > 0)
                {
                    break;
                }
                countA++;
            }
        });
        Thread B = new Thread(delegate ()
        {
            // Find closest tile for this tile
            for (int xB = (closestBiomeX + 1) * worldGenerator.biomeNoisePixelSizeWidth - 1; xB > (closestBiomeX) * worldGenerator.biomeNoisePixelSizeWidth; xB--)
            {
                for (int yB = (closestBiomeY) * worldGenerator.biomeNoisePixelSizeHeight; yB < (closestBiomeY + 1) * worldGenerator.biomeNoisePixelSizeHeight; yB++)
                {
                    // If there is not air at a position
                    if (worldGenerator.TileExistsAt(xB, yB))
                    {
                        continue;
                    }
                    emptyTilesB.Add(new Coord(xB, yB));
                }

                if (countB > biomeConnectionScanSize && emptyTilesB.Count > 0)
                {
                    break;
                }
                countB++;
            }
        });

        // Wait for threads to finnish
        A.Start();
        B.Start();
        A.Join();
        B.Join();

        return FindClosestPoint(emptyTilesA, emptyTilesB);
    }

    List<Coord> ClosestBiomeTileFromRight(int biomeX, int biomeY, int closestBiomeX, int closestBiomeY)
    {
        List<Coord> emptyTilesA = new List<Coord>();
        List<Coord> emptyTilesB = new List<Coord>();

        int countA = 0;
        int countB = 0;

        Thread A = new Thread(delegate ()
        {
            // Loop through each biome tile
            for (int xA = (biomeX + 1) * worldGenerator.biomeNoisePixelSizeWidth - 1; xA > (biomeX) * worldGenerator.biomeNoisePixelSizeWidth; xA--)
            {
                for (int yA = biomeY * worldGenerator.biomeNoisePixelSizeHeight; yA < (biomeY + 1) * worldGenerator.biomeNoisePixelSizeHeight; yA++)
                {
                    // If there is not air at a position
                    if (generator.TileExistsAt(xA, yA))
                    {
                        continue;
                    }

                    emptyTilesA.Add(new Coord(xA, yA));
                }

                if (countA > biomeConnectionScanSize && emptyTilesA.Count > 0)
                {
                    break;
                }
                countA++;
            }
        });
        Thread B = new Thread(delegate ()
        {
            // Find closest tile for this tile
            for (int xB = (closestBiomeX) * worldGenerator.biomeNoisePixelSizeWidth; xB < (closestBiomeX + 1) * worldGenerator.biomeNoisePixelSizeWidth; xB++)
            {
                for (int yB = (closestBiomeY) * worldGenerator.biomeNoisePixelSizeHeight; yB < (closestBiomeY + 1) * worldGenerator.biomeNoisePixelSizeHeight; yB++)
                {
                    // If there is not air at a position
                    if (worldGenerator.TileExistsAt(xB, yB))
                    {
                        continue;
                    }
                    emptyTilesB.Add(new Coord(xB, yB));
                }

                if (countB > biomeConnectionScanSize && emptyTilesB.Count > 0)
                {
                    break;
                }
                countB++;
            }
        });

        // Wait for threads to finnish
        A.Start();
        B.Start();
        A.Join();
        B.Join();


        return FindClosestPoint(emptyTilesA, emptyTilesB);
    }

    List<Coord> FindClosestPoint(List<Coord> pointsA, List<Coord> pointsB)
    {
        List<Coord> closestPoints = new List<Coord>();
        closestPoints.Add(new Coord(0, 0));
        closestPoints.Add(new Coord(0, 0));

        int bestDistance = 0;
        bool found = false;

        foreach (Coord pointA in pointsA)
        {
            foreach (Coord pointB in pointsB)
            {
                int distanceBetweenTiles = (int)(Mathf.Pow(pointA.tileX - pointB.tileX, 2) + Mathf.Pow(pointA.tileY - pointB.tileY, 2));

                if (distanceBetweenTiles < bestDistance || !found)
                {
                    found = true;
                    closestPoints[0] = pointA;
                    closestPoints[1] = pointB;
                    bestDistance = distanceBetweenTiles;
                }
            }
        }

        if (!found)
        {
            return new List<Coord>();
        }

        return closestPoints;
    }

    void CreatePassageToBiome(Biome biome, Coord bestTileA, Coord bestTileB, int biomeXA, int biomeYA, int biomeXB, int biomeYB)
    {
        this.SetConnectedBiome(biome);


        List<Coord> line = GetLine(bestTileA, bestTileB);
        System.Random rand = new System.Random(settings.globalSeed);

        List<Thread> threads = new List<Thread>();

        foreach (Coord c in line)
        {
            Thread t = new Thread(delegate ()
            {
                int biomeConnectionRadius = rand.Next(biomeConnectionRadiusMin, biomeConnectionRadiusMax);
                List<Coord> circleCoords = DrawCircle(c, biomeConnectionRadius);

                // Add coords to biomepassage list to process them later
                foreach (Coord coord in circleCoords)
                {
                    biomePassageCoords.Add(coord);
                }
            });
            t.Start();
        }

        foreach(Thread t in threads)
        {
            t.Join();
        }
    }

    List<Coord> DrawCircle(Coord c, int r)
    {
        // Returns a list of coords forming a circle

        List<Coord> circleCoords = new List<Coord>();
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x * x + y * y <= r * r)
                {
                    circleCoords.Add(new Coord(c.tileX + x, c.tileY + y));
                }
            }
        }
        return circleCoords;
    }

    public void FillBiomePassages()
    {
        // Fills biome passages to tilemap aka creates air
        foreach (Coord tile in biomePassageCoords)
        {
            worldGenerator.SetTileAt(tile.tileX, tile.tileY, null);
        }
        biomePassageCoords.Clear();
    }

    bool IsInBiomeBounds(int biomeX, int biomeY, int x, int y)
    {
        return ((x >= biomeX * worldGenerator.biomeNoisePixelSizeWidth && x <= (biomeX + 1) * worldGenerator.biomeNoisePixelSizeWidth) && (y >= biomeY * worldGenerator.biomeNoisePixelSizeHeight && y <= (biomeY + 1) * worldGenerator.biomeNoisePixelSizeHeight));
    }

    List<Coord> GetLine(Coord from, Coord to)
    {
        // Big math dont touch
        List<Coord> line = new List<Coord>();

        int x = from.tileX;
        int y = from.tileY;

        int dx = to.tileX - from.tileX;
        int dy = to.tileY - from.tileY;

        bool inverted = false;
        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if (longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Coord(x, y));

            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }

        return line;
    }

    public bool IsConnectedToBiome(Biome biome)
    {
        return this.connectedBiomes.Contains(biome);
    }

    void SetConnectedBiome(Biome biome)
    {
        if (this.IsConnectedToBiome(biome))
        {
            return;
        }

        this.connectedBiomes.Add(biome);
        biome.SetConnectedBiome(this);
    }

    public Generator GetGenerator() { return generator; }
}
