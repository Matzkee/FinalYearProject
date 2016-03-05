using UnityEngine;
using System.Collections;

public class Test2 : MonoBehaviour {

    Vector3 start;
    Vector3 end;
    
    float distance = 2f;
    public float leafSize = 2f;
    [Range(0,2)]
    public float leafGravity;
    Vector3 toStart, toEnd, toLeafStart;
    Vector3 leafStart, leafEnd;
    Vector3 leafPosPerpendicular;
    public Material leafMaterial;

    Vector3 bottomLeft, bottomRight, topLeft, topRight;


    // Use this for initialization
    void Start () {

        // Sample geometry to work with, a skewed line
        Vector3 centre = transform.position;
        Quaternion startRot = transform.rotation;
        transform.Translate(Vector3.forward * distance);
        start = transform.position;
        transform.position = centre;
        transform.rotation = startRot;
        transform.Rotate(Vector3.right * -90f);
        transform.Translate(Vector3.forward * distance * 5);
        end = transform.position;

        // leaf position calculation
        leafStart = Vector3.Lerp(start, end, Random.value);
        // direction towards start point from centre point
        toStart = start - centre;
        toStart.Normalize();
        // direction towards end point from start point
        toEnd = end - start;
        toEnd.Normalize();
        // perpendicular vector to random leaf position vector
        leafPosPerpendicular = Vector3.Cross(toEnd, toStart);
        // at this point we can take 2 points setting boundaries
        bottomRight = leafStart + (leafPosPerpendicular * (leafSize / 2));
        bottomLeft = leafStart + (leafPosPerpendicular * -(leafSize / 2));

        // Now we need to calculate 2 perpendicular point to the ones jsut created
        // First calculate direction from just created bottomRight point to leaf starting point
        toLeafStart = (leafStart - bottomRight).normalized;
        // Then get the cross oriduct between toEnd direction and toLeafStart direction
        leafPosPerpendicular = Vector3.Cross(toEnd, toLeafStart);
        // Now that we have a perpendicular vector we can add it to previosly created 2 points to
        // complete the box, also we can apply gravity
        Vector3 gravity = Vector3.down * leafGravity;
        topLeft = bottomLeft + (leafPosPerpendicular * (leafSize));
        topRight = bottomRight + (leafPosPerpendicular * (leafSize));
        topLeft += gravity;
        topRight += gravity;
        bottomLeft += gravity;

        GenerateMesh();

    }

    void GenerateMesh()
    {
        // Make new object for leaves
        GameObject leaves = new GameObject("Leaves");
        Mesh mesh;
        MeshRenderer meshRenderer;

        mesh = leaves.AddComponent<MeshFilter>().mesh;
        meshRenderer = leaves.AddComponent<MeshRenderer>();
        mesh.Clear();

        // 2 triangles, 3 vertices per triangle
        int triangleCount = 6;
        int vertexCount = 4;
        
        int vertexIndex = 0;
        int triangleIndex = 0;
        int uvIndex = 0;
        int normalIndex = 0;

        int tLeft = triangleIndex;
        int bLeft = triangleIndex + 1;
        int tRight = triangleIndex + 2;
        int bRight = triangleIndex + 3;
        // Alocate new arrays
        Vector3[] vertices = new Vector3[vertexCount];
        Vector3[] normals = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        int[] triangles = new int[triangleCount];


        // Apply vertices
        vertices[vertexIndex++] = topLeft;
        vertices[vertexIndex++] = bottomLeft;
        vertices[vertexIndex++] = topRight;
        vertices[vertexIndex++] = bottomRight;

        // Apply normals
        normals[normalIndex++] = topLeft + Vector3.up;
        normals[normalIndex++] = bottomLeft + Vector3.up;
        normals[normalIndex++] = topRight + Vector3.up;
        normals[normalIndex++] = bottomRight + Vector3.up;

        // Apply uvs
        uvs[uvIndex++] = new Vector2(0, 1);
        uvs[uvIndex++] = new Vector2(0, 0);
        uvs[uvIndex++] = new Vector2(1, 1);
        uvs[uvIndex++] = new Vector2(1, 0);

        // Apply triangles
        triangles[triangleIndex++] = tLeft;
        triangles[triangleIndex++] = bRight;
        triangles[triangleIndex++] = bLeft;
        triangles[triangleIndex++] = tLeft;
        triangles[triangleIndex++] = tRight;
        triangles[triangleIndex++] = bRight;
        

        mesh.vertices = vertices;
        mesh.uv = uvs;
        //mesh.normals = normals;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        // Assing leaves to the tree structure
        meshRenderer.material = leafMaterial;
        leaves.transform.parent = transform;
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(start, end);

        Gizmos.color = Color.gray;
        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomLeft, topLeft);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topLeft, topRight);
    }
}
