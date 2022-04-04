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
    public Vector2 volume = new Vector2(0.5f, 0.5f);
    [Range(0f, 3f)]
    public Vector2 pitch = new Vector2(1f, 1f);

    public bool useSemitones = false;
    [Range(-10,10)]
    public Vector2Int semitones = new Vector2Int(0, 0);

    public AudioMixerGroup targetMixerGroup;

    enum SFXPlayerOrder
    {
        RANDOM, 
        IN_ORDER,
        REVERSED
    }

    [SerializeField] SFXPlayerOrder clipPlayOrder;
    private int playIndex = 0;

#if UNITY_EDITOR
    public AudioSource previewer;

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
                Random.Range(0, audioClips.Length);
                break;
        }

        return clip;
    }

    public void SyncPitchAndSemitones()
    {
        if(useSemitones)
        {
            pitch.x = Mathf.Pow(1.05946f, semitones.x);
            pitch.y = Mathf.Pow(1.05946f, semitones.y);
        }
        else
        {
            semitones.x = Mathf.RoundToInt(Mathf.Log10(pitch.x) / Mathf.Log10(1.05946f));
            semitones.y = Mathf.RoundToInt(Mathf.Log10(pitch.y) / Mathf.Log10(1.05946f));
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
        source.volume = Random.Range(volume.x, volume.y);
        source.pitch = useSemitones ? Mathf.Pow(1.05946f, Random.Range(semitones.x, semitones.y+1)) :  Random.Range(pitch.x, pitch.y);
        source.outputAudioMixerGroup = targetMixerGroup;

        source.Play();
        if (newSource) Destroy(source.gameObject, source.clip.length / source.pitch);


        return source;
    }
}