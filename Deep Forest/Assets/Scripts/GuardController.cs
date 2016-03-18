using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GuardController : MonoBehaviour {

    public Vector3 velocity;
    public Vector3 acceleration;
    public Vector3 force;
    public float mass = 1.0f;
    [Range(0, 1)]
    public float damping = 0.0f;

    public float maxSpeed = 3.0f;
    public float maxForce = 3.0f;

    public bool seekPlayerPosition = false;
    public Vector3 targetPosition;

	void Start () {
	}
	
	void Update () {
        force = Vector3.zero;

        if (seekPlayerPosition)
        {
            force += SeekPlayer(targetPosition);
        }

        force = Vector3.ClampMagnitude(force, maxForce);
        acceleration = force / mass;
        velocity += acceleration * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        transform.position += velocity * Time.deltaTime;
        if (velocity.magnitude > float.Epsilon)
        {
            transform.forward = velocity;
        }

        velocity *= (1.0f - damping);
    }

    public Vector3 SeekPlayer(Vector3 target)
    {
        Vector3 toTarget = (target - transform.position).normalized;
        Vector3 desired = toTarget * maxSpeed;

        return desired - velocity;
    }
}
