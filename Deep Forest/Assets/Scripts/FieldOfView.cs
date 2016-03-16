using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FieldOfView : MonoBehaviour {

    Transform player;
    LayerMask walls;

    public float viewRange;
    public float viewAngle;
    bool canSee;

	void Start () {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        walls = LayerMask.GetMask("Walls");
	}
	
	void Update () {
        canSee = false;
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
                    Debug.Log("Enemy sees you!");
                    canSee = true;
                }
            }
        }
	}

    void OnDrawGizmos()
    {
        if (canSee)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}
