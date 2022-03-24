using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class MatchManager : MonoBehaviour
{
    PlayerState[] players;
    List<Vector3> startingPositions = new List<Vector3>();
    [SerializeField] float deathHeight;
    public static MatchManager instance = null; //non-persistant singleton

    private void OnEnable()
    {
        if(!SceneManager.GetSceneByBuildIndex(5).isLoaded) SceneManager.LoadScene(5, LoadSceneMode.Additive);
        PlayerState.onRespawn += RespawnPlayer;
    }

    private void OnDisable()
    {
        if (SceneManager.GetSceneByBuildIndex(5).isLoaded) SceneManager.UnloadSceneAsync(5);
        PlayerState.onRespawn -= RespawnPlayer;
    }

    private void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);

        players = FindObjectsOfType<PlayerState>();
        startingPositions.Clear();
        for (int i = 0; i < players.Length; i++)
        {
            startingPositions.Add(players[i].transform.position);
        }
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private void Update()
    {
        for(int i = 0; i < players.Length; i++)
        {
            if(players[i].transform.position.y <= deathHeight)
            {
                //kill the player so they respawn at their starting position
                players[i].TakeDamage(100);
            }
        }
    }

    private void RespawnPlayer(PlayerState player)
    {
        for(int i = 0; i < players.Length; i++)
        {
            if(player.GetDisplayName() == players[i].GetDisplayName())
            {
                players[i].transform.position = startingPositions[i];
                Rigidbody rb = players[i].GetComponent<Rigidbody>();
                if (rb != null) rb.AddForce(-rb.velocity, ForceMode.VelocityChange);
                break;
            }
        }
    }
}
