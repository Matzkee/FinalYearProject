using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LifeForm : MonoBehaviour {

    GameObject treeStructure = null;

    Rule[] ruleset;
    LSystem lsystem;
    Turtle turtle;
    List<Segment> branches;
    List<Circle> circles;
    
    public Material treeBark;

    public float length = 5.0f;
    public float angleX = 22.5f;
    public float angleY = 30.0f;
    public float lengthRatio = 0.7f;
    public float startRadius = 1.0f;
    public int treeRoundness = 8;
    public float widthRatio = 0.7f;
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

        turtle = new Turtle(startRadius, treeRoundness, lsystem.GetAlphabet(), length, angleX, angleY, gameObject);

        // Generate the alphabet n(generations) times
        for (int i = 0; i <= generations; i++)
        {
            lsystem.Generate();
            
            // Adjust turtle ratios 
            // (normaly it happens after the skeleton generation, 
            // in this case we need to apply it now)
            turtle.ChangeLength(lengthRatio);
            turtle.ChangeWidth(widthRatio);
        }
        // Save current transform position & rotation
        Vector3 currentP = transform.position;
        Quaternion currentR = transform.rotation;


        // Generate the alphabet & pass it to the turtle
        turtle.SetAlphabet(lsystem.GetAlphabet());
        turtle.DrawPlant();


        // Get vector arrays
        GetTreeBranches();
        transform.position = currentP;
        transform.rotation = currentR;

        DestroyTree();
        RenderTree(branches);
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
        branches = turtle.GetBranches();
        circles = turtle.GetCircles();
    }

    // Make new object for each branch with mesh and material applied
    void RenderTree(List<Segment> _segments)
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
        // 3 Vertices per triangle, 2 triangles
        int verticesPerCell = 6;
        int vertexCount = (verticesPerCell * 2 * numOfPoints) * _segments.Count;

        // Alocate new arrays
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[vertexCount];

        int vertexIndex = 0;

        foreach (Segment s in _segments)
        {
            for (int i = 0; i < numOfPoints; i++)
            {
                Vector3 cellBottomLeft = s.startCircle.circlePoints[i];
                Vector3 cellTopLeft = s.endCircle.circlePoints[i];
                Vector3 cellTopRight = s.endCircle.circlePoints[(i + 1) % numOfPoints];
                Vector3 cellBottomRight = s.startCircle.circlePoints[(i + 1) % numOfPoints];

                int startVertex = vertexIndex;
                vertices[vertexIndex++] = cellTopLeft;
                vertices[vertexIndex++] = cellBottomLeft;
                vertices[vertexIndex++] = cellBottomRight;
                vertices[vertexIndex++] = cellTopLeft;
                vertices[vertexIndex++] = cellBottomRight;
                vertices[vertexIndex++] = cellTopRight;

                // Make triangles
                for (int j = 0; j < verticesPerCell; j++)
                {
                    triangles[startVertex + j] = startVertex + j;
                }
            }
        }

        // Assign values to the mesh
        mesh.vertices = vertices;
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
