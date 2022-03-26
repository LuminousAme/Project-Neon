using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemotePlayer : MonoBehaviour
{
    [SerializeField] Rigidbody targetRB;
    Vector3 position, velocity, angularVelocity;
    Quaternion rotation;

    public void SetData(Vector3 pos, Vector3 vel, Quaternion rot, Vector3 angularVel)
    {
        position = pos;
        velocity = vel;
        rotation = rot;
        angularVelocity = angularVel;

        targetRB.position = position;
        targetRB.AddForce(-targetRB.velocity, ForceMode.VelocityChange);
        targetRB.AddForce(velocity, ForceMode.VelocityChange);

        targetRB.rotation = rotation;
        targetRB.AddTorque(-targetRB.angularVelocity, ForceMode.VelocityChange);
        targetRB.AddTorque(angularVelocity, ForceMode.VelocityChange);
    }

    // Start is called before the first frame update
    void Start()
    {
        position = targetRB.position;
        velocity = targetRB.velocity;
        angularVelocity = targetRB.angularVelocity;
        rotation = targetRB.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        //set the velocity to always be the one recieved from the client
        targetRB.AddForce(-targetRB.velocity, ForceMode.VelocityChange);
        targetRB.AddForce(velocity, ForceMode.VelocityChange);

        targetRB.AddTorque(-targetRB.angularVelocity, ForceMode.VelocityChange);
        targetRB.AddTorque(angularVelocity, ForceMode.VelocityChange);
    }
}
