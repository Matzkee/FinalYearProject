using UnityEngine;
using System.Collections;
using System;

public class ReturningToPatrolState : State
{
    GuardController guardController;
    Pathfinding pathfinder;

    public ReturningToPatrolState(GuardBehaviour owner) : base(owner)
    {
    }

    public override string Description()
    {
        return "Returning to Patrol";
    }

    public override void Enter()
    {
        guardController = owner.GetComponent<GuardController>();
        pathfinder = owner.pathfinder;

        guardController.followPath = FindYourWayBack();
        guardController.followingPath = true;
    }

    public override void Exit()
    {
        guardController.followingPath = false;
    }

    public override void Update()
    {
        if (owner.seesTarget)
        {
            owner.SwitchState(new ChasingState(owner));
        }
        else
        {
            if (guardController.followPath.isLast)
            {
                owner.SwitchState(new PatrolState(owner));
            }
        }
    }

    Path FindYourWayBack()
    {
        // Create a new path and use the pathfinder to trace the path & optimize it
        Path returnPath = new Path();
        Vector3 lastWaypoint = guardController.patrollingPath.NextWaypoint();
        pathfinder.FindPath(owner.transform.position, lastWaypoint);
        returnPath.waypoints = pathfinder.OptimizePath(pathfinder.tracePath);

        return returnPath;
    }
}
