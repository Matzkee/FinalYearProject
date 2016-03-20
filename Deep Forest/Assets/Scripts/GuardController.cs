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
    public bool seekEnabled = false;
    public Vector3 targetPosition;

    [Header("Following Path")]
    public bool followingEnabled = false;
    public Path path = new Path();

    [Header("Arriving at target")]
    public bool arriveEnabled = false;
    public float slowingDistance = 2.0f;

    void Start () {
        rigidbody = gameObject.AddComponent<Rigidbody>();
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
    }
	
	void Update () {
        force = Vector3.zero;

        if (seekEnabled)
        {
            force += SeekTarget(targetPosition);
        }
        if (followingEnabled)
        {
            force += FollowingPath();
        }
        if (arriveEnabled)
        {
            force += Arrive(targetPosition);
        }

        force = Vector3.ClampMagnitude(force, maxForce);
        acceleration = force / mass;
        velocity += acceleration * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        transform.position += velocity * Time.deltaTime;
        //transform.position += velocity * Time.deltaTime;
        if (velocity.magnitude > float.Epsilon)
        {
            transform.forward = velocity;
        }

        velocity *= (1.0f - damping);
    }

    public Vector3 Arrive(Vector3 target)
    {
        Vector3 toTarget = target - transform.position;
        float distance = toTarget.magnitude;
        if (distance < 0.2f)
        {
            velocity = Vector3.zero;
            return Vector3.zero;
        }
        float rampedSpeed = maxSpeed * (distance / slowingDistance);
        float clampedSpeed = Mathf.Min(rampedSpeed, maxSpeed);
        Vector3 desired = clampedSpeed * (toTarget / distance);

        return desired - velocity;
    }

    public Vector3 FollowingPath()
    {
        float skipDistance = 0.25f;
        float toNextWaypoint = (transform.position - path.NextWaypoint().worldPosition).magnitude;
        if (toNextWaypoint < skipDistance)
        {
            path.AdvanceToNextWaypoint();
        }
        if (path.isLast)
        {
            return Arrive(path.NextWaypoint().worldPosition);
        }
        else if (path.reachedLastWaypoint)
        {
            return Vector3.zero;
        }
        else
        {
            return SeekTarget(path.NextWaypoint().worldPosition);
        }
    }

    public Vector3 SeekTarget(Vector3 target)
    {
        Vector3 toTarget = (target - transform.position).normalized;
        Vector3 desired = toTarget * maxSpeed;

        return desired - velocity;
    }

    void OnDrawGizmos()
    {
        if (path.waypoints != null)
        {
            Gizmos.color = Color.white;
            for (int i = 0; i < path.waypoints.Count; i++)
            {
                Gizmos.DrawCube(path.waypoints[i].worldPosition, Vector3.one * 0.2f);
                Gizmos.DrawLine(
                    path.waypoints[i].worldPosition,
                    path.waypoints[(i + 1) % path.waypoints.Count].worldPosition);
            }
        }
    }
}
