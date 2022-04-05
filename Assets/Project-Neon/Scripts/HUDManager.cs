using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using System;

public class HUDManager : MonoBehaviour
{
    PlayerState player;
    [SerializeField] TMP_Text scoreText;
    int fakeScore, actualScore;
    [SerializeField] float scoreUpdateTime = 5f;
    float scoreElapsedtime;
    [SerializeField] HealthBar healthBar;
    [SerializeField] GrappleUI GrappleUI;
    [SerializeField] DashBar dashUI;
    [SerializeField] AttackHUD crosshair;
    //[SerializeField] DashBar dash2;
    [SerializeField] TMP_Text timeText;

    private void OnEnable()
    {
        if(MatchManager.instance != null )
            player = FindObjectsOfType<PlayerState>().ToList().Find(p => p.GetPlayerID() == MatchManager.instance.GetThisPlayerID());

        PlayerState.onRespawn += HandleHPOnRespawn;
        fakeScore = player.GetBounty();
        actualScore = player.GetBounty();
        scoreElapsedtime = 0f;
        GrappleUI.SetPlayer(FindObjectOfType<BasicPlayerController>());
        dashUI.SetPlayer(FindObjectOfType<BasicPlayerController>());
        crosshair.SetPlayer(FindObjectOfType<BasicPlayerController>());
        
    }

    private void OnDisable()
    {
        PlayerState.onRespawn -= HandleHPOnRespawn;
    }

    // Update is called once per frame
    void Update()
    {
        healthBar.SetHealth(player.GetHP());

        if(actualScore != player.GetBounty())
        {
            int displayedBefore = Mathf.RoundToInt(Mathf.Lerp(fakeScore, actualScore, scoreElapsedtime / scoreUpdateTime));
            actualScore = player.GetBounty();
            float percentNow = MathUlits.InverseLerp(fakeScore, actualScore, displayedBefore);
            scoreElapsedtime = Mathf.Lerp(0f, scoreUpdateTime, percentNow); 
        } 
        int displayed = Mathf.RoundToInt(Mathf.Lerp(fakeScore, actualScore, scoreElapsedtime / scoreUpdateTime));
        scoreText.text = "Bounty: $" + displayed;
        scoreElapsedtime = Mathf.Clamp(scoreElapsedtime + Time.deltaTime, 0f, scoreUpdateTime);
        if (scoreElapsedtime >= scoreUpdateTime) fakeScore = actualScore;

        if(MatchManager.instance != null)
        {
            TimeSpan time = TimeSpan.FromSeconds(MatchManager.instance.GetTimeRemaining());
            float minutes = time.Minutes;
            float seconds = time.Seconds;
            string minutesText = $"{minutes}";
            string secondsText = (seconds >= 10) ? $"{seconds}" : $"0{seconds}";
            timeText.text = minutesText + ":" + secondsText;
        }
        else
        {
            timeText.text = "";
        }
        
        if (Input.GetKeyDown(KeyCode.F1)) player.TakeDamage(10, new Vector3(1000f, 1000f, 1000f));
    }

    void HandleHPOnRespawn(PlayerState p)
    {
        if(p == player) healthBar.HardSetHealth(player.GetHP());
    }
}
