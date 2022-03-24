using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class ChatManager : MonoBehaviour
{
    [SerializeField] private int maxMessages = 25;

    List<Message> messages = new List<Message>();
    [SerializeField] GameObject messagePrefab, contentTab;
    [SerializeField] TMP_InputField chatInput;
    string thisClientDisplayName;
    bool selectionStatus = false;
    [SerializeField] bool shouldLockCursor = false;

    public static Action onStartType, onStopType;

    private void Start()
    {
        thisClientDisplayName = PlayerPrefs.GetString("DisplayName");
    }

    private void Update()
    {
        if (selectionStatus && chatInput.IsActive() && chatInput.text != "" && Input.GetKeyDown(KeyCode.Return))
        {
            AddMessageToChat(thisClientDisplayName, chatInput.text);
            if (Client.instance != null) Client.instance.SendMessageToOtherPlayers(chatInput.text);
            chatInput.text = "";
            chatInput.ActivateInputField();
            chatInput.Select();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            chatInput.ActivateInputField();
            chatInput.Select();
            ChangeSelectionStatus(true);
            onStartType?.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            chatInput.ReleaseSelection();
            chatInput.DeactivateInputField();
            ChangeSelectionStatus(false);
            onStopType?.Invoke();
        }
    }

    public void ChangeSelectionStatus(bool status)
    {
        selectionStatus = status;

        if(selectionStatus == false)
        {
            chatInput.text = "";
            if(shouldLockCursor)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
        else
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    public void AddMessageToChat(string displayName, string messageContent)
    {
        if (messages.Count >= maxMessages)
        {
            if (messages[0].textObj != null) Destroy(messages[0].textObj.gameObject);
            messages.RemoveAt(0);
        }

        Message newMessage = new Message(displayName, messageContent);

        GameObject newObj = Instantiate(messagePrefab, contentTab.transform);
        newMessage.SetTextObj(newObj.GetComponent<TMP_Text>());

        messages.Add(newMessage);
    }

    public bool GetSelectionStatus()
    {
        return selectionStatus;
    }
}

public class Message
{
    public string displayName;
    public string messageContent;
    public TMP_Text textObj;

    public Message()
    {
        displayName = "";
        messageContent = "";
    }

    public Message(string dn, string content)
    {
        displayName = dn;
        messageContent = content;
    }

    public void SetTextObj(TMP_Text obj)
    {
        if (obj == null) return;

        textObj = obj;
        textObj.text = displayName + ": " + messageContent;
    }
}
