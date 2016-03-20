using UnityEngine;
using System.Collections;

public class ChasingState : State
{
    Transform target;
    GuardController guardController;

    public ChasingState(GuardBehaviour owner):base(owner)
    {
    }

    public override string Description()
    {
        return "Chasing State";
    }

    public override void Enter()
    {
        target = owner.player.transform;
        guardController = owner.GetComponent<GuardController>();
        guardController.seekEnabled = true;
    }

    public override void Exit()
    {
        guardController.seekEnabled = false;
    }

    public override void Update()
    {
        if (owner.seesTarget)
        {
            guardController.targetPosition = target.position;
        }
        else
        {
            owner.SwitchState(new SearchingState(owner, target.position));
        }
    }
}
