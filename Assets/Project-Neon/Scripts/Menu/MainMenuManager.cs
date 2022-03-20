using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    struct FlickerInfo
    {
        public bool flickering;
        public bool started;
        public bool on;
        public float time;
        public int number;
    }

    [SerializeField] List<GameObject> logoObjects = new List<GameObject>();
    List<FlickerInfo> logoObjectsFlickering = new List<FlickerInfo>();
    [SerializeField] List<MenuButton> flickeringButtons = new List<MenuButton>();
    List<FlickerInfo> buttonsFlickering = new List<FlickerInfo>();

    [SerializeField] TMP_InputField nameField;
    [SerializeField] SceneTransition sceneTransition;
    [SerializeField] float startDelay = 0f;

    [SerializeField] float minTimeBetweenLogoFlickers = 5f, maxTimeBetweenLogoFlickers = 10f;
    float currentTimeBetweenLogoFlickers;
    [SerializeField] float minLogoFlickerLenght = 0.1f, maxLogoFlickerLenght = 0.5f;
    [SerializeField] float minTimeBetweenButtonFlickers = 5f, maxTimeBetweenButtonFlickers = 10f;
    float currentTimeBetweenButtonFlickers;
    [SerializeField] float minButtonFlickerLenght = 0.1f, maxButtonFlickerLenght = 0.5f;

    [SerializeField] float offsetPerElementOnStart = 2f;
    float timeSinceStart = 0.0f;
    float timeToStartFinish = 0.0f;
    bool startFinished = false;

    float timeElapsedButton = 0f;
    int lastButtonIndex = -1;
    float timeElapsedLogo = 0f;
    int lastLogoIndex = -1;

    // Start is called before the first frame update
    void Start()
    {
        string name = PlayerPrefs.GetString("DisplayName", "");
        nameField.text = name;

        startFinished = false;
        timeToStartFinish = 0.0f;
        timeElapsedButton = minTimeBetweenButtonFlickers;
        timeElapsedLogo = minTimeBetweenLogoFlickers;

        for (int i = 0; i < logoObjects.Count; i++)
        {
            logoObjects[i].SetActive(false);
            timeToStartFinish += offsetPerElementOnStart;

            FlickerInfo flicker = new FlickerInfo();
            flicker.flickering = false;
            flicker.started = false;
            flicker.on = false;
            logoObjectsFlickering.Add(flicker);
        }
        for (int i = 0; i < flickeringButtons.Count; i++)
        {
            flickeringButtons[i].lightOff = true;
            timeToStartFinish += offsetPerElementOnStart;

            FlickerInfo flicker = new FlickerInfo();
            flicker.flickering = false;
            flicker.started = false;
            flicker.on = false;
            buttonsFlickering.Add(flicker);
        }

        timeSinceStart = -startDelay;

        currentTimeBetweenLogoFlickers = Random.Range(minTimeBetweenLogoFlickers, maxTimeBetweenLogoFlickers);
        currentTimeBetweenButtonFlickers = Random.Range(minTimeBetweenButtonFlickers, maxTimeBetweenButtonFlickers);
    }

    // Update is called once per frame
    void Update()
    {
        if (startFinished)
        {
            //handle starting new flickers
            timeElapsedButton += Time.deltaTime;
            timeElapsedLogo += Time.deltaTime;

            //buttons
            if (timeElapsedButton > currentTimeBetweenButtonFlickers)
            {
                timeElapsedButton = 0f;
                currentTimeBetweenButtonFlickers = Random.Range(minTimeBetweenButtonFlickers, maxTimeBetweenButtonFlickers);
                int newButtonIndex = Random.Range(0, flickeringButtons.Count);
                while (newButtonIndex == lastButtonIndex)
                {
                    newButtonIndex = Random.Range(0, flickeringButtons.Count);
                }
                lastButtonIndex = newButtonIndex;
                StartFlickering(1, newButtonIndex);
            }

            //logo
            if (timeElapsedLogo > currentTimeBetweenLogoFlickers)
            {
                timeElapsedLogo = 0f;
                currentTimeBetweenLogoFlickers = Random.Range(minTimeBetweenLogoFlickers, maxTimeBetweenLogoFlickers);
                int newLogoIndex = Random.Range(0, logoObjects.Count);
                while (newLogoIndex == lastLogoIndex)
                {
                    newLogoIndex = Random.Range(0, logoObjects.Count);
                }
                lastLogoIndex = newLogoIndex;
                StartFlickering(0, newLogoIndex);
            }
        }
        else menuFirstStageUpdate();

        //handle existing flickers
        HandleExistingFlickers();
    }

    void menuFirstStageUpdate()
    {
        timeSinceStart += Time.deltaTime;

        int index = (int)(timeSinceStart / offsetPerElementOnStart);

        for (int i = 0; i < logoObjectsFlickering.Count; i++)
        {
            if (logoObjectsFlickering[i].started) continue;

            if (i <= index)
            {
                StartFlickering(0, i);
            }
        }

        for (int i = 0; i < buttonsFlickering.Count; i++)
        {
            if (buttonsFlickering[i].started) continue;

            int j = i + logoObjectsFlickering.Count;

            if (j <= index)
            {
                StartFlickering(1, i);
            }
        }

        if (timeSinceStart >= timeToStartFinish)
        {
            startFinished = true;
        }
    }

    void StartFlickering(int type, int index)
    {
        if (type == 0)
        {
            FlickerInfo flicker = logoObjectsFlickering[index];
            flicker.flickering = true;
            flicker.started = true;
            flicker.number = Random.Range(4, 8);
            flicker.time = Random.Range(minLogoFlickerLenght, maxLogoFlickerLenght);
            flicker.on = true;
            logoObjects[index].SetActive(true);
            logoObjectsFlickering[index] = flicker;
        }
        else if (type == 1)
        {
            FlickerInfo flicker = buttonsFlickering[index];
            flicker.flickering = true;
            flicker.started = true;
            flicker.number = Random.Range(4, 8);
            flicker.time = Random.Range(minButtonFlickerLenght, maxButtonFlickerLenght);
            flicker.on = true;
            flickeringButtons[index].lightOff = false;
            buttonsFlickering[index] = flicker;
        }
    }

    void HandleExistingFlickers()
    {
        //logo
        for(int i = 0; i < logoObjectsFlickering.Count; i++)
        {
            if (!logoObjectsFlickering[i].flickering) continue;

            FlickerInfo flicker = logoObjectsFlickering[i];
            flicker.time -= Time.deltaTime;
            if(flicker.time <= 0.0f)
            {
                flicker.number -= 1;
                if(flicker.number == 0)
                {
                    flicker.flickering = false;
                    flicker.on = true;
                }
                else
                {
                    flicker.time = Random.Range(minLogoFlickerLenght, maxLogoFlickerLenght);
                    flicker.on = !flicker.on;
                }

                logoObjects[i].SetActive(flicker.on);
            }

            logoObjectsFlickering[i] = flicker;
        }

        //buttons
        for (int i = 0; i < buttonsFlickering.Count; i++)
        {
            if (!buttonsFlickering[i].flickering) continue;

            FlickerInfo flicker = buttonsFlickering[i];
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
                    flicker.time = Random.Range(minButtonFlickerLenght, maxButtonFlickerLenght);
                    flicker.on = !flicker.on;
                }

                flickeringButtons[i].lightOff = !flicker.on;
            }

            buttonsFlickering[i] = flicker;
        }
    }

    public void PlayButtonPressed()
    {
        sceneTransition.beginTransition(2);
    }

    public void OptionsButtonPressed()
    {
        sceneTransition.beginTransition(4);
    }

    public void CreditsButtonPressed()
    {
        sceneTransition.beginTransition(1);
    }

    public void QuitButtonPressed()
    {
        Application.Quit();
    }

    public void SelectNameField(string name)
    {
        Vector3 scale = nameField.transform.localScale;
        scale.x = 1.2f;
        scale.y = 1.2f;
        nameField.transform.localScale = scale;
    }

    public void DeselectNameField(string name)
    {
        Vector3 scale = nameField.transform.localScale;
        scale.x = 1f;
        scale.y = 1f;
        nameField.transform.localScale = scale;
    }

    public void FinishedEditing(string name)
    {
        PlayerPrefs.SetString("DisplayName", name);
    }
}
