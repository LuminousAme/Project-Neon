using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayer : MonoBehaviour
{
    [SerializeField] Rigidbody targetRB;
    [SerializeField] float updateServerTime = 0.05f;
    float elapsedTime = 0f;
    Vector3 position, velocity, angularVelocity;
    Quaternion rotation;

    public void UpdateData()
    {
        position = targetRB.position;
        velocity = targetRB.velocity;
        angularVelocity = targetRB.angularVelocity;
        rotation = targetRB.rotation;
    }

    private void Start()
    {
        position = targetRB.position;
        velocity = targetRB.velocity;
        angularVelocity = targetRB.angularVelocity;
        rotation = targetRB.rotation;
    }

    private void Update()
    {
        UpdateData();

        if (Client.instance != null)
        {
            elapsedTime += Time.deltaTime;
            if(elapsedTime >= updateServerTime)
            {
                Client.instance.SendPosRotUpdate(position, velocity, rotation, angularVelocity);
            }
        }
    }
}
