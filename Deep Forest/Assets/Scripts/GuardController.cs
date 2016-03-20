using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GuardController : MonoBehaviour {

    new Rigidbody rigidbody;

    [Header("Guard Options")]
    public Vector3 velocity;
    public Vector3 acceleration;
    public Vector3 force;
    public float mass = 1.0f;
    [Range(0, 1)]
    public float damping = 0.0f;

    public float maxSpeed = 3.0f;
    public float maxForce = 3.0f;

    [Header("Seeking Target")]
    public bool seekPlayerPosition = false;
    public Vector3 targetPosition;

    [Header("Patrolling")]
    public bool patrolling = false;
    public Path patrollingPath = new Path();

    [Header("Following Path")]
    public bool followingPath = false;
    public Path followPath = new Path();

    void Start () {
        rigidbody = gameObject.AddComponent<Rigidbody>();
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
    }
	
	void Update () {
        force = Vector3.zero;

        if (seekPlayerPosition)
        {
            force += SeekTarget(targetPosition);
        }
        if (patrolling)
        {
            force += PatrolPath();
        }
        if (followingPath)
        {
            force += FollowingPath();
        }

        force = Vector3.ClampMagnitude(force, maxForce);
        acceleration = force / mass;
        velocity += acceleration * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        rigidbody.position += velocity * Time.deltaTime;
        //transform.position += velocity * Time.deltaTime;
        if (velocity.magnitude > float.Epsilon)
        {
            transform.forward = velocity;
        }

        velocity *= (1.0f - damping);
    }

    public Vector3 PatrolPath()
    {
        float skipDistance = 0.25f;
        float toNextWaypoint = (transform.position - patrollingPath.NextWaypoint()).magnitude;
        if (toNextWaypoint < skipDistance)
        {
            patrollingPath.AdvanceToNextWaypoint();
        }
        return SeekTarget(patrollingPath.NextWaypoint());
    }

    public Vector3 FollowingPath()
    {
        float skipDistance = 0.25f;
        float toNextWaypoint = (transform.position - followPath.NextWaypoint()).magnitude;
        if (toNextWaypoint < skipDistance)
        {
            followPath.AdvanceToNextWaypoint();
        }
        return SeekTarget(followPath.NextWaypoint());
    }

    public Vector3 SeekTarget(Vector3 target)
    {
        Vector3 toTarget = (target - transform.position).normalized;
        Vector3 desired = toTarget * maxSpeed;

        return desired - velocity;
    }
}
