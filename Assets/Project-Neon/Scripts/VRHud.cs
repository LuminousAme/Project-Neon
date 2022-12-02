using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;

public class VRHud : MonoBehaviour
{
    Transform origin;
    [SerializeField] Transform child;
    [SerializeField] float acceptableRange = 15f;
    [SerializeField] Vector3 offset = new Vector3(0f, 2f, 0f);

    Vector3 desiredForward;
    Vector3 desiredRight;
    Vector3 desiredUp;
    float timeOutOfRange;

    [SerializeField] float acceptableTime = 1.5f;
    [SerializeField] float speed = 2f;
    [SerializeField] float distance = 2f;


    // Start is called before the first frame update
    void Start()
    {
        origin = Camera.main.transform;
        desiredForward = origin.forward;
        timeOutOfRange = 0f;

        if(GameSettings.instance == null || !GameSettings.instance.vrFOV)
        {
            Vector3 localPos = child.localPosition + (distance * desiredForward);
            child.SetParent(origin);
            child.localPosition = localPos;
        }
    }

    private void Update()
    {
        if (GameSettings.instance != null && GameSettings.instance.vrFOV)
        {
            //follow the origin around with position only
            transform.position = origin.parent.position + offset;


            float forangle = Vector3.Angle(desiredForward, origin.forward);
            float rigangle = Vector3.Angle(desiredRight, origin.right);
            float upangle = Vector3.Angle(desiredUp, origin.up);
            if (forangle < acceptableRange && rigangle < acceptableRange && upangle < acceptableRange)
            {
                timeOutOfRange = 0f;
            }
            else
            {
                timeOutOfRange += Time.deltaTime;
            }

            if (timeOutOfRange > acceptableTime)
            {
                desiredForward = origin.forward;
                desiredRight = origin.right;
                desiredUp = origin.up;
            }

            Quaternion adjustRot = Quaternion.FromToRotation(transform.forward, desiredForward) *
                Quaternion.FromToRotation(transform.right, desiredRight) *
                Quaternion.FromToRotation(transform.up, desiredUp) *
                transform.rotation;

            transform.rotation = Quaternion.Slerp(transform.rotation, adjustRot, speed * Time.deltaTime);


            child.position = transform.position + (distance * transform.forward);
        }
    }

}
