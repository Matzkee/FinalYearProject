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
    List<Leaf> leaves;

    [Header("Tree Options")]
    public float length = 5.0f;
    public float width = 1.0f;
    public float turn = 22.5f;
    public float pitch = 30.0f;
    public float roll = 30.0f;
    public float lengthRatio = 0.7f;
    public float widthRatio = 0.7f;
    public int treeRoundness = 8;
    public string axiom;
    public char[] ruleChars;
    public string[] ruleStrings;
    public bool skeletonLines = false;
    public bool skeletonCircles = false;
    public int generations = 0;
    public Material treeBark;

    [Header("Leaf Options")]
    public float leafSize = 2f;
    [Range(0,1)]
    public float leafGravity;
    public Material leafMaterial;
    int leavesCount;

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
            length, turn, pitch, roll, gameObject, widthRatio, lengthRatio);

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
        MakeLeaves();
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
        
        //Debug.Log("Number of vertices: " + optimalVertsCount + "\nPolygons to render: " + triangleIndex / 3);
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

    void RenderLeaves()
    {
        // Make new object for leaves
        GameObject l = new GameObject("Leaves");
        Mesh mesh;
        MeshRenderer meshRenderer;

        mesh = l.AddComponent<MeshFilter>().mesh;
        meshRenderer = l.AddComponent<MeshRenderer>();
        mesh.Clear();

        int vertexCount = leaves.Count * 4;
        int triangleCount = leaves.Count * 6;
        // Indexes
        int vertexIndex = 0;
        int triangleIndex = 0;
        int uvIndex = 0;
        // Triangle indexes
        int tLeft = vertexIndex;
        int bLeft = vertexIndex + 1;
        int tRight = vertexIndex + 2;
        int bRight = vertexIndex + 3;
        // Alocate new arrays
        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        int[] triangles = new int[triangleCount];

        foreach (Leaf t in leaves)
        {
            // Apply vertices
            vertices[vertexIndex++] = t.tLeft;
            vertices[vertexIndex++] = t.bLeft;
            vertices[vertexIndex++] = t.tRight;
            vertices[vertexIndex++] = t.bRight;
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

            tLeft = vertexIndex;
            bLeft = vertexIndex + 1;
            tRight = vertexIndex + 2;
            bRight = vertexIndex + 3;
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        // Assing leaves to the tree structure
        meshRenderer.material = leafMaterial;
        l.transform.parent = treeStructure.transform;
    }

    void MakeLeaves()
    {
        leaves = new List<Leaf>();

        foreach (BranchEnd b in branchEnds)
        {
            for (int i = 0; i < treeRoundness; i++)
            {
                MakeLeaf(b.start, b.startCircle.circlePoints[i], b.end, leafSize, leafGravity);
                leavesCount++;
            }
        }

        RenderLeaves();
        Debug.Log("Leaves Count: " + leavesCount + " Polygon Count: " + leavesCount * 2 + " Vertex Count: " + leavesCount * 4);
    }

    // Future work:
    // Change this to use less vertices & apply uv maps for texturing
    void MakeLeaf(Vector3 centre, Vector3 start, Vector3 end, float size, float leafGravity)
    {
        Vector3 toStart, toEnd, toLeafStart;
        Vector3 leafStart;
        Vector3 leafPosPerpendicular;
        Vector3 topLeft, bottomLeft, topRight, bottomRight;

        // Create a leaf position at a random spot between start and end points
        leafStart = Vector3.Lerp(start, end, Random.value);
        // Calculate direction vectors
        toStart = (start - centre).normalized;
        toEnd = (end - start).normalized;
        // perpendicular vector to leaf position vector
        leafPosPerpendicular = Vector3.Cross(toEnd, toStart);
        // at this point we can take 2 points setting boundaries
        bottomRight = leafStart + (leafPosPerpendicular * (leafSize / 2));
        bottomLeft = leafStart + (leafPosPerpendicular * -(leafSize / 2));
        // Now we need to calculate 2 perpendicular points to the ones just created
        // First calculate direction from just created bottomRight point to leaf starting point
        toLeafStart = (leafStart - bottomRight).normalized;
        // Then get the cross priduct between toEnd direction and toLeafStart direction
        leafPosPerpendicular = Vector3.Cross(toEnd, toLeafStart);
        // Now that we have a perpendicular vector we can add it to previosly created 2 points to
        // complete the box, also we can apply gravity
        Vector3 gravity = Vector3.down * leafGravity;
        topLeft = bottomLeft + (leafPosPerpendicular * (leafSize));
        topRight = bottomRight + (leafPosPerpendicular * (leafSize));
        topLeft += gravity;
        topRight += gravity;
        // Apply gravity to bottomLeft as well to create a small illusion of 3d object
        bottomLeft += gravity;

        leaves.Add(new Leaf(topLeft, bottomLeft, topRight, bottomRight));
    }

    struct Leaf
    {
        public Vector3 tLeft;
        public Vector3 bLeft;
        public Vector3 tRight;
        public Vector3 bRight;

        public Leaf(Vector3 topLeft, Vector3 bottomLeft, Vector3 topRight, Vector3 bottomRight)
        {
            tLeft = topLeft;
            bLeft = bottomLeft;
            tRight = topRight;
            bRight = bottomRight;
        }
    }
}
