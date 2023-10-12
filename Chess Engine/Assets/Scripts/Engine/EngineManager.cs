using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class EngineManager
{

    #region UCI - Universal Chess Interface

    [Header("Engine Information")]
    public static readonly string EngineName = "Endura";
    public static readonly string EngineAuthor = "Nico Rst";
    public static readonly string EngineVersion = Application.version;

    // GUI controlled engine states
    public static bool AwaitingInitialization = false;
    public static bool AwaitingPositionLoad = false;
    public static bool AwaitingSearch = false;

    // Engine self controlled states
    private static bool initializationFinished = false;
    private static bool newGamePrepared = false;
    private static bool positionLoaded = false;
    private static bool searchCompleted = false;

    public static void InitializeUCI(bool consoleMode = false)
    {
        if (consoleMode)
        {
            Console.Instance.AddToConsole($"id name {EngineName}");
            Console.Instance.AddToConsole($"id version {EngineVersion}");
            Console.Instance.AddToConsole($"id author {EngineAuthor}");

            Console.Instance.AddToConsole($"uciok");
        }

        Initialize();
    }

    private static void Initialize()
    {
        // Initialize Engine
        Log.Initialize();
        Board.Initialize();
        Engine.Initialize();
        Zobrist.Initialize();

        initializationFinished = true;
        EngineReady();
    }

    public static void ResetEngine()
    {
        // Reset engine

        newGamePrepared = true;
        EngineReady();
    }

    private static void LoadPosition()
    {
        FENManager.LoadFenPosition();

        positionLoaded = true;
        EngineReady();
    }

    private static void StartSearch()
    {
        // Search
        Log.Message("------------------------------------------------");
        Engine.Search();

        searchCompleted = true;
        EngineReady();
    }

    public static void EngineReady()
    {
        if (AwaitingInitialization && initializationFinished)
        {
            AwaitingInitialization = false;
            initializationFinished = false; 
            
            Console.Instance.AddToConsole("readyok");
        }

        if (AwaitingPositionLoad && newGamePrepared)
        {
            AwaitingPositionLoad = false;
            newGamePrepared = false;

            LoadPosition();
        }

        if (AwaitingSearch && positionLoaded)
        {
            AwaitingSearch = false;
            positionLoaded = false;

            StartSearch();
        }

        if (searchCompleted)
        {
            searchCompleted = false;

            Console.Instance.AddToConsole("bestmove 0000");
        }
    }

    #endregion

}
