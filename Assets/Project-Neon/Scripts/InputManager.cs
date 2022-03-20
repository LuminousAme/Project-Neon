using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System;

public static class InputManager
{
    //based on this video https://youtu.be/TD0R5x0yL0Y
    public static PlayerControls controls = new PlayerControls();
    public static Action rebindCancelled;
    public static Action rebindComplete;

    public static void StartRebind(string actionName, int bindingIndex, TMP_Text statusText, bool exludeMouse) {
        InputAction action = controls.asset.FindAction(actionName);
        if (action == null || action.bindings.Count <= bindingIndex)
        {
            Debug.LogWarning("Couldn't find action or binding");
            return;
        }

        if (action.bindings[bindingIndex].isComposite)
        {
            int firstPartIndex = bindingIndex + 1;
            if (firstPartIndex < action.bindings.Count && action.bindings[firstPartIndex].isPartOfComposite)
                Rebind(action, firstPartIndex, statusText, true, exludeMouse);
            else rebindCancelled?.Invoke();
        }
        else Rebind(action, bindingIndex, statusText, false, exludeMouse);
    }

    private static void Rebind(InputAction action, int bindingIndex, TMP_Text statusText, bool allCompositeParts, bool excludeMouse)
    {
        if (action == null || bindingIndex < 0) return;

        statusText.text = $"{action.expectedControlType}";

        action.Disable();

        var rebind = action.PerformInteractiveRebinding(bindingIndex);
        rebind.OnComplete(x =>
        {
            action.Enable();
            x.Dispose();

            if(allCompositeParts)
            {
                int nextBindingIndex = bindingIndex + 1;
                if (nextBindingIndex < action.bindings.Count && action.bindings[nextBindingIndex].isPartOfComposite) 
                    Rebind(action, nextBindingIndex, statusText, allCompositeParts, excludeMouse);
            }

            SaveBindingOverrides(action);
            rebindComplete?.Invoke();
        });

        rebind.OnCancel(x =>
        {
            action.Enable();
            x.Dispose();
            rebindCancelled?.Invoke();
        });

        rebind.WithCancelingThrough("<Keyboard>/escape");

        if (excludeMouse) rebind.WithControlsExcluding("Mouse");

        rebind.Start(); //acutally start the rebinding process
    }

    public static string GetBindingName(string actionName, int bindingIndex)
    {
        InputAction action = controls.asset.FindAction(actionName);
        return action.GetBindingDisplayString(bindingIndex);
    }

    private static void SaveBindingOverrides(InputAction action)
    {
        for(int i = 0; i < action.bindings.Count; i++)
        {
            PlayerPrefs.SetString(action.actionMap + action.name + i, action.bindings[i].overridePath);
        }
    }

    public static void LoadAllBindingOverrides()
    {
        for(int i = 0; i < controls.asset.actionMaps.Count; i++)
        {
            foreach(InputAction action in controls.asset.actionMaps[i].actions)
            {
                LoadBindingOverride(action.name);
            }
        }
    }

    public static void LoadBindingOverride(string actionName)
    {
        InputAction action = controls.asset.FindAction(actionName);

        for(int i = 0; i < action.bindings.Count; i++)
        {
            if (!string.IsNullOrEmpty(PlayerPrefs.GetString(action.actionMap + action.name + i)))
                action.ApplyBindingOverride(i, PlayerPrefs.GetString(action.actionMap + action.name + i));
        }
    }
}