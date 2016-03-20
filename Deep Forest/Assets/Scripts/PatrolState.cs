using UnityEngine;
using System.Collections;
using System;

public class PatrolState : State {
    
    GuardController guardController;

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
        guardController.patrolling = true;
    }

    public override void Exit()
    {
        guardController.patrolling = false;
    }

    public override void Update()
    {
        if (owner.seesTarget)
        {
            owner.SwitchState(new ChasingState(owner));
        }
    }
}
