using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemotePlayer : MonoBehaviour
{
    [SerializeField] Rigidbody targetRB;
    [SerializeField] Transform lookControl;
    Vector3 position, velocity;
    float HoriRot;
    float VertRot;
    [SerializeField] private PlayerMoveSettings movementSettings;
    Vector3 savedPos;
    float timeSinceUpdate = 0f;

    [Header("Lag Mitigation")]
    //position
    [SerializeField] float PosSnapDistance = 5f;
    [SerializeField] float PosAcceptableOffset = 0.1f;
    Vector3 targetPosition, positionOnRec;
    float yPos;
    bool correctionPos = false;
    float PosCorrectTime = 1;

    [Space]
    [SerializeField] float rotAdjustmentSpeed = 5f;
    Quaternion horiRotTarget, vertRotTarget;

    [Header("Animation")]
    //weapon
    [SerializeField] Animator weaponHandAnimator;
    [SerializeField] private LineRenderer grapplingLine;
    [SerializeField] int quality;
    [SerializeField] float strength;
    [SerializeField] float damper;
    [SerializeField] float target;
    [SerializeField] float grappleVelocity;
    float acutalVelocity;
    private float value;
    [SerializeField] float waveCount;
    [SerializeField] float waveHeight;
    [SerializeField] AnimationCurve affectCurve;
    bool isGrappling = false;
    [SerializeField] Transform grappleLatch, grappleLaunch, acutalGraple, grapleRest, grapleParent;
    private Vector3 hookPosition, adjustedHookPosition;
    Quaternion desiredRotForGrapple;

    public void SetData(Vector3 pos, Vector3 vel, float VertRot, float HoriRot)
    {
        //we can just take the velocities and speeds
        velocity = vel;

        //convert to 0-360 angle range
        VertRot.AngleDegreeRange();
        HoriRot.AngleDegreeRange();

        this.VertRot = Mathf.Clamp(VertRot, movementSettings.GetVertMinAngle(), movementSettings.GetVertMaxAngle());
        vertRotTarget = Quaternion.Euler(this.VertRot, 0.0f, 0.0f);

        this.HoriRot = HoriRot;
        horiRotTarget = Quaternion.Euler(0.0f, this.HoriRot, 0.0f);

        //for the acutal values we want to do some smoothly damping though
        NewPositionRecieved(pos);

        Debug.Log("updated!");
        timeSinceUpdate = 0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        position = targetRB.position;
        targetPosition = position;
        positionOnRec = position;
        PosCorrectTime = 0f;
        correctionPos = false;
        velocity = Vector3.zero;
        savedPos = lookControl.localPosition;
        timeSinceUpdate = 0f;

        VertRot = lookControl.localRotation.eulerAngles.x;
        HoriRot = targetRB.rotation.eulerAngles.y;

        isGrappling = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (correctionPos)
        {
            float t = Mathf.Clamp(timeSinceUpdate / PosCorrectTime, 0f, 1f);
            if (t >= 1f) correctionPos = false;
            position = Vector3.Lerp(positionOnRec, targetPosition, t);
            position.y = yPos;
        }

        targetRB.position = position + velocity * timeSinceUpdate;

        //do not dead recokon this it makes it actively worse
        lookControl.localRotation = Quaternion.Slerp(lookControl.localRotation, vertRotTarget, Time.deltaTime * rotAdjustmentSpeed * movementSettings.GetVerticalLookSpeed());
        transform.localRotation = Quaternion.Slerp(transform.localRotation, horiRotTarget, Time.deltaTime * rotAdjustmentSpeed  *  movementSettings.GetHorizontalLookSpeed());

        timeSinceUpdate += Time.deltaTime;

        //do the update for the grappling hook if it is active
        if (isGrappling) GrapplingHookUpdate();
        else
        {
            adjustedHookPosition = grapleRest.position;
            desiredRotForGrapple = Quaternion.LookRotation(transform.forward);
        }

        acutalGraple.position = MathUlits.LerpClamped(acutalGraple.position, adjustedHookPosition, movementSettings.GetGrapplePullSpeed() * 2.0f);
        grapleParent.rotation = Quaternion.Slerp(grapleParent.rotation, desiredRotForGrapple, Time.deltaTime * 5f);
    }

    private void LateUpdate()
    {
        DrawGrapplingHook();
    }

    void NewPositionRecieved(Vector3 newPos)
    {
        correctionPos = false;
        positionOnRec = targetRB.position;
        positionOnRec.y = 0;
        targetPosition = newPos;
        targetPosition.y = 0;
        yPos = newPos.y;

        float dist = Vector3.Distance(positionOnRec, targetPosition);

        //if it's too far just snap it
        if (dist >= PosSnapDistance)
        {
            position = targetPosition;
            position.y = yPos;

        }
        //if it's close enough don't bother
        else if (dist <= PosAcceptableOffset)
        {
            position = positionOnRec;
            position.y = yPos;
        }
        //otherwise try to smoothly damp it
        else
        {
            correctionPos = true;
            PosCorrectTime = dist / movementSettings.GetBaseMaxSpeed();
            position = positionOnRec;
            position.y = yPos;
        }

        targetRB.position = position;
    }

    public void BeginQuickAttack()
    {
        weaponHandAnimator.SetTrigger("Quick Attack");
    }

    public void BeginRaiseHeavyAttack()
    {
        weaponHandAnimator.SetTrigger("BeginHeavy");
    }

    public void BeginHeavyDown()
    {
        weaponHandAnimator.SetTrigger("EndHeavy");
    }

    //will have to implement this in a bit
    public void SetGrappleStatus(bool status, Vector3 target)
    {
        isGrappling = status;

        if(grapplingLine != null)
        {
            if(isGrappling) grapplingLine.positionCount = quality + 1;
            else grapplingLine.positionCount = 0;
        }

        hookPosition = target;
        adjustedHookPosition = hookPosition - grappleLaunch.localPosition;
        value = 0;
        acutalVelocity = grappleVelocity;
    }

    void GrapplingHookUpdate()
    {
        desiredRotForGrapple = Quaternion.LookRotation(acutalGraple.position - grapleRest.position);
    }

    void DrawGrapplingHook()
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
        }
    }
}
