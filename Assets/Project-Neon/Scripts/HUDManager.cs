using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class HUDManager : MonoBehaviour
{
    PlayerState player;
    [SerializeField] TMP_Text scoreText;
    int fakeScore;
    [SerializeField] float scoreUpdateRate = 5f;
    [SerializeField] HealthBar healthBar;
    [SerializeField] GrappleUI GrappleUI;

    private void OnEnable()
    {
        player = FindObjectsOfType<PlayerState>().ToList().Find(p => p.GetDisplayName() == PlayerPrefs.GetString("DisplayName"));
        PlayerState.onRespawn += HandleHPOnRespawn;
        fakeScore = player.GetBounty();
        GrappleUI.SetPlayer(FindObjectOfType<BasicPlayerController>());
    }

    private void OnDisable()
    {
        PlayerState.onRespawn -= HandleHPOnRespawn;
    }

    // Update is called once per frame
    void Update()
    {
        healthBar.SetHealth(player.GetHP());
        fakeScore = Mathf.RoundToInt(Mathf.Lerp(fakeScore, player.GetBounty(), Time.deltaTime * scoreUpdateRate));
        scoreText.text = "Bounty: $" + fakeScore;

        if (Input.GetKeyDown(KeyCode.F1)) player.TakeDamage(10);
    }

    void HandleHPOnRespawn(PlayerState p)
    {
        if(p == player) healthBar.HardSetHealth(player.GetHP());
    }
}
