using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Basic character movement script based on the character controller from Very Very Valet 
//https://youtu.be/qdskE8PJy6Q
[RequireComponent(typeof(Rigidbody))]
public class BasicPlayerController : MonoBehaviour
{
    private Rigidbody rb;

    private Quaternion targetRotation;

    private Vector3 targetVelocity = Vector3.zero;
    private Vector3 inputDirection;
    private PlayerControls controls;

    private float coyoteTimer;
    private uint airJumpsTaken;
    private bool grounded;
    private Vector3 currentGravity;

    [SerializeField] private PlayerMoveSettings movementSettings;
    [SerializeField] private Transform lookAtTarget;
    private Vector2 lookInput;
    private Vector3 eulerAngles;

    private void Awake()
    {
        //create the player controls asset, and enable the default player controls
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Enable();

        //assign the jump function to the performed event of the jump action
        controls.Player.Jump.started += ctx => Jump();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        targetRotation = transform.rotation;

        airJumpsTaken = 0;
        grounded = false;
        coyoteTimer = 0.0f;

        currentGravity = Vector3.down * movementSettings.GetGravityGoingDown();

        eulerAngles = lookAtTarget.localRotation.eulerAngles;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        //grab the input direction from the controls and update the current input direction
        Vector2 inputVec2 = controls.Player.Move.ReadValue<Vector2>();
        inputDirection = transform.forward * inputVec2.y + transform.right * inputVec2.x;
        inputDirection.Normalize();

        //update the rotation for the camera
        lookInput = controls.Player.Look.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        //calculate gravity
        FixedCalculateGravity();

        //keep the capsule floating, applying any approriate forces related to that, and applying gravity if not needed
        FixedRaiseCapsule();

        //apply the 2 dimensional (forward, and side to side) basic character motion
        FixedCharacterMove();

        //rotate the player towards their target rotation using the
        FixedRotatePlayer();
    }

    void FixedRotatePlayer()
    {
        //handle rotation from player input
        eulerAngles.y -= lookInput.y * movementSettings.GetVerticalLookSpeed() * Time.deltaTime;
        eulerAngles.y = Mathf.Clamp(eulerAngles.y, movementSettings.GetVertMinAngle(), movementSettings.GetVertMaxAngle());
        lookAtTarget.localPosition = new Vector3(0.0f, 0.0f, 1.0f);
        lookAtTarget.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        lookAtTarget.RotateAround(lookAtTarget.parent.position, lookAtTarget.right, eulerAngles.y);

        //update the rotation for the rigidbody
        float horiRot = lookInput.x * movementSettings.GetHorizontalLookSpeed() * Time.deltaTime;
        Quaternion yaw = Quaternion.AngleAxis(horiRot, transform.up);
        targetRotation = yaw * targetRotation;

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
        if(lookInput.x != 0.0f)
        {
            Vector3 torque = (axis * (rotR * movementSettings.GetRotationSpringDamp()) - (rb.angularVelocity));
            rb.AddTorque(torque);
        }
        else
        {
            rb.AddTorque(-rb.angularVelocity, ForceMode.VelocityChange);
            targetRotation = transform.rotation;
        }
    }

    void FixedRaiseCapsule()
    {
        //do a raycast down
        RaycastHit rayHit;
        Vector3 rayDir = Vector3.down;

        Debug.Log(movementSettings.GetRideHeight());
        //if it hit something calculate the force that should be applied as a result
        if (Physics.Raycast(transform.position, rayDir, out rayHit, movementSettings.GetRideHeight(), movementSettings.GetWalkableMask()))
        {
            grounded = true;
            airJumpsTaken = 0;
            coyoteTimer = 0.0f;

            float speedAlongRayDir = Vector3.Dot(rayDir, rb.velocity); //the speed that the player is moving along the ray's direction
            float otherVelAlongRayDir = 0.0f; //the speed that the object the ray has collided with is moving along the ray's direction, it is zero if it didn't hit another rigidbody
            if (rayHit.rigidbody != null)
            {
                //but if did hit a rigidbody we need to calculate it's value
                otherVelAlongRayDir = Vector3.Dot(rayDir, rayHit.rigidbody.velocity);
            }

            //calculate how much force needs to be used to keep the player out of the ground
            float relativeSpeed = speedAlongRayDir - otherVelAlongRayDir;
            float x = rayHit.distance - movementSettings.GetRideHeight();
            float springForce = (x * movementSettings.GetSpringStr()) - (relativeSpeed * movementSettings.GetSpringDamp());

            //apply that force to the player
            rb.AddForce(rayDir * springForce);

            //uncomment if we want the player to be able to apply force to object below them
            /*
            //and if it's collided with another rigidbody, apply to it the same force in the opposite direction at the point of collision
            if (rayHit.rigidbody != null)
            {
                rayHit.rigidbody.AddForceAtPosition(rayDir * -springForce, rayHit.point);
            }*/
        }
        //if the player is not on the ground, apply the force of gravity to them
        else
        {
            rb.AddForce(currentGravity, ForceMode.Acceleration);
            grounded = false;
            coyoteTimer += Time.fixedDeltaTime;
        }
    }

    void FixedCharacterMove()
    {
        //calculate the ideal velocity for the character this frame
        Vector3 desiredVelocity = inputDirection * movementSettings.GetBaseMaxSpeed();

        //calculate what the velocity should be, adjusted for the fact it needs to be faster if the player is moving away from the direction they were in the last physics update
        float moveDirectionDot = Vector3.Dot(targetVelocity.normalized, desiredVelocity.normalized);
        float ReMappedAccelFromDot = MathUlits.SmoothStepFromValue(1.0f, 2.0f, MathUlits.ReMapClamped(-1.0f, 0.0f, 2.0f, 1.0f, moveDirectionDot, 1.0f, 2.0f));
        float accel = movementSettings.GetBaseAcceleration() * ReMappedAccelFromDot;

        //claculate the acutal new target velocity, based on the acceleration
        targetVelocity = Vector3.MoveTowards(targetVelocity, desiredVelocity, movementSettings.GetBaseAcceleration() * Time.fixedDeltaTime);

        //figure out how much force it would take to get to that velocity
        Vector3 forceRequired = (targetVelocity - new Vector3(rb.velocity.x, 0.0f, rb.velocity.z)) / Time.fixedDeltaTime;
        //clamp the magnitude of the force to the maximum
        float maxForce = movementSettings.GetBaseMaxAccelForce() * ReMappedAccelFromDot;
        forceRequired = Vector3.ClampMagnitude(forceRequired, maxForce);

        //apply that force to the rigidbody
        rb.AddForce(forceRequired * rb.mass);
    }

    void FixedCalculateGravity()
    {
        //set gravity based on player velocity - will see how this works with the floating capsule, we'll have to see
        if (rb.velocity.y >= 0) currentGravity = Vector3.up * movementSettings.GetGravityGoingUp();
        else if (rb.velocity.y < 0) currentGravity = Vector3.up * movementSettings.GetGravityGoingDown();
    }

    private void Jump()
    {
        //if the number of jumps the user has taken is less than the maximum, do a jump
        if(grounded || coyoteTimer <= movementSettings.GetCoyoteTime() || airJumpsTaken < movementSettings.GetAirJumps())
        {
            //add the force - The - rb.velocity.y term here negates any existing y velocity when jumping mid air, making that feel better
            rb.AddForce(Vector3.up * (movementSettings.GetJumpInitialVerticalVelo() - rb.velocity.y), ForceMode.VelocityChange); 
            //and if not on the ground, increase the number of air jumps taken
            if(!(grounded && coyoteTimer <= movementSettings.GetCoyoteTime())) airJumpsTaken++;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if(movementSettings != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, Vector3.down * movementSettings.GetRideHeight());
            Gizmos.color = Color.white;
        }
    }

    public Vector3 GetCamEulerAngles() => eulerAngles;
}
