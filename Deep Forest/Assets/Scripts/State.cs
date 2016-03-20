using UnityEngine;
using System.Collections;

public abstract class State
{
    public GuardBehaviour owner;

    public State(GuardBehaviour _owner)
    {
        owner = _owner;
    }

    public abstract string Description();
    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}
