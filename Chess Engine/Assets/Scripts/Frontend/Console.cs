using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Console : MonoBehaviour
{
    public static Console Instance;

    public InputField inputField;
    public Text consoleText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Log.Initialize();

        inputField.onEndEdit.AddListener(SubmitCommand);
        inputField.ActivateInputField();
    }

    public void SubmitCommandChain(string commandChain)
    {
        string[] commands = commandChain.Split(',');

        foreach (string command in commands)
        {
            SubmitCommand(command);
        }
    }

    private void SubmitCommand(string command)
    {
        // Prevent user from submitting empty string.
        if (string.IsNullOrEmpty(command)) return;

        AddToConsole(command, true);

        switch (command.ToLower())
        {
            case "uci":
                OnUCI();
                break;

            case "isready":
                OnIsReady();
                break;
            
            case "ucinewgame":
                OnUCINewGame();
                break;

            case "position startpos":
                OnLoadPosition();
                break;

            case "go":
                OnGo();
                break;

            default:
                if (command.Contains("position"))
                {
                    string toRemove = command.Split(' ')[0];
                    string position = command.Remove(0, toRemove.Length);

                    if (position.ToCharArray()[0] == ' ')
                    {
                        position = position.Remove(0, 1);
                    }
                    OnLoadPosition(position);
                }
                break;
        }

        inputField.text = "";
        inputField.ActivateInputField();
    }

    public void AddToConsole(string text, bool command = false)
    {
        Log.Message(text);

        if (command)
        {
            consoleText.text += $"> {text} \n";
        }
        else
        {
            consoleText.text += $"    {text} \n";
        }
    }

    private void OnUCI()
    {
        EngineManager.InitializeUCI(true);
    }

    private void OnIsReady()
    {
        EngineManager.AwaitingInitialization = true;

        EngineManager.EngineReady();
    }

    private void OnUCINewGame()
    {
        EngineManager.ResetEngine();
    }

    private void OnLoadPosition(string posFEN = "")
    {
        if (!string.IsNullOrEmpty(posFEN))
        {
            FENManager.startFEN = posFEN;
        }

        EngineManager.AwaitingPositionLoad = true;

        EngineManager.EngineReady();
    }

    private void OnGo()
    {
        EngineManager.AwaitingSearch = true;

        EngineManager.EngineReady();
    }
}
