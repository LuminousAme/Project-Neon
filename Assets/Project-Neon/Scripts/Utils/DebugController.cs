using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugController : MonoBehaviour
{
    //based on this video https://youtu.be/VzOEM-4A2OM

    bool showConsole = false;
    bool showHelp = false;
    Vector2 scroll;
    string input;


    public static DebugCommand HELP;
    public static DebugCommand<string> SET_IP_ADDRESS;

    public List<object> commandList;

    private void Awake()
    {
        commandList = new List<object>();

        HELP = new DebugCommand("help", "shows lists of all commands and their data", "help", () => {
            showHelp = true;
            Debug.Log("helping");
        });

        commandList.Add(HELP);

        SET_IP_ADDRESS = new DebugCommand<string>("set_ip_address", "Sets the IP Address of the Server", "set_ip_address <ip>", (newIP) =>
        {
            //use new ip to update the ip address here
        });

        commandList.Add(SET_IP_ADDRESS);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.BackQuote)) {
            showConsole = !showConsole;
            showHelp = false;
        }
        if(showConsole && Input.GetKeyDown(KeyCode.Return))
        {
            HandleInput();
            input = "";
        }
    }

    private void OnGUI()
    {
        if (!showConsole) return;


        float y = 0;


        if (showHelp == true)
        {
            GUI.Box(new Rect(0, y, Screen.width, 100), "");

            Rect viewPort = new Rect(0, 0, Screen.width - 30, 20 * commandList.Count);

            scroll = GUI.BeginScrollView(new Rect(0, y + 5f, Screen.width, 90), scroll, viewPort);

            for(int i = 0; i < commandList.Count; i++)
            {
                DebugCommandBase command = commandList[i] as DebugCommandBase;

                string label = $"{command.GetFormat()} - {command.GetDescription()}";

                Rect labelRect = new Rect(5, 20 * i, viewPort.width - 100, 20);

                GUI.Label(labelRect, label);
            }

            GUI.EndScrollView();

            y += 100;
        }


        GUI.Box(new Rect(0, y, Screen.width, 30), "");
        GUI.backgroundColor = new Color(1, 1, 1, 0.5f);

        input = GUI.TextField(new Rect(10f, y + 5f, Screen.width - 20f, 20f), input);
    }

    void HandleInput()
    {
        string[] props = input.Split(' ');

        for(int i = 0; i < commandList.Count; i++)
        {
            DebugCommandBase commandBase = commandList[i] as DebugCommandBase;

            if(input.Contains(commandBase.GetId()))
            {
                if (commandList[i] as DebugCommand != null)
                {
                    (commandList[i] as DebugCommand).Invoke();
                }
                else if (commandList[i] as DebugCommand<string> != null)
                {
                    (commandList[i] as DebugCommand<string>).Invoke(props[1]);
                }
            }
        }
    }
}
