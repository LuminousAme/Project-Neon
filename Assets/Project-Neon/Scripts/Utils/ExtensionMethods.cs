using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

public static class ExtensionMethods
{
    //rect transfrom extenstion from this source: https://orbcreation.com/cgi-bin/orbcreation/page.pl?1099
    public static void SetSize(this RectTransform trans, Vector2 newSize)
    {
        Vector2 oldSize = trans.rect.size;
        Vector2 deltaSize = newSize - oldSize;
        trans.offsetMin = trans.offsetMin - new Vector2(deltaSize.x * trans.pivot.x, deltaSize.y * trans.pivot.y);
        trans.offsetMax = trans.offsetMax + new Vector2(deltaSize.x * (1f - trans.pivot.x), deltaSize.y * (1f - trans.pivot.y));
    }

    public static void AlignUp(this Transform trans, Vector3 newUp)
    {
        Quaternion newRot = Quaternion.FromToRotation(trans.up, newUp) * trans.rotation;
        trans.rotation = newRot;
    }

    public static bool Compare(this Resolution resolution, Resolution other)
    {
        if (resolution.width == other.width && resolution.height == other.height) return true;
        return false;
    }

    //check if a socket is connected
    public static bool IsConnected(this Socket s)
    {
        //taken from here https://stackoverflow.com/questions/2661764/how-to-check-if-a-socket-is-connected-disconnected-in-c/2661876#2661876
        try
        {
            return !((s.Poll(1000, SelectMode.SelectRead) && (s.Available == 0)) || !s.Connected);
        }
        catch (SocketException e)
        {
            if (e.SocketErrorCode == SocketError.WouldBlock) return true;
            else return false;
        }
    }

    //control how long a socket can stay alive
    public static void SetKeepAliveValues(this Socket s, int keepAliveTime, int keepAliveInterval)
    {
        int size = sizeof(uint);
        byte[] values = new byte[size * 3];

        BitConverter.GetBytes((uint)(1)).CopyTo(values, 0);
        BitConverter.GetBytes((uint)keepAliveTime).CopyTo(values, size);
        BitConverter.GetBytes((uint)keepAliveInterval).CopyTo(values, size * 2);

        byte[] outvalues = BitConverter.GetBytes(0);

        s.IOControl(IOControlCode.KeepAliveValues, values, outvalues);
    }
}
