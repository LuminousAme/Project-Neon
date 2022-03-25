using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class HUDManager : MonoBehaviour
{
    PlayerState player;
    [SerializeField] TMP_Text scoreText;
    int fakeScore, actualScore;
    [SerializeField] float scoreUpdateTime = 5f;
    float scoreElapsedtime;
    [SerializeField] HealthBar healthBar;
    [SerializeField] GrappleUI GrappleUI;

    private void OnEnable()
    {
        player = FindObjectsOfType<PlayerState>().ToList().Find(p => p.GetDisplayName() == PlayerPrefs.GetString("DisplayName"));
        PlayerState.onRespawn += HandleHPOnRespawn;
        fakeScore = player.GetBounty();
        actualScore = player.GetBounty();
        scoreElapsedtime = 0f;
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

        
        if (Input.GetKeyDown(KeyCode.F1)) player.TakeDamage(10);
    }

    void HandleHPOnRespawn(PlayerState p)
    {
        if(p == player) healthBar.HardSetHealth(player.GetHP());
    }
}
