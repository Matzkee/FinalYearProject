using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Path{

    public List<Vector3> waypoints = new List<Vector3>();
    public int next = 0;

    public Vector3 NextWaypoint()
    {
        return waypoints[next];
    }

    public void AdvanceToNextWaypoint()
    {
        next = (next + 1) % waypoints.Count;
    }

    public bool isLast
    {
        get
        {
            return (next == waypoints.Count - 1);
        }
    }
}
