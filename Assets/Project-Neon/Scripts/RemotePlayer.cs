using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemotePlayer : MonoBehaviour
{
    [SerializeField] Rigidbody targetRB;
    [SerializeField] Transform lookControl;
    Vector3 position, velocity;
    float HoriRot, HoriRotSpeed;
    float VertRot, VertRotSpeed;
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

    //VertRot
    [SerializeField] float VertRotSnapAmount = 90f, VertRotAcceptableOffSet = 5f;
    float targetVertRot, VertRotOnRec;
    bool correctionVertRot = false;
    float VertRotCorrectTime = 1f;

    //HoriRot
    [SerializeField] float HoriRotSnapAmount = 90f, HoriRotAcceptableOffSet = 5f;
    float targetHoriRot, HoriRotOnRec;
    bool correctionHoriRot = false;
    float HoriRotCorrectTime = 1f;

    [Space]
    [SerializeField] float rotAdjustmentSpeed = 5f;
    float horiVel = 0f;
    float vertVel = 0f;

    Quaternion horiRotTarget, vertRotTarget;

    public void SetData(Vector3 pos, Vector3 vel, float VertRot, float VertRotSpeed, float HoriRot, float HoriRotSpeed)
    {
        //we can just take the velocities and speeds
        velocity = vel;
        this.VertRotSpeed = VertRotSpeed;
        this.HoriRotSpeed = HoriRotSpeed;

        //convert to 0-360 angle range
        VertRot.AngleDegreeRange();
        HoriRot.AngleDegreeRange();

        //for the acutal values we want to do some smoothly damping though
        NewPositionRecieved(pos);
        NewVertRotHandle(VertRot);
        NewHoriRotHandle(HoriRot);

        {
            /*
            //position = pos; //will be set seperately
            targetPosition = pos;
            velocity = vel;
            this.VertRot = VertRot;
            this.VertRotSpeed = VertRotSpeed;
            this.HoriRot = HoriRot;
            this.HoriRotSpeed = HoriRotSpeed;
            correcting = false;


            if (targetRB != null)
            {
                realPositionOnRec = targetRB.position;

                float dist = Vector3.Distance(realPositionOnRec, targetPosition);

                //if the distance is too large or too small to care just snap too it, otherwise we will try to smoothly correc
                if (dist >= snapDistance || dist <= acceptableOffset)
                {
                    realPositionOnRec = targetPosition;
                    position = targetPosition;
                    targetRB.MovePosition(position);
                }
                else
                {
                    correcting = true;
                    position = realPositionOnRec;
                    timeToCorrect = dist / movementSettings.GetBaseMaxSpeed(); //t = d/v
                }
            }
            else
            {
                realPositionOnRec = transform.position;

                //if the distance is too large just snap too it, otherwise we will try to smoothly correc
                if (Vector3.Distance(realPositionOnRec, targetPosition) >= snapDistance)
                {
                    realPositionOnRec = targetPosition;
                    position = targetPosition;
                    transform.position = position;
                }
                else
                {
                    position = realPositionOnRec;
                }
            }

            transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            transform.Rotate(transform.up, this.HoriRot);

            lookControl.localPosition = savedPos;
            lookControl.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            lookControl.RotateAround(lookControl.parent.position, lookControl.right, this.VertRot);*/
        }

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
        VertRotSpeed = 0f;
        HoriRotSpeed = 0f;

        horiVel = 0f;
        vertVel = 0f;
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

        if (correctionVertRot)
        {
            Debug.Log("Correcting VertRot!");
            float t = Mathf.Clamp(timeSinceUpdate / VertRotCorrectTime, 0f, 1f);
            if (t >= 1f) correctionVertRot = false;
            VertRot = Mathf.Lerp(VertRotOnRec, targetVertRot, t);
        }

        if (correctionHoriRot)
        {
            Debug.Log("Correcting HoriRot!");
            float t = Mathf.Clamp(timeSinceUpdate / HoriRotCorrectTime, 0f, 1f);
            if (t >= 1f) correctionHoriRot = false;
            HoriRot = Mathf.Lerp(HoriRotOnRec, targetHoriRot, t);
        }

        targetRB.position = position + velocity * timeSinceUpdate;


        //do not dead recokon this it makes it actively worse
        lookControl.localRotation = Quaternion.Slerp(lookControl.localRotation, vertRotTarget, Time.deltaTime * rotAdjustmentSpeed * movementSettings.GetVerticalLookSpeed());
        transform.localRotation = Quaternion.Slerp(transform.localRotation, horiRotTarget, Time.deltaTime * rotAdjustmentSpeed  *  movementSettings.GetHorizontalLookSpeed());

        timeSinceUpdate += Time.deltaTime;
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

    void NewVertRotHandle(float newVertRot)
    {
        /*
        correctionVertRot = false;
        VertRotOnRec = VertRot;
        VertRotOnRec.AngleDegreeRange();
        targetVertRot = newVertRot;
        targetVertRot.AngleDegreeRange();

        float delta = Mathf.Abs(Mathf.Atan2(Mathf.Sin(targetVertRot - VertRotOnRec), Mathf.Cos(targetVertRot - VertRotOnRec)));

        if(delta >= VertRotSnapAmount)
        {
            VertRot = targetVertRot;
        }
        else if (delta <= VertRotAcceptableOffSet)
        {
            VertRot = VertRotOnRec;
        }

        else
        {
            correctionVertRot = true;
            VertRotCorrectTime = delta / movementSettings.GetVerticalLookSpeed();
            VertRot = VertRotOnRec;
        }*/

        VertRot = Mathf.Clamp(newVertRot, movementSettings.GetVertMinAngle(), movementSettings.GetVertMaxAngle());
        vertRotTarget = Quaternion.Euler(VertRot, 0.0f, 0.0f);
        //lookControl.localPosition = savedPos;
        //lookControl.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        //lookControl.RotateAround(lookControl.parent.position, lookControl.right, VertRot);
    }

    void NewHoriRotHandle(float newHoriRot)
    {
        /*
        correctionHoriRot = false;
        HoriRotOnRec = HoriRot;
        HoriRotOnRec.AngleDegreeRange();
        targetHoriRot = newHoriRot;
        targetHoriRot.AngleDegreeRange();

        float delta = Mathf.Abs(Mathf.Atan2(Mathf.Sin(targetHoriRot - HoriRotOnRec), Mathf.Cos(targetHoriRot - HoriRotOnRec)));

        
        if (delta >= HoriRotSnapAmount)
        {
            HoriRot = targetHoriRot;

        }
        else if (delta <= HoriRotAcceptableOffSet)
        {
            HoriRot = HoriRotOnRec;
        }
        else
        {
            correctionHoriRot = true;
            HoriRotCorrectTime = delta / movementSettings.GetHorizontalLookSpeed();
            HoriRot = HoriRotOnRec;
        }*/

        //correctionHoriRot = false;
        HoriRot = newHoriRot;
        horiRotTarget = Quaternion.Euler(0.0f, HoriRot, 0.0f);
        //transform.RotateAround(transform.position, transform.up, HoriRot);
       // Debug.Log("HoriRot: " + HoriRot + ", acutal HoriRot send:" + newHoriRot);
    }
}
