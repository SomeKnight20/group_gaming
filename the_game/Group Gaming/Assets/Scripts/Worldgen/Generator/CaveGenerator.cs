using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

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
    [Tooltip("How many times each cave connects to some other cave. NOTE: This is very performance heavy.")]
    public int caveRoomConnectionIterations = 1;
    [Tooltip("How wide tunnels are created between caves")]
    public int caveRoomConnectionRadius = 3;

    protected List<Thread> caveLineThreads = new List<Thread>();

    public override void CreateMapFromArea(int startX, int startY, int width, int height)
    {
        base.CreateMapFromArea(startX, startY, width, height);

        // Use a smoothing algorithm to create caves
        for (int i = 0; i < smoothingIterationCount; i++)
        {
            SmoothMap(startX, startY, width, height);
        }

        List<CaveRoom> caveRooms = FindCaveRooms(startX, startY, width, height);
        caveRooms.Sort();
        if (caveRooms.Count > 0)
        {
            caveRooms[0].SetConnectedToMainRoom(true);

            // Connect caves together
            for(int i = 0; i < caveRoomConnectionIterations; i++)
            {
                ConnectClosestCaveRooms(caveRooms);
            }
            ConnectCaveRoomsToMainRoom(caveRooms);

            for (int i = 0; i < caveLineThreads.Count; i++)
            {
                caveLineThreads[i].Join();
            }
            caveLineThreads = new List<Thread>();
        }
    }

    public void SetTileAt(int x, int y, TileType type)
    {
        Coord coord = new Coord(x, y);
        SetTileAt(coord, type);
    }
    public void SetTileAt(Coord coord, TileType type)
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
                    map[coord] = TileType.SOLID;
                }
                else if (neighbourWallTiles < 4 - adjancedTileCountOffset)
                {
                    map[coord] = TileType.AIR;
                }

            }
        }
    }

    void ConnectClosestCaveRooms(List<CaveRoom> caveRooms)
    {
        // Connects each cave room to the closest one nearby

        Coord bestTileA = new Coord(); // Best edge tile for roomA
        Coord bestTileB = new Coord();
        CaveRoom bestRoomA = new CaveRoom();
        CaveRoom bestRoomB = new CaveRoom(); // Best room for roomB

        // Loop through each cave room twice
        foreach (CaveRoom roomA in caveRooms)
        {
            bool foundPath = false;
            float bestDistance = 0;

            foreach (CaveRoom roomB in caveRooms)
            {
                // We can't connect the same room into itself or already connected to roomB
                if (roomA == roomB || roomA.IsConnectedToRoom(roomB))
                {
                    continue;
                }
                // Loop through each edge tile and find closest one
                foreach (Coord tileA in roomA.edgeTiles)
                {
                    foreach (Coord tileB in roomB.edgeTiles)
                    {
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        if (distanceBetweenRooms < bestDistance || !foundPath)
                        {
                            bestDistance = distanceBetweenRooms;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                            foundPath = true;
                        }
                    }
                }
            }
            

            if (foundPath)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }
    }

    void ConnectCaveRoomsToMainRoom(List<CaveRoom> caveRooms)
    {
        // Connects each cave room to the closest one nearby

        Coord bestTileA = new Coord(); // Best edge tile for roomA
        Coord bestTileB = new Coord();
        CaveRoom bestRoomA = new CaveRoom();
        CaveRoom bestRoomB = new CaveRoom(); // Best room for roomB

        // Loop through each cave room twice
        foreach (CaveRoom roomA in caveRooms)
        {
            if (roomA.IsConnectedToMainRoom())
            {
                continue;
            }

            bool foundPath = false;
            float bestDistance = 999999999;

            foreach (CaveRoom conncetedRoomA in roomA.AllConnectedRooms())
            {
                foreach (CaveRoom roomB in caveRooms)
                {
                    // We can't connect the same room into itself and it also checks that roomB is connected to the main room if forced
                    if (roomA == roomB || !roomB.IsConnectedToMainRoom())
                    {
                        continue;
                    }
                    // Loop through each edge tile and find closest one
                    foreach (Coord tileA in roomA.edgeTiles)
                    {
                        foreach (Coord tileB in roomB.edgeTiles)
                        {
                            int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                            if (distanceBetweenRooms < bestDistance)
                            {
                                bestDistance = distanceBetweenRooms;
                                bestTileA = tileA;
                                bestTileB = tileB;
                                bestRoomA = roomA;
                                bestRoomB = roomB;
                                foundPath = true;
                            }
                        }
                    }
                }
            }
            if (foundPath)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }
    }

    void CreatePassage(CaveRoom roomA, CaveRoom roomB, Coord tileA, Coord tileB)
    {
        roomA.ConnectToRoom(roomB);

        Thread thread = new Thread(() =>
        {
            List<Coord> line = GetLine(tileA, tileB);
            foreach (Coord c in line)
            {
                DrawCircle(c, caveRoomConnectionRadius);
            }
        });

        thread.Start();

        caveLineThreads.Add(thread);
    }


    void DrawCircle(Coord c, int r)
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
                    if (PositionIsGeneratedAt(drawX, drawY))
                    {
                        map[PositionToCoord(drawX, drawY)] = 0;
                    }
                }
            }
        }
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
    List<CaveRoom> FindCaveRooms(int startX, int startY, int width, int height)
    {
        HashSet<Coord> checkedTiles = new HashSet<Coord>();
        List<CaveRoom> caveRooms = new List<CaveRoom>();

        // Loop through each tile in given range
        for (int x = startX; x < startX + width; x++)
        {
            for (int y = startY; y < startY + height; y++)
            {
                // If current tile is air and it is not already a part of some cave room
                if (AirExistsAt(x, y) && !checkedTiles.Contains(PositionToCoord(x,y)))
                {
                    List<Coord> airTiles = MapCaveRoomAt(x, y);
                    CaveRoom room = new CaveRoom(this, airTiles);

                    // If cave room is too small, fill it
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

                        caveRooms.Add(room);
                    }
                }
            }
        }

        return caveRooms;
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
        public List<Coord> tiles;
        public List<Coord> edgeTiles;
        public int roomSize = 0; // How many many air tiles in this cave area
        public List<CaveRoom> connectedRooms = new List<CaveRoom>();
        public bool connectedToMainRoom = false;

        public CaveRoom()
        {

        }

        public CaveRoom(CaveGenerator caveGenerator, List<Coord> tiles)
        {
            this.caveGenerator = caveGenerator;
            this.roomSize = tiles.Count;
            this.tiles = tiles;
            this.edgeTiles = new List<Coord>();
            this.connectedRooms = new List<CaveRoom>();
            
            foreach(Coord tile in tiles)
            {
                // Finds edge tiles
                for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
                {
                    for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                    {
                        // If tile is not a diagnonal tile
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

        public HashSet<CaveRoom> AllConnectedRooms(HashSet<CaveRoom> allConnectedRooms = null)
        {
            // Returns all connected rooms from the "linked list"

            // If allconnectedrooms is null, aka. first run
            if (allConnectedRooms == null)
            {
                allConnectedRooms = new HashSet<CaveRoom>();
            }

            // If current room is already inside the all connected rooms, then return
            if (allConnectedRooms.Contains(this))
            {
                return allConnectedRooms;
            }
            allConnectedRooms.Add(this);
            
            // Loop through each connected room and repeat this process
            foreach (CaveRoom room in connectedRooms)
            {
                room.AllConnectedRooms(allConnectedRooms);
            }

            return allConnectedRooms;
        }

        public bool IsConnectedToMainRoom()
        {
            return this.connectedToMainRoom;
        }

        public void SetConnectedToMainRoom(bool connected = true)
        {
            connectedToMainRoom = connected;

            // If this is connected to the main room then the connected rooms are too
            foreach (CaveRoom connectedRoom in connectedRooms)
            {
                if (!connectedRoom.IsConnectedToMainRoom())
                {
                    connectedRoom.SetConnectedToMainRoom(connected);
                }
            }
        }

        public bool IsConnectedToRoom(CaveRoom room)
        {
            return connectedRooms.Contains(room);
        }

        public void ConnectToRoom(CaveRoom room)
        {
            if (IsConnectedToRoom(room))
            {
                return;
            }

            // Make sure all cave rooms are marked as "connected to main cave room"
            if (room.IsConnectedToMainRoom())
            {
                this.SetConnectedToMainRoom(true);
            }

            this.connectedRooms.Add(room);
            room.ConnectToRoom(this);
        }

        public void Fill()
        {
            foreach(Coord tile in tiles)
            {
                caveGenerator.SetTileAt(tile, TileType.SOLID);
            }
        }

        int IComparable<CaveRoom>.CompareTo(CaveRoom other)
        {
            return other.roomSize.CompareTo(roomSize);
        }
    }

}
