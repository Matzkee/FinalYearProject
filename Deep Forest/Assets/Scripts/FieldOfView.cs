using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FieldOfView : MonoBehaviour {

    public Transform player;

    public float viewRange;
    public float viewAngle;
    bool canSee;

    string message;
    Text text;

	void Start () {
        text = GameObject.FindGameObjectWithTag("UI").GetComponentInChildren<Text>();
	}
	
	void Update () {
        canSee = false;
        message = ("Out of range!");
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer < viewRange)
        {
            Vector3 toPlayer = (player.position - transform.position).normalized;
            float dot = Mathf.Clamp(Vector3.Dot(toPlayer, transform.forward), -1.0f, 1.0f);
            float angleToPlayer = Mathf.Acos(dot) * Mathf.Rad2Deg;
            message = ("Player in range! Angle to player: " + Mathf.RoundToInt(angleToPlayer));
            if (angleToPlayer < viewAngle)
            {
                message = ("Player in view!");
                canSee = true;
            }
        }
	}

    void LateUpdate()
    {
        text.text = message;
    }
}
