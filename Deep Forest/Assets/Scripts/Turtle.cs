using UnityEngine;
using System.Collections.Generic;
/*  L-Systems contain 3 main components:
        Alphabet - composition of characters which the program works with
        Axiom - an Initial state of the system using characters from alphabet
        Rules - rules of an L-system applied recursively (predecessor & successor)

        Example:
            Alphabet: A B
            Aciom: A
            Rules: (A -> AB), (B -> A)

        Now Lets create our alphabet
        F: translate + add branch to the list
        G: Move Forward
        +: rotate along X axis(angle)
        -: rotate along X axis(-angle)
        z: rotate along Y axis(angle)
        a: rotate along Y axis(-angle)
        [: push position & rotation to a stack
        ]: pop position & rotation from the stack

    We have 3 classes which perform an L-System:
        Rule.cs
        LSystem.cs
        Turtle.cs
    Rule is used to specify one or more rules to an L-System lifeform
    LSystem performs string appending with specified rules
    Turtle uses the specified commands and applies them to each letter found in previously
    appended string from L-System
*/
/*
    To do:
    Implement either booolean flags or linked list to determine tree trunk and its childs
    This step will allow for easier branch sizing and more control over tree growth
    */
public class Turtle{

    float length;
    float turn, pitch, roll;
    string alphabetToDraw;
    int treeRoundness;
    float trunkWidth;
    float widthDecreseRatio;
    float lengthDecreaseRatio;

    public List<Segment> branches, branchesToDelete;
    public List<Circle> circles;
    public List<BranchEnd> branchEnds;
    Stack<Coord> coordStack;
    // We need position information from the attached gameObject
    Transform treeTransform;
    // Assign a tree bark material
    public Material material;

    public Turtle(float _radius, int _detail, string a, float _length, float _turn, float _pitch, float _roll, 
        GameObject _currentTree, float widthRatio, float lengthRatio)
    {
        treeRoundness = _detail;
        trunkWidth = _radius;

        treeTransform = _currentTree.transform;
        branches = new List<Segment>();
        branchesToDelete = new List<Segment>();
        branchEnds = new List<BranchEnd>();
        coordStack = new Stack<Coord>();

        alphabetToDraw = a;
        length = _length;
        turn = _turn;
        pitch = _pitch;
        roll = _roll;

        widthDecreseRatio = widthRatio;
        lengthDecreaseRatio = lengthRatio;
    }

    public void GenerateSkeleton()
    {
        Vector3 lastPosition;
        Quaternion lastRotation;
        Circle lastCircle;
        // Make a new branches list
        branches = new List<Segment>();
        circles = new List<Circle>();
        int repeats = 0;
        // Add and extra character for better case scenario
        alphabetToDraw += "x";

        lastPosition = treeTransform.position;
        lastRotation = treeTransform.rotation;
        lastCircle = CreateCircleAt(treeTransform, trunkWidth, treeRoundness);
        circles.Add(lastCircle);
        // Follow the action depending on current character in alphabet
        for (int i = 0; i < alphabetToDraw.Length; i++)
        {
            char c = alphabetToDraw[i];
            if (c == 'F')
            {
                treeTransform.Translate(Vector3.forward * length);
                length *= lengthDecreaseRatio;
                if (alphabetToDraw[i + 1].Equals('F'))
                {
                    // Keep going until another character is found
                    repeats++;
                }
                else
                {
                    // Decrease width ratio
                    trunkWidth *= widthDecreseRatio;
                    // Make a new Circle
                    Circle newCircle = CreateCircleAt(treeTransform, trunkWidth, treeRoundness);
                    // This is mainly for debugging - comment this out in the future
                    circles.Add(newCircle);
                    // Add a new segment
                    branches.Add(new Segment(lastPosition, treeTransform.position, lastCircle, newCircle, lastRotation));

                    // Set a new vector for tracking position
                    lastRotation = treeTransform.rotation;
                    lastPosition = treeTransform.position;
                    lastCircle = newCircle;
                    // Reset the counter
                    repeats = 0;
                }
            }
            // +- Turn right or left
            else if (c == '+')
            {
                treeTransform.Rotate(Vector3.up * turn);
            }
            else if (c == '-')
            {
                treeTransform.Rotate(Vector3.up * -turn);
            }
            // v^ pitch down, pitch up
            else if (c == 'v')
            {
                treeTransform.Rotate(Vector3.right * pitch);
            }
            else if (c == '^')
            {
                treeTransform.Rotate(Vector3.right * -pitch);
            }
            // \/ roll left, roll right
            else if (c == '\\')
            {
                treeTransform.Rotate(Vector3.forward * roll);
            }
            else if(c == '/'){
                treeTransform.Rotate(Vector3.forward * -roll);
            }
            // Save current position
            else if (c == '[')
            {
                Coord currentCoord = new Coord(treeTransform.position, treeTransform.rotation,
                    lastCircle, trunkWidth, length);
                coordStack.Push(currentCoord);
                trunkWidth *= 0.7f;
                lastCircle = CreateCircleAt(treeTransform, trunkWidth, treeRoundness);
            }
            // Restore last position saved
            else if (c == ']')
            {
                Coord lastCord = coordStack.Pop();
                // Check if the branch is last and change its debug color

                if (!Vector3.Equals(treeTransform.position,lastCord.branchPos))
                {
                    // Delete the cylinder and make a cone instead
                    // Also remove the last set of circle points
                    BranchEnd branchEnd = new BranchEnd(
                        branches[branches.Count - 1].start,
                        branches[branches.Count - 1].end,
                        branches[branches.Count - 1].startCircle);
                    branchesToDelete.Add(branches[branches.Count - 1]);
                    circles.Remove(branches[branches.Count - 1].endCircle);
                    branchEnd.color = Color.green;
                    branchEnds.Add(branchEnd);
                }
                treeTransform.position = lastCord.branchPos;
                treeTransform.rotation = lastCord.branchRot;

                length = lastCord.length;
                trunkWidth = lastCord.width;
                lastCircle = lastCord.circle;
                lastPosition = lastCord.branchPos;
                lastRotation = lastCord.branchRot;
            }
        }

        // Recycle branches at the end
        RecycleBranches();
    }

    void RecycleBranches()
    {
        foreach (Segment s in branchesToDelete)
        {
            branches.Remove(s);
        }
        branchesToDelete.Clear();
        branchesToDelete.TrimExcess();
    }

    public Circle CreateCircleAt(Transform _centre, float _radius, int _numpoints)
    {
        //** Note: Change this to quaternion rotation in the future
        //** and test for performance?

        List<Vector3> newPoints = new List<Vector3>();
        float theta = Mathf.PI * 2.0f / _numpoints;
        // Save current transform's rotation
        Quaternion prevRot = _centre.rotation;
        for (int i = 0; i < _numpoints; i++)
        {
            // Rotate the transform by preset theta and get point directly above it
            _centre.Rotate(Vector3.forward * (theta * Mathf.Rad2Deg));
            Vector3 newPoint = (_centre.position + (_centre.up * _radius));
            newPoints.Add(newPoint);
        }
        // Restore the transform to previous rotation
        _centre.rotation = prevRot;
        return new Circle(newPoints);
    }

    // This class is used to store transform properties
    public class Coord
    {
        public Vector3 branchPos;
        public Quaternion branchRot;
        public Circle circle;
        public float width, length;

        public Coord(Vector3 _branchPos, Quaternion _branchRot, Circle _circle, float _width, float _length)
        {
            branchPos = _branchPos;
            branchRot = _branchRot;
            circle = _circle;
            width = _width;
            length = _length;
        }
    }

    // Getters & Setters
    public void SetAlphabet(string newAlphabet)
    {
        alphabetToDraw = newAlphabet;
    }
}
