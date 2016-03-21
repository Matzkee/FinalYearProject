using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LookingAroundState : State {
    
    GuardController guardController;

    public LookingAroundState(GuardBehaviour owner):base(owner)
    {

    }

    public override string Description()
    {
        return "Looking Around";
    }

    public override void Enter()
    {
        guardController = owner.GetComponent<GuardController>();
        guardController.iteration = 0;
        List<Quaternion> directions = new List<Quaternion>();
        for (int i = 0; i < owner.directionsToLook; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere + owner.transform.position;
            randomDirection.y = owner.transform.position.y;

            Quaternion targetRotation =
                    Quaternion.LookRotation(randomDirection - owner.transform.position, Vector3.up);

            directions.Add(targetRotation);
        }

        guardController.lookingDirections = directions;
        guardController.lookingEnabled = true;
    }

    public override void Exit()
    {
        guardController.lookingEnabled = false;
    }

    public override void Update()
    {
        if (owner.seesTarget)
        {
            owner.SwitchState(new ChasingState(owner));
        }
        if (!guardController.lookingEnabled)
        {
            owner.SwitchState(new PatrolState(owner));
        }

    }
}
