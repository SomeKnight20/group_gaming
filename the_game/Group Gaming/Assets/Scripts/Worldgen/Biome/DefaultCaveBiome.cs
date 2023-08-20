using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Worldgen/Biome/DefaultCaveBiome")]
public class DefaultCaveBiome : Biome
{
    public TileAtlasTile stone;

    public override void ProcessMap(int startX, int startY, int width, int height)
    {
        base.ProcessMap(startX, startY, width, height);

        Dictionary<Generator.Coord, TileType> map = generator.GetMapData();

        foreach (KeyValuePair<Generator.Coord, TileType> tile in map)
        {
            Generator.Coord coord = tile.Key;
            TileType tileType = tile.Value;

            if (tileType == TileType.SOLID)
            {
                this.tilemapData.Add(coord, stone);
            }
        }
    }
}
