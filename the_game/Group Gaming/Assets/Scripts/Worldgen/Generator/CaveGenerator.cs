using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

[CreateAssetMenu(menuName = "Worldgen/Generator/CaveGenerator")]
public class CaveGenerator : Generator
{
    [Header("Smoothing")]
    [Tooltip("How many smoothing iterations to do. More = more smooth caves")]
    public int smoothingIterationCount = 4;
    [Range(0, 4)]
    [Tooltip("How 'rough' the caves are")]
    public int adjancedTileCountOffset = 0;

    [Header("Cave Rooms")]
    [Tooltip("How big cave rooms must be")]
    public int minCaveRoomSize = 50;

    public override void CreateMapFromArea(int startX, int startY, int width, int height)
    {
        base.CreateMapFromArea(startX, startY, width, height);

        // Use a smoothing algorithm to create caves
        for (int i = 0; i < smoothingIterationCount; i++)
        {
            SmoothMap(startX, startY, width, height);
        }

        FindCaveRooms(startX, startY, width, height);
    }

    public void SetTileAt(int x, int y, int type)
    {
        Coord coord = new Coord(x, y);
        map[coord] = type;
    }
    public void SetTileAt(Coord coord, int type)
    {
        map[coord] = type;
    }

    void SmoothMap(int startX, int startY, int width, int height)
    {
        // Loop through each tile in given range
        for (int x = startX; x < startX + width; x++)
        {
            for (int y = startY; y < startY + height; y++)
            {
                int neighbourWallTiles = AdjancedTileCount(x, y); // Get adjanced tiles
                Coord coord = PositionToCoord(x, y);

                // Determine if tile should be air or solid
                if (neighbourWallTiles > 4 + adjancedTileCountOffset)
                {
                    map[coord] = 1;
                }
                else if (neighbourWallTiles < 4 - adjancedTileCountOffset)
                {
                    map[coord] = 0;
                }

            }
        }
    }

    void FindCaveRooms(int startX, int startY, int width, int height)
    {
        HashSet<Coord> checkedTiles = new HashSet<Coord>();

        // Loop through each tile in given range
        for (int x = startX; x < startX + width; x++)
        {
            for (int y = startY; y < startY + height; y++)
            {
                // If current tile is air
                if (AirExistsAt(x, y) && !checkedTiles.Contains(PositionToCoord(x,y)))
                {
                    List<Coord> airTiles = MapCaveRoomAt(x, y);
                    CaveRoom room = new CaveRoom(this, airTiles);

                    if (room.roomSize < minCaveRoomSize)
                    {
                        room.Fill();
                    }
                    else
                    {
                        foreach(Coord coord in room.tiles)
                        {
                            checkedTiles.Add(coord);
                        }
                    }
                }
            }
        }
    }

    List<Coord> MapCaveRoomAt(int x, int y, HashSet<Coord> visited = null, List<Coord> mapped = null)
    {
        // If visited dictionary doesn't exist yet
        if (visited == null)
        {
            visited = new HashSet<Coord>();
        }

        if (mapped == null)
        {
            mapped = new List<Coord>();
        }

        Coord pos = PositionToCoord(x, y); // Get x,y as coord

        // If current position has been visited, return
        if (visited.Contains(pos))
        {
            return mapped;
        }
        
        // Remember that we visited this position
        visited.Add(pos);

        // If there is a solid block and not air, return
        if (!AirExistsAt(x, y))
        {
            return mapped;
        }

        // Add air to list
        mapped.Add(pos);

        // Recursively check the neighboring tiles
        MapCaveRoomAt(x, y + 1, visited, mapped); // Up
        MapCaveRoomAt(x, y - 1, visited, mapped); // Down
        MapCaveRoomAt(x - 1, y, visited, mapped); // Left
        MapCaveRoomAt(x + 1, y, visited, mapped); // Right

        return mapped;
    }
    private class CaveRoom : IComparable<CaveRoom>
    {
        public CaveGenerator caveGenerator;
        public HashSet<Coord> tiles;
        public HashSet<Coord> edgeTiles;
        public int roomSize = 0; // How many many air tiles in this cave area

        public CaveRoom(CaveGenerator caveGenerator, List<Coord> tiles)
        {
            this.caveGenerator = caveGenerator;
            this.roomSize = tiles.Count;
            this.tiles = new HashSet<Coord>();
            this.edgeTiles = new HashSet<Coord>();
            
            foreach(Coord tile in tiles)
            {
                this.tiles.Add(tile);

                // Finds edge tiles
                for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
                {
                    for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                    {
                        if (x == tile.tileX || y == tile.tileY)
                        {
                            // If position is solid
                            if (caveGenerator.TileExistsAt(x, y))
                            {
                                edgeTiles.Add(tile);
                            }
                        }
                    }
                }
            }
        }

        public void Fill()
        {
            foreach(Coord tile in tiles)
            {
                caveGenerator.SetTileAt(tile, 1);
            }
        }

        int IComparable<CaveRoom>.CompareTo(CaveRoom other)
        {
            return other.roomSize.CompareTo(roomSize);
        }
    }

}
