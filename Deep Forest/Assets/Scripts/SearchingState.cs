using UnityEngine;
using System.Collections;
using System;

public class SearchingState : State {

    GuardController guardController;
    Pathfinding pathfinder;
    Vector3 lastSeenPosition;

    public SearchingState(GuardBehaviour owner, Vector3 _lastSeenPosition):base(owner)
    {
        lastSeenPosition = _lastSeenPosition;
    }

    public override string Description()
    {
        return "Searching State";
    }

    public override void Enter()
    {
        guardController = owner.GetComponent<GuardController>();
        pathfinder = owner.pathfinder;
        Path searchPath = pathfinder.GetBestPossiblePath(owner.transform.position, lastSeenPosition);

        guardController.path = searchPath;
        guardController.followingEnabled = true;
    }

    public override void Exit()
    {
        guardController.path = null;
        guardController.followingEnabled = false;
    }

    public override void Update()
    {
        if (owner.seesTarget)
        {
            owner.SwitchState(new ChasingState(owner));
        }

        if (guardController.followingEnabled)
        {
            if (guardController.path.reachedLastWaypoint)
            {
                owner.SwitchState(new LookingAroundState(owner));
            }
        }
    }
}
