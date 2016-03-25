using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class TerrainGenerator : MonoBehaviour
{
    const string alphabet = "abcdefghijklmnopqrstuvwxyz0123456789";

    GameObject walls;
    GameObject terrainWeeds;
    List<GameObject> trees;
    List<List<Vector3>> orderedEdgeMaps;
    List<SquarePolygon> weeds;
    public List<Vector3> patrolPoints;
    public Vector3 endObjPoint;
    public Node[,] worldGrid;
    int[,] vegetationFlags;
    [HideInInspector]
    public Vector3 playerSpawn, guardSpawn;

    System.Random rng;
    Vector2[] octaveOffsets;
    Mesh terrainMesh;
    MeshRenderer terrainMeshRenderer;
    MeshCollider terrainColider;
    MapGenerator mapGenerator;

    [Header("Map Options")]
    public int width = 100;
    public int height = 100;
    [Range(1, 5)]
    public int roomRadius;
    [Range(0, 100)]
    public int fillPercentage;
    public int minCharAmount = 0;
    public int maxCharAmount = 0;
    public string seed;
    [Header("Perlin Noise")]
    public float scale = 10f;
    public int octaves = 2;
    public float persistance = 1.5f;
    public float lacunarity = 1f;
    [Header("Weed Options")]
    public float weedSize = 1f;
    public Material weedMaterial;
    [Header("Other Options")]
    public float wallHeight = 4;
    public float treeSeparation = 3f;
    public bool useRandomSeed = false;
    public bool generateTrees = false;
    public bool drawGizmos = false;

    public GameObject player;
    public GameObject guard;
    public GameObject endObjective;
    public GameObject[] prefabs;
    public Material terrainMaterial;

    void Start()
    {
        // Instantiate arrays & lists
        worldGrid = new Node[width, height];
        vegetationFlags = new int[width, height];
        trees = new List<GameObject>();
        // Add components
        terrainMesh = gameObject.AddComponent<MeshFilter>().mesh;
        terrainMeshRenderer = gameObject.AddComponent<MeshRenderer>();
        terrainColider = gameObject.AddComponent<MeshCollider>();
        terrainMesh.Clear();
        // Launch
        Generate();
    }

    public void Generate()
    {
        GenerateSeed();
        GenerateMesh();
        GenerateWalls();
        GeneratePatrolPoints();
        GenerateEndObjSpawnPoints();
        GenerateSpawnPoints();
        // For debugging issues a bool is used
        if (generateTrees)
        {
            GenerateTrees();
        }
        GenerateWeeds();
        SpawnEndObj();
        // Spawn player and guard
        SpawnPlayer(playerSpawn);
        SpawnGuard(guardSpawn);
    }

    /*
        Gizmos for debugging purposes
    */
    void OnDrawGizmos()
    {
        if (drawGizmos)
        {
            if (worldGrid != null)
            {
                foreach (Node n in worldGrid)
                {
                    Gizmos.color = new Color(0, 0, 0, 0.3f);
                    if (n != null && n.walkable)
                    {
                        Gizmos.DrawCube(n.worldPosition, Vector3.one);
                    }
                }
            }
            Gizmos.color = Color.white;
            Gizmos.DrawCube(endObjPoint, Vector3.one);
            Gizmos.color = Color.green;
            Gizmos.DrawCube(playerSpawn, Vector3.one);
            Gizmos.color = Color.red;
            Gizmos.DrawCube(guardSpawn, Vector3.one);
        }
    }

    public void GenerateSeed()
    {
        if (useRandomSeed)
        {
            StringBuilder randomString = new StringBuilder();
            int charAmount = Random.Range(minCharAmount, maxCharAmount);
            // Create random string
            for (int i = 0; i < charAmount; i++)
            {
                randomString.Append(alphabet[Random.Range(0, alphabet.Length)]);
            }

            seed = randomString.ToString();
        }
        // Generate the seed
        rng = new System.Random(seed.GetHashCode());
        mapGenerator = new MapGenerator(width, height, fillPercentage, roomRadius, rng);

        // Get the generated gameplay points & lists 
        orderedEdgeMaps = mapGenerator.orderedEdgeMaps;
        patrolPoints = mapGenerator.patrolPoints;
        endObjPoint = mapGenerator.endObjPosition;
    }

    public void SpawnPlayer(Vector3 spawnPoint)
    {
        GameObject prefab = GameObject.FindGameObjectWithTag("Player");
        if (prefab == null)
        {
            Instantiate(player, spawnPoint, transform.rotation);
        }
        else
        {
            prefab.transform.position = spawnPoint;
        }
    }
    public void SpawnGuard(Vector3 spawnPoint)
    {
        GameObject prefab = GameObject.FindGameObjectWithTag("Guard");
        if (prefab == null)
        {
            Instantiate(guard, spawnPoint, transform.rotation);
        }
        else
        {
            prefab.transform.position = spawnPoint;
            prefab.GetComponent<GuardBehaviour>().Restart();
        }
    }
    public void SpawnEndObj()
    {
        Vector3 toPlayer = (playerSpawn - endObjPoint).normalized;
        Quaternion targetRot = Quaternion.LookRotation(toPlayer, Vector3.up);
        GameObject prefab = GameObject.FindGameObjectWithTag("EndObjective");
        if (prefab == null)
        {
            Instantiate(endObjective, endObjPoint, targetRot);
        }
        else
        {
            prefab.transform.position = endObjPoint;
            prefab.transform.rotation = targetRot;
        }
    }

    public void GenerateEndObjSpawnPoints()
    {
        endObjPoint = mapGenerator.endObjPosition;
        Vector3 worldEndObjPoint = endObjPoint;
        worldEndObjPoint = worldGrid[(int)worldEndObjPoint.x, (int)worldEndObjPoint.z].worldPosition;
        endObjPoint = worldEndObjPoint + Vector3.down;
    }

    public void GenerateSpawnPoints()
    {
        playerSpawn = mapGenerator.playerSpawn;
        playerSpawn = worldGrid[(int)playerSpawn.x, (int)playerSpawn.z].worldPosition + Vector3.up;
        guardSpawn = mapGenerator.guardSpawn;
        guardSpawn = worldGrid[(int)(guardSpawn.x), (int)guardSpawn.z].worldPosition + Vector3.up;
        //Debug.Log("Player Spawn: " + playerSpawn + "Guard Spawn: " + guardSpawn);
    }

    /*
        Create patrol points previously made in MapGenerator class
    */
    public void GeneratePatrolPoints()
    {
        patrolPoints = mapGenerator.patrolPoints;
        for (int i = 0; i < patrolPoints.Count; i++)
        {
            Vector3 worldPatrolPoint = patrolPoints[i];
            worldPatrolPoint = worldGrid[(int)worldPatrolPoint.x, (int)worldPatrolPoint.z].worldPosition;
            patrolPoints[i] = worldPatrolPoint;
        }
    }

    /* 
        Calculate the Perlin Noise at those coordinates
    */
    float Sample(float x, float y)
    {
        float perlin = 0;
        float frequency = 1f;
        float amplitude = 1f;
        for (int i = 0; i < octaves; i++)
        {
            float sampleX = x / scale * frequency + octaveOffsets[i].x;
            float sampleY = y / scale * frequency + octaveOffsets[i].y;
            perlin = Mathf.PerlinNoise(sampleX, sampleY);
            perlin *= amplitude;
            amplitude *= persistance;
            frequency *= lacunarity;
        }

        return perlin;
    }

    public void GenerateWalls()
    {
        Destroy(walls);
        walls = new GameObject("Walls");
        walls.layer = LayerMask.NameToLayer("Walls");
        MeshCollider meshCollider = walls.AddComponent<MeshCollider>();
        Mesh mesh = new Mesh();

        int wallTiles = 0;
        foreach (List<Vector3> wallList in orderedEdgeMaps)
        {
            wallTiles += wallList.Count;
        }
        // 2 traingles per square 2 * 3 = 6
        int triangleIndexCount = 6 * wallTiles;
        int vertexCount = wallTiles * 2;

        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[triangleIndexCount];

        // Indexes
        int vertexIndex = 0;
        int triangleIndex = 0;

        foreach (List<Vector3> edgeMap in orderedEdgeMaps)
        {
            // Triangle Indexes
            int tLeft = vertexIndex;
            int bLeft = vertexIndex + 1;
            int tRight = vertexIndex + 2;
            int bRight = vertexIndex + 3;

            for (int i = 0; i < edgeMap.Count; i++)
            {
                Vector3 bottomLeft = edgeMap[i];
                Vector3 topLeft = new Vector3(bottomLeft.x, wallHeight, bottomLeft.z);

                vertices[vertexIndex++] = topLeft;
                vertices[vertexIndex++] = bottomLeft;

                triangles[triangleIndex++] = tLeft;
                triangles[triangleIndex++] = bLeft;
                triangles[triangleIndex++] = bRight;
                triangles[triangleIndex++] = tLeft;
                triangles[triangleIndex++] = bRight;
                triangles[triangleIndex++] = tRight;

                // Rearrange triangle indexes
                tLeft = tRight;
                tRight += 2;
                tRight = (i == edgeMap.Count - 2) ? tRight - edgeMap.Count * 2 : tRight;
                bLeft = bRight;
                bRight += 2;
                bRight = (i == edgeMap.Count - 2) ? bRight - edgeMap.Count * 2 : bRight;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        meshCollider.sharedMesh = mesh;
    }

    public void GenerateTrees()
    {
        DestroyTrees();
        GameObject forest = new GameObject("Forest");
        int[,] map = mapGenerator.map;
        int index = 0;
        int treeType = Random.Range(0, prefabs.Length);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                vegetationFlags[x, y] = 0;
                if (map[x, y] == 1 && (x % treeSeparation) == 0 && (y % treeSeparation) == 0)
                {
                    Vector3 posToSpawn = worldGrid[x, y].worldPosition + Vector3.down;
                    GameObject tree = (GameObject)Instantiate(prefabs[treeType], posToSpawn, transform.rotation);
                    tree.transform.parent = forest.transform;
                    tree.transform.Rotate(Vector3.up * Random.Range(0, 360));
                    trees.Add(tree);
                    vegetationFlags[x, y] = 1;
                }
                index += 6;
            }
        }
    }

    public void DestroyTrees()
    {
        if (trees != null)
        {
            foreach (GameObject e in trees)
            {
                Destroy(e);
            }
        }
    }

    public void GenerateMesh()
    {
        octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            octaveOffsets[i] = new Vector2(rng.Next(-100000, 100000), rng.Next(-100000, 100000));
        }

        // To make the mesh at origin we need to start from here
        Vector3 bottomLeft = new Vector3(-width / 2 - 0.5f, 0, -height / 2 - 0.5f);

        // 3 vertices per triangle and 2 triangles
        int verticesPerCell = 6;
        int vertexCount = (int)(verticesPerCell * width * height);

        // Allocate the arrays
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        int vertexIndex = 0;


        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Make the vertex positions
                Vector3 cellBottomLeft = bottomLeft + new Vector3(x, 0, y);
                Vector3 cellTopLeft = bottomLeft + new Vector3(x, 0, y + 1);
                Vector3 cellTopRight = bottomLeft + new Vector3(x + 1, 0, y + 1);
                Vector3 cellBotomRight = bottomLeft + new Vector3(x + 1, 0, y);

                // Sample for the y co-ord
                cellBottomLeft.y += Sample(x, y);
                cellTopLeft.y += Sample(x, y + 1);
                cellTopRight.y += Sample(x + 1, y + 1);
                cellBotomRight.y += Sample(x + 1, y);

                // Map vertices to triangles
                int startVertex = vertexIndex;
                vertices[vertexIndex++] = cellBottomLeft;
                vertices[vertexIndex++] = cellTopLeft;
                vertices[vertexIndex++] = cellTopRight;
                vertices[vertexIndex++] = cellBottomLeft;
                vertices[vertexIndex++] = cellTopRight;
                vertices[vertexIndex++] = cellBotomRight;

                // Map triangles
                for (int i = 0; i < 6; i++)
                {
                    triangles[startVertex + i] = startVertex + i;
                    uvs[startVertex + i] = new Vector2(vertices[startVertex + i].x, vertices[startVertex + i].z);
                }

                // Add the navigation point to the grid, add vector.up to each node for better navigation
                bool walkable = (mapGenerator.map[x, y] == 0) ? true : false;
                worldGrid[x, y] = new Node(walkable, cellBottomLeft + (cellTopRight - cellBottomLeft) / 2, x, y);
                worldGrid[x, y].worldPosition += Vector3.up;
            }

        }
        // Assign arrays to the mesh
        terrainMesh.vertices = vertices;
        terrainMesh.triangles = triangles;
        terrainMesh.uv = uvs;

        terrainMesh.RecalculateNormals();
        terrainColider.sharedMesh = terrainMesh;
        terrainMeshRenderer.material = terrainMaterial;
    }

    /* 
        Create weeds wherever there is a free space and not a play area
    */
    void GenerateWeeds()
    {
        Destroy(terrainWeeds);
        weeds = new List<SquarePolygon>();
        int[,] map = mapGenerator.map;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int randomSeparation = Random.Range(1, 3);
                if (map[x, y] == 1 && vegetationFlags[x, y] == 0 && (x % randomSeparation) == 0 && (y % randomSeparation) == 0)
                {
                    float angle = Random.Range(0, 90);
                    float displacement = 40;
                    Vector3 centre = worldGrid[x, y].worldPosition + Vector3.down;
                    // Make 3 squares 
                    for (int i = 0; i < 3; i++)
                    {
                        // Pick a random point along a unit circle using sin/cos
                        Vector3 bottomRight = new Vector3();
                        bottomRight.x = centre.x + (Mathf.Sin(angle + displacement * i) * (weedSize / 2));
                        bottomRight.z = centre.z + (Mathf.Cos(angle + displacement * i) * (weedSize / 2));
                        bottomRight.y = centre.y;

                        Vector3 toCentre = (centre - bottomRight).normalized;
                        Vector3 bottomLeft = bottomRight + (toCentre * weedSize);
                        Vector3 topRight = bottomRight + (Vector3.up * weedSize);
                        Vector3 topLeft = bottomLeft + (Vector3.up * weedSize);

                        weeds.Add(new SquarePolygon(topLeft, bottomLeft, topRight, bottomRight));
                    }
                    vegetationFlags[x, y] = 1;
                }
            }
        }

        MeshGenerator meshGen = new MeshGenerator();

        terrainWeeds = meshGen.GenerateWeeds(weeds, weedMaterial);
        terrainWeeds.transform.position = transform.position;

        //Debug.Log("Number of weeds: " + weeds.Count);
    }

}