using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainGenerator : MonoBehaviour {

    [Header("Map Options")]
    public int width = 100;
    public int height = 100;
    [Range(0, 100)]
    public int fillPercentage;
    public float scale = 10f;
    public int octaves = 4;
    public float persistance = 1.5f;
    public float lacunarity = 1f;
    public string seed;
    public bool useRandomSeed = false;
    public bool autoUpdate = false;

    public GameObject[] prefabs;
    public Material terrainMaterial;

    System.Random rng;
    Vector2[] octaveOffsets;
    Mesh mesh;
    MeshRenderer meshRenderer;
    CellularAutomata ca;
    List<GameObject> trees;

    // Use this for initialization
    void Start()
    {
        trees = new List<GameObject>();
        mesh = gameObject.AddComponent<MeshFilter>().mesh;
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        mesh.Clear();

        GenerateSeed();
        GenerateMesh();
        GenerateTrees();
    }

    public void GenerateSeed()
    {
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }
        // Generate the seed
        rng = new System.Random(seed.GetHashCode());
        ca = new CellularAutomata(width, height, fillPercentage, rng);
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

    public void GenerateTrees()
    {
        DestroyTrees();
        int[,] map = ca.map;
        int index = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (map[x,y] == 1)
                {
                    int prefabNo = Random.Range(0, prefabs.Length);
                    Vector3 posToSpawn = new Vector3(
                        x - (width / 2), 
                        mesh.vertices[index].y, 
                        y - (height / 2));
                    GameObject tree = (GameObject)Instantiate(prefabs[prefabNo], posToSpawn, transform.rotation);
                    tree.transform.parent = transform;
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
        Vector3 bottomLeft = new Vector3(-width / 2, 0, -height / 2);

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
            }

        }
        // Assign arrays to the mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();
        meshRenderer.material = terrainMaterial;
    }
}
