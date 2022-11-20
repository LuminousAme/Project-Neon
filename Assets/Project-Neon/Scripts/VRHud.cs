using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;

public class VRHud : MonoBehaviour
{
    Transform origin;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 localPos = transform.localPosition;
        origin = FindObjectOfType<XROrigin>().transform;
        transform.SetParent(origin);
        transform.localPosition = localPos;
    }

}
