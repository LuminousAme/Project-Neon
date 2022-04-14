using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DebugCommandBase
{
    //based on this video https://youtu.be/VzOEM-4A2OM

    private string id;
    private string description;
    private string format;

    public string GetId() => id;
    public string GetDescription() => description;
    public string GetFormat() => format;

    public DebugCommandBase(string id, string description, string format)
    {
        this.id = id;
        this.description = description;
        this.format = format;
    }
}

public class DebugCommand : DebugCommandBase
{
    private Action command;

    public DebugCommand(string id, string desc, string format, Action command) : base(id, desc, format)
    {
        this.command = command;
    }

    public void Invoke()
    {
        command.Invoke();
    }
}

public class DebugCommand<T> : DebugCommandBase
{
    private Action<T> command;

    public DebugCommand(string id, string desc, string format, Action<T> command) : base(id, desc, format)
    {
        this.command = command;
    }

    public void Invoke(T value)
    {
        command.Invoke(value);
    }
}