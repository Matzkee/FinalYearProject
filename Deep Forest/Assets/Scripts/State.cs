using UnityEngine;
using System.Collections;

public abstract class State
{
    public FinalStateMachine owner;

    public State(FinalStateMachine _owner)
    {
        owner = _owner;
    }

    public abstract string Description();
    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}
