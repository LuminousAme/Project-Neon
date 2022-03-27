using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemotePlayer : MonoBehaviour
{
    [SerializeField] Rigidbody targetRB;
    [SerializeField] Transform lookControl;
    Vector3 position, velocity, angularVelocity;
    Quaternion rotation;
    float yaw, yawSpeed;
    [SerializeField] private PlayerMoveSettings movementSettings;
    Vector3 savedPos;
    float timeSinceUpdate = 0f;

    public void SetData(Vector3 pos, Vector3 vel, Quaternion rot, Vector3 angularVel, float yaw, float yawSpeed)
    {
        position = pos;
        velocity = vel;
        rotation = rot;
        angularVelocity = angularVel;
        this.yaw = yaw;
        this.yawSpeed = yawSpeed;


        if(targetRB != null)
        {
            targetRB.MovePosition(position);
            targetRB.MoveRotation(rotation);
        }
        else
        {
            transform.position = position;
            transform.rotation = rotation;
        }

        lookControl.localPosition = savedPos;
        lookControl.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        lookControl.RotateAround(lookControl.parent.position, lookControl.right, yaw);

        Debug.Log("updated!");
        timeSinceUpdate = 0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        if(targetRB != null)
        {
            position = targetRB.position;
            velocity = targetRB.velocity;
            angularVelocity = targetRB.angularVelocity;
        }
        else
        {
            position = transform.position;
            velocity = Vector3.zero;
            angularVelocity = Vector3.zero;
        }

        rotation = targetRB.rotation;
        savedPos = lookControl.localPosition;
        timeSinceUpdate = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if(targetRB != null)
        {
            //set the velocity to always be the one recieved from the client
            targetRB.MovePosition(position + velocity * timeSinceUpdate);

            //will have to figure out rotation latter
            //targetRB.AddTorque(-targetRB.angularVelocity, ForceMode.VelocityChange);
            //targetRB.AddTorque(angularVelocity, ForceMode.VelocityChange);
        }


        yaw += yawSpeed * Time.deltaTime;
        yaw = Mathf.Clamp(yaw, movementSettings.GetVertMinAngle(), movementSettings.GetVertMaxAngle());
        lookControl.localPosition = savedPos;
        lookControl.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        lookControl.RotateAround(lookControl.parent.position, lookControl.right, yaw);

        timeSinceUpdate += Time.deltaTime;
    }
}
