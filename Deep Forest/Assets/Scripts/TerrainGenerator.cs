using UnityEngine;
using System.Collections;

public class TerrainGenerator : MonoBehaviour {

    public int width = 50;
    public int height = 50;
    public Material terrainMaterial;

    public float scale = 10;
    public float amplitude = 3;

    Mesh mesh;
    MeshRenderer meshRenderer;

    // Use this for initialization
    void Start()
    {
        mesh = gameObject.AddComponent<MeshFilter>().mesh;
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        mesh.Clear();
        
        GenerateMesh();
    }

    // Calculate the Perlin Noise at those coordinates
    float Sample(float x, float y)
    {
        return Mathf.PerlinNoise(x / scale, y / scale) * amplitude;
    }

    public void GenerateMesh()
    {
        // We will start the actual xyz's of the mesh from this position
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

            // Assign arrays to the mesh
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;

            mesh.RecalculateNormals();
            meshRenderer.material = terrainMaterial;
        }
    }
}
