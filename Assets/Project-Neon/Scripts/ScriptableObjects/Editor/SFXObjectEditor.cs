using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(SoundEffect))]
public class SFXObjectEditor : Editor
{
    private SoundEffect sfxObj;

    private void OnEnable()
    {
        sfxObj = target as SoundEffect;    
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Play Preview"))
        {
            sfxObj.PlayPreview();
        }

        if(sfxObj.previewer.isPlaying)
        {
            if (GUILayout.Button("Stop Preivew"))
            {
                sfxObj.StopPreviewer();
            }
        }
    }
}
