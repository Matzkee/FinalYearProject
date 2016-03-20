using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LookingAroundState : State {

    bool nothingFound;
    List<Vector3> directions;
    int iteration = 0;

    public LookingAroundState(GuardBehaviour owner):base(owner)
    {

    }

    public override string Description()
    {
        return "Looking Around";
    }

    public override void Enter()
    {
        directions = new List<Vector3>();
        nothingFound = false;
        for (int i = 0; i < owner.directionsToLook; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere;
            randomDirection.y = owner.transform.position.y;
            directions.Add(randomDirection);
        }
    }

    public override void Exit()
    {
        
    }

    public override void Update()
    {
        if (owner.seesTarget)
        {
            owner.SwitchState(new ChasingState(owner));
        }
        if (nothingFound)
        {
            owner.SwitchState(new PatrolState(owner));
        }
        else
        {
            if (directions != null)
            {
                Quaternion targetRotation = 
                    Quaternion.LookRotation(directions[iteration] - owner.transform.position, Vector3.up);
                float angle = Quaternion.Angle(owner.transform.rotation, targetRotation);
                if (angle > 0.1f)
                {
                    owner.transform.rotation = 
                        Quaternion.Slerp(owner.transform.rotation, targetRotation, Time.deltaTime * owner.rotationSpeed);
                }
                else
                {
                    iteration++;
                    if (iteration == directions.Count)
                    {
                        nothingFound = true;
                    }
                }
            }
        }

    }
}
