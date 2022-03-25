using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LobbyMenu : MonoBehaviour
{
    struct FlickerInfo
    {
        public bool flickering;
        public bool started;
        public bool on;
        public float time;
        public int number;
    }

    [SerializeField] List<GameObject> flickerObjects = new List<GameObject>();
    List<FlickerInfo> objectsFlickeringInfo = new List<FlickerInfo>();
    [SerializeField] SceneTransition sceneTransition;
    [SerializeField] float startDelay = 0f;

    [SerializeField] float minTimeBetweenFlickers = 5f, maxTimeBetweenFlickers = 10f;
    float currentTimeBetweenFlickers;
    [SerializeField] float minFlickerLenght = 0.1f, maxFlickerLenght = 0.5f;

    [SerializeField] float offsetPerElementOnStart = 2f;
    float timeSinceStart = 0.0f;
    float timeToStartFinish = 0.0f;
    bool startFinished = false;

    float timeElapsed;
    int lastIndex;

    [SerializeField] GameObject createOrJoinPanel, inRoomPanel;
    [SerializeField] List<FontManager> playerList = new List<FontManager>();
    [SerializeField] MenuButton readyButton, joinButton, createButton, backButton;
    [SerializeField] TMP_InputField lobbyCode;
    [SerializeField] TMP_Text ipLobby, ipLobbyUnlit;
    [SerializeField] List<TMP_Text> playerNames = new List<TMP_Text>();

    // Start is called before the first frame update
    void Start()
    {
        createOrJoinPanel.SetActive(true);
        inRoomPanel.SetActive(false);

        startFinished = false;
        timeToStartFinish = 0.0f;
        timeElapsed = 0.0f;

        for (int i = 0; i < flickerObjects.Count; i++)
        {
            if (flickerObjects[i].GetComponent<MenuButton>() == null) flickerObjects[i].SetActive(false);
            else flickerObjects[i].GetComponent<MenuButton>().lightOff = true;
            timeToStartFinish += offsetPerElementOnStart;

            FlickerInfo flicker = new FlickerInfo();
            flicker.flickering = false;
            flicker.started = false;
            flicker.on = false;
            objectsFlickeringInfo.Add(flicker);
        }

        timeSinceStart = -startDelay;
        currentTimeBetweenFlickers = Random.Range(minTimeBetweenFlickers, maxTimeBetweenFlickers);
    }

    // Update is called once per frame
    void Update()
    {
        if (startFinished)
        {
            //handle starting new flickers
            timeElapsed += Time.deltaTime;

            if (timeElapsed > currentTimeBetweenFlickers)
            {
                timeElapsed = 0f;
                currentTimeBetweenFlickers = Random.Range(minTimeBetweenFlickers, maxTimeBetweenFlickers);
                int newIndex = Random.Range(0, flickerObjects.Count);
                while (newIndex == lastIndex)
                {
                    newIndex = Random.Range(0, flickerObjects.Count);
                }
                lastIndex = newIndex;
                StartFlickering(newIndex);
            }
        }
        else LobbyFirstStageUpdate();

        //handle existing flickers
        HandleExistingFlickers();

        if(inRoomPanel.activeSelf)
        {
            GetLobbyCode();

            if (Client.instance != null)
            {
                List<Player> players = Client.instance.GetPlayers();
                for (int i = 0; i < players.Count; i++)
                {
                    if (i >= 4) break;
                    playerNames[i].gameObject.SetActive(true);
                    playerNames[i].text = players[i].name;
                    if (players[i].ready) playerNames[i].GetComponent<FontManager>().ChangeFontColor(1);
                    else playerNames[i].GetComponent<FontManager>().ChangeFontColor(0);
                }
                Debug.Log("Number of players: " + players.Count);
                for(int i = 3; i > players.Count-1; i--)
                {
                    playerNames[i].gameObject.SetActive(false);
                }
            }
        }
    }

    void LobbyFirstStageUpdate()
    {
        timeSinceStart += Time.deltaTime;

        int index = (int)(timeSinceStart / offsetPerElementOnStart);

        for (int i = 0; i < flickerObjects.Count; i++)
        {
            if (objectsFlickeringInfo[i].started) continue;

            if (i <= index)
            {
                StartFlickering(i);
            }
        }

        if (timeSinceStart >= timeToStartFinish)
        {
            startFinished = true;
        }
    }

    void StartFlickering(int index)
    {

        FlickerInfo flicker = objectsFlickeringInfo[index];
        flicker.flickering = true;
        flicker.started = true;
        flicker.number = Random.Range(4, 8);
        flicker.time = Random.Range(minFlickerLenght, maxFlickerLenght);
        flicker.on = true;
        if (flickerObjects[index].GetComponent<MenuButton>() == null) flickerObjects[index].SetActive(true);
        else flickerObjects[index].GetComponent<MenuButton>().lightOff = false;
        objectsFlickeringInfo[index] = flicker;
    }

    void HandleExistingFlickers()
    {
        //logo
        for (int i = 0; i < flickerObjects.Count; i++)
        {
            if (!objectsFlickeringInfo[i].flickering) continue;

            FlickerInfo flicker = objectsFlickeringInfo[i];
            flicker.time -= Time.deltaTime;
            if (flicker.time <= 0.0f)
            {
                flicker.number -= 1;
                if (flicker.number == 0)
                {
                    flicker.flickering = false;
                    flicker.on = true;
                }
                else
                {
                    flicker.time = Random.Range(minFlickerLenght, maxFlickerLenght);
                    flicker.on = !flicker.on;
                }

                if (flickerObjects[i].GetComponent<MenuButton>() == null) flickerObjects[i].SetActive(flicker.on);
                else flickerObjects[i].GetComponent<MenuButton>().lightOff = !flicker.on;
            }

            objectsFlickeringInfo[i] = flicker;
        }
    }

    public void BackButtonPressed()
    {
        if (inRoomPanel.activeSelf)
        {
            inRoomPanel.SetActive(false);
            createOrJoinPanel.SetActive(true);
            LeaveLobby();
            backButton.UnClick();
            backButton.OnStopHover();
        }
        else
        {
            sceneTransition.beginTransition(0);
        }
    }

    //handle the process of leaving the lobby
    void LeaveLobby()
    {
        joinButton.UnClick();
        joinButton.OnStopHover();
        createButton.UnClick();
        createButton.OnStopHover();
        Client.instance.Disconnect();
    }

    public void JoinLobby()
    {
        string targetLobby = lobbyCode.text;
        if (Client.instance != null)
        {
            Client.instance.roomCode = targetLobby;
            Client.instance.EnterGame(1);
        }
        // lobbyCode.text = "";
        //acutally use that target lobby to connect to the lobby
        Debug.Log(targetLobby);
        EnterLobby();
    }

    public void CreateLobby()
    {
        if (Client.instance != null) Client.instance.EnterGame(0);
        EnterLobby();
    }

    void EnterLobby()
    {
        inRoomPanel.SetActive(true);
        createOrJoinPanel.SetActive(false);
        readyButton.lightOff = false;
        ipLobby.text = "Lobby Code: " + lobbyCode.text;
        ipLobbyUnlit.text = "Lobby Code: " + lobbyCode.text;
    }

    public void GetLobbyCode()
    {
        string newCode = "";
        if (Client.instance != null)
        {
            newCode = Client.instance.roomCode;
        }

        ipLobby.text = newCode;
        ipLobbyUnlit.text = newCode;
    }

    public void LaunchGame()
    {
        if (Client.instance != null)
        {
            if(Client.instance.GetAllPlayersReady())
            {
                Client.instance.LaunchGameForAll();
                StartGame();
            }
        }
    }

    public void StartGame()
    {
        sceneTransition.beginTransition(6);
    }

    public void Ready()
    {
        if(readyButton.GetClicked())
        {
            readyButton.UnClick();
            if (Client.instance != null) Client.instance.SetReady(false);
        }
        else if (Client.instance != null) Client.instance.SetReady(true);
    }

    public void SetRoomCode(string newCode)
    {
        if (Client.instance != null) Client.instance.roomCode = newCode;
    }
}