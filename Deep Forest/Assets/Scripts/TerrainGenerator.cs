using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainGenerator : MonoBehaviour {

    GameObject walls;
    List<GameObject> trees;
    List<Vector3> orderedEdgeMap;
    public Node[,] worldGrid;

    System.Random rng;
    Vector2[] octaveOffsets;
    Mesh mesh;
    MeshRenderer meshRenderer;
    MeshCollider colider;
    MapGenerator ca;

    [Header("Map Options")]
    public int width = 100;
    public int height = 100;
    [Range(0, 100)]
    public int fillPercentage;
    public float scale = 10f;
    public int octaves = 2;
    public float persistance = 1.5f;
    public float lacunarity = 1f;
    public float wallHeight = 4;
    public float treeSeparation = 3f;
    public string seed;
    public bool useRandomSeed = false;
    public bool autoUpdate = false;
    public bool generateTrees = false;

    public GameObject[] prefabs;
    public Material terrainMaterial;



    // Use this for initialization
    void Start()
    {
        worldGrid = new Node[width, height];
        trees = new List<GameObject>();
        mesh = gameObject.AddComponent<MeshFilter>().mesh;
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        colider = gameObject.AddComponent<MeshCollider>();
        mesh.Clear();

        GenerateSeed();
        GenerateMesh();
        GenerateWalls();
        if (generateTrees)
        {
            GenerateTrees();
        }
    }

    void OnDrawGizmos()
    {
        if (ca != null && ca.patrolPoints != null)
        {
            foreach (Vector3 patrolNode in ca.patrolPoints)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireCube(
                    worldGrid[Mathf.RoundToInt(patrolNode.x + width/2), Mathf.RoundToInt(patrolNode.z + height/2)].worldPosition, 
                    Vector3.one);
            }
        }
    }

    public void GenerateSeed()
    {
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }
        // Generate the seed
        rng = new System.Random(seed.GetHashCode());
        ca = new MapGenerator(width, height, fillPercentage, rng);
        orderedEdgeMap = ca.orderedEdgeMap;
    }

    // Calculate the Perlin Noise at those coordinates
    float Sample(float x, float y)
    {
        float perlinValue = 0;
        float frequency = 1f;
        float amplitude = 1f;
        for (int i = 0; i < octaves; i++)
        {
            float sampleX = x / scale * frequency + octaveOffsets[i].x;
            float sampleY = y / scale * frequency + octaveOffsets[i].y;
            perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
            perlinValue *= amplitude;
            amplitude *= persistance;
            frequency *= lacunarity;
        }

        return perlinValue;
    }

    public void GenerateWalls()
    {
        Destroy(walls);
        walls = new GameObject("Walls");
        walls.layer = LayerMask.NameToLayer("Walls");
        MeshCollider meshCollider = walls.AddComponent<MeshCollider>();
        Mesh mesh = new Mesh();

        // 2 traingles per square 2 * 3 = 6
        int triangleIndexCount = 6 * orderedEdgeMap.Count;
        int vertexCount = orderedEdgeMap.Count * 2;
        
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[triangleIndexCount];

        // Indexes
        int vertexIndex = 0;
        int triangleIndex = 0;
        // Triangle Indexes
        int tLeft = vertexIndex;
        int bLeft = vertexIndex + 1;
        int tRight = vertexIndex + 2;
        int bRight = vertexIndex + 3;

        for (int i = 0; i < orderedEdgeMap.Count; i++)
        {
            Vector3 bottomLeft = orderedEdgeMap[i];
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
            tRight = tRight % vertexCount;
            bLeft = bRight;
            bRight += 2;
            bRight = bRight % vertexCount;
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
        int[,] map = ca.map;
        int index = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (map[x,y] == 1 && (x % treeSeparation) == 0 && (y % treeSeparation) == 0)
                {
                    int prefabNo = Random.Range(0, prefabs.Length);
                    Vector3 posToSpawn = new Vector3(
                        x - (width / 2), 
                        mesh.vertices[index].y, 
                        y - (height / 2));
                    GameObject tree = (GameObject)Instantiate(prefabs[prefabNo], posToSpawn, transform.rotation);
                    tree.transform.parent = forest.transform;
                    tree.transform.Rotate(Vector3.up * Random.Range(0, 360));
                    trees.Add(tree);
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

                // Add the navigation point to the grid
                bool walkable = (ca.map[x, y] == 0) ? true : false;
                worldGrid[x, y] = new Node(walkable, cellBottomLeft + (cellTopRight - cellBottomLeft) / 2, x, y);
            }

        }
        // Assign arrays to the mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();
        colider.sharedMesh = mesh;
        meshRenderer.material = terrainMaterial;
    }

}
