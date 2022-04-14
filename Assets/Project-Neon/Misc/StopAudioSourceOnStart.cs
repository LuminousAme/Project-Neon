using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopAudioSourceOnStart : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        if (audioSource != null && audioSource.isPlaying) audioSource.Stop();
    }
}
