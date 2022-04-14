using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Editor;
using UnityEditor;

#if UNITY_EDITOR
public class ScreenScalingForInputEditor : InputParameterEditor<ScreenScalingForInput>
{
    private GUIContent labelx = new GUIContent("Scale X");
    private GUIContent labely = new GUIContent("Scale Y");

    public override void OnGUI()
    {
        target.scaleX = EditorGUILayout.FloatField(labelx, target.scaleX);
        target.scaleY = EditorGUILayout.FloatField(labely, target.scaleY);
    }
}
#endif