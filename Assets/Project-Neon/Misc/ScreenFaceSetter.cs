using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenFaceSetter : MonoBehaviour
{
    [SerializeField] ScreenFaceController faceController;
    [SerializeField] Texture targetFace;


    [SerializeField] List<SoundEffect> playOnStart = new List<SoundEffect>();

    // Start is called before the first frame update
    void Start()
    {
        if (faceController != null && targetFace != null) faceController.UpdateTexture(targetFace);
        foreach (SoundEffect SFX in playOnStart) SFX.Play();
    }
}
