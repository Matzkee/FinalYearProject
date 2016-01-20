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
public class Turtle{

    float length;
    float angleX, angleY;
    string alphabetToDraw;
    int treeRoundness;
    float treeWidth;

    List<Segment> branches;
    List<Circle> circles;
    Stack<Coord> coordStack;
    // We need position information from the attached gameObject
    Transform treeTransform;
    // Assign a tree bark material
    public Material material;

    public Turtle(float _radius, int _detail, string a, float _length, float _angleX, float _angleZ, 
        GameObject _currentTree)
    {
        treeRoundness = _detail;
        treeWidth = _radius;

        treeTransform = _currentTree.transform;
        branches = new List<Segment>();
        coordStack = new Stack<Coord>();

        alphabetToDraw = a;
        length = _length;
        angleX = _angleX;
        angleY = _angleZ;
        
    }

    public void DrawPlant()
    {
        Vector3 lastPosition;

        Quaternion lastRotation;
        Circle lastCircle;
        // Make a new branches list
        branches = new List<Segment>();
        circles = new List<Circle>();
        int repeats = 0;

        alphabetToDraw += "x";

        lastPosition = treeTransform.position;
        lastRotation = treeTransform.rotation;
        lastCircle = CreateCircleAt(treeTransform, treeWidth, treeRoundness);
        circles.Add(lastCircle);
        // Follow the action depending on current character in alphabet
        for (int i = 0; i < alphabetToDraw.Length; i++)
        {
            char c = alphabetToDraw[i];
            if (c == 'F')
            {
                treeTransform.Translate(Vector3.forward * length);
                if (alphabetToDraw[i + 1].Equals('F'))
                {
                    // Keep going until another character is found
                    repeats++;
                }
                else
                {
                    // Make a new Circle
                    Circle newCircle = CreateCircleAt(treeTransform, treeWidth, treeRoundness);
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
            // Rotate along X axis
            else if (c == '+')
            {
                treeTransform.Rotate(Vector3.right * angleX);
            }
            else if (c == '-')
            {
                treeTransform.Rotate(Vector3.right * -angleX);
            }
            // Save current position
            else if (c == '[')
            {
                Coord currentCoord = new Coord(treeTransform.position, treeTransform.rotation, 
                    CreateCircleAt(treeTransform, treeWidth, treeRoundness));
                coordStack.Push(currentCoord);
            }
            else if (c == ']')
            {
                // Set the last branches colour to green
                branches[branches.Count - 1].color = Color.green;
                Coord lastCord = coordStack.Pop();
                treeTransform.position = lastCord.branchPos;
                treeTransform.rotation = lastCord.branchRot;

                lastCircle = lastCord.circle;
                lastPosition = lastCord.branchPos;
                lastRotation = lastCord.branchRot;
            }

            //Rotate along Y axis
            else if (c == 'z')
            {
                treeTransform.Rotate(Vector3.up * angleY);
            }
            else if(c == 'a'){
                treeTransform.Rotate(Vector3.up * -angleY);
            }
        }
    }

    public Circle CreateCircleAt(Transform _centre, float _radius, int _numpoints)
    {
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

        public Coord(Vector3 _branchPos, Quaternion _branchRot, Circle _circle)
        {
            branchPos = _branchPos;
            branchRot = _branchRot;
            circle = _circle;
        }
    }

    // Getters & Setters
    public void SetLength(float newLength)
    {
        length = newLength;
    }
    public void ChangeLength(float changeRatio)
    {
        length *= changeRatio;
    }
    public void ChangeWidth(float widthRatio)
    {
        treeWidth *= widthRatio;
    }
    public void SetAngles(float newAngleX, float newAngleZ)
    {
        angleX = newAngleX;
        angleY = newAngleZ;
    }
    public void SetAlphabet(string newAlphabet)
    {
        alphabetToDraw = newAlphabet;
    }
    public List<Segment> GetBranches()
    {
        return branches;
    }
    public List<Circle> GetCircles()
    {
        return circles;
    }
}
