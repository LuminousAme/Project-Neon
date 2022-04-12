using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanRotate : MonoBehaviour
{
    [SerializeField] float speed = 360;

    void Update()
    {
        transform.RotateAround(transform.position, Vector3.up, speed * Time.deltaTime);
    }
}
