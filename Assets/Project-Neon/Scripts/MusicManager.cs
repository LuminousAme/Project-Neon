using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance = null;
    [SerializeField] List<AudioClip> tracks = new List<AudioClip>();
    AudioSource musicPlayer;

    // Start is called before the first frame update
    void Start()
    {
        if(instance == null)
        {
            instance = this;
            musicPlayer = this.GetComponent<AudioSource>();
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void PlayTrack(int index, float volume = 0.5f)
    {
        if(index < tracks.Count)
        {
            musicPlayer.clip = tracks[index];
            musicPlayer.volume = volume;
            musicPlayer.Play();
        }
    }

    public void SetLooping(bool looping)
    {
        musicPlayer.loop = looping;
    }

    public void StopCurrentTrack()
    {
        musicPlayer.Stop();
    }

    public void SilenceAllOtherSounds(List<AudioSource> excluding = null)
    {
        AudioSource[] audioSources = GetComponents<AudioSource>();

        for(int i = 0; i < audioSources.Length; i++)
        {
            if (audioSources[i] == musicPlayer) continue;

            if(excluding != null)
            {
                if (excluding.Contains(audioSources[i])) continue;
            }

            if (audioSources[i].isPlaying) audioSources[i].Stop();
        }
    }
}
