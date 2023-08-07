using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tiles/TileAtlas/TileAtlas")]
public class TileAtlas : ScriptableObject
{
    public List<TileAtlasTile> tiles = new List<TileAtlasTile>();
    public Dictionary<int, TileAtlasTile> mappedTiles = new Dictionary<int, TileAtlasTile>(); // ID, Tile

    private void Awake()
    {
        MapTiles();
    }

    void MapTiles()
    {
        mappedTiles = new Dictionary<int, TileAtlasTile>();

        // Create a dictionary of the tiles for easier and less performance heavy lookup based on the tileID
        foreach (TileAtlasTile tile in tiles)
        {
            mappedTiles[tile.tileID] = tile;
        }
    }

    public TileAtlasTile GetTileWithID(int id)
    {
        if (!mappedTiles.ContainsKey(id))
        {
            MapTiles();
        }
        return mappedTiles[id];
    }
}
