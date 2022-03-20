using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class RebindUI : MonoBehaviour
{
    //based on this video https://youtu.be/TD0R5x0yL0Y

    [SerializeField] InputActionReference inputActionReference; //on the asset, not the script

    [SerializeField] bool excludeMouse = true;

    [Range(0, 10)]
    [SerializeField] private int selectedBinding;

    [SerializeField]
    private InputBinding.DisplayStringOptions displayStringOptions;

    private string actionName;

    [SerializeField] InputBinding binding;
    InputBinding acutalBinding;
    int bindingIndex;

    [SerializeField] TMP_Text controlText;
    [SerializeField] MenuButton button;

    private void OnValidate()
    {
        if (inputActionReference == null) return;

        GetBindingInfo();
        UpdateUI();
    }

    private void OnEnable()
    {
        if(inputActionReference != null)
        {
            GetBindingInfo();
            InputManager.LoadBindingOverride(actionName);
            UpdateUI();
        }

        InputManager.rebindComplete += CompleteOrCanel;
        InputManager.rebindCancelled += CompleteOrCanel;
    }

    private void OnDisable()
    {
        InputManager.rebindComplete -= CompleteOrCanel;
        InputManager.rebindCancelled -= CompleteOrCanel;
    }

    void GetBindingInfo()
    {
        if (inputActionReference.action != null) actionName = inputActionReference.action.name;
        if(inputActionReference.action.bindings.Count > selectedBinding)
        {
            acutalBinding = inputActionReference.action.bindings[selectedBinding];
            binding = acutalBinding;
            bindingIndex = selectedBinding;
        }
    }

    void UpdateUI()
    {
        if(controlText != null)
        {
            if (Application.isPlaying)
            {
                //grab info from input manager
                controlText.text = InputManager.GetBindingName(actionName, bindingIndex);
            }
            else
                controlText.text = inputActionReference.action.GetBindingDisplayString(bindingIndex);
        }
    }

    public void Rebind()
    {
        InputManager.StartRebind(actionName, bindingIndex, controlText, excludeMouse);
    }

    private void CompleteOrCanel()
    {
        UpdateUI();
        button.UnClick();
    }
}
