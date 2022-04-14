using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class ControllerSensitivityInterface : InputProcessor<Vector2>
{
#if UNITY_EDITOR
    static ControllerSensitivityInterface()
    {
        Initialize();
    }
#endif

    [RuntimeInitializeOnLoadMethod]
    static void Initialize()
    {
        InputSystem.RegisterProcessor<ControllerSensitivityInterface>();
    }

    public override Vector2 Process(Vector2 value, InputControl control)
    {
        //scale value by controller sensitivity
        if (GameSettings.instance != null)
        {
            value *= GameSettings.instance.controllerSensitivity;
        }

        return value;
    }
}
