using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FOVAdjuster : MonoBehaviour
{
    [SerializeField] Transform cam;
    [SerializeField] Rigidbody rb;
    [SerializeField] float speed = 0.5f;
    [SerializeField] float distance = 1f;
    bool zoom;
    float elapsedTime;
    float totalTime;
    float t;
    float localDist;

    private void Start()
    {
        zoom = false;
        elapsedTime = 0f;
        totalTime = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameSettings.instance != null && GameSettings.instance.vrFOV)
        {
            Vector3 velocity = rb.velocity;
            velocity.y = 0;

            if (velocity.magnitude > 0.5f && !zoom)
            {
                zoom = true;
                elapsedTime = 0f;
                localDist = cam.localPosition.z;
                float diff = Mathf.Abs(distance);
                totalTime = diff / speed;
            }
            else if (velocity.magnitude < 0.5f && zoom)
            {
                zoom = false;
                elapsedTime = 0f;
                localDist = cam.localPosition.z;
                float diff = Mathf.Abs(localDist);
                totalTime = diff / speed;
            }

            t = Mathf.Clamp(elapsedTime / totalTime, 0f, 1f);
            //float smootht = Mathf.SmoothStep(0f, 1f, t);
            if (zoom)
            {
                //cam.localPosition = distance * transform.GetChild(0).forward;

                cam.localPosition = new Vector3(0f, 0f, MathUlits.Lerp(cam.localPosition.z, distance, speed * Time.deltaTime));
            }
            else
            {
                //cam.localPosition = new Vector3(0f, 0f, 0f);
                cam.localPosition = new Vector3(0f, 0f, MathUlits.Lerp(cam.localPosition.z, 0f, speed * Time.deltaTime));
            }

            elapsedTime += Time.deltaTime;
        }
    }
}
