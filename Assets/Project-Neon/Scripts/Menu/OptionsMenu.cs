using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    [SerializeField] MenuButton graphicsButton, audioButton, controlsButton, gamepadButton, vrButton;
    [SerializeField] GameObject graphicsPanel, audioPanel, controlsPanel, gamepadPanel;

    [SerializeField] AudioMixer mixer;

    Resolution[] resolutions;
    [SerializeField] TMP_Dropdown resolutionDropDown, qualityDropDown;
    [SerializeField] Toggle fullscreenToggle, stylizedTextToggle;
    [SerializeField] Slider masterVol, musicVol, sfxVol;

    [SerializeField] Slider mouseSens, ControlerSens;
    [SerializeField] TMP_Text mouseSensText, controllerSensText;
    [SerializeField] Toggle grappleToogleToggle;

    [SerializeField] GameObject VRPanel;
    [SerializeField] Toggle VrFovToggle;

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
        grappleToogleToggle.isOn = GameSettings.instance.toogleGrapple;
        VrFovToggle.isOn = GameSettings.instance.vrFOV;

        masterVol.value = GameSettings.instance.masterVolume;
        musicVol.value = GameSettings.instance.musicVolume;
        sfxVol.value = GameSettings.instance.SFXVolume;


        mouseSens.value = MathUlits.ReMapTwoRanges(0.1f, 1f, 0f, 0.5f, 1f, 10f, 0.5f, 1f, GameSettings.instance.mouseSensitivity);
        ControlerSens.value = MathUlits.ReMapTwoRanges(0.1f, 1f, 0f, 0.5f, 1f, 10f, 0.5f, 1f, GameSettings.instance.controllerSensitivity);

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
            graphicsButton.ClickWithoutSound();
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

        if(mouseSensText.gameObject.activeSelf)
        {
            mouseSensText.text = GameSettings.instance.mouseSensitivity.ToString();
        }
        if (controllerSensText.gameObject.activeSelf)
        {
            controllerSensText.text = GameSettings.instance.controllerSensitivity.ToString();
        }
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

        //if the hud is loaded, just unload this scene asyncly 
        if (SceneManager.GetSceneByBuildIndex(5).isLoaded)
        {
            SceneManager.UnloadSceneAsync(4);
        }
        //otherwise go to the 
        else
        {
            sceneTransition.beginTransition(0);
        }

    }

    public void GraphicsButtonPressed()
    {
        graphicsButton.FakeClick();
        audioButton.UnClick();
        controlsButton.UnClick();
        gamepadButton.UnClick();
        vrButton.UnClick();

        graphicsPanel.SetActive(true);
        audioPanel.SetActive(false);
        controlsPanel.SetActive(false);
        gamepadPanel.SetActive(false);
        VRPanel.SetActive(false);
    }

    public void AudioButtonPressed()
    {
        audioButton.FakeClick();
        graphicsButton.UnClick();
        controlsButton.UnClick();
        gamepadButton.UnClick();
        vrButton.UnClick();

        graphicsPanel.SetActive(false);
        audioPanel.SetActive(true);
        controlsPanel.SetActive(false);
        gamepadPanel.SetActive(false);
        VRPanel.SetActive(false);
    }

    public void ControlsButtonPressed()
    {
        controlsButton.FakeClick();
        graphicsButton.UnClick();
        audioButton.UnClick();
        gamepadButton.UnClick();
        vrButton.UnClick();

        graphicsPanel.SetActive(false);
        audioPanel.SetActive(false);
        controlsPanel.SetActive(true);
        gamepadPanel.SetActive(false);
        VRPanel.SetActive(false);
    }

    public void GamepadButtonPressed()
    {
        gamepadButton.FakeClick();
        graphicsButton.UnClick();
        audioButton.UnClick();
        controlsButton.UnClick();
        vrButton.UnClick();

        graphicsPanel.SetActive(false);
        audioPanel.SetActive(false);
        controlsPanel.SetActive(false);
        gamepadPanel.SetActive(true);
        VRPanel.SetActive(false);
    }

    public void VRButtonPressed()
    {
        vrButton.FakeClick();
        graphicsButton.UnClick();
        audioButton.UnClick();
        controlsButton.UnClick();
        gamepadButton.UnClick();

        graphicsPanel.SetActive(false);
        audioPanel.SetActive(false);
        controlsPanel.SetActive(false);
        gamepadPanel.SetActive(false);
        VRPanel.SetActive(true);
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

    public void SetIsGrappleToogle(bool isToogle)
    {
        GameSettings.instance.toogleGrapple = isToogle;
    }

    public void SetVRFovToggle(bool isToggle)
    {
        GameSettings.instance.vrFOV = isToggle;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution newResolution = resolutions[resolutionIndex];
        Screen.SetResolution(newResolution.width, newResolution.height, Screen.fullScreen);
        GameSettings.instance.resolutionIndex = resolutionIndex;
    }

    public void SetMouseSensitivty(float value)
    {
        GameSettings.instance.mouseSensitivity = MathUlits.ReMapTwoRanges(0f, 0.5f, 0.1f, 1f, 0.5f, 1f, 1f, 10f, value);
    }

    public void SetControllerSensitivity(float value)
    {
        GameSettings.instance.controllerSensitivity = MathUlits.ReMapTwoRanges(0f, 0.5f, 0.1f, 1f, 0.5f, 1f, 1f, 10f, value);
    }

    public void ResetMouseSens()
    {
        GameSettings.instance.mouseSensitivity = 1f;
        mouseSens.value = 0.5f;
    }

    public void ResetControllerSens()
    {
        GameSettings.instance.controllerSensitivity = 1f;
        ControlerSens.value = 0.5f;
    }
}