using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tiles/TileAtlas/TileAtlas")]
public class TileAtlas : ScriptableObject
{
    public List<TileAtlasTile> tiles = new List<TileAtlasTile>();
    public Dictionary<int, TileAtlasTile> mappedTiles; // ID, Tile

    private void Awake()
    {
        mappedTiles = new Dictionary<int, TileAtlasTile>();

        // Create a dictionary of the tiles for easier and less performance heavy lookup based on the tileID
        foreach (TileAtlasTile tile in tiles)
        {
            mappedTiles[tile.tileID] = tile;
        }
    }
}
