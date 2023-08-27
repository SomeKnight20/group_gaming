using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Tiles/TileAtlas/TileAtlasTile")]
public class TileAtlasTile : ScriptableObject
{
    public int tileID = 0;
    public TileType tileType = TileType.SOLID;
    public Tile tile;
}
