using UnityEngine;
using System.Collections;

public class FinalStateMachine : MonoBehaviour {

    static string STATE_CHASING = "Chasing state";

    State state = null;
    Transform player;
    LayerMask walls;
    bool seesTarget = false;

    public float viewRange;
    public float viewAngle;

    void Start () {
	    player = GameObject.FindGameObjectWithTag("Player").transform;
        walls = LayerMask.GetMask("Walls");
        StartCoroutine("LookOut");
    }
	
    public void SwitchState(State _state)
    {
        // Exit the old state and enter a new one
        if (state != null)
        {
            state.Exit();
        }
        state = _state;
        if (state != null)
        {
            _state.Enter();
        }
    }

	void Update () {
        if (state != null)
        {
            state.Update();
        }
	}

    // In order to assure that our guard looks for player no matter in which state it currently is
    // We need the guard to constantly calculate positions at set intervals.
    IEnumerator LookOut()
    {
        while (true)
        {
            seesTarget = false;
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer < viewRange)
            {
                Vector3 toPlayer = (player.position - transform.position).normalized;
                float dot = Mathf.Clamp(Vector3.Dot(toPlayer, transform.forward), -1.0f, 1.0f);
                float angleToPlayer = Mathf.Acos(dot) * Mathf.Rad2Deg;
                if (angleToPlayer < viewAngle / 2)
                {
                    if (!Physics.Raycast(transform.position, toPlayer, distanceToPlayer, walls))
                    {
                        seesTarget = true;
                    }
                }
            }
            // If the player is seen, start chasing the player
            if (seesTarget)
            {
                if (!STATE_CHASING.Equals(state.Description()))
                {
                    SwitchState(new ChasingState(this, player.transform));
                }
            }
            else
            {
                SwitchState(new PatrolState(this));
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
}
