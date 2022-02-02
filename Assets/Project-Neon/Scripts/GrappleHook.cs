using UnityEngine;

public class GrappleHook : MonoBehaviour
{
    private Camera playerCam;

    private Vector3 hookPos;

    // Start is called before the first frame update
    private void Start()
    {
        playerCam = this.GetComponent<Camera>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out RaycastHit rayHit))
            {
                //hit something
                hookPos = rayHit.point;
            }
        }
    }
}