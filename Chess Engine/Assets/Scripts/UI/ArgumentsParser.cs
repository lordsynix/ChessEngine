using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Die Klasse <c>ArgumentsParser</c> dient der Ermittlung des Nutzernamen. Die 
/// Klasse hat keinen Einfluss auf die Arbeit und kann vernachlaessigt werden.
/// </summary>
public class ArgumentsParser : MonoBehaviour
{
    public static ArgumentsParser instance;
    public string[] cmdArgs;

    void Awake()
    {
        instance = this;

        cmdArgs = System.Environment.GetCommandLineArgs();
    }

    public string GetArg(string arg)
    {
        for (int i = 0; i < cmdArgs.Length; i++)
        {
            if (cmdArgs[i] == arg)
                return cmdArgs[i + 1];
        }

        return null;
    }

    public bool HasArg(string arg)
    {
        return GetArg(arg) != null;
    }
}
