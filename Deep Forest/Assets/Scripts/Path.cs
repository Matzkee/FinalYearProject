using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Path{

    public List<Node> waypoints = new List<Node>();
    public int next = 0;
    public bool reachedLastWaypoint = false;

    public Node NextWaypoint()
    {
        return waypoints[next];
    }

    public void AdvanceToNextWaypoint()
    {
        next = (next + 1);
        if (next > waypoints.Count - 1)
        {
            reachedLastWaypoint = true;
        }
    }

    public bool isLast
    {
        get
        {
            return (next == waypoints.Count - 1);
        }
    }
}
