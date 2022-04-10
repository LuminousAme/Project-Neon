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
    [SerializeField] float PlayerHPRegenRate = 1f;
    float timeRemainingInMatch; //used to determine timestamps and when a game has ended
    bool multiplayer = true;
    Guid thisPlayerID;
    bool active;

    [SerializeField] List<Color> batColors = new List<Color>();

    public Guid GetThisPlayerID() => thisPlayerID;

    public static string winnerName;
    public static int winnerScore;

    private void OnEnable()
    {
        PlayerState.onRespawn += RespawnPlayer;
        QuickAttack.OnQuickAttack += RecordQuickAttack;
        HeavyAttack.OnHeavyAttack += RecordHeavyAttack;
        BasicPlayerController.OnDash += RecordDash;
        BasicPlayerController.OnGrapple += RecordGrapple;
        BasicPlayerController.OnDoubleJump += RecordDoubleJump;
    }

    private void OnDisable()
    {
        PlayerState.onRespawn -= RespawnPlayer;
        QuickAttack.OnQuickAttack -= RecordQuickAttack;
        HeavyAttack.OnHeavyAttack -= RecordHeavyAttack;
        BasicPlayerController.OnDash -= RecordDash;
        BasicPlayerController.OnGrapple -= RecordGrapple;
        BasicPlayerController.OnDoubleJump -= RecordDoubleJump;
    }

    public List<PlayerState> GetPlayers() => players;

    private void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);

        if(AsyncClient.instance != null)
        {
            multiplayer = true;
            List<Player> allInRoom = AsyncClient.instance.GetPlayers();
            for(int i = 0; i < allInRoom.Count; i++)
            {
                //spawn the local player
                if(allInRoom[i].id == AsyncClient.instance.GetThisClientID())
                {
                    GameObject newPlayer = Instantiate(localPlayerPrefab, initialSpawns[i].position, initialSpawns[i].rotation);
                    PlayerState state = newPlayer.GetComponent<PlayerState>();
                    state.SetPlayerID(allInRoom[i].id);
                    state.SetUseName(true, allInRoom[i].name);
                    thisPlayerID = state.GetPlayerID();
                    players.Add(state);
                    newPlayer.GetComponentInChildren<BatColourManager>().ApplyColour(batColors[i]);
                }
                //spawn remote player
                else
                {
                    GameObject newPlayer = Instantiate(remotePlayerPrefab, initialSpawns[i].position, initialSpawns[i].rotation);
                    PlayerState state = newPlayer.GetComponent<PlayerState>();
                    state.SetPlayerID(allInRoom[i].id);
                    state.SetUseName(false, allInRoom[i].name);
                    players.Add(state);
                    newPlayer.GetComponentInChildren<BatColourManager>().ApplyColour(batColors[i]);
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

        if (MusicManager.instance != null)
        {
            MusicManager.instance.StopCurrentTrack();
            MusicManager.instance.PlayTrack(0, 0.3f);
            MusicManager.instance.SetLooping(false);
        }

        PlayerState.hpRegenRate = PlayerHPRegenRate;
        active = true;
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
        active = false;

        for(int i = 0; i < players.Count; i++)
        {
            //force a score update on the server
            if(players[i].GetPlayerID() == thisPlayerID) players[i].DealDamage(0, false);
        }

        BasicPlayerController controller = FindObjectOfType<BasicPlayerController>();
        if (controller != null) controller.EndMatch();

        {
            MusicManager.instance.SilenceAllOtherSounds();
        }

        StartCoroutine(BeginEndMatchScene());
    }

    IEnumerator BeginEndMatchScene()
    {
        //wait for a second to allow the server to update all of the scores 
        yield return new WaitForSeconds(1f);

        //change the scene
        if (SceneManager.GetSceneByBuildIndex(5).isLoaded) SceneManager.UnloadSceneAsync(5);
        SceneManager.LoadScene(7, LoadSceneMode.Additive);
        if (AsyncClient.instance != null) AsyncClient.instance.Disconnect();
    }

    private void Update()
    {
        if(active)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].transform.position.y <= deathHeight)
                {
                    //kill the player so they respawn at their starting position
                    if (AsyncClient.instance == null || (AsyncClient.instance != null && AsyncClient.instance.GetThisClientID() == players[i].GetPlayerID()))
                        players[i].TakeDamage(100, new Vector3(1000f, 1000f, 1000f));
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

    public List<PlayerState> GetPlayersSortedByScore()
    {
        List<PlayerState> newPlayerStateList = new List<PlayerState>();

        for(int i = 0; i < players.Count; i++)
        {
            newPlayerStateList.Add(players[i]);
        }

        //using two lists just in case, I don't know if OrderBy changes the underlying code or not
        List<PlayerState> sortedPlayers = newPlayerStateList.OrderByDescending(p => p.GetBounty()).ToList();

        string roomCode = "test room";
        if (AsyncClient.instance != null) roomCode = AsyncClient.instance.roomCode;
        DataSaver.WriteGameResult(roomCode, sortedPlayers);

        return sortedPlayers;
    }

    private void RecordQuickAttack()
    {
        string roomCode = "test room";
        if (AsyncClient.instance != null) roomCode = AsyncClient.instance.roomCode;

        DataSaver.WriteData(roomCode, players.Find(p => p.GetPlayerID() == thisPlayerID).GetDisplayName(), "Quick Attack", timeRemainingInMatch);
    }

    private void RecordHeavyAttack()
    {
        string roomCode = "test room";
        if (AsyncClient.instance != null) roomCode = AsyncClient.instance.roomCode;

        DataSaver.WriteData(roomCode, players.Find(p => p.GetPlayerID() == thisPlayerID).GetDisplayName(), "Heavy Attack", timeRemainingInMatch);
    }

    private void RecordDash()
    {
        string roomCode = "test room";
        if (AsyncClient.instance != null) roomCode = AsyncClient.instance.roomCode;

        DataSaver.WriteData(roomCode, players.Find(p => p.GetPlayerID() == thisPlayerID).GetDisplayName(), "Dash", timeRemainingInMatch);
    }

    private void RecordGrapple()
    {
        string roomCode = "test room";
        if (AsyncClient.instance != null) roomCode = AsyncClient.instance.roomCode;

        DataSaver.WriteData(roomCode, players.Find(p => p.GetPlayerID() == thisPlayerID).GetDisplayName(), "Grapple", timeRemainingInMatch);
    }

    private void RecordDoubleJump()
    {
        string roomCode = "test room";
        if (AsyncClient.instance != null) roomCode = AsyncClient.instance.roomCode;

        DataSaver.WriteData(roomCode, players.Find(p => p.GetPlayerID() == thisPlayerID).GetDisplayName(), "Double Jump", timeRemainingInMatch);
    }
}
