using UnityEngine;
using System.Collections.Generic;
using System;

public class MapGenerator
{
    public int width;
    public int height;
    public string seed;
    public int fillPercent;
    public int[,] map;
    public System.Random rng;
    public List<Vector3> orderedEdgeMap;

    public MapGenerator(int mapWidth, int mapHeight, int mapFillPercent, System.Random randomSeed)
    {
        width = mapWidth;
        height = mapHeight;
        fillPercent = mapFillPercent;
        rng = randomSeed;

        GenerateMap();
    }

    public void GenerateMap()
    {
        map = new int[width, height];
        RandomFillMap();

        for (int i = 0; i < 5; i++)
        {
            SmoothMap();
        }

        ProcessMap();
    }

    void RandomFillMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    map[x, y] = 1;
                }
                else
                {
                    map[x, y] = (rng.Next(0, 100) < fillPercent) ? 1 : 0;
                }
            }
        }
    }

    void ProcessMap()
    {
        List<Room> survivingRooms = new List<Room>();

        List<List<Coord>> wallRegions = GetRegions(1);

        int wallThresholdSize = 50;
        foreach (List<Coord> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThresholdSize)
            {
                foreach (Coord tile in wallRegion)
                {
                    map[tile.tileX, tile.tileY] = 0;
                }
            }
        }

        int roomThresholdSize = 50;
        List<List<Coord>> roomRegions = GetRegions(0);

        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize)
            {
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            }
            else
            {
                survivingRooms.Add(new Room(roomRegion, map));
            }
        }
        survivingRooms.Sort();
        survivingRooms[0].isMainRoom = true;
        survivingRooms[0].isAccessibleFromMainRoom = true;

        ConnectClosestRooms(survivingRooms);
        PreparePlayArea();

    }

    void PreparePlayArea()
    {
        List<Coord> edgeCoords = new List<Coord>();
        List<Coord> orderedEdges = new List<Coord>();

        // Get the edge tiles for play area, since GetRegions will return a list with only 1 list
        // assign it to a playRoom variable
        Room playRoom = new Room();
        List<List<Coord>> playAreas = GetRegions(0);
        foreach (List<Coord> playArea in playAreas)
        {
            playRoom = new Room(playArea, map);
        }

        // Calculate and add wall tiles which are edging with playroom tiles and
        // which do not exist in edgeCoord list
        foreach (Coord tile in playRoom.tiles)
        {
            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (x == tile.tileX || y == tile.tileY)
                    {
                        if (map[x, y] == 1 && !edgeCoords.Contains(tile))
                        {
                            Coord edge = new Coord(x, y);
                            if (!edgeCoords.Contains(edge))
                            {
                                edgeCoords.Add(edge);
                            }
                        }
                    }
                }
            }
        }
        // Calculate outline tile edges
        orderedEdges = OrderOutlineEdges(map, edgeCoords);
        orderedEdgeMap = new List<Vector3>();
        foreach (Coord edge in orderedEdges)
        {
            orderedEdgeMap.Add(new Vector3(-width / 2 + edge.tileX, 0, -height / 2 + edge.tileY));
        }
    }

    // Function to order edge coordinates to later generate the collision mesh
    List<Coord> OrderOutlineEdges(int[,] map, List<Coord> outlineEdges)
    {
        List<Coord> orderedEdges = new List<Coord>();
        // Add the first edge from the list
        orderedEdges.Add(outlineEdges[0]);
        // Keep adding edges until lists are the same in size
        while (orderedEdges.Count != outlineEdges.Count)
        {
            Coord lastEdge = orderedEdges[orderedEdges.Count - 1];
            orderedEdges.Add(GetNextEdge(lastEdge, orderedEdges, outlineEdges));
        }

        return orderedEdges;
    }

    bool TilesTouchingSameEdge(Coord currentTile, Coord nextTile, List<Coord> currentTouchingEdges)
    {
        int i = 0;
        List<Coord> nextTileTouchingEdges = new List<Coord>();
        for (int x = nextTile.tileX - 1; x <= nextTile.tileX + 1; x++)
        {
            for (int y = nextTile.tileY - 1; y <= nextTile.tileY + 1; y++)
            {
                if (IsInMapRange(x, y) && map[x, y] == 0)
                {
                    nextTileTouchingEdges.Add(new Coord(x, y));
                }
            }
        }
        foreach (Coord edge in currentTouchingEdges)
        {
            if (nextTileTouchingEdges.Contains(edge))
            {
                i++;
            }
        }

        return i > 0;
    }

    Coord GetNextEdge(Coord lastEdge, List<Coord> orderedEdges, List<Coord> outlineEdges)
    {
        Coord nextEdge = new Coord();
        List<Coord> touchingEdges = new List<Coord>();
        List<Coord> touchingWalls = new List<Coord>();

        for (int x = lastEdge.tileX - 1; x <= lastEdge.tileX + 1; x++)
        {
            for (int y = lastEdge.tileY - 1; y <= lastEdge.tileY + 1; y++)
            {
                if (IsInMapRange(x, y) && map[x, y] == 0)
                {
                    touchingEdges.Add(new Coord(x, y));
                }
                else
                {
                    Coord nextCoord = new Coord(x, y);
                    if (!orderedEdges.Contains(nextCoord) && outlineEdges.Contains(nextCoord))
                    {
                        touchingWalls.Add(nextCoord);
                    }
                }
            }
        }

        foreach (Coord edge in touchingWalls)
        {
            if (TilesTouchingSameEdge(lastEdge, edge, touchingEdges))
            {
                nextEdge = edge;
            }
        }

        return nextEdge;
    }


    void ConnectClosestRooms(List<Room> allRooms, bool forceAccessabilityFromMainRoom = false)
    {
        List<Room> roomListA = new List<Room>();
        List<Room> roomListB = new List<Room>();

        if (forceAccessabilityFromMainRoom)
        {
            foreach (Room room in allRooms)
            {
                if (room.isAccessibleFromMainRoom)
                {
                    roomListB.Add(room);
                }
                else
                {
                    roomListA.Add(room);
                }
            }
        }
        else
        {
            roomListA = allRooms;
            roomListB = allRooms;
        }

        int bestDistance = 0;
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnectionFound = false;

        foreach (Room roomA in roomListA)
        {
            if (!forceAccessabilityFromMainRoom)
            {
                possibleConnectionFound = false;
                if (roomA.connectedRooms.Count > 0)
                {
                    continue;
                }
            }
            possibleConnectionFound = false;

            foreach (Room roomB in roomListB)
            {
                if (roomA == roomB || roomA.isConnected(roomB))
                {
                    continue;
                }
                for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
                    {
                        Coord tileA = roomA.edgeTiles[tileIndexA];
                        Coord tileB = roomB.edgeTiles[tileIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) +
                            Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }

            if (possibleConnectionFound && !forceAccessabilityFromMainRoom)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

        if (possibleConnectionFound && forceAccessabilityFromMainRoom)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(allRooms, true);
        }

        if (!forceAccessabilityFromMainRoom)
        {
            ConnectClosestRooms(allRooms, true);
        }
    }

    void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB)
    {
        Room.ConnectRooms(roomA, roomB);

        List<Coord> line = GetLine(tileA, tileB);
        foreach (Coord c in line)
        {
            DrawCircle(c, 1);
        }
    }

    void DrawCircle(Coord c, int radius)
    {
        /* 
            Radius is scaling which tiles we traverse through the grid e.g.

            (0,2) (1,2) (2,2) if the tile we look at any centre point and the radius 
            (0,1) (1,1) (2,1) we want to carve out the path with is 1
            (0,0) (1,0) (2,0) then the grid will look like this

            The equation for a circle is: (x-a)^2 + (y+b)^2 = r^2
            Where a and b are centre points of the circle
            Since we are using 0,0 for our initial circle, the equation shrinks down
            and all left to do is jsut to add the point to the centre at the end
        */
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    int realX = c.tileX + x;
                    int realY = c.tileY + y;
                    if (IsInMapRange(realX, realY))
                    {
                        map[realX, realY] = 0;
                    }
                }
            }
        }
    }

    List<Coord> GetLine(Coord from, Coord to)
    {
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

    List<List<Coord>> GetRegions(int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFLags = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapFLags[x, y] == 0 && map[x, y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (Coord tile in newRegion)
                    {
                        mapFLags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }

    List<Coord> GetRegionTiles(int startX, int startY)
    {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[width, height];
        int tileType = map[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (IsInMapRange(x, y) && (y == tile.tileY || x == tile.tileX))
                    {
                        if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }

        return tiles;
    }

    bool IsInMapRange(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    void SmoothMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);

                if (neighbourWallTiles > 4)
                {
                    map[x, y] = 1;
                }
                else if (neighbourWallTiles < 4)
                {
                    map[x, y] = 0;
                }
            }
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (IsInMapRange(neighbourX, neighbourY))
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        wallCount += map[neighbourX, neighbourY];
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }

        return wallCount;
    }

    struct Coord
    {
        public int tileX;
        public int tileY;

        public Coord(int x, int y)
        {
            tileX = x;
            tileY = y;
        }
    }

    class Room : IComparable<Room>
    {
        public List<Coord> tiles;
        public List<Coord> edgeTiles;
        public List<Room> connectedRooms;
        public int roomSize;
        public bool isAccessibleFromMainRoom;
        public bool isMainRoom;

        // Default contructor
        public Room()
        {
        }

        public Room(List<Coord> roomTiles, int[,] map)
        {
            tiles = roomTiles;
            roomSize = tiles.Count;
            connectedRooms = new List<Room>();

            edgeTiles = new List<Coord>();
            foreach (Coord tile in tiles)
            {
                for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
                {
                    for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                    {
                        if (x == tile.tileX || y == tile.tileY)
                        {
                            if (map[x, y] == 1 && !edgeTiles.Contains(tile))
                            {
                                edgeTiles.Add(tile);
                            }
                        }
                    }
                }
            }
        }

        public static void ConnectRooms(Room roomA, Room roomB)
        {
            if (roomA.isAccessibleFromMainRoom)
            {
                roomB.SetAccessibleFromMainRoom();
            }
            else if (roomB.isAccessibleFromMainRoom)
            {
                roomA.SetAccessibleFromMainRoom();
            }
            roomA.connectedRooms.Add(roomB);
            roomB.connectedRooms.Add(roomA);
        }

        public void SetAccessibleFromMainRoom()
        {
            if (!isAccessibleFromMainRoom)
            {
                isAccessibleFromMainRoom = true;
                foreach (Room connectedRoom in connectedRooms)
                {
                    connectedRoom.SetAccessibleFromMainRoom();
                }
            }
        }

        public bool isConnected(Room otherRoom)
        {
            return connectedRooms.Contains(otherRoom);
        }

        public int CompareTo(Room otherRoom)
        {
            return otherRoom.roomSize.CompareTo(roomSize);
        }
    }

}
