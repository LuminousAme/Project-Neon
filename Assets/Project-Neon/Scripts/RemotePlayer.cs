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

    public void SetData(Vector3 pos, Vector3 vel, Quaternion rot, Vector3 angularVel, float yaw, float yawSpeed)
    {
        position = pos;
        velocity = vel;
        rotation = rot;
        angularVelocity = angularVel;
        this.yaw = yaw;
        this.yawSpeed = yawSpeed;

        transform.position = position;
        // targetRB.MovePosition(position);
        // targetRB.AddForce(-targetRB.velocity, ForceMode.VelocityChange);
        // targetRB.AddForce(velocity, ForceMode.VelocityChange);

        transform.rotation = rotation;
       // targetRB.MoveRotation(rotation);
       // targetRB.AddTorque(-targetRB.angularVelocity, ForceMode.VelocityChange);
       // targetRB.AddTorque(angularVelocity, ForceMode.VelocityChange);

        lookControl.localPosition = savedPos;
        lookControl.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        lookControl.RotateAround(lookControl.parent.position, lookControl.right, yaw);

        Debug.Log("updated!");
    }

    // Start is called before the first frame update
    void Start()
    {
        if(targetRB != null)
        {
            position = targetRB.position;
            velocity = targetRB.velocity;
            angularVelocity = targetRB.angularVelocity;
            rotation = targetRB.rotation;
            savedPos = lookControl.localPosition;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //set the velocity to always be the one recieved from the client
        //targetRB.AddForce(-targetRB.velocity, ForceMode.VelocityChange);
        //targetRB.AddForce(velocity, ForceMode.VelocityChange);

       // targetRB.AddTorque(-targetRB.angularVelocity, ForceMode.VelocityChange);
       // targetRB.AddTorque(angularVelocity, ForceMode.VelocityChange);

       // yaw += yawSpeed * Time.deltaTime;
       // yaw = Mathf.Clamp(yaw, movementSettings.GetVertMinAngle(), movementSettings.GetVertMaxAngle());
      //  lookControl.localPosition = savedPos;
       // lookControl.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
       // lookControl.RotateAround(lookControl.parent.position, lookControl.right, yaw);
    }
}
