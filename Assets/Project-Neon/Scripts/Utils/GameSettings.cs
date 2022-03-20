using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class GameSettings : MonoBehaviour
{
    public static GameSettings instance;

    public float masterVolume, musicVolume, SFXVolume;
    public bool fullscreen, stylizedText;
    public int graphicsQuality, resolutionIndex;

    [SerializeField] AudioMixer mixer;
    Resolution[] resolutions;

    // Start is called before the first frame update
    void Start()
    {
        if(instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            ReadValuesFromFile();
        }
    }

    void ReadValuesFromFile()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0);
        mixer.SetFloat("MasterVolume", masterVolume);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0);
        mixer.SetFloat("MusicVolume", musicVolume);
        SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 0);
        mixer.SetFloat("SFXVolume", SFXVolume);

        int fullscreenint = PlayerPrefs.GetInt("Fullscreen", 0);
        fullscreen = fullscreenint == 1;
        Screen.fullScreen = fullscreen;

        int stylizedTextint = PlayerPrefs.GetInt("TextStylized", 1);
        stylizedText = stylizedTextint == 1;
        if (stylizedText)
        {
            FontManager.fontIndex = 0;
            MenuButton.fontIndex = 0;
        }
        else
        {
            FontManager.fontIndex = 1;
            MenuButton.fontIndex = 1;
        }

        graphicsQuality = PlayerPrefs.GetInt("GraphicsQuality", QualitySettings.GetQualityLevel());
        QualitySettings.SetQualityLevel(graphicsQuality);

        resolutions = Screen.resolutions;
        resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", resolutions.Length - 1);
        Resolution newResolution = resolutions[resolutionIndex];
        Screen.SetResolution(newResolution.width, newResolution.height, Screen.fullScreen);
    }

    public void SaveValuesToFile()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.GetFloat("SFXVolume", SFXVolume);

        int fullscreenInt = (fullscreen) ? 1 : 0;
        PlayerPrefs.SetInt("Fullscreen", fullscreenInt);
        int stylizedTextInt = (stylizedText) ? 1 : 0;
        PlayerPrefs.SetInt("TextStylized", stylizedTextInt);

        PlayerPrefs.SetInt("GraphicsQuality", graphicsQuality);
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
    }
}
