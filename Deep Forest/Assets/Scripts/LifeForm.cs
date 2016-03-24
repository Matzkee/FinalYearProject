using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LifeForm : MonoBehaviour {

    GameObject treeStructure, treeLeaves;
    MeshGenerator meshGenerator;

    Rule[] ruleset;
    LSystem lsystem;
    Turtle turtle;
    List<BranchSegment> branchSegments;
    List<BranchTip> branchTips;
    List<Circle> circles;
    List<SquarePolygon> leaves;

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
    public int maxGenerations = 0;
    public Material treeBark;

    [Header("Leaf Options")]
    public float leafSize = 2f;
    [Range(0,1)]
    public float leafGravity;
    public Material leafMaterial;

	void Start () {
        meshGenerator = new MeshGenerator();
        //Randomize generation numbers
        maxGenerations = Random.Range(2, maxGenerations + 1);

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
        for (int i = 0; i < maxGenerations; i++)
        {
            lsystem.Generate();
        }
        // Save current transform position & rotation
        Vector3 currentP = transform.position;
        Quaternion currentR = transform.rotation;

        // Generate the alphabet & pass it to the turtle
        turtle.SetAlphabet(lsystem.GetAlphabet());
        turtle.GenerateSkeleton();

        transform.position = currentP;
        transform.rotation = currentR;

        // Get vector arrays & render the tree
        GetTreeBranches();
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
            Destroy(treeLeaves);
        }
    }
    // Get vector lists
    void GetTreeBranches()
    {
        branchSegments = turtle.branchSegments;
        circles = turtle.circles;
        branchTips = turtle.branchTips;
    }

    // Make new object for each branch with mesh and material applied
    void RenderTree()
    {
        // Generate new object with MeshFilter and Renderer
        treeStructure = meshGenerator.GenerateTreeMesh(turtle.branchSegments, turtle.branchTips, treeBark);
        // Set the tree structure object to its parent
        treeStructure.transform.parent = transform;
    }

    // Draw debug lines
    void OnDrawGizmos()
    {
        
        if (branchSegments != null && skeletonLines)
        {
            foreach (BranchSegment b in branchSegments)
            {
                Gizmos.color = b.color;
                Gizmos.DrawLine(b.start, b.end);
            }
            foreach (BranchTip be in branchTips)
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

    void MakeLeaves()
    {
        leaves = new List<SquarePolygon>();

        foreach (BranchTip b in branchTips)
        {
            for (int i = 0; i < treeRoundness; i++)
            {
                MakeLeaf(b.start, b.startCircle.circlePoints[i], b.end, leafSize, leafGravity);
            }
        }

        treeLeaves = meshGenerator.GenerateTreeLeaves(leaves, leafMaterial);
        treeLeaves.transform.parent = transform;
    }


    void MakeLeaf(Vector3 branchCentre, Vector3 branchStart, Vector3 branchEnd, float size, float leafGravity)
    {
        Vector3 toStart, toEnd, toLeafStart;
        Vector3 leafStart;
        Vector3 leafPosPerpendicular;
        Vector3 topLeft, bottomLeft, topRight, bottomRight;

        // Create a leaf position at a random spot between start and end points
        leafStart = Vector3.Lerp(branchStart, branchEnd, Random.value);
        // Calculate direction vectors
        toStart = (branchStart - branchCentre).normalized;
        toEnd = (branchEnd - branchStart).normalized;
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
        bottomLeft += ((branchCentre - bottomLeft).normalized * leafGravity);

        leaves.Add(new SquarePolygon(topLeft, bottomLeft, topRight, bottomRight));
    }
}
