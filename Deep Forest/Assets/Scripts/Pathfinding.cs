using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pathfinding
{
    Node[,] grid;
    float width, height;

    public Pathfinding(Node[,] _grid)
    {
        grid = _grid;
        width = grid.GetLength(0);
        height = grid.GetLength(1);
    }

    public Path GetBestPossiblePath(Vector3 startPos, Vector3 targetPos)
    {
        Path path = new Path();
        List<Node> waypoints = FindPath(startPos, targetPos);

        // Since the algorithm checks the first 3 nodes in list we need to add this list
        // instead of optimizing it
        if (waypoints.Count < 3)
        {
            path.waypoints.AddRange(waypoints);
        }
        else
        {
            waypoints = OptimizePath(waypoints);
            path.waypoints.AddRange(waypoints);
        }

        return path;
    }

    public List<Node> OptimizePath(List<Node> path)
    {
        bool previousNodeDiagonal;
        List<Node> optimizedPath = new List<Node>();
        int indexSum;

        // Path is already ordered
        // Add the first node to the list
        // Compare first 2 nodes to find direction
        optimizedPath.Add(path[0]);
        indexSum = nodeIndexSum(path[0], path[1]);
        if (indexSum == 1 || indexSum == -1)
        {
            previousNodeDiagonal = false;
        }
        else
        {
            previousNodeDiagonal = true;
        }
        // Iterate through the path and add the best nodes
        // due to how map is created we can be sure that each direction takes at least 
        // 2 tiles and there is no sudden change
        for (int i = 2; i < path.Count; i++)
        {
            Node current = path[i];
            Node last = path[i - 1];
            indexSum = nodeIndexSum(last, current);

            // if current sum is horizontal or vertical
            if (indexSum == 1 || indexSum == -1)
            {
                if (previousNodeDiagonal)
                {
                    optimizedPath.Add(last);
                    previousNodeDiagonal = false;
                }
            }
            // else the sum is either 2, -2 or 0. Diagonal in each case
            else
            {
                if (!previousNodeDiagonal)
                {
                    optimizedPath.Add(last);
                    previousNodeDiagonal = true;
                }
            }
        }
        // Add the last node if the path does not contain it already
        if (!optimizedPath.Contains(path[path.Count - 1]))
        {
            optimizedPath.Add(path[path.Count - 1]);
        }

        return optimizedPath;
    }

    int nodeIndexSum(Node a, Node b)
    {
        return (b.gridX - a.gridX) + (b.gridY - a.gridY);
    }

    public List<Node> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        List<Node> calculatedPath = new List<Node>();
        Node startNode = grid[
            Mathf.RoundToInt((startPos.x) + width / 2),
            Mathf.RoundToInt((startPos.z) + height / 2)];
        Node targetNode = grid[
            Mathf.RoundToInt((targetPos.x) + width / 2),
            Mathf.RoundToInt((targetPos.z) + height / 2)];

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 0; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                calculatedPath = RetracePath(startNode, targetNode);
                return calculatedPath;
            }

            foreach (Node neighbour in GetNeighbours(currentNode))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                {
                    continue;
                }

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }
        }
        return calculatedPath;
    }

    List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        // 14 is the cost for diagonal movement on a 2D grid
        // horizontal/vertical cost is 10
        if (dstX > dstY)
        {
            return 14 * dstY + 10 * (dstX - dstY);
        }
        return 14 * dstX + 10 * (dstY - dstX);
    }

    List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                {
                    continue;
                }
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < width && checkY >= 0 && checkY < height)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }
}
