using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour {

    TerrainGenerator tg;
    public Transform seeker, target;
    public List<Node> tracePath;
    public List<Vector3> betterPath;

    void Start()
    {
        tg = GameObject.FindGameObjectWithTag("TerrainGenerator").GetComponent<TerrainGenerator>();
    }

    void Update()
    {
        FindPath(seeker.position, target.position);
        betterPath = GetOptimizedPath(tracePath);
    }

    void OnDrawGizmos()
    {
        if (tracePath != null)
        {
            if (betterPath != null)
            {
                Gizmos.color = Color.magenta;
                foreach (Vector3 pos in betterPath)
                {
                    Gizmos.DrawWireSphere(pos, 0.5f);
                }
            }
            if (tg.worldGrid != null)
            {
                foreach (Node n in tg.worldGrid)
                {
                    Gizmos.color = Color.black;
                    if (n.walkable)
                    {
                        if (tracePath.Contains(n))
                        {
                            Gizmos.color = Color.cyan;
                        }
                        if (tg.patrolPoints != null&& tg.patrolPoints.Contains(n.worldPosition))
                        {
                            Gizmos.color = Color.white;
                        }
                        Gizmos.DrawWireCube(n.worldPosition, Vector3.one);
                    }
                }
            }
        }
    }

    List<Vector3> GetOptimizedPath(List<Node> path)
    {
        bool previousNodeDiagonal;
        List<Vector3> optimizedPath = new List<Vector3>();
        int indexSum;

        // Path is already ordered
        // Add the first node to the list
        // Compare first 2 nodes to find direction
        optimizedPath.Add(path[0].worldPosition);
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
                    optimizedPath.Add(last.worldPosition);
                    previousNodeDiagonal = false;
                }
            }
            // else the sum is either 2, -2 or 0. Diagonal in each case
            else
            {
                if (!previousNodeDiagonal)
                {
                    optimizedPath.Add(last.worldPosition);
                    previousNodeDiagonal = true;
                }
            }
        }
        // Add the last node if the path does not contain it already
        if (!optimizedPath.Contains(path[path.Count - 1].worldPosition))
        {
            optimizedPath.Add(path[path.Count - 1].worldPosition);
        }
        return optimizedPath;
    }

    int nodeIndexSum(Node a, Node b)
    {
        return (b.gridX - a.gridX) + (b.gridY - a.gridY);
    }

    void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = tg.worldGrid[
            Mathf.RoundToInt(startPos.x) + tg.width/2, 
            Mathf.RoundToInt(startPos.z) + tg.height/2];
        Node targetNode = tg.worldGrid[
            Mathf.RoundToInt(targetPos.x) + tg.width / 2, 
            Mathf.RoundToInt(targetPos.z) + tg.height / 2];

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
                RetracePath(startNode, targetNode);
                return;
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
    }

    void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        tracePath = path;
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

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

                if (checkX >= 0 && checkX < tg.width && checkY >= 0 && checkY < tg.height)
                {
                    neighbours.Add(tg.worldGrid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }
}
