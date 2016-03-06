using UnityEngine;

public class BranchTip{

    public Vector3 start;
    public Vector3 end;
    public Color color;
    public Circle startCircle;

    public BranchTip(Vector3 _start, Vector3 _end, Circle _startCircle)
    {
        start = _start;
        end = _end;
        startCircle = _startCircle;
    }
}
