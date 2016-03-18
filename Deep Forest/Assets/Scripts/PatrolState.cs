using UnityEngine;
using System.Collections;
using System;

public class PatrolState : State {

    GuardController guardController;

    public PatrolState(FinalStateMachine owner):base(owner)
    {

    }

    public override string Description()
    {
        return "Patrolling State";
    }

    public override void Enter()
    {
        guardController = owner.GetComponent<GuardController>();

    }

    public override void Exit()
    {
        throw new NotImplementedException();
    }

    public override void Update()
    {
        throw new NotImplementedException();
    }
}
