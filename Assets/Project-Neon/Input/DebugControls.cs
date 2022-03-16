// GENERATED AUTOMATICALLY FROM 'Assets/Project-Neon/Input/DebugControls.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @DebugControls : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @DebugControls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""DebugControls"",
    ""maps"": [
        {
            ""name"": ""DebugCommands"",
            ""id"": ""515941f3-485c-4db6-8d39-6451d57f67a5"",
            ""actions"": [
                {
                    ""name"": ""ToogleDebugBox"",
                    ""type"": ""Button"",
                    ""id"": ""683101f0-be00-49b8-98a5-037bd77f0eb2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""UseCommand"",
                    ""type"": ""Button"",
                    ""id"": ""9977e52f-65be-4e72-8c85-d7d08a6ca860"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""fab21c1b-81c6-4175-8117-0cda57f6a412"",
                    ""path"": ""<Keyboard>/backquote"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ToogleDebugBox"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""22332274-87e1-4776-ae31-646b3076a05c"",
                    ""path"": ""<Keyboard>/enter"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""UseCommand"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // DebugCommands
        m_DebugCommands = asset.FindActionMap("DebugCommands", throwIfNotFound: true);
        m_DebugCommands_ToogleDebugBox = m_DebugCommands.FindAction("ToogleDebugBox", throwIfNotFound: true);
        m_DebugCommands_UseCommand = m_DebugCommands.FindAction("UseCommand", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // DebugCommands
    private readonly InputActionMap m_DebugCommands;
    private IDebugCommandsActions m_DebugCommandsActionsCallbackInterface;
    private readonly InputAction m_DebugCommands_ToogleDebugBox;
    private readonly InputAction m_DebugCommands_UseCommand;
    public struct DebugCommandsActions
    {
        private @DebugControls m_Wrapper;
        public DebugCommandsActions(@DebugControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @ToogleDebugBox => m_Wrapper.m_DebugCommands_ToogleDebugBox;
        public InputAction @UseCommand => m_Wrapper.m_DebugCommands_UseCommand;
        public InputActionMap Get() { return m_Wrapper.m_DebugCommands; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(DebugCommandsActions set) { return set.Get(); }
        public void SetCallbacks(IDebugCommandsActions instance)
        {
            if (m_Wrapper.m_DebugCommandsActionsCallbackInterface != null)
            {
                @ToogleDebugBox.started -= m_Wrapper.m_DebugCommandsActionsCallbackInterface.OnToogleDebugBox;
                @ToogleDebugBox.performed -= m_Wrapper.m_DebugCommandsActionsCallbackInterface.OnToogleDebugBox;
                @ToogleDebugBox.canceled -= m_Wrapper.m_DebugCommandsActionsCallbackInterface.OnToogleDebugBox;
                @UseCommand.started -= m_Wrapper.m_DebugCommandsActionsCallbackInterface.OnUseCommand;
                @UseCommand.performed -= m_Wrapper.m_DebugCommandsActionsCallbackInterface.OnUseCommand;
                @UseCommand.canceled -= m_Wrapper.m_DebugCommandsActionsCallbackInterface.OnUseCommand;
            }
            m_Wrapper.m_DebugCommandsActionsCallbackInterface = instance;
            if (instance != null)
            {
                @ToogleDebugBox.started += instance.OnToogleDebugBox;
                @ToogleDebugBox.performed += instance.OnToogleDebugBox;
                @ToogleDebugBox.canceled += instance.OnToogleDebugBox;
                @UseCommand.started += instance.OnUseCommand;
                @UseCommand.performed += instance.OnUseCommand;
                @UseCommand.canceled += instance.OnUseCommand;
            }
        }
    }
    public DebugCommandsActions @DebugCommands => new DebugCommandsActions(this);
    public interface IDebugCommandsActions
    {
        void OnToogleDebugBox(InputAction.CallbackContext context);
        void OnUseCommand(InputAction.CallbackContext context);
    }
}
