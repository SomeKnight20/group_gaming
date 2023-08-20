using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Worldgen/Generator/Generator")]
public class Generator : ScriptableObject
{
    [Header("Noise")]
    [Tooltip("What noise this generator uses to create pixels")]
    public Noise noise;
    public Dictionary<Coord, TileType> map = new Dictionary<Coord, TileType>(); // All tile positions for this specific generator, 1 = tile, 0 = air
    protected GlobalSettings settings;

    public virtual void OnStart()
    {

    }

    public virtual void ResetMap()
    {
        map = new Dictionary<Coord, TileType>(); // Resets all tiles in the map
    }

    public void SetGlobalSettings(GlobalSettings globalSettings)
    {
        settings = globalSettings;
    }

    public virtual void CreateMapFromArea(int startX, int startY, int width, int height)
    {
        // This function creates the map data for pixels, for example, 0 -> air, 1 -> solid

        // Generate a random number (0 or 1) for each position in map in an area
        for (int x = startX; x < startX + width; x++)
        {
            for (int y = startY; y < startY + height; y++)
            {
                int noiseValue = noise.GenerateNoiseAt(x, y); // Get the noise at x, y
                map[new Coord(x, y)] = (TileType)noiseValue; // 0 or 1, 0: no tile, 1: tile
            }
        }
    }

    public Dictionary<Coord, TileType> GetMapData()
    {
        return map;
    }

    public virtual int AdjancedTileCount(int tileX, int tileY)
    {
        int tileCount = 0;
        for (int x = tileX - 1; x <= tileX + 1; x++)
        {
            for (int y = tileY - 1; y <= tileY + 1; y++)
            {
                // If there is air or a solid tile at position
                if (PositionIsGeneratedAt(x, y))
                {
                    // Skip current tile we are checking
                    if (x != tileX || y != tileY)
                    {
                        tileCount += (int)map[PositionToCoord(x, y)];
                    }
                }
                else
                {
                    // We assume that non-generated tiles are solid, for example edges of the map
                    tileCount++;
                }
            }
        }

        return tileCount;
    }
    public virtual Coord PositionToCoord(int x, int y)
    {
        // Converts a position to coord-object
        return new Coord(x, y);
    }

    public virtual bool PositionIsGeneratedAt(int x, int y)
    {
        // Checks if a position has been generated, aka. is a solid tile or air
        Coord coord = PositionToCoord(x, y);
        return PositionIsGeneratedAt(coord);
    }

    public virtual bool PositionIsGeneratedAt(Coord coord)
    {
        // Checks if a position has been generated, aka. is a solid tile or air
        return map.ContainsKey(coord);
    }

    public virtual bool TileExistsAt(int x, int y)
    {
        // Checks if there is a solid tile at a position
        Coord coord = PositionToCoord(x, y);
        return TileExistsAt(coord);
    }
    public virtual bool TileExistsAt(Coord coord)
    {
        // Checks if there is a solid tile at a position
        return map.ContainsKey(coord) && map[coord] == TileType.SOLID;
    }
    public virtual bool AirExistsAt(int x, int y)
    {
        // Checks if there is a solid tile at a position
        Coord coord = PositionToCoord(x, y);
        return AirExistsAt(coord);
    }

    public virtual bool AirExistsAt(Coord coord)
    {
        // Checks if there is an air tile at a position
        return map.ContainsKey(coord) && map[coord] == TileType.AIR;
    }

    public struct Coord
    {
        // A coordinate-type struct
        public int tileX;
        public int tileY;

        public Coord(int x, int y)
        {
            tileX = x;
            tileY = y;
        }

        // Override Equals and GetHashCode methods for HashSet comparison
        public override bool Equals(object obj)
        {
            if (!(obj is Coord otherCoordinate))
                return false;

            return tileX == otherCoordinate.tileX && tileY == otherCoordinate.tileY;
        }

        public override int GetHashCode()
        {
            return (tileX, tileY).GetHashCode();
        }
    }
}
