using UnityEngine;
using System.Collections;

public class PatrolState : State {
    
    GuardController guardController;
    Pathfinding pathfinder;
    TerrainGenerator tg;

    public PatrolState(GuardBehaviour owner):base(owner)
    {
    }

    public override string Description()
    {
        return "Patrolling State";
    }

    public override void Enter()
    {
        guardController = owner.GetComponent<GuardController>();
        pathfinder = owner.pathfinder;
        tg = owner.tg;

        int patrolArea = Random.Range(0, tg.patrolPoints.Count);
        Path patrolPath = pathfinder.GetBestPossiblePath(owner.transform.position, tg.patrolPoints[patrolArea]);
        guardController.path = patrolPath;

        guardController.followingEnabled = true;
    }

    public override void Exit()
    {
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
