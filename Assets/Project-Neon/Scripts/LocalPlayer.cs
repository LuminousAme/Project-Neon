using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayer : MonoBehaviour
{
    [SerializeField] Rigidbody targetRB;
    [SerializeField] float updateServerTime = 0.05f;
    float elapsedTime = 0f;
    Vector3 position, velocity;
    float yaw;
    float pitch;

    //non-persistant singleton
    public static LocalPlayer instance = null;

    public void UpdateData()
    {
        position = targetRB.position;
        velocity = targetRB.velocity;
        velocity.RemoveTinyValues(0.01f);
    }

    //I think I might have yaw and pitch inverted but I'll figure that out later
    public void UpdateRotData(float yaw) 
    {
        this.yaw = yaw;
        pitch = transform.rotation.eulerAngles.y;
    }

    private void Start()
    {
        position = targetRB.position;
        velocity = targetRB.velocity;
        if (instance == null) instance = this;
        else Destroy(this.gameObject);
    }

    private void Update()
    {
        UpdateData();
        if (Client.instance != null)
        {
            elapsedTime += Time.deltaTime;
            if(elapsedTime >= updateServerTime)
            {
                Client.instance.SendPosRotUpdate(position, velocity, yaw, pitch);
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
