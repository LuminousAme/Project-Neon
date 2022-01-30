using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Basic character movement script based on the character controller from Very Very Valet 
//https://youtu.be/qdskE8PJy6Q
[RequireComponent(typeof(Rigidbody))]
public class BasicPlayerController : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Floating Capsule Controls")]
    [SerializeField] private float rideHeight = 0.5f;
    [SerializeField] private float rideSpringStr = 1f;
    [SerializeField] private float rideSpringDamp = 1f;

    [Space]
    [Header("Rotation Correction Controls")]
    private Quaternion targetRotation;
    [SerializeField] private float rotationSpringDamp = 1f;

    [Space]
    [Header("Running Controls")]
    [SerializeField] private float baseMaxSpeed = 1f;
    [SerializeField] private float baseAcceleration = 1f;
    [SerializeField] private float baseMaxAccelForce = 1f;
    private Vector3 inputDirection;


    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        targetRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        //keep the capsule floating, applying any approriate forces related to that
        FixedRaiseCapsule();

        //rotate the player towards their target rotation using the
        FixedRotatePlayer();
    }

    void FixedRotatePlayer()
    {
        //get the target rotation such that it tries to orient the player to stay upright 
        targetRotation = Quaternion.FromToRotation(transform.up, Vector3.up) * targetRotation;

        //find the rotation needed to move the player's current rotation to the target rotation
        Quaternion deltaRot = targetRotation * Quaternion.Inverse(transform.rotation);

        //get the axis angle represensetation of the rotation
        Vector3 axis;
        float rotD;
        deltaRot.ToAngleAxis(out rotD, out axis);
        axis.Normalize();

        //convert the angle to radians
        float rotR = rotD * Mathf.Deg2Rad;

        //calculate the torque needed to move it
        Vector3 torque = (axis * (rotR * rotationSpringDamp) - (rb.velocity));
        rb.AddTorque(torque);
    }

    void FixedRaiseCapsule()
    {
        //do a raycast down
        RaycastHit rayHit;
        Vector3 rayDir = Vector3.down;

        //if it hit something calculate the force that should be applied as a result
        if(Physics.Raycast(transform.position, rayDir, out rayHit, rideHeight * 2.0f))
        {
            float speedAlongRayDir = Vector3.Dot(rayDir, rb.velocity); //the speed that the player is moving along the ray's direction
            float otherVelAlongRayDir = 0.0f; //the speed that the object the ray has collided with is moving along the ray's direction, it is zero if it didn't hit another rigidbody
            if(rayHit.rigidbody != null)
            {
                //but if did hit a rigidbody we need to calculate it's value
                otherVelAlongRayDir = Vector3.Dot(rayDir, rayHit.rigidbody.velocity);
            }

            //calculate how much force needs to be used to keep the player out of the ground
            float relativeSpeed = speedAlongRayDir - otherVelAlongRayDir;
            float x = rayHit.distance - rideHeight;
            float springForce = (x * rideSpringStr) - (relativeSpeed * rideSpringDamp);

            //apply that force to the player
            rb.AddForce(rayDir * springForce);

            //and if it's collided with another rigidbody, apply to it the same force in the opposite direction at the point of collision
            if(rayHit.rigidbody != null)
            {
                rayHit.rigidbody.AddForceAtPosition(rayDir * -springForce, rayHit.point);
            }
        }
    }
}
