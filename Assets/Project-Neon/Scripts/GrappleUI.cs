using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrappleUI : MonoBehaviour
{
    [SerializeField] private GameObject Grappling;
    [SerializeField] private GameObject notGrappling;
    [SerializeField] private GameObject cooldownObj;
    private BasicPlayerController player;

    public void SetPlayer(BasicPlayerController p) => player = p;

    // Start is called before the first frame update
    void Start()
    {
        //start of game player is not grappling
        Grappling.SetActive(false);
        notGrappling.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        //if player is grappling
        if (player.GetIsGrappling())
        {
            Grappling.SetActive(true);
            notGrappling.SetActive(false);
            cooldownObj.SetActive(false);
        }
        else if (player.GetGrappleOnCooldown())
        {
            Grappling.SetActive(false);
            notGrappling.SetActive(false);
            cooldownObj.SetActive(true);
        }
        else
        {
            Grappling.SetActive(false);
            notGrappling.SetActive(true);
            cooldownObj.SetActive(false);
        }
    }
}
