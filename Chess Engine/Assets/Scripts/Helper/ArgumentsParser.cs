using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArgumentsParser : MonoBehaviour
{
    public static ArgumentsParser instance;
    public string[] cmdArgs;

    void Awake()
    {
        instance = this;

        cmdArgs = System.Environment.GetCommandLineArgs();

        // Logging
        for (int i = 0; i < cmdArgs.Length; i++)
        {
            Debug.Log("ARG " + i + ": " + cmdArgs[i]);
        }
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
