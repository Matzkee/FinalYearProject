using UnityEngine;
using System.Collections;
using System;

public class PatrolState : State {

    public PatrolState(FinalStateMachine owner):base(owner)
    {

    }

    public override string Description()
    {
        return "Patrolling State";
    }

    public override void Enter()
    {
        throw new NotImplementedException();
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
