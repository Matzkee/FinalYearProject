using UnityEngine;
using System.Collections;
using System;

public class ChasingState : State
{
    Transform target;
    GuardController guardController;

    public ChasingState(FinalStateMachine owner, Transform _target):base(owner)
    {
        target = _target;
    }

    public override string Description()
    {
        return "Chasing State";
    }

    public override void Enter()
    {
        guardController = owner.GetComponent<GuardController>();
        guardController.seekPlayerPosition = true;
    }

    public override void Exit()
    {
        guardController.seekPlayerPosition = false;
    }

    public override void Update()
    {
        guardController.targetPosition = target.position;
    }
}
