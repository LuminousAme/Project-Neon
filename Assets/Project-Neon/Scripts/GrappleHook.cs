using UnityEngine;

public class GrappleHook : MonoBehaviour
{
    public Camera playerCam;

    private Vector3 hookPos;
    private SpringJoint joint;

    public Transform player;

    private LineRenderer hook;

    // Start is called before the first frame update
    private void Start()
    {
        //   playerCam = this.GetComponent<Camera>();
        hook = GetComponent<LineRenderer>();
        hook.SetPosition(0, player.transform.position);
    }

    // Update is called once per frame
    private void Update()
    {
        DrawGrapple();

        StartGrapple();

        GrappleEnd();
    }

    private void StartGrapple()
    {
        //help form: https://www.youtube.com/watch?v=Xgh4v1w5DxU
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out RaycastHit rayHit))
            {
                //hit something
                hookPos = rayHit.point;
                joint = player.gameObject.AddComponent<SpringJoint>();
                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = hookPos;
                Debug.Log("HIT");
                //float distFromPoint = Vector3.Distance(player.position, hookPos);

                //transform.position

                joint.spring = 4.5f;
                joint.damper = 7f;
                joint.massScale = 5.5f;

                hook.positionCount = 2;
            }
        }
    }

    private void GrappleEnd()
    {
        float distFromPoint = Vector3.Distance(player.position, hookPos);
        if (distFromPoint <= 0.01)
        {
            // hookPos = null;
            EndGrapple();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            EndGrapple();
        }
    }

    private void DrawGrapple()
    {
        if (!joint) return;

        hook.SetPosition(0, player.position);
        hook.SetPosition(1, hookPos);
    }

    private void EndGrapple()
    {
        hook.positionCount = 0;
        Destroy(joint);
        Debug.Log("NED  grappling");
    }
}