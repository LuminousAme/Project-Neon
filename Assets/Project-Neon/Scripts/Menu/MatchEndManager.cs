using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MatchEndManager : MonoBehaviour
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

    [SerializeField] TMP_Text FirstPlaceText, FirstPlaceBounty, SecondPlaceText, SecondPlaceBounty, ThirdPlaceText, ThirdPlaceBounty, FourthPlaceText, FourthPlaceBounty;
    [SerializeField] TMP_Text unlitFirstPlaceText, unlitFirstPlaceBounty, unlitSecondPlaceText, unlitSecondPlaceBounty, unlitThirdPlaceText, unlitThirdPlaceBounty, unlitFourthPlaceText, unlitFourthPlaceBounty;

    List<PlayerState> players;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        players = MatchManager.instance.GetPlayersSortedByScore();

        if(players.Count > 0)
        {
            FirstPlaceText.text = "1st: " + players[0].GetDisplayName();
            FirstPlaceBounty.text = "$" + players[0].GetBounty().ToString();
            unlitFirstPlaceText.text = "1st: " + players[0].GetDisplayName();
            unlitFirstPlaceBounty.text = "$" + players[0].GetBounty().ToString();
        }
        else
        {
            FirstPlaceText.text = "";
            FirstPlaceBounty.text = "";
            unlitFirstPlaceText.text = "";
            unlitFirstPlaceBounty.text = "";
        }

        if (players.Count > 1)
        {
            SecondPlaceText.text = "2nd: " + players[1].GetDisplayName();
            SecondPlaceBounty.text = "$" + players[1].GetBounty().ToString();
            unlitSecondPlaceText.text = "2nd: " + players[1].GetDisplayName();
            unlitSecondPlaceBounty.text = "$" + players[1].GetBounty().ToString();
        }
        else
        {
            SecondPlaceText.text = "";
            SecondPlaceBounty.text = "";
            unlitSecondPlaceText.text = "";
            unlitSecondPlaceBounty.text = "";
        }

        if (players.Count > 2)
        {
            ThirdPlaceText.text = "3rd: " + players[2].GetDisplayName();
            ThirdPlaceBounty.text = "$" + players[2].GetBounty().ToString();
            unlitThirdPlaceText.text = "3rd: " + players[2].GetDisplayName();
            unlitThirdPlaceBounty.text = "$" + players[2].GetBounty().ToString();
        }
        else
        {
            ThirdPlaceText.text = "";
            ThirdPlaceBounty.text = "";
            unlitThirdPlaceText.text = "";
            unlitThirdPlaceBounty.text = "";
        }

        if (players.Count > 3)
        {
            FourthPlaceText.text = "4th: " + players[3].GetDisplayName();
            FourthPlaceBounty.text = "$" + players[3].GetBounty().ToString();
            unlitFourthPlaceText.text = "4th: " + players[3].GetDisplayName();
            unlitFourthPlaceBounty.text = "$" + players[3].GetBounty().ToString();
        }
        else
        {
            FourthPlaceText.text = "";
            FourthPlaceBounty.text = "";
            unlitFourthPlaceText.text = "";
            unlitFourthPlaceBounty.text = "";
        }

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
        else endMatchScreenFirstStageUpdate();

        //handle existing flickers
        HandleExistingFlickers();
    }

    void endMatchScreenFirstStageUpdate()
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

    public void MenuButtonPressed()
    {
        sceneTransition.beginTransition(0);
    }
}
