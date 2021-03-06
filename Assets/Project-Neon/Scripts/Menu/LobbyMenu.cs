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
    [SerializeField] MenuButton readyButton, joinButton, createButton, backButton;
    [SerializeField] TMP_InputField lobbyCode, customIPField;
    [SerializeField] TMP_Text codeLobbyText, codeLobbyTextUnlit, customIPText, customIPUnlit;
    [SerializeField] List<TMP_Text> playerNames = new List<TMP_Text>();

    [SerializeField] TMP_Text badConnectText;

    // Start is called before the first frame update
    void Start()
    {
        createOrJoinPanel.SetActive(true);
        inRoomPanel.SetActive(false);

        GetCustomIP();

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


            if (AsyncClient.instance != null)
            {
                List<Player> players = AsyncClient.instance.GetPlayers();
                for (int i = 0; i < players.Count; i++)
                {
                    if (i >= 4) break;
                    playerNames[i].gameObject.SetActive(true);
                    playerNames[i].text = players[i].name;
                    if (players[i].ready) playerNames[i].GetComponent<FontManager>().ChangeFontColor(1);
                    else playerNames[i].GetComponent<FontManager>().ChangeFontColor(0);
                }
                //Debug.Log("Number of players: " + players.Count);
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
            readyButton.UnClick();
            readyButton.OnStopHover();
            inRoomPanel.SetActive(false);
            createOrJoinPanel.SetActive(true);
            LeaveLobby();
            backButton.UnClick();
            backButton.OnStopHover();
            GetCustomIP();
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
        AsyncClient.instance.Disconnect();
    }

    public void JoinLobby()
    {
        if (badConnectText != null) badConnectText.text = "";

        string targetLobby = lobbyCode.text;
        if (AsyncClient.instance != null)
        {
            AsyncClient.instance.roomCode = targetLobby.ToUpper();
            AsyncClient.instance.EnterGame(1);
        }
        // lobbyCode.text = "";
        //acutally use that target lobby to connect to the lobby
        Debug.Log(targetLobby);
        EnterLobby();
    }

    public void CreateLobby()
    {
        if (badConnectText != null) badConnectText.text = "";

        if (AsyncClient.instance != null) AsyncClient.instance.EnterGame(0);
        EnterLobby();
    }

    void EnterLobby()
    {
        inRoomPanel.SetActive(true);
        createOrJoinPanel.SetActive(false);
        readyButton.lightOff = false;
        codeLobbyText.text = "Lobby Code: " + lobbyCode.text;
        codeLobbyTextUnlit.text = "Lobby Code: " + lobbyCode.text;
    }

    public void GetLobbyCode()
    {
        string newCode = "";
        if (AsyncClient.instance != null)
        {
            newCode = AsyncClient.instance.roomCode;
        }

        codeLobbyText.text = newCode;
        codeLobbyTextUnlit.text = newCode;
    }

    public void GetCustomIP()
    {
        string ip = GameSettings.instance.customServerIP;
        customIPText.text = ip;
        customIPUnlit.text = ip;
        customIPField.text = ip;
    }

    public void LaunchGame()
    {
        if (AsyncClient.instance != null)
        {
            if(AsyncClient.instance.GetAllPlayersReady())
            {
                AsyncClient.instance.LaunchGameForAll();
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
            if (AsyncClient.instance != null) AsyncClient.instance.SetReady(false);
        }
        else if (AsyncClient.instance != null) AsyncClient.instance.SetReady(true);
    }

    public void SetRoomCode(string newCode)
    {
        if (AsyncClient.instance != null) AsyncClient.instance.roomCode = newCode;
    }

    public void SetCustomIP(string customIP)
    {
        GameSettings.instance.customServerIP = customIP;
        GameSettings.instance.SaveValuesToFile();
    }

    public void SetBadConnectMessage(string msg)
    {
        if (badConnectText != null) badConnectText.text = msg;

        if (inRoomPanel.activeSelf)
        {
            inRoomPanel.SetActive(false);
            createOrJoinPanel.SetActive(true);
            LeaveLobby();
        }
    }
}