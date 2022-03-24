using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchManager : MonoBehaviour
{
    PlayerState[] players;
    List<Vector3> startingPositions = new List<Vector3>();
    [SerializeField] float deathHeight;

    private void OnEnable()
    {
        if(!SceneManager.GetSceneByBuildIndex(5).isLoaded) SceneManager.LoadScene(5, LoadSceneMode.Additive);
    }

    private void OnDisable()
    {
        if (SceneManager.GetSceneByBuildIndex(5).isLoaded) SceneManager.UnloadSceneAsync(5);
    }

    private void Start()
    {
        players = FindObjectsOfType<PlayerState>();
        startingPositions.Clear();
        for (int i = 0; i < players.Length; i++)
        {
            startingPositions.Add(players[i].transform.position);
        }
    }

    private void Update()
    {
        for(int i = 0; i < players.Length; i++)
        {
            if(players[i].transform.position.y <= deathHeight)
            {
                //kill the player and respawn them at their starting position
                players[i].TakeDamage(100);
                players[i].transform.position = startingPositions[i];
            }
        }
    }
}
