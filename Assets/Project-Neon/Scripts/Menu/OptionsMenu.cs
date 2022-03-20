using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
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
    bool firstFrame;

    [SerializeField] MenuButton graphicsButton, audioButton, controlsButton, gamepadButton;
    [SerializeField] GameObject graphicsPanel, audioPanel, controlsPanel, gamepadPanel;

    [SerializeField] AudioMixer mixer;

    Resolution[] resolutions;
    [SerializeField] TMP_Dropdown resolutionDropDown, qualityDropDown;
    [SerializeField] Toggle fullscreenToggle, stylizedTextToggle;
    [SerializeField] Slider masterVol, musicVol, sfxVol;

    // Start is called before the first frame update
    void Start()
    {
        firstFrame = true;
        startFinished = false;
        timeToStartFinish = 0.0f;
        timeElapsed = 0.0f;

        resolutions = Screen.resolutions;
        resolutionDropDown.ClearOptions();
        List<string> options = new List<string>();
        for(int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);
        }
        resolutionDropDown.AddOptions(options);
        resolutionDropDown.value = GameSettings.instance.resolutionIndex;
        resolutionDropDown.RefreshShownValue();

        qualityDropDown.value = GameSettings.instance.graphicsQuality;
        qualityDropDown.RefreshShownValue();

        fullscreenToggle.isOn = GameSettings.instance.fullscreen;
        stylizedTextToggle.isOn = GameSettings.instance.stylizedText;

        masterVol.value = GameSettings.instance.masterVolume;
        musicVol.value = GameSettings.instance.musicVolume;
        sfxVol.value = GameSettings.instance.SFXVolume;

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
        if(firstFrame)
        {
            graphicsButton.OnClick();
            firstFrame = false;
        }
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
        else OptionsFirstStageUpdate();

        //handle existing flickers
        HandleExistingFlickers();
    }


    void OptionsFirstStageUpdate()
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
        GameSettings.instance.SaveValuesToFile();
        sceneTransition.beginTransition(0);
    }

    public void GraphicsButtonPressed()
    {
        audioButton.UnClick();
        controlsButton.UnClick();
        gamepadButton.UnClick();

        graphicsPanel.SetActive(true);
        audioPanel.SetActive(false);
        controlsPanel.SetActive(false);
        gamepadPanel.SetActive(false);
    }

    public void AudioButtonPressed()
    {
        graphicsButton.UnClick();
        controlsButton.UnClick();
        gamepadButton.UnClick();

        graphicsPanel.SetActive(false);
        audioPanel.SetActive(true);
        controlsPanel.SetActive(false);
        gamepadPanel.SetActive(false);
    }

    public void ControlsButtonPressed()
    {
        graphicsButton.UnClick();
        audioButton.UnClick();
        gamepadButton.UnClick();


        graphicsPanel.SetActive(false);
        audioPanel.SetActive(false);
        controlsPanel.SetActive(true);
        gamepadPanel.SetActive(false);
    }

    public void GamepadButtonPressed()
    {
        graphicsButton.UnClick();
        audioButton.UnClick();
        controlsButton.UnClick();

        graphicsPanel.SetActive(false);
        audioPanel.SetActive(false);
        controlsPanel.SetActive(false);
        gamepadPanel.SetActive(true);
    }

    public void SetMasterVolume(float volume)
    {
        mixer.SetFloat("MasterVolume", volume);
        GameSettings.instance.masterVolume = volume;
    }

    public void SetMusicVolume(float volume)
    {
        mixer.SetFloat("MusicVolume", volume);
        GameSettings.instance.musicVolume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        mixer.SetFloat("SFXVolume", volume);
        GameSettings.instance.SFXVolume = volume;
    }

    public void SetGraphicsQuality(int QualityIndex)
    {
        QualitySettings.SetQualityLevel(QualityIndex);
        GameSettings.instance.graphicsQuality = QualityIndex;
    }

    public void SetIsFullScreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        GameSettings.instance.fullscreen = isFullscreen;
    }

    public void SetIsStylizedText(bool isStylized)
    {
        if (isStylized)
        {
            FontManager.fontIndex = 0;
            MenuButton.fontIndex = 0;
        }
        else
        {
            FontManager.fontIndex = 1;
            MenuButton.fontIndex = 1;
        }

        GameSettings.instance.stylizedText = isStylized;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution newResolution = resolutions[resolutionIndex];
        Screen.SetResolution(newResolution.width, newResolution.height, Screen.fullScreen);
        GameSettings.instance.resolutionIndex = resolutionIndex;
    }
}