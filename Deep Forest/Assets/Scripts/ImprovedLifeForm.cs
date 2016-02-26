using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ImprovedLifeForm : MonoBehaviour {

    GameObject treeStructure;

    Rule[] ruleset;
    LSystem lsystem;
    Turtle turtle;
    List<Segment> branches;
    List<Circle> circles;
    List<BranchEnd> branchEnds;

    //Mesh mesh;
    //MeshRenderer meshRenderer;
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
        generations = Random.Range(1, generations);

        // Look up so we rotate the tree structure
        transform.Rotate(Vector3.right * -90.0f);
        // Rules can be applied in an inspector, once game is started all information is
        // taken from an editor
        if (ruleChars != null)
        {
            ruleset = new Rule[ruleChars.Length];
            for (int i = 0; i < ruleChars.Length; i++)
            {
                ruleset[i] = new Rule(ruleChars[i], ruleStrings[i]);
            }
        }
        // Create the L-System and a new Turtle
        lsystem = new LSystem(axiom, ruleset);

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
        MeshRenderer meshRenderer;

        mesh = treeStructure.AddComponent<MeshFilter>().mesh;
        meshRenderer = treeStructure.AddComponent<MeshRenderer>();
        mesh.Clear();

        int numOfPoints = treeRoundness;

        int vertexCount = ((9 * 6) * branches.Count) + 
            ((9 * 3) * branchEnds.Count);
        int optimalVertsCount = (2 * (numOfPoints + 1) * branches.Count) +
           ((numOfPoints + 2) * branchEnds.Count);

        // Alocate new arrays
        Vector3[] vertices = new Vector3[optimalVertsCount];
        Vector2[] uvs = new Vector2[optimalVertsCount];
        int[] triangles = new int[vertexCount];

        int vertexIndex = 0;
        int vertexIndexUV = 0;
        int sideCounter = 0;
        int triangleIndex = 0;
        float tilling = (float)(sideCounter++) / treeRoundness;

        // Set triangle indexes
        int tLeft, bLeft, tRight, bRight, centre;

        Vector2 uvBottom = new Vector2(tilling, 0f);
        Vector2 uvTop = new Vector2(tilling, 1f / treeRoundness);

        foreach (Segment s in branches)
        {
            tLeft = vertexIndex;
            bLeft = vertexIndex + 1;
            tRight = vertexIndex + 2;
            bRight = vertexIndex + 3;
            for (int i = 0; i < numOfPoints + 1; i++)
            {
                vertices[vertexIndex++] = s.endCircle.circlePoints[i % numOfPoints];
                vertices[vertexIndex++] = s.startCircle.circlePoints[i % numOfPoints];
            }

            for (int i = 0; i < numOfPoints + 1; i++)
            {
                // Assign uv control nodes to its corresponding vertices
                uvs[vertexIndexUV++] = uvBottom;
                uvs[vertexIndexUV++] = uvTop;

                // Calculate next uv offset
                tilling = (float)(sideCounter++) / numOfPoints;
                uvBottom = new Vector2(tilling, 0f);
                uvTop = new Vector2(tilling, 1f / numOfPoints);

                // Assign triangle indexes
                triangles[triangleIndex++] = tLeft;
                triangles[triangleIndex++] = bLeft;
                triangles[triangleIndex++] = bRight;
                triangles[triangleIndex++] = tLeft;
                triangles[triangleIndex++] = bRight;
                triangles[triangleIndex++] = tRight;

                // Rearrange triangle indexes
                tLeft = tRight;
                tRight += 2;
                tRight = (tRight >= vertexIndex) ? tRight - 18 : tRight;
                bLeft = bRight;
                bRight += 2;
                bRight = (bRight >= vertexIndex) ? bRight - 18 : bRight;
            }
        }

        //Create the mesh for cones
        foreach (BranchEnd c in branchEnds)
        {
            sideCounter = 0;
            bLeft = vertexIndex;
            bRight = vertexIndex + 1;
            for (int i = 0; i < numOfPoints + 1; i++)
            {
                vertices[vertexIndex++] = c.startCircle.circlePoints[i % numOfPoints];
            }
            // Add extra vertex as centre for uv mapping
            vertices[vertexIndex++] = c.end;
            centre = vertexIndex - 1;
            // Add the centre and set its index
            for (int i = 0; i < numOfPoints + 1; i++)
            {
                // Assign uv control nodes to its corresponding vertices & calculate next offset
                tilling = (float)(sideCounter++) / numOfPoints;
                uvBottom = new Vector2(tilling, 1f / numOfPoints);
                uvs[vertexIndexUV++] = uvBottom;

                // Assign triangle indexes
                triangles[triangleIndex++] = bLeft;
                triangles[triangleIndex++] = bRight;
                triangles[triangleIndex++] = centre;


                // Rearrange triangle indexes
                bLeft = bRight;
                bRight += 1;
                bRight = (bRight >= vertexIndex) ? bRight - 9 : bRight;
            }
            // Use 0.5f for now later on trace circle points on mesh and assign values this way
            Vector2 uvEndPoint = new Vector2(0.5f, 0.5f);
            uvs[vertexIndexUV++] = uvEndPoint; 
        }
        
        // Assign values to the mesh
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        meshRenderer.material = treeBark;
        // Set the tree structure object to its parent
        treeStructure.transform.parent = transform;

        Debug.Log("Number of vertices: " + optimalVertsCount + "\nPolygons to render: " + triangleIndex / 3);
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
