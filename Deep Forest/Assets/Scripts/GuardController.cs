﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GuardController : MonoBehaviour {

    new Rigidbody rigidbody;

    [Header("Guard Options")]
    Vector3 velocity;
    Vector3 acceleration;
    Vector3 force;
    public float mass = 1.0f;
    [Range(0, 1)]
    public float damping = 0.0f;
    public float maxSpeed = 3.0f;
    public float maxForce = 3.0f;
    public float slowingDistance = 5.0f;
    public float lookingRotationSpeed = 1.5f;

    [HideInInspector]
    public bool seekEnabled = false, followingEnabled = false, lookingEnabled = false;
    [HideInInspector]
    public int iteration;
    [HideInInspector]
    public Vector3 targetPosition;
    public Path path = null;
    public List<Quaternion> lookingDirections = null;
    
    

    void Start () {
        rigidbody = gameObject.AddComponent<Rigidbody>();
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        ResetAll();
    }

    public void ResetAll()
    {
        seekEnabled = followingEnabled = lookingEnabled = false;
        path = null;
        lookingDirections = null;
    }
	
	void Update () {
        force = Vector3.zero;

        if (seekEnabled)
        {
            force += SeekTarget(targetPosition);
        }
        if (followingEnabled && path != null)
        {
            force += FollowingPath();
        }
        if (lookingEnabled && lookingDirections != null)
        {
            LookAround();
        }

        force = Vector3.ClampMagnitude(force, maxForce);
        acceleration = force / mass;
        velocity += acceleration * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        transform.position += velocity * Time.deltaTime;
        //transform.position += velocity * Time.deltaTime;
        if (velocity.magnitude > float.Epsilon)
        {
            if (velocity != Vector3.zero)
            {
                transform.forward = velocity;
            }
        }

        velocity *= (1.0f - damping);
    }

    public Vector3 Arrive(Vector3 target)
    {
        Vector3 toTarget = target - transform.position;

        float distance = toTarget.magnitude;
        if (distance < 0.3f)
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
        if (path.reachedLastWaypoint)
        {
            return Vector3.zero;
        }
        float skipDistance = 0.4f;
        float toNextWaypoint = (transform.position - path.NextWaypoint().worldPosition).magnitude;
        if (toNextWaypoint < skipDistance)
        {
            path.AdvanceToNextWaypoint();
        }
        if (path.isLast)
        {
            return Arrive(path.NextWaypoint().worldPosition);
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

    public void LookAround()
    {
        Quaternion targetRotation = lookingDirections[iteration];
        float angle = Quaternion.Angle(transform.rotation, targetRotation);
        if (angle > 0.1f)
        {
            transform.rotation =
                Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * lookingRotationSpeed);
        }
        else
        {
            iteration++;
            if (iteration == lookingDirections.Count)
            {
                lookingEnabled = false;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (path != null && path.waypoints != null)
        {
            Gizmos.color = Color.white;
            for (int i = 0; i < path.waypoints.Count - 1; i++)
            {
                Gizmos.DrawCube(path.waypoints[i].worldPosition, Vector3.one * 0.2f);
                Gizmos.DrawLine(
                    path.waypoints[i].worldPosition,
                    path.waypoints[i + 1].worldPosition);
                Gizmos.DrawCube(path.waypoints[i + 1].worldPosition, Vector3.one * 0.2f);
            }
        }
    }
}
