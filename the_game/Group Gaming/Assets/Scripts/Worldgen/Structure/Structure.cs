using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Generator;

public class Structure : ScriptableObject
{
    [Header("Other")]
    public StructureDataManager dataManager;

    protected Dictionary<Coord, TileAtlasTile> tiles = new Dictionary<Coord, TileAtlasTile>();

    public virtual void LoadTileData(){}

    public Dictionary<Coord, TileAtlasTile> GetTiles() { return tiles; }
    public TileAtlasTile GetTileAt(int x, int y)
    {
        return GetTileAt(new Coord(x, y));
    }

    public virtual void GenerateStructureAt(int x, int y, Biome biome)
    {
        // This method should be called after biome.ProcessMap
    }

    public TileAtlasTile GetTileAt(Coord coord)
    {
        return tiles[coord];
    }
}
