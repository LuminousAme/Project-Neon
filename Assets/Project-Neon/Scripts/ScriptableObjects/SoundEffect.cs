using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "SFXObj", menuName = "ProjectNeon/Audio/SFXObj")]
public class SoundEffect : ScriptableObject
{
    //based on this video https://youtu.be/xDLqdZu0ll0
    public AudioClip[] audioClips;
    [Range(0f, 1f)]
    public float minVolume = 0.5f, maxVolume = 0.5f;
    [Range(0f, 3f)]
    public float minPitch = 1, maxPitch = 1f;

    public bool useSemitones = false;
    [Range(-10, 10)]
    public int minSemitones = 0, maxSemitones = 0;

    public AudioMixerGroup targetMixerGroup;
    private AudioSource tempSource;

    enum SFXPlayerOrder
    {
        RANDOM, 
        IN_ORDER,
        REVERSED
    }

    [SerializeField] SFXPlayerOrder clipPlayOrder;
    private int playIndex = 0;

#if UNITY_EDITOR
    [HideInInspector] public AudioSource previewer;

    private void OnEnable()
    {
        previewer = EditorUtility.CreateGameObjectWithHideFlags("AudioPreviewer", HideFlags.HideAndDontSave, typeof(AudioSource)).GetComponent<AudioSource>();
    }

    private void OnDisable()
    {
        DestroyImmediate(previewer.gameObject);
    }

    public void PlayPreview()
    {
        Play(previewer);
    }

    public void StopPreviewer()
    {
        previewer.Stop();
    }
#endif

    private AudioClip GetClip()
    {
        //get currenet clip
        AudioClip clip = audioClips[playIndex >= audioClips.Length ? 0 : playIndex];

        switch(clipPlayOrder)
        {
            case SFXPlayerOrder.IN_ORDER:
                playIndex = (playIndex + 1) % audioClips.Length;
                break;
            case SFXPlayerOrder.REVERSED:
                playIndex = (playIndex +  audioClips.Length - 1) % audioClips.Length;
                break;
            case SFXPlayerOrder.RANDOM:
                playIndex = Random.Range(0, audioClips.Length);
                break;
        }

        return clip;
    }

    public void SyncPitchAndSemitones()
    {
        if(useSemitones)
        {
            minPitch = Mathf.Pow(1.05946f, minSemitones);
            maxPitch = Mathf.Pow(1.05946f, maxSemitones);
        }
        else
        {
            minSemitones = Mathf.RoundToInt(Mathf.Log10(minPitch) / Mathf.Log10(1.05946f));
            maxSemitones = Mathf.RoundToInt(Mathf.Log10(maxPitch) / Mathf.Log10(1.05946f));
        }
    }

    private void OnValidate()
    {
        SyncPitchAndSemitones();
    }

    public AudioSource Play(AudioSource audioParam = null)
    {
        if(audioClips.Length == 0)
        {
            Debug.LogWarning("Missing Audio clips");
            return null;
        }

        bool newSource = false;
        AudioSource source = audioParam;
        if(source == null)
        {
            GameObject newObj = new GameObject("Audio Effect", typeof(AudioSource));
            source = newObj.GetComponent<AudioSource>();
            newSource = true;
        }

        //set the source confirguation
        source.clip = GetClip();
        source.volume = Random.Range(minVolume, maxVolume);
        source.pitch = useSemitones ? Mathf.Pow(1.05946f, Random.Range(minSemitones, maxSemitones+1)) :  Random.Range(minPitch, maxPitch);
        source.outputAudioMixerGroup = targetMixerGroup;

        source.Play();
        if (newSource) Destroy(source.gameObject, source.clip.length / source.pitch);

        return source;
    }

    public AudioSource PlayDuplicate(AudioSource toDup)
    {
        GameObject newObj = Instantiate(toDup.gameObject);
        AudioSource newSource = newObj.GetComponent<AudioSource>();
        Play(newSource);
        Destroy(newObj, newSource.clip.length / newSource.pitch);
        return newSource;
    }
}