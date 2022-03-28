using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System;

public class MatchManager : MonoBehaviour
{
    List<PlayerState> players = new List<PlayerState>();
    [SerializeField] GameObject localPlayerPrefab, remotePlayerPrefab;
    List<Vector3> startingPositions = new List<Vector3>();
    [SerializeField] float deathHeight;
    public static MatchManager instance = null; //non-persistant singleton
    [SerializeField] Transform[] initialSpawns = new Transform[4];
    [SerializeField] float matchTimeSeconds = 300f;
    float timeRemainingInMatch; //used to determine timestamps and when a game has ended
    bool multiplayer = true;
    Guid thisPlayerID;
    public Guid GetThisPlayerID() => thisPlayerID;

    public static string winnerName;
    public static int winnerScore;

    private void OnEnable()
    {

        PlayerState.onRespawn += RespawnPlayer;
    }

    private void OnDisable()
    {

        PlayerState.onRespawn -= RespawnPlayer;
    }

    public List<PlayerState> GetPlayers() => players;

    private void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);

        if(Client.instance != null)
        {
            multiplayer = true;
            List<Player> allInRoom = Client.instance.GetPlayers();
            for(int i = 0; i < allInRoom.Count; i++)
            {
                //spawn the local player
                if(allInRoom[i].id == Client.instance.GetThisClientID())
                {
                    GameObject newPlayer = Instantiate(localPlayerPrefab, initialSpawns[i].position, initialSpawns[i].rotation);
                    PlayerState state = newPlayer.GetComponent<PlayerState>();
                    state.SetPlayerID(allInRoom[i].id);
                    thisPlayerID = state.GetPlayerID();
                    players.Add(state);
                }
                //spawn remote player
                else
                {
                    GameObject newPlayer = Instantiate(remotePlayerPrefab, initialSpawns[i].position, initialSpawns[i].rotation);
                    PlayerState state = newPlayer.GetComponent<PlayerState>();
                    state.SetPlayerID(allInRoom[i].id);
                    players.Add(state);
                }
            }
        }
        else
        {
            multiplayer = false;
            GameObject newPlayer = Instantiate(localPlayerPrefab, initialSpawns[0].position, initialSpawns[0].rotation);
            PlayerState state = newPlayer.GetComponent<PlayerState>();
            state.SetPlayerID(new Guid());
            thisPlayerID = state.GetPlayerID();
            players.Add(state);
        }

        startingPositions.Clear();
        for (int i = 0; i < players.Count; i++)
        {
            startingPositions.Add(players[i].transform.position);
        }

        if (!SceneManager.GetSceneByBuildIndex(5).isLoaded) SceneManager.LoadScene(5, LoadSceneMode.Additive);

        timeRemainingInMatch = matchTimeSeconds;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
            if (SceneManager.GetSceneByBuildIndex(5).isLoaded) SceneManager.UnloadSceneAsync(5);
        }
    }

    private void EndMatch()
    {
        int winnerIndex = -1, highestScore = -1;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].GetBounty() > highestScore)
            {
                highestScore = players[i].GetBounty();
                winnerIndex = i;
            }
        }

        winnerName = players[winnerIndex].GetDisplayName();
        winnerScore = players[winnerIndex].GetBounty();
    }

    private void Update()
    {
        for(int i = 0; i < players.Count; i++)
        {
            if(players[i].transform.position.y <= deathHeight)
            {
                //kill the player so they respawn at their starting position
                if(Client.instance == null || (Client.instance != null && Client.instance.GetThisClientID() == players[i].GetPlayerID()))
                    players[i].TakeDamage(100);
            }
        }

        if (multiplayer) timeRemainingInMatch -= Time.deltaTime;
        if (timeRemainingInMatch <= 0.0f) EndMatch();

        /*
        if(Input.GetKeyDown(KeyCode.F2))
        {
            if (!SceneManager.GetSceneByBuildIndex(4).isLoaded) SceneManager.LoadScene(4, LoadSceneMode.Additive);
            else if (SceneManager.GetSceneByBuildIndex(4).isLoaded) SceneManager.UnloadSceneAsync(4);
        }*/
    }

    private void RespawnPlayer(PlayerState player)
    {
        for(int i = 0; i < players.Count; i++)
        {
            if(player.GetPlayerID() == players[i].GetPlayerID())
            {
                players[i].transform.position = startingPositions[i];
                Rigidbody rb = players[i].GetComponent<Rigidbody>();
                if (rb != null) rb.AddForce(-rb.velocity, ForceMode.VelocityChange);
                break;
            }
        }
    }

    public float GetTimeRemaining() => timeRemainingInMatch;
}
