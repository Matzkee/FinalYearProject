using UnityEngine;
using System.Collections.Generic;

// Class for storing x amount of points along a circle
public class Circle
{
    public List<Vector3> circlePoints;

    public Circle(List<Vector3> _points)
    {
        circlePoints = _points;
    }
}