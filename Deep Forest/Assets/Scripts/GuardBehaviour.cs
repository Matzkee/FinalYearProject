using UnityEngine;
using System.Collections;

public class GuardBehaviour : MonoBehaviour {
    
    State state = null;
    LayerMask walls;
    [HideInInspector]
    public TerrainGenerator tg;
    public Pathfinding pathfinder;
    [HideInInspector]
    public Transform player;
    [HideInInspector]
    public bool seesTarget = false;

    public float viewRange;
    public float viewAngle;

    void Start () {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        walls = LayerMask.GetMask("Walls");
        tg = GameObject.FindGameObjectWithTag("TerrainGenerator").GetComponent<TerrainGenerator>();

        // Start warming up
        StartCoroutine("WarmUp");
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
    // The states then decide when to switch depending if the guard sees the target
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
            yield return new WaitForSeconds(0.5f);
        }
    }

    // To get to actual map generator script and generated waypoints without interfering with current
    // processing, we have to wait for it to load and give it time to prepare
    IEnumerator WarmUp()
    {
        while (tg == null || tg.worldGrid == null)
        {
            yield return new WaitForSeconds(0.5f);
        }
        // Make a new pathfnder with the grid form terrain generator
        pathfinder = new Pathfinding(tg.worldGrid);
        // Start Patrolling the area
        SwitchState(new PatrolState(this));
        // Start looking out for player endlesly
        StartCoroutine("LookOut");
    }
}
