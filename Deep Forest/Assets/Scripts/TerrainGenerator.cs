using UnityEngine;
using System.Collections;

public class TerrainGenerator : MonoBehaviour {

    public Vector2 samples = new Vector2(20, 20);
    public Material terrainMaterial;

    public float scale = 5;
    public float amplitude = 10;

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

    float Sample(float x, float y)
    {

        //float theta = Mathf.PI;
        //return Mathf.Sin(theta * (x / samples.x)) * Mathf.Sin(theta * (y / samples.y)) * amplitude;
        return Mathf.PerlinNoise(x / scale, y / scale) * amplitude;
    }

    public void GenerateMesh()
    {
        // We will start the actual xyz's of the mesh from this position
        Vector3 bottomLeft = new Vector3(-samples.x / 2, 0, -samples.y / 2);

        // 3 vertices per triangle and 2 triangles
        int verticesPerCell = 6;
        int vertexCount = (int)(verticesPerCell * samples.x * samples.y);

        // Allocate the arrays
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        int vertexIndex = 0;


        for (int y = 0; y < samples.y; y++)
        {
            for (int x = 0; x < samples.x; x++)
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

                // Map triangles and uv's
                for (int i = 0; i < 6; i++)
                {
                    triangles[startVertex + i] = startVertex + i;
                    uvs[startVertex + i] = new Vector2((float)x / samples.x, (float)y / samples.y);
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
