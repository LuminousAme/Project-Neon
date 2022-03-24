using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrappleUI : MonoBehaviour
{
    [SerializeField] private GameObject Grappling;
    [SerializeField] private GameObject notGrappling;
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
        if (player.GetIsGrappling() == true)
        {
            Grappling.SetActive(true);
            notGrappling.SetActive(false);
        }
        else
        {
            Grappling.SetActive(false);
            notGrappling.SetActive(true);
        }
    }
}
