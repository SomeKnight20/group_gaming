using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Worldgen/Biome/Biome")]
public class Biome : ScriptableObject
{
    [Tooltip("What generator system this biome uses")]
    [SerializeField]
    protected Generator generator;
    
    [Tooltip("Chance to spawn this biome. 1 = default")]
    public float spawnWeight = 1;

    [Header("Tiles")]
    public TileAtlas tileAtlas;
    protected Tilemap tilemap;

    protected Dictionary<Generator.Coord, TileAtlasTile> tilemapData = new Dictionary<Generator.Coord, TileAtlasTile>();

    public virtual void SetDefaultTilemap(Tilemap tilemap)
    {
        this.tilemap = tilemap;
    }

    public virtual void ResetMap()
    {
        generator.ResetMap();
        tilemapData = new Dictionary<Generator.Coord, TileAtlasTile>();

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
        this.tilemapData = new Dictionary<Generator.Coord, TileAtlasTile>();
    }

    public virtual void FillTilemap(int startX, int startY, int width, int height)
    {
        // This fills the tilemap with tiles
        if (this.tilemap == null) {
            return;
        }

        foreach(KeyValuePair<Generator.Coord, TileAtlasTile> tile in this.tilemapData)
        {
            Generator.Coord coord = tile.Key;
            TileAtlasTile tileData = tile.Value;
            tilemap.SetTile(new Vector3Int(coord.tileX, coord.tileY, 0), tileData.tile);
        }

        generator.ResetMap();
    }

    public Generator GetGenerator() { return generator; }
}
