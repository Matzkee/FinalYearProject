using UnityEngine;

/*
    This class is used to store information between 2 points in
    an L-System. It is mainly used to create branches
    */
public class BranchSegment
{
    public Vector3 start;
    public Vector3 end;
    public Color color;
    public Circle startCircle;
    public Circle endCircle;
    public Quaternion orientation;

    // Constructor
    public BranchSegment(Vector3 _start, Vector3 _end, Circle _startCircle, Circle _endCircle, Quaternion _orientation)
    {
        start = _start;
        end = _end;
        color = Color.grey;
        startCircle = _startCircle;
        endCircle = _endCircle;
        orientation = _orientation;
    }
    
}
