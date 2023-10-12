using UnityEngine;
using System.IO;
using System;

public static class Log
{
    private static string logFilePath;

    public static void Initialize()
    {
        // Erstellt einen Pfad für die Log-Datei.
        logFilePath = Application.dataPath + "/Logs/log.txt";

        // Loescht die vorherige Log-Datei, falls vorhanden.
        if (File.Exists(logFilePath))
        {
            File.Delete(logFilePath);
        }

        // Schreibt eine Startmeldung in die Log-Datei.
        WriteToLog("Loaded chess board scene");
    }

    private static void WriteToLog(string message)
    {
        if (string.IsNullOrEmpty(logFilePath))
        {
            Debug.LogWarning("Log file path is null or empty. " + logFilePath);
            return;
        }

        try
        {
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error writing to log file: {e.Message}");
        }
    }

    public static void Message(string message)
    {
        WriteToLog(message);
    }
}