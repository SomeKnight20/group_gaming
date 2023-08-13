using System;
using System.Collections;
using System.Collections.Generic;
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
    
    [Tooltip("Chance to spawn this biome. 1 = default")]
    public float spawnWeight = 1;

    [Tooltip("How wide are the tunnels that connect two biomes together")]
    public int biomeConnectionRadius = 3;

    [Header("Tiles")]
    public TileAtlas tileAtlas;
    protected Tilemap tilemap;

    // Connected to biomes
    public HashSet<Biome> connectedBiomes = new HashSet<Biome>();

    protected Dictionary<Coord, TileAtlasTile> tilemapData = new Dictionary<Coord, TileAtlasTile>();

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
        tilemapData = new Dictionary<Coord, TileAtlasTile>();
        connectedBiomes = new HashSet<Biome>();

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
        if (this.tilemap == null) {
            return;
        }

        foreach(KeyValuePair<Coord, TileAtlasTile> tile in this.tilemapData)
        {
            Coord coord = tile.Key;
            TileAtlasTile tileData = tile.Value;
            tilemap.SetTile(new Vector3Int(coord.tileX, coord.tileY, 0), tileData.tile);
        }

        generator.ResetMap();
    }

    public void ConnectToClosestBiome(int biomeX, int biomeY)
    {
        // Connects this biome to the closes one nearby
        Debug.Log($"Connecting To: {biomeX} {biomeY}");

        Biome closestBiome = null;
        int closestBiomeX = 0;
        int closestBiomeY = 0;

        // Loop through biomes around this one and find one
        for (int x = biomeX - 1; x <= biomeX + 1; x++)
        {
            for (int y = biomeY - 1; y <= biomeY + 1; y++)
            {
                if(x == biomeX || y == biomeY && !(x == biomeX && y == biomeY))
                {
                    if (worldGenerator.IsBiomeGeneratedAt(x, y))
                    {
                        Debug.Log($"Found Biome At: {x}, {y}");
                        closestBiome = worldGenerator.BiomeAt(x, y);
                        closestBiomeX = x;
                        closestBiomeY = y;
                        break;
                    }
                }
            }
        }

        // If there is a biome nearby
        if (closestBiome != null)
        {
            Coord bestTileA = new Coord(1, 0);
            Coord bestTileB = new Coord(2, 0);
            bool found = false; // Is there a possible connection

            Debug.Log($"Params: {biomeX * worldGenerator.biomeNoisePixelSizeWidth} {(biomeX + 1) * worldGenerator.biomeNoisePixelSizeWidth}");
            Debug.Log($"Params: {biomeY * worldGenerator.biomeNoisePixelSizeHeight} {(biomeY + 1) * worldGenerator.biomeNoisePixelSizeHeight}");
            Debug.Log($"Params: {closestBiomeX * worldGenerator.biomeNoisePixelSizeWidth} {(closestBiomeX + 1) * worldGenerator.biomeNoisePixelSizeWidth}");
            Debug.Log($"Params: {closestBiomeY * worldGenerator.biomeNoisePixelSizeHeight} {(closestBiomeY + 1) * worldGenerator.biomeNoisePixelSizeHeight}");

            // Loop through each biome tile
            for (int xA = biomeX * worldGenerator.biomeNoisePixelSizeWidth; xA < (biomeX + 1) * worldGenerator.biomeNoisePixelSizeWidth; xA++)
            {
                for (int yA = biomeY * worldGenerator.biomeNoisePixelSizeHeight; yA < (biomeY + 1) * worldGenerator.biomeNoisePixelSizeHeight; yA++)
                {
                    // If there is not air at a position
                    if (generator.TileExistsAt(xA, yA))
                    {
                        continue;
                    }

                    bestTileA = new Coord(xA, yA);
                    // Find closest tile for this tile
                    for (int xB = closestBiomeX * worldGenerator.biomeNoisePixelSizeWidth; xB < (closestBiomeX + 1) * worldGenerator.biomeNoisePixelSizeWidth; xB++)
                    {
                        for (int yB = closestBiomeY * worldGenerator.biomeNoisePixelSizeHeight; yB < (closestBiomeY + 1) * worldGenerator.biomeNoisePixelSizeHeight; yB++)
                        {
                            // If there is not air at a position
                            if (worldGenerator.TileExistsAt(xB, yB))
                            {
                                continue;
                            }

                            bestTileB = new Coord(xB, yB);
                            found = true;
                            goto after_loop; // Break out of all loops
                        }
                    }
                }
            }
            after_loop:;

            if (found)
            {
                CreatePassageToBiome(closestBiome, bestTileA, bestTileB, biomeX, biomeY, closestBiomeX, closestBiomeY);
            }
        }
    }

    void CreatePassageToBiome(Biome biome, Coord bestTileA, Coord bestTileB, int biomeXA, int biomeYA, int biomeXB, int biomeYB)
    {
        this.SetConnectedBiome(biome);

        Debug.Log($"Creating passage: ({biomeXA} {biomeYA}) ->({biomeXB} {biomeYB}) ({bestTileA.tileX}, {bestTileA.tileY}) -> ({bestTileB.tileX}, {bestTileB.tileY})");

        List<Coord> line = GetLine(bestTileA, bestTileB);
        Debug.Log(line.Count);
        foreach (Coord c in line)
        {
            DrawCircle(biome, c, biomeConnectionRadius, biomeXA, biomeYA, biomeXB, biomeYB);
        }
    }

    void DrawCircle(Biome biome, Coord c, int r, int biomeXA, int biomeYA, int biomeXB, int biomeYB)
    {
        // ???
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x * x + y * y <= r * r)
                {
                    int drawX = c.tileX + x;
                    int drawY = c.tileY + y;
                    if (IsInBiomeBounds(biomeXA, biomeYA, drawX, drawY))
                    {
                        generator.map[new Coord(drawX, drawY)] = 0;
                    } else if(biome != null)
                    {
                        biome.DrawCircle(null, c, r, biomeXB, biomeYB, 0, 0);
                    }
                }
            }
        }
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
