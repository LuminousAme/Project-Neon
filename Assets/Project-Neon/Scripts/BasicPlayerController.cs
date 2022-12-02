using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

//Basic character movement script based on the character controller from Very Very Valet
//https://youtu.be/qdskE8PJy6Q
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
    [SerializeField] private Transform horizontalLookRot;
    private Vector2 lookInput;
    private Vector3 eulerAngles;

    //dashes
    private float timeRemainingInDash = 0.0f;

    private float activeDashCooldown = 0f;
    private uint numberOfDashesTaken = 0;

    private bool isGrappling = false, isRidingMommentum = false;
    private float timeSinceLastGrappleEnd = 0.0f;
    private Vector3 hookPosition, adjustedHookPosition;
    private SpringJoint grapplingHookJoint;
    private Vector3 grapplingMomentum;
    [SerializeField] private LineRenderer grapplingLine;
    [SerializeField] private Transform grappleLatch, grappleLaunch, acutalGraple, grapleRest, grapleParent;
    [SerializeField] private Transform grappleAim;
    [SerializeField] private bool useJoint = true;
    private Quaternion desiredRotForGrapple;

    [Header("Grappling Line Animation")]
    //based on this video https://youtu.be/8nENcDnxeVE
    [SerializeField] private int quality;

    [SerializeField] private float strength;
    [SerializeField] private float damper;
    [SerializeField] private float target;
    [SerializeField] private float velocity;
    private float acutalVelocity;
    private float value;
    [SerializeField] private float waveCount;
    [SerializeField] private float waveHeight;
    [SerializeField] private AnimationCurve affectCurve;

    [Header("Combat Controls")]
    private float timeSinceAttackDown = 0f;

    private bool attackDown = false;
    [SerializeField] private float attackCooldownTime = 1f;
    [SerializeField] private float beginHeavyAttackTime = 0.5f;
    private float timeSinceAttackRelease = 0f;
    [SerializeField] private QuickAttack quickAttack;
    [SerializeField] private HeavyAttack heavyAttack;

    [Header("Animation")]
    [SerializeField] private CharacterAnimation animController;

    [Header("Sound")]
    [SerializeField] private SoundEffect dashSFX, GrappleLaunchSFX, GrappleReelSFX;

    [SerializeField] private AudioSource grappleReelSource;

    public static Action OnGrapple;
    public static Action OnDoubleJump;
    public static Action OnDash;

    [SerializeField] Transform mainCamera;

    private void Awake()
    {
        //create the player controls asset, and enable the default player controls
        InputManager.RestartControls();
        controls = InputManager.controls;
        InputManager.LoadAllBindingOverrides();
    }

    private void OnEnable()
    {
        controls.Enable();

        //assign the jump function to the started event of the jump action
        controls.Player.Jump.started += ctx => Jump();
        //assign the startdash function to the started event of the dash action
        controls.Player.Dash.started += ctx => StartDash();
        //assign the handlegrapple function to the started event of the grapple action
        controls.Player.Grapple.started += ctx => HandleGrapplePressed();
        controls.Player.Grapple.canceled += ctx => HandleGrappleRelease();

        //assign the attack donw function to the start event of the attack action
        controls.Player.Attack.started += ctx => PressedDownAttack();
        //assigned the attack release function to the finish event of the attack action
        controls.Player.Attack.canceled += ctx => ReleasedAttack();

        ChatManager.onStartType += HandleTypingStart;
        ChatManager.onStopType += HandleTpyingEnd;
    }

    private void OnDisable()
    {
        controls.Disable();
        ChatManager.onStartType -= HandleTypingStart;
        ChatManager.onStopType -= HandleTpyingEnd;
    }

    private void HandleTypingStart() => controls.Disable();

    private void HandleTpyingEnd() => controls.Enable();

    // Start is called before the first frame update
    private void Start()
    {
        rb = transform.GetChild(0).GetComponent<Rigidbody>();
        targetRotation = transform.rotation;

        airJumpsTaken = 0;
        grounded = true;
        coyoteTimer = 0.0f;

        currentGravity = Vector3.down * movementSettings.GetGravityGoingDown();

        eulerAngles = lookAtTarget.localRotation.eulerAngles;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isGrappling = false;
        grapplingMomentum = Vector3.zero;

        timeSinceAttackDown = 0f;
        timeSinceAttackRelease = 0f;
        attackDown = false;

        activeDashCooldown = 0f;
    }

    // Update is called once per frame
    private void Update()
    {
        //grab the input direction from the controls and update the current input direction
        Vector2 inputVec2 = controls.Player.Move.ReadValue<Vector2>();
        inputDirection = horizontalLookRot.forward * inputVec2.y + horizontalLookRot.right * inputVec2.x;
        inputDirection.Normalize();

        if (inputDirection.magnitude > 0f) animController.SetMoving(true);
        else animController.SetMoving(false);

        if (grounded) animController.SetOnGround(true);
        else animController.SetOnGround(false);

        if (!grounded && currentGravity.y == movementSettings.GetGravityGoingUp()) animController.SetIsJumping(true);
        else animController.SetIsJumping(false);

        if (!grounded && currentGravity.y == movementSettings.GetGravityGoingDown()) animController.SetIsFalling(true);
        else animController.SetIsFalling(false);

        //update the rotation for the camera
        lookInput = controls.Player.Look.ReadValue<Vector2>();
        //lookInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        //reduce the time and cooldown remaining for dashing
        if (timeRemainingInDash > 0.0) timeRemainingInDash -= Time.deltaTime;
        activeDashCooldown = Mathf.Max(activeDashCooldown - Time.deltaTime, 0f);

        //do the update for the grappling hook if it is active
        if (isGrappling) GrapplingHookUpdate();
        else
        {
            adjustedHookPosition = grapleRest.position;
            desiredRotForGrapple = Quaternion.LookRotation(Camera.main.transform.forward);
        }

        acutalGraple.position = MathUlits.LerpClamped(acutalGraple.position, adjustedHookPosition, movementSettings.GetGrapplePullSpeed() * 2.0f);
        //grapleParent.rotation = Quaternion.Slerp(grapleParent.rotation, desiredRotForGrapple, Time.deltaTime * 5f);

        //reduce the cooldown remaing for the grappling hook
        if (timeSinceLastGrappleEnd > 0.0f && !isGrappling) timeSinceLastGrappleEnd -= Time.deltaTime;

        if (attackDown)
        {
            if (!heavyAttack.GetAttackActive() && timeSinceAttackDown >= beginHeavyAttackTime)
            {
                heavyAttack.BeginAttack();
            }
            timeSinceAttackDown += Time.deltaTime;
        }
        if (timeSinceAttackRelease <= attackCooldownTime) timeSinceAttackRelease += Time.deltaTime;

        //rotate the player towards their target rotation using the
        FixedRotatePlayer();
    }

    //Late update is called after every gameobject has had their update called
    private void LateUpdate()
    {
        DrawGrapplingHook();
    }

    private void FixedUpdate()
    {
        //calculate gravity
        FixedCalculateGravity();

        //keep the capsule floating, applying any approriate forces related to that, and applying gravity if not needed
        FixedRaiseCapsule();

        //apply the 2 dimensional (forward, and side to side) basic character motion
        if (!isGrappling && !isRidingMommentum) FixedCharacterMove();

        //apply any mommentum from the grappling hook
        FixedGrapplingHookPull();
    }

    private void FixedRotatePlayer()
    {
        //handle rotation from player input
        /*
        float vertRot = -lookInput.y * movementSettings.GetVerticalLookSpeed();
        eulerAngles.y += vertRot * Time.deltaTime;
        eulerAngles.y = Mathf.Clamp(eulerAngles.y, movementSettings.GetVertMinAngle(), movementSettings.GetVertMaxAngle());
        lookAtTarget.localPosition = new Vector3(0.0f, 0.0f, 1.0f);
        lookAtTarget.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        lookAtTarget.RotateAround(lookAtTarget.parent.position, lookAtTarget.right, eulerAngles.y);*/

        //update the rotation for the rigidbody
        /*
        float horiRot = lookInput.x * movementSettings.GetHorizontalLookSpeed();
        eulerAngles.x += horiRot * Time.deltaTime;
        horizontalLookRot.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        horizontalLookRot.RotateAround(horizontalLookRot.position, horizontalLookRot.up, eulerAngles.x);*/

        Vector3 rawforward = mainCamera.forward;
        Vector3 forward = new Vector3(rawforward.x, 0f, rawforward.z).normalized;

        horizontalLookRot.LookAt(horizontalLookRot.position + forward, Vector3.up);
        mainCamera.LookAt(mainCamera.position + rawforward);

        eulerAngles = mainCamera.rotation.eulerAngles;

        /*
        Quaternion yaw = Quaternion.AngleAxis(horiRot * Time.deltaTime, transform.up);
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
        if (lookInput.x != 0.0f)
        {
            Vector3 torque = (axis * (rotR * movementSettings.GetRotationSpringDamp()) - (rb.angularVelocity));
            rb.AddTorque(torque);
        }
        else
        {
            rb.AddTorque(-rb.angularVelocity, ForceMode.VelocityChange);
            targetRotation = transform.rotation;
        }*/

        if (LocalPlayer.instance != null) LocalPlayer.instance.UpdateRotData(eulerAngles.y, eulerAngles.x);
    }

    private void FixedRaiseCapsule()
    {
        //do a raycast down
        RaycastHit rayHit;
        Vector3 rayDir = Vector3.down;

        //if it hit something calculate the force that should be applied as a result
        if (Physics.Raycast(rb.position, rayDir, out rayHit, 1f * movementSettings.GetRideHeight(), movementSettings.GetWalkableMask()))
        {
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

            if (!isGrappling)
            {
                //apply drag onto the grappling hook momentum
                float currentGrapplingMag = grapplingMomentum.magnitude;
                if (currentGrapplingMag <= movementSettings.GetGrappleDrag())
                {
                    grapplingMomentum = Vector3.zero;
                    isRidingMommentum = false;
                }
                else grapplingMomentum -= grapplingMomentum.normalized * movementSettings.GetGrappleDrag();
            }

            //uncomment if we want the player to be able to apply force to object below them
            /*
            //and if it's collided with another rigidbody, apply to it the same force in the opposite direction at the point of collision
            if (rayHit.rigidbody != null)
            {
                rayHit.rigidbody.AddForceAtPosition(rayDir * -springForce, rayHit.point);
            }*/
        }
        //if the player is not on the ground, apply the force of gravity to them
        else if (!isGrappling)
        {
            rb.AddForce(currentGravity, ForceMode.Acceleration);
            grounded = false;
            coyoteTimer += Time.fixedDeltaTime;
        }

        if (Physics.Raycast(rb.position, rayDir, out rayHit, 2.5f * movementSettings.GetRideHeight(), movementSettings.GetWalkableMask()))
        {
            grounded = true;
            airJumpsTaken = 0;
            coyoteTimer = 0.0f;
        }
    }

    private void FixedCharacterMove()
    {
        //figure out if the player's dash speed should be
        float dashSpeed = (timeRemainingInDash > 0.0) ? movementSettings.GetDashSpeed() : 0.0f;

        //calculate the ideal velocity for the character this frame
        Vector3 desiredVelocity = inputDirection * (movementSettings.GetBaseMaxSpeed() + dashSpeed);

        //calculate what the velocity should be, adjusted for the fact it needs to be faster if the player is moving away from the direction they were in the last physics update
        float moveDirectionDot = Vector3.Dot(targetVelocity.normalized, desiredVelocity.normalized);
        float ReMappedAccelFromDot = MathUlits.SmoothStepFromValue(1.0f, 2.0f, MathUlits.ReMapClamped(-1.0f, 0.0f, 2.0f, 1.0f, moveDirectionDot, 1.0f, 2.0f));
        float accel = movementSettings.GetBaseAcceleration() * ReMappedAccelFromDot;

        //claculate the acutal new target velocity, based on the acceleration
        targetVelocity = Vector3.MoveTowards(targetVelocity, desiredVelocity, movementSettings.GetBaseAcceleration() * Time.fixedDeltaTime);

        //figure out how much force it would take to get to that velocity
        Vector3 existingVelocity = new Vector3(rb.velocity.x, 0.0f, rb.velocity.z);
        Vector3 forceRequired = (targetVelocity - existingVelocity) / Time.fixedDeltaTime;
        //clamp the magnitude of the force to the maximum
        float maxForce = movementSettings.GetBaseMaxAccelForce() * ReMappedAccelFromDot;
        forceRequired = Vector3.ClampMagnitude(forceRequired, maxForce);

        //apply that force to the rigidbody
        rb.AddForce(forceRequired * rb.mass);
    }

    private void FixedCalculateGravity()
    {
        //set gravity based on player velocity - will see how this works with the floating capsule, we'll have to see
        if (rb.velocity.y >= 0) currentGravity = Vector3.up * movementSettings.GetGravityGoingUp();
        else if (rb.velocity.y < 0) currentGravity = Vector3.up * movementSettings.GetGravityGoingDown();
    }

    //grappling hook mechanics based on this video https://www.youtube.com/watch?v=Xgh4v1w5DxU
    private void TryStartGrappling()
    {
        //check if we even hit something we can grapple too
        RaycastHit rayHit;
        if (Physics.Raycast(grappleAim.position, grappleAim.forward, out rayHit,
            movementSettings.GetMaxGrappleRange(), movementSettings.GetGrappleableMask()))
        {
            //if we did set the point we hit to the anchor point
            hookPosition = rayHit.point;
            adjustedHookPosition = hookPosition - grappleLaunch.localPosition;
            value = 0;
            acutalVelocity = velocity;
            if (AsyncClient.instance != null) AsyncClient.instance.SendGrappleStatus(true, hookPosition);

            //code that handles setting up the joint and stuff, uncomment later
            {
                if (useJoint)
                {
                    //and set up a spring joint to connect the player to that point
                    grapplingHookJoint = this.gameObject.AddComponent<SpringJoint>();
                    grapplingHookJoint.autoConfigureConnectedAnchor = false;
                    grapplingHookJoint.connectedAnchor = hookPosition;
                    //spring force used to keep the 2 objects together (higher means faster hook in, lower means slower)
                    grapplingHookJoint.spring = movementSettings.GetGrappleJointSpring();
                    //damper force used to dampen the spring force. (lower means faster hook, higher means slower)
                    grapplingHookJoint.damper = movementSettings.GetGrappleJointDamp();
                    //scale to apply to the inverse mass and inertia tensor of the body (seems to affect momentum)
                    grapplingHookJoint.massScale = movementSettings.GetGrappleJointMassScale();

                    //setting the starting distances to grapple between
                    float distanceFromHookPoint = Vector3.Distance(rb.position, hookPosition);
                    grapplingHookJoint.maxDistance = distanceFromHookPoint * 0.8f;
                    grapplingHookJoint.minDistance = distanceFromHookPoint * 0.25f;
                    //grapplingHookJoint.minDistance = movementSettings.GetGrappleCloseDistance();
                }

                //if a line renderer for the grappling line exists then set it to have 2 points
                if (grapplingLine != null) grapplingLine.positionCount = quality + 1;
            }

            //finally mark the player as actively grappling
            isGrappling = true;
            OnGrapple?.Invoke();
            rb.AddForce(-rb.velocity, ForceMode.VelocityChange);

            //play the sound effect
            AudioSource newAS = null;
            if (GrappleLaunchSFX != null) newAS = GrappleLaunchSFX.Play();

            if (newAS != null && GrappleReelSFX != null && grappleReelSource != null)
            {
                float time = newAS.clip.length / newAS.pitch;
                StartCoroutine(StartGrappleReelSFX(time));
            }
        }
    }

    private IEnumerator StartGrappleReelSFX(float startDelay)
    {
        yield return new WaitForSeconds(startDelay);
        GrappleReelSFX.Play(grappleReelSource);
    }

    private void StopGrappling()
    {
        if (grapplingLine != null) grapplingLine.positionCount = 0;
        if (grapplingHookJoint != null) Destroy(grapplingHookJoint);
        isGrappling = false;
        timeSinceLastGrappleEnd = movementSettings.GetGrappleCooldown();
        if (AsyncClient.instance != null) AsyncClient.instance.SendGrappleStatus(false, hookPosition);
        if (grappleReelSource != null) grappleReelSource.Stop();
        //rb.AddForce(-rb.velocity, ForceMode.VelocityChange);
    }

    private void GrapplingHookUpdate()
    {
        //if (!grapplingHookJoint) return;

        //reduce the current maximum distance so the grappling hook pulls the user towards the hook point
        //float currentMaxDist = grapplingHookJoint.maxDistance;
        //grapplingHookJoint.maxDistance = currentMaxDist - movementSettings.GetGrapplePullSpeed() * Time.deltaTime;
        desiredRotForGrapple = Quaternion.LookRotation(acutalGraple.position - grapleRest.position);

        //adjust the area that you can swing in
        float distanceFromHookPoint = Vector3.Distance(rb.position, hookPosition);
        if (grapplingHookJoint != null)
        {
            grapplingHookJoint.maxDistance = distanceFromHookPoint * 0.8f;
            grapplingHookJoint.minDistance = distanceFromHookPoint * 0.25f;
        }

        //if the distance between the player and the hooked point is less than the minimum, stop grappling
        //if (distanceFromHookPoint <= movementSettings.GetGrappleCloseDistance()) StopGrappling(); //playtester didn't really like this
    }

    private void FixedGrapplingHookPull()
    {
        if (isGrappling)
        {
            Vector3 GrappleDir = hookPosition - rb.position;
            grapplingMomentum = GrappleDir.normalized * movementSettings.GetGrapplePullSpeed() * Time.fixedDeltaTime;
            isRidingMommentum = true;
        }
        else isRidingMommentum = false;

        if (isRidingMommentum)
        {
            rb.AddForce(-rb.velocity, ForceMode.VelocityChange);
            rb.AddForce(grapplingMomentum, ForceMode.VelocityChange);
            Vector3 startPoint = grappleLaunch.position;
            Vector3 endPoint = grappleLatch.position;
            //stop the reel sound effect when grappling finishes
            if (grappleReelSource != null && Vector3.Distance(startPoint, endPoint) <= 0.5f) grappleReelSource.Stop();

            if (useJoint)
            {
                Vector3 inputMove = inputDirection.normalized * movementSettings.GetGrappleHoriInputForce();
                rb.AddForce(inputMove, ForceMode.VelocityChange);
            }
        }
    }

    private void DrawGrapplingHook()
    {
        if (grapplingLine != null && grapplingLine.positionCount > 0)
        {
            // based on this video https://youtu.be/8nENcDnxeVE
            Vector3 startPoint = grappleLaunch.position;
            Vector3 endPoint = grappleLatch.position;
            Vector3 up = Quaternion.LookRotation(endPoint - startPoint).normalized * Vector3.up;

            float dir = target - value >= 0 ? 1f : -1f;
            float force = Mathf.Abs(target - value) * strength;
            acutalVelocity += (force * dir - acutalVelocity * damper) * Time.deltaTime;
            value += acutalVelocity * Time.deltaTime;

            for (int i = 0; i < quality + 1; i++)
            {
                float delta = (float)i / (float)quality;
                Vector3 offset = up * waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI * value * affectCurve.Evaluate(delta));

                grapplingLine.SetPosition(i, Vector3.Lerp(startPoint, endPoint, delta) + offset);
            }

            //grapplingLine.SetPosition(0, grappleLaunch.position);
            //grapplingLine.SetPosition(1, grappleLatch.position);
        }
    }

    private void Jump()
    {
        //if the number of jumps the user has taken is less than the maximum, do a jump
        if (isGrappling || grounded || coyoteTimer <= movementSettings.GetCoyoteTime() || airJumpsTaken < movementSettings.GetAirJumps())
        {
            //add the force - The - rb.velocity.y term here negates any existing y velocity when jumping mid air, making that feel better
            rb.AddForce(Vector3.up * (movementSettings.GetJumpInitialVerticalVelo() - rb.velocity.y), ForceMode.VelocityChange);
            //and if not on the ground, increase the number of air jumps taken
            if (!(grounded && coyoteTimer <= movementSettings.GetCoyoteTime()) || !isGrappling)
            {
                airJumpsTaken++;
                OnDoubleJump?.Invoke();
            }
        }

        if (isGrappling) StopGrappling();
    }

    private void StartDash()
    {
        if (activeDashCooldown < movementSettings.GetDashCooldown() && timeRemainingInDash <= 0.0f) //timeSinceLastDash <= 0.0)
        {
            activeDashCooldown += movementSettings.GetDashCooldown();
            //numberOfDashesTaken++;
            timeRemainingInDash = movementSettings.GetDashLenght();
            OnDash?.Invoke();
            //play the dash sound, we'll just play it locally as though it were in 2D space
            if (dashSFX != null) dashSFX.Play();
        }
    }

    private void HandleGrapplePressed()
    {
        if (isGrappling && GameSettings.instance.toogleGrapple)
        {
            //end grappling
            StopGrappling();
        }
        else if (timeSinceLastGrappleEnd <= 0.0f)
        {
            //try to start a grapple
            TryStartGrappling();
        }
    }

    private void HandleGrappleRelease()
    {
        if (isGrappling && !GameSettings.instance.toogleGrapple)
        {
            StopGrappling();
        }
    }

    //getter
    public bool GetIsGrappling()
    {
        return isGrappling;
    }

    public bool GetGrappleOnCooldown()
    {
        if (isGrappling) return false;
        else if (timeSinceLastGrappleEnd > 0.0f) return true;
        return false;
    }

    public float GetDashTotalCooldown() => activeDashCooldown;

    public uint GetNumOfDashesTaken()
    {
        return numberOfDashesTaken;
    }

    public bool GetAttackDown()
    {
        return attackDown;
    }

    public float GetHeavyAttackTime() => beginHeavyAttackTime;
    public float GetAttackDownTime() => timeSinceAttackDown;

    public void PressedDownAttack()
    {
        if (timeSinceAttackRelease > attackCooldownTime)
        {
            attackDown = true;
            timeSinceAttackDown = 0f;
        }
    }

    public void ReleasedAttack()
    {
        if (attackDown)
        {
            attackDown = false;
            timeSinceAttackRelease = 0f;
            if (!heavyAttack.GetAttackActive()) quickAttack.BeginAttack();
            else heavyAttack.ReleaseAttack();
        }
    }

    public void VRLightAttack()
    {
        quickAttack.BeginAttack();
    }

    public void VRHeavyAttack()
    {
        heavyAttack.BeginAttack();
        StartCoroutine(VRFinishHeavyAttack());
    }

    IEnumerator VRFinishHeavyAttack()
    {
        yield return new WaitForSeconds(0.25f);
        heavyAttack.ReleaseAttack();
    }

    private void OnDrawGizmosSelected()
    {
        if (movementSettings != null)
        {
            Gizmos.color = Color.red;
            //Gizmos.DrawRay(rb.position, Vector3.down * movementSettings.GetRideHeight());
            Gizmos.color = Color.white;
        }
    }

    public void EndMatch()
    {
        controls.Player.Disable();
    }

    public Vector3 GetCamEulerAngles() => eulerAngles;

    public void setControlsState(bool on)
    {
        if (on)
        {
            controls.Enable();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            controls.Disable();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}