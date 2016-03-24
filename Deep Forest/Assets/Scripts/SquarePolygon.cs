using UnityEngine;

public struct SquarePolygon{
    public Vector3 tLeft;
    public Vector3 bLeft;
    public Vector3 tRight;
    public Vector3 bRight;

    public SquarePolygon(Vector3 topLeft, Vector3 bottomLeft, Vector3 topRight, Vector3 bottomRight)
    {
        tLeft = topLeft;
        bLeft = bottomLeft;
        tRight = topRight;
        bRight = bottomRight;
    }
}
