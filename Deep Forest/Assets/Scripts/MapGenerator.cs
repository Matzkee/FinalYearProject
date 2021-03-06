﻿using UnityEngine;
using System.Collections.Generic;
using System;

public class MapGenerator
{
    public int width;
    public int height;
    public int connectionRadius;
    public string seed;
    public int fillPercent;
    public int[,] map;
    public System.Random rng;
    public List <List<Vector3>> orderedEdgeMaps;
    public List <Vector3> patrolPoints;
    public Vector3 endObjPosition;
    public Vector3 playerSpawn, guardSpawn;

    int smoothValue = 5;

    public MapGenerator(int mapWidth, int mapHeight, int mapFillPercent, int roomRadius, System.Random randomSeed)
    {
        width = mapWidth;
        height = mapHeight;
        fillPercent = mapFillPercent;
        connectionRadius = roomRadius;
        rng = randomSeed;

        GenerateMap();
    }

    public void GenerateMap()
    {
        map = new int[width, height];
        FillMap();

        for (int i = 0; i < smoothValue; i++)
        {
            SmoothMap();
        }

        ProcessMap();
    }

    /*
        Randomly fill the map using the system.Random created in terrain generator
        if the array index is an outline set it to 1
        otherwise fill the map with prefered percentage
    */
    void FillMap()
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

        // Remove smaller filled rooms
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

        // Remove smaller non-filled rooms
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
        survivingRooms[0].isReachableFromMainRoom = true;
        
        ConnectClosestRooms(survivingRooms);
        // Smooth the map again after connecting rooms to remove outliers
        // Also remove smaller filled areas
        wallRegions = GetRegions(1);
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
        for (int i = 0; i < smoothValue; i++)
        {
            SmoothMap();
        }
        CalculatePatrolPoints(survivingRooms);
        CreateSpawnPoints(patrolPoints);
        PreparePlayArea();

    }

    void CreateSpawnPoints(List<Vector3> patrolPoints)
    {
        // For spawning points I will use a simple logic
        // get the 2 most distant patrol points  and pick one for player and other for guard
        float distance = 0;
        Vector3 bestSpawnA = new Vector3();
        Vector3 bestSpawnB = new Vector3();
        foreach (Vector3 pointA in patrolPoints)
        {
            foreach (Vector3 pointB in patrolPoints)
            {
                float newDistance = Vector3.Distance(pointA, pointB);
                if (newDistance > distance)
                {
                    distance = newDistance;
                    bestSpawnA = pointA;
                    bestSpawnB = pointB;
                }
            }
        }
        // Set the spawns
        playerSpawn = bestSpawnA;
        guardSpawn = bestSpawnB;
    }

    void CalculatePatrolPoints(List<Room> rooms)
    {
        // Since we already have calculated the inside tiles in each room
        // All we are left to do is check which tile has the most tiles
        // next to it
        patrolPoints = new List<Vector3>();
        endObjPosition = new Vector3();
        float furthest = 0;

        foreach (Room room in rooms)
        {
            Coord bestTile = new Coord();
            int bestTileAmount = 0;
            // Calculate which tile has the most tiles next to it in 4 directions
            // Only tiles touching most tiles on each side count
            foreach (Coord currentTile in room.tiles)
            {
                int currentTileAmount = 0;
                int i = 0;

                // Check tiles in all directions
                while (map[currentTile.tileX + i, currentTile.tileY + i] != 1 &&
                    map[currentTile.tileX - i, currentTile.tileY + i] != 1 &&
                    map[currentTile.tileX + i, currentTile.tileY - i] != 1 &&
                    map[currentTile.tileX - i, currentTile.tileY - i] != 1)
                {
                    currentTileAmount++;
                    i++;
                }

                if (bestTileAmount < currentTileAmount)
                {
                    bestTileAmount = currentTileAmount;
                    bestTile = currentTile;
                }
            }
            // Calculate Best positions for objective tiles
            // For now lets just pick the most distant tile from a random vector on the map
            Vector3 random = new Vector3(UnityEngine.Random.Range(0, width), 0, UnityEngine.Random.Range(0, height));
            foreach (Coord tile in room.tiles)
            {
                Vector3 toCheck = new Vector3(-width / 2 + tile.tileX, 0, -height / 2 + tile.tileY);
                float newDist = Vector3.Distance(random, toCheck);
                if (newDist > furthest)
                {
                    furthest = newDist;
                    endObjPosition = new Vector3(tile.tileX, 0, tile.tileY);
                }
            }

            patrolPoints.Add(new Vector3(bestTile.tileX, 0, bestTile.tileY));
        }
    }

    void PreparePlayArea()
    {
        List<Coord> edgeCoords = new List<Coord>();
        List<List<Coord>> orderedEdges = new List<List<Coord>>();

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
        orderedEdges = OrderEdges(map, edgeCoords);
        orderedEdgeMaps = new List<List<Vector3>>();

        //Debug.Log("Number of Wall Maps: " + orderedEdges.Count);

        foreach (List<Coord> edgeMap in orderedEdges)
        {
            List<Vector3> newEdgeMap = new List<Vector3>();
            foreach (Coord tile in edgeMap)
            {
                newEdgeMap.Add(new Vector3(-width / 2 + tile.tileX, 0, -height / 2 + tile.tileY));
            }
            orderedEdgeMaps.Add(newEdgeMap);
        }
    }

    List<List<Coord>> OrderEdges(int[,] map, List<Coord> outlineEdges)
    {
        List<Coord> edgesToAssign = outlineEdges;
        List<List<Coord>> orderedEdges = new List<List<Coord>>();

        while (edgesToAssign.Count != 0)
        {
            List<Coord> newOrderedEdges = new List<Coord>();
            newOrderedEdges.Add(edgesToAssign[0]);

            // Keep adding edges until null is returned
            bool reachedEnd = false;
            while (!reachedEnd)
            {
                Coord lastEdge = newOrderedEdges[newOrderedEdges.Count - 1];
                Coord toAdd = GetNextEdge(lastEdge, newOrderedEdges, edgesToAssign);
                Coord newCoord = new Coord();
                newCoord.tileX = -1;
                if (toAdd.tileX != newCoord.tileX)
                {
                    newOrderedEdges.Add(toAdd);
                }
                else
                {
                    reachedEnd = true;
                }
            }
            foreach (Coord tile in newOrderedEdges)
            {
                edgesToAssign.Remove(tile);
            }
            orderedEdges.Add(newOrderedEdges);
        }

        return orderedEdges;
    }

    bool TouchingSamePlayTile(Coord currentTile, Coord nextTile, List<Coord> playTiles)
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
        foreach (Coord edge in playTiles)
        {
            if (nextTileTouchingEdges.Contains(edge))
            {
                i++;
            }
        }

        return i > 0;
    }

    /*
        Method to get the next edge and follow the path
    */
    Coord GetNextEdge(Coord lastEdge, List<Coord> orderedEdges, List<Coord> outlineEdges)
    {
        Coord nextEdge = new Coord();
        nextEdge.tileX = -1;
        List<Coord> playTiles = new List<Coord>();
        List<Coord> wallTiles = new List<Coord>();

        for (int x = lastEdge.tileX - 1; x <= lastEdge.tileX + 1; x++)
        {
            for (int y = lastEdge.tileY - 1; y <= lastEdge.tileY + 1; y++)
            {
                if (IsInMapRange(x, y) && map[x, y] == 0)
                {
                    playTiles.Add(new Coord(x, y));
                }
                else
                {
                    Coord nextCoord = new Coord(x, y);
                    if (!orderedEdges.Contains(nextCoord) && outlineEdges.Contains(nextCoord))
                    {
                        wallTiles.Add(nextCoord);
                    }
                }
            }
        }

        foreach (Coord edge in wallTiles)
        {
            if (TouchingSamePlayTile(lastEdge, edge, playTiles))
            {
                nextEdge = edge;
            }
        }

        return nextEdge;
    }

    /*
        Function to connect all rooms within the grid
        Method loops through the rooms and checks for the best possible tiles to connect them with
    */
    void ConnectClosestRooms(List<Room> allRooms, bool forceAccessabilityFromMainRoom = false)
    {
        List<Room> roomListA = new List<Room>();
        List<Room> roomListB = new List<Room>();

        if (forceAccessabilityFromMainRoom)
        {
            foreach (Room room in allRooms)
            {
                if (room.isReachableFromMainRoom)
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
            DrawCircle(c, connectionRadius);
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
            and all left to do is just to add the point to the centre at the end
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

    /*
        Project a line on a grid using pixel method
        return the list of coords with tiles on the line
    */

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

    /*
        Get rooms with tiles of type specified
        use another int array to flag the tiles already picked
    */
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

    /*
        This is similar as to how microsoft paint does fill bucket function
        It picks a tile and queues all tiles touching it with same type and then adds them 
        to the list
    */

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

    /*
        For every tile if there are 4 surrounding tiles of same type
        then change it to this type
    */

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

    /*
        Method returning a number of surrounding tiles counted as 'walls'
    */

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

    /*
        Class to store tile indexes from 2d array -> map[,]
        Also used for many calculations
    */

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

    /*
        Class to store information on various tiles generated on a 2d grid
        tiles ---> list of every tile within the room itself
        edgeTiles ---> list of every wall tile touching the room horizontaly & verticaly
        connectedRooms ---> list of rooms connected to the current room
    */
    class Room : IComparable<Room>
    {
        public List<Coord> tiles;
        public List<Coord> edgeTiles;
        public List<Room> connectedRooms;
        public int roomSize;
        public bool isReachableFromMainRoom;
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
            if (roomA.isReachableFromMainRoom)
            {
                roomB.SetAccessibleFromMainRoom();
            }
            else if (roomB.isReachableFromMainRoom)
            {
                roomA.SetAccessibleFromMainRoom();
            }
            roomA.connectedRooms.Add(roomB);
            roomB.connectedRooms.Add(roomA);
        }

        public void SetAccessibleFromMainRoom()
        {
            if (!isReachableFromMainRoom)
            {
                isReachableFromMainRoom = true;
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

        // C# sort() specific method form IComparable interface
        public int CompareTo(Room otherRoom)
        {
            return otherRoom.roomSize.CompareTo(roomSize);
        }
    }

}
