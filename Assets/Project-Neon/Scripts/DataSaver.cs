using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public static class DataSaver
{
    static string path = null;
    static string GetPath()
    {
        if (path == null)path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        return path;
    }

    static string fileName = "Project Neon Data.csv";
    static string GetFileName() => fileName;

    static StreamWriter sw = null;

    static string GetFileNameAndPath() => GetPath() + "\\" + fileName;

    public static void WriteData(string roomCode, string displayName, string actionName, float timeStamp)
    {
        try
        {
            if (sw == null) sw = new StreamWriter(GetFileNameAndPath(), true);

            sw.WriteLine(roomCode + "," + actionName + "," + displayName + "," + Mathf.RoundToInt(timeStamp).ToString());
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    public static void WriteGameResult(string roomCode, List<PlayerState> sortedPlayers)
    {
        try
        {
            if (sw == null) sw = new StreamWriter(GetFileNameAndPath(), true);

            string result = roomCode + ",match result,";
            for(int i = 0; i < sortedPlayers.Count; i++)
            {
                result += sortedPlayers[i].GetDisplayName() + "," + sortedPlayers[i].GetBounty();
            }

            sw.WriteLine();
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    } 

    public static void Shutdown()
    {
        if (sw != null)
        {
            sw.Flush();
            sw.Close();
            sw = null;
        }
    }
}
