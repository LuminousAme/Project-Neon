using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayer : MonoBehaviour
{
    [SerializeField] Rigidbody targetRB;
    [SerializeField] float updateServerTime = 0.05f;
    float elapsedTime = 0f;
    Vector3 position, velocity, angularVelocity;
    Vector3 lastVelocity;
    Quaternion rotation;
    float yaw, yawSpeed;

    //non-persistant singleton
    public static LocalPlayer instance = null;

    public void UpdateData()
    {
        position = targetRB.position;
        velocity = targetRB.velocity;
        velocity.RemoveTinyValues(0.01f);
        angularVelocity = targetRB.angularVelocity;
        rotation = targetRB.rotation;
    }

    public void UpdateCamData(float yaw, float yawSpeed)
    {
        this.yaw = yaw;
        this.yawSpeed = yawSpeed;
    }

    private void Start()
    {
        position = targetRB.position;
        velocity = targetRB.velocity;
        angularVelocity = targetRB.angularVelocity;
        rotation = targetRB.rotation;
        if (instance == null) instance = this;
        else Destroy(this.gameObject);
    }

    private void Update()
    {
        UpdateData();
        if (Client.instance != null)
        {
            elapsedTime += Time.deltaTime;
            if(elapsedTime >= updateServerTime && velocity != lastVelocity)
            {
                lastVelocity = velocity;
                Client.instance.SendPosRotUpdate(position, velocity, rotation, angularVelocity, yaw, yawSpeed);
                elapsedTime = 0.0f;
            }
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
