using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestLifeForm : MonoBehaviour {

    GameObject treeStructure = null;

    Rule[] ruleset;
    LSystem lsystem;
    Turtle turtle;
    List<Segment> branches;
    List<Circle> circles;
    List<BranchEnd> branchEnds;
    
    public Material treeBark;

    public float length = 5.0f;
    public float width = 1.0f;
    public float angleX = 22.5f;
    public float angleY = 30.0f;
    public float lengthRatio = 0.7f;
    public float widthRatio = 0.7f;
    public int treeRoundness = 8;
    public string axiom;
    public char[] ruleChars;
    public string[] ruleStrings;
    public bool skeletonLines = false;
    public bool skeletonCircles = false;

    public int generations = 0;

	void Start () {
        //Randomize generation numbers
        generations = Random.Range(1,generations);

        // Look up so we rotate the tree structure
        transform.Rotate(Vector3.right * -90.0f);
        // Rules can be applied in an inspector, once game is started all information is
        // taken from an editor
        if (ruleChars != null)
        {
            ruleset = new Rule[ruleChars.Length];
            for(int i = 0; i < ruleChars.Length; i++)
            {
                ruleset[i] = new Rule(ruleChars[i], ruleStrings[i]);
            }
        }
        // Create the L-System and a new Turtle
        lsystem = new LSystem(axiom,ruleset);

        turtle = new Turtle(width, treeRoundness, lsystem.GetAlphabet(), 
            length, angleX, angleY, gameObject, widthRatio, lengthRatio);

        // Generate the alphabet n(generations) times
        for (int i = 0; i <= generations; i++)
        {
            lsystem.Generate();
        }
        // Save current transform position & rotation
        Vector3 currentP = transform.position;
        Quaternion currentR = transform.rotation;

        // Generate the alphabet & pass it to the turtle
        turtle.SetAlphabet(lsystem.GetAlphabet());
        turtle.GenerateSkeleton();

        // Get vector arrays
        GetTreeBranches();
        transform.position = currentP;
        transform.rotation = currentR;

        DestroyTree();
        RenderTree();
    }

    // Destroy previous tree structure, if exist
    void DestroyTree()
    {
        if (treeStructure != null)
        {
            Destroy(treeStructure);
        }
    }
    // Get vector lists
    void GetTreeBranches()
    {
        branches = turtle.branches;
        circles = turtle.circles;
        branchEnds = turtle.branchEnds;
    }

    // Make new object for each branch with mesh and material applied
    void RenderTree()
    {
        // Generate new object with MeshFilter and Renderer
        treeStructure = new GameObject("Tree Structure");
        Mesh mesh;
        MeshFilter filter;
        MeshRenderer meshRenderer;

        filter = treeStructure.AddComponent<MeshFilter>();
        mesh = filter.mesh;
        meshRenderer = treeStructure.AddComponent<MeshRenderer>();
        mesh.Clear();

        int numOfPoints = treeRoundness;
        // 3 Vertices per triangle/polygon
        int verticesPerPolygon = 3;
        int vertexCount = ((verticesPerPolygon * 2 * numOfPoints) * branches.Count) + 
            ((verticesPerPolygon * numOfPoints) * branchEnds.Count);
        int optimalVertsCount = ((2 * numOfPoints) * branches.Count);// +
           // ((verticesPerPolygon * numOfPoints) * branchEnds.Count);

        Debug.Log("Number of vertices: " + optimalVertsCount + "\nPolygons to render: " + vertexCount/3);
        
        // Alocate new arrays
        Vector3[] vertices = new Vector3[optimalVertsCount];
        Vector2[] uvs = new Vector2[optimalVertsCount];
        int[] triangles = new int[vertexCount];

        int vertexIndex = 0;
        int vertexIndexUV = 0;
        int sideCounter = 0;
        float tilling = (float)(sideCounter++) / (1f / treeRoundness);


        int segmentIndex = 0;

        int tLeft = 0;
        int bLeft = 1;
        int tRight = 2;
        int bRight = 3;

        Vector2 uvBottomLeft = new Vector2(0f, tilling);
        Vector2 uvBottomRight = new Vector2(1f / treeRoundness, tilling);
        tilling = (float)(sideCounter++) / (1f / treeRoundness);
        Vector2 uvTopLeft = new Vector2(0f, tilling);
        Vector2 uvTopRight = new Vector2(1f / treeRoundness, tilling);

        foreach (Segment s in branches)
        {
            for (int i = 0; i < numOfPoints; i++)
            {
                vertices[segmentIndex++] = s.endCircle.circlePoints[i];
                vertices[segmentIndex++] = s.startCircle.circlePoints[i];
            }

            for (int i = 0; i < numOfPoints; i++)
            {
                uvs[vertexIndexUV++] = uvBottomLeft;
                uvs[vertexIndexUV++] = uvBottomRight;

                uvBottomLeft = uvTopLeft;
                uvBottomRight = uvTopRight;
                tilling = (float)(sideCounter++) / (1f/ treeRoundness);
                uvTopLeft = new Vector2(0f, tilling);
                uvTopRight = new Vector2(1f / treeRoundness, tilling);

                int startVertex = vertexIndex;
                vertexIndex += 6;

                triangles[startVertex] = tLeft;
                triangles[startVertex + 1] = bLeft;
                triangles[startVertex + 2] = bRight;
                triangles[startVertex + 3] = tLeft;
                triangles[startVertex + 4] = bRight;
                triangles[startVertex + 5] = tRight;
                tLeft = tRight;
                tRight += 2;
                tRight = (tRight >= segmentIndex)? tRight - 16 : tRight;
                bLeft = bRight;
                bRight += 2;
                bRight = (bRight >= segmentIndex)? bRight - 16 : bRight;

                //vertices[vertexIndex++] = cellTopLeft;
                //vertices[vertexIndex++] = cellBottomLeft;
                //vertices[vertexIndex++] = cellBottomRight;
                //vertices[vertexIndex++] = cellTopLeft;
                //vertices[vertexIndex++] = cellBottomRight;
                //vertices[vertexIndex++] = cellTopRight;

                //uvs[vertexIndexUV++] = uvTopLeft;
                //uvs[vertexIndexUV++] = uvBottomLeft;
                //uvs[vertexIndexUV++] = uvBottomRight;

                //uvs[vertexIndexUV++] = uvTopLeft;
                //uvs[vertexIndexUV++] = uvBottomRight;
                //uvs[vertexIndexUV++] = uvTopRight;

                // Make triangles
                //for (int j = 0; j < verticesPerPolygon * 2; j++)
                //{
                //    triangles[startVertex + j] = startVertex + j;
                //}
            }

            tLeft = segmentIndex;
            bLeft = segmentIndex + 1;
            tRight = segmentIndex + 2;
            bRight = segmentIndex + 3;
        }

        /*//Create the mesh for cones
        foreach (BranchEnd c in branchEnds)
        {
            for (int i = 0; i < numOfPoints; i++)
            {
                Vector3 bottomLeft = c.startCircle.circlePoints[i];
                Vector3 bottomRight = c.startCircle.circlePoints[(i + 1) % numOfPoints];
                Vector3 endPoint = c.end;

                Vector2 uvBottomLeft = new Vector2(0f, tilling);
                Vector2 uvBottomRight = new Vector2(1f / treeRoundness, tilling);
                tilling = (float)(sideCounter++) / (1f / treeRoundness);
                Vector2 uvEndPoint = new Vector2(0.5f / treeRoundness, tilling);

                int startVertex = vertexIndex;
                vertices[vertexIndex++] = bottomLeft;
                vertices[vertexIndex++] = bottomRight;
                vertices[vertexIndex++] = endPoint;

                //uvs[vertexIndexUV++] = uvBottomLeft;
                //uvs[vertexIndexUV++] = uvBottomRight;
                //uvs[vertexIndexUV++] = uvEndPoint;

                // Make triangles
                for (int j = 0; j < verticesPerPolygon; j++)
                {
                    triangles[startVertex + j] = startVertex + j;
                }


            }
        }*/
        //int vert = 0;
        //int sideCounter = 0;
        //float t = (float)(sideCounter++) / (sideCounter % treeRoundness);
        //while (vert < vertices.Length)
        //{

        //    Vector2 bottomLeft = new Vector2(t, 0f);
        //    Vector2 bottomRight = new Vector2(t, 1f);
        //    t = (float)(sideCounter++) / (treeRoundness);
        //    Vector2 topLeft = new Vector2(t, 0f);
        //    Vector2 topRight = new Vector2(t, 1f);

        //    uvs[vert++] = topLeft;
        //    uvs[vert++] = bottomLeft;
        //    uvs[vert++] = bottomRight;

        //    uvs[vert++] = topLeft;
        //    uvs[vert++] = bottomRight;
        //    uvs[vert++] = topRight;

        //    //uvs[vert++] = new Vector2(t, 0f);
        //    //uvs[vert++] = new Vector2(t, 1f);
        //}


        // Assign values to the mesh
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        meshRenderer.material = treeBark;
        // Set the tree structure object to its parent
        treeStructure.transform.parent = transform;
    }

    // Draw debug lines
    void OnDrawGizmos()
    {
        
        if (branches != null && skeletonLines)
        {
            foreach (Segment b in branches)
            {
                Gizmos.color = b.color;
                Gizmos.DrawLine(b.start, b.end);
            }
            foreach (BranchEnd be in branchEnds)
            {
                Gizmos.color = be.color;
                Gizmos.DrawLine(be.start, be.end);
            }
        }
        
        if (circles != null && skeletonCircles)
        {
            for (int i = 0; i < circles.Count; i++)
            {
                for (int j = 1; j <= circles[i].circlePoints.Count; j++)
                {
                    Vector3 prev = circles[i].circlePoints[j - 1];
                    Vector3 next = circles[i].circlePoints[j % circles[i].circlePoints.Count];
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(prev, next);
                }
            }
        }
    }
}
