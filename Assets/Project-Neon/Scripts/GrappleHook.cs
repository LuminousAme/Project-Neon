using UnityEngine;

public class GrappleHook : MonoBehaviour
{
    public Camera playerCam;

    private Vector3 hookPos;
    private SpringJoint joint;

    public Transform player;

    private LineRenderer hook;

    //bool for if hook has been deployed
    private bool Grappling;

    //delay for hooking again
    private float hookDelay;

    //cooldown for grapple hook
    private float hookCD;

    // Start is called before the first frame update
    private void Start()
    {
        //   playerCam = this.GetComponent<Camera>();
        hook = GetComponent<LineRenderer>(); //placeholder visual
        hook.SetPosition(0, player.transform.position);
        Grappling = false;
        hookDelay = 0.25f;
        hookCD = 0f;
    }

    // Update is called once per frame
    private void Update()
    {
        //incremeent cooldown timer
        hookCD = hookCD - Time.deltaTime;
        if (hookCD < 0f)
        {
            hookCD = 0f;
        }

        DrawGrapple();
        //press key to grapple
        if (Input.GetKeyDown(KeyCode.E) && !Grappling && (hookCD <= 0f))
        {
            Debug.Log("HOOKING");

            StartGrapple();
            //hookCD = hookDelay;
        }
        //press again to cnacel
        else if (Input.GetKeyDown(KeyCode.E) && Grappling)
        {
            Debug.Log("NOT HOOKING");

            EndGrapple();
            hookCD = hookDelay;
        }

        //if the player has reached the grapple hook
        GrappleOver();
    }

    private void StartGrapple()
    {
        //help form: https://www.youtube.com/watch?v=Xgh4v1w5DxU
        //if (Input.GetKeyDown(KeyCode.E) && !Grappling)
        //{
        if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out RaycastHit rayHit))
        {
            //hit something
            hookPos = rayHit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = hookPos;
            // Debug.Log("HIT");
            //float distFromPoint = Vector3.Distance(player.position, hookPos);

            //transform.position
            //joint parameter stuff
            joint.spring = 4.5f;
            joint.damper = 7f;
            joint.massScale = 5.5f;

            hook.positionCount = 2;
            //the player is grappling
            Grappling = true;
        }
        // }
    }

    //grapple hook reaches it's destination
    private void GrappleOver()
    {
        float distFromPoint = Vector3.Distance(player.position, hookPos);
        //when it reaches the end
        if (distFromPoint <= 0.1)
        {
            // hookPos = null;
            EndGrapple();
        }

        //if (Input.GetKeyDown(KeyCode.E))
        //{
        //    EndGrapple();
        //}
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
        Grappling = false;
        Debug.Log("NED  grappling");
    }
}