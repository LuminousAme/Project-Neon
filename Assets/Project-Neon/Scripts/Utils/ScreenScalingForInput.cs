using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class ScreenScalingForInput : InputProcessor<Vector2>
{
    #if UNITY_EDITOR
    static ScreenScalingForInput()
    {
        Initialize();
    }
    #endif

    [RuntimeInitializeOnLoadMethod]
    static void Initialize()
    {
        InputSystem.RegisterProcessor<ScreenScalingForInput>();
    }

    public float scaleX = 5f;
    public float scaleY = 5f;

    public override Vector2 Process(Vector2 value, InputControl control)
    {
        //get the scale of the screen
        float width = Screen.width;
        float height = Screen.height;

        //normalized
        value /= new Vector2(width, height);
        //scaled to 1920 x 1080
        value *= new Vector2(1920f, 1080f);

        //scale back down
        value *= new Vector2(scaleX, scaleY);

        //scale value by mouse sensitivity
        if (GameSettings.instance != null)
        {
            value *= GameSettings.instance.mouseSensitivity;
        }

        return value;
    }
}
