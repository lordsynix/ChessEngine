using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

/// <summary>
/// Die Klasse <c>GameManager</c> dient der Kommunkation zwischen dem Front- und Backend der Anwendung. Die Klasse beinhaltet
/// einige Funktionen, um die UI-Elemente zu aktualisieren, was fuer das Verstaendnis der Anwendung nicht relevant ist.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Windows")]
    public GameObject boardWindow;
    public GameObject promotionWindow;
    public GameObject checkMateWindow;
    public GameObject historyWindow;
    public GameObject debugWindow;
    public GameObject diagnosticsWindow;

    [Header("UI Elements")]
    public GameObject moveInformationPrefab;
    public GameObject moveInformationHolder;
    public RectTransform evaluationBar;
    public InputField fenInputField; 

    public enum Mode
    {
        HumanComputer,
        Testing
    }

    // Hier kann der Spielmodus fuers Testen angepasst werden
    public static Mode GameMode = Mode.HumanComputer;

    [HideInInspector] public bool DebugMode = false;

    [HideInInspector] public int LatestSlotNum;
    [HideInInspector] public GameObject StartSquare;

    [HideInInspector] public List<Move> PossibleMoves = new();
    [HideInInspector] public List<GameObject> VisualizedMoves = new();

    private Move curMove;

    // Visualisierung des letzten Zuges von beiden Seiten
    private GameObject lastBlackMoveFromGO;
    private GameObject lastBlackMoveToGO;
    private GameObject lastWhiteMoveFromGO;
    private GameObject lastWhiteMoveToGO;

    // Farben fuer die Visualisierungen der Felder
    private Color32 possibleMoveColor = new Color32(64, 100, 120, 255);
    private Color32 lastBlackMoveColor = new Color32(111, 36, 57, 255);
    private Color32 lastWhiteMoveColor = new Color32(56, 150, 53, 255);

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Bereitet ein neues Spiel vor
        NewGame();

        // Richtet die Evaluation Bar fuer die richtige Seite aus
        evaluationBar.localPosition = Board.GetPlayerColor() == Piece.WHITE ? new(0, -150f) : new(0f, 150f);
    }

    /// <summary>
    /// Die Funktion <c>NewGame</c> bereitet die Anwendung auf ein neues Spiel vor.
    /// </summary>
    private void NewGame(string customFen = null)
    {
        if (!string.IsNullOrEmpty(customFen))
        {
            // Setzt das Brett zurueck
            BoardGeneration.Instance.ResetBoard();
            FENManager.startFEN = customFen;

            // Setzt die Move History zurueck
            for (int i = moveInformationHolder.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(moveInformationHolder.transform.GetChild(i).gameObject);
            }
            (moveInformationHolder.transform as RectTransform).pivot = new Vector2(0.5f, 1);
        }

        // Initialisiert die Klassen
        Log.Initialize();
        Board.Initialize();
        Engine.Initialize();
        Zobrist.Initialize();

        // Laedt eine neue Brettstellung
        FENManager.LoadFenPosition();
        Engine.StartSearch();
    }

    /// <summary>
    /// Die Funktion <c>MakePhysicalMove</c> spielt einen physischen Zug auf der Benutzeroberflaeche.
    /// </summary>
    /// <param name="pointerDrag">UI-Element der gespielten Figur</param>
    /// <param name="targetSlotNum">Index des Zielfeldes in der 12x10-Darstellung</param>
    public void MakePhysicalMove(GameObject pointerDrag, int targetSlotNum)
    {
        // Simuliert einen Drag and Drop, falls nicht das selbe Feld angewählt wurde.
        if (targetSlotNum != LatestSlotNum)
        {
            if (StartSquare != null)
            {
                pointerDrag.GetComponent<SquareSlot>().VerifyMove(StartSquare);
            }

            StartSquare = pointerDrag;
            LatestSlotNum = targetSlotNum;
        }
    }

    /// <summary>
    /// Die Funktion <c>MakeEngineMove</c> wird mit dem bestmoeglichen Zug vom Backend aufgerufen, um den Zug der Engine auszufuehren.
    /// </summary>
    /// <param name="move">Der evaluierte Zug der Engine</param>
    public void MakeEngineMove(Move move)
    {
        StartCoroutine(ExecuteMove(move));
    }

    IEnumerator ExecuteMove(Move move)
    {
        // Wartet 0.5s, damit das Program noch funktioniert :)
        yield return new WaitForSeconds(0.5f);

        // Weist fuer alle beteiligten Felder die GameObjects zu
        GameObject startSquare = BoardGeneration.Instance.squaresGO[move.StartSquare];
        GameObject targetSquare = BoardGeneration.Instance.squaresGO[move.TargetSquare];

        GameObject startSquarePiece = startSquare.transform.GetChild(0).gameObject;

        // Spielt den Zug
        targetSquare.GetComponentInChildren<SquareSlot>().VerifyMove(startSquarePiece, true);
    }

    /// <summary>
    /// Die Funktion <c>VisualizePossibleMoves</c> visualisiert alle moeglichen Zuege fuer eine angeklickte Figur.
    /// </summary>
    /// <param name="startSquare">Das Ursprungsfeld in der 12x10-Darstellung</param>
    public void VisualizePossibleMoves(int startSquare)
    {
        DevisualizePossibleMoves();

        // Aktiviert die grafische Visualisierung der moeglichen Felder
        if (PossibleMoves.Count == 0 || PossibleMoves == null) Debug.LogWarning("No possible moves assigned");

        foreach (Move move in PossibleMoves)
        {
            if (move.StartSquare != Board.ConvertIndex120To64(startSquare)) continue;

            int targetSquare = move.TargetSquare;
            GameObject targetSquareGO = BoardGeneration.Instance.squaresGO[targetSquare];

            // Verhindert, dass eine existierende Visualisierung ueberschrieben wird
            if (!targetSquareGO.transform.GetChild(2).gameObject.activeSelf)
            {
                VisualizedMoves.Add(targetSquareGO);
                targetSquareGO.transform.GetChild(2).gameObject.SetActive(true);
                targetSquareGO.transform.GetChild(2).GetComponent<Image>().color = possibleMoveColor;
            }
        }
    }

    /// <summary>
    /// Die Funktion <c>DevisualizePossibleMoves</c> loescht die Visualisierung der moeglichen Zuege einer angeklickten Figur.
    /// </summary>
    public void DevisualizePossibleMoves()
    {
        // Deaktiviert die grafische Visualisierung der moeglichen Felder
        foreach (GameObject go in VisualizedMoves)
        {
            go.transform.GetChild(2).gameObject.SetActive(false);
        }
        VisualizedMoves = new List<GameObject>();
    }

    /// <summary>
    /// Die Funktion <c>UpdateMoveHistory</c> aktualisiert das UI-Element der gespielten Zuege bei einem Zug.
    /// </summary>
    /// <param name="move">Der gespielte Zug</param>
    public void UpdateMoveHistory(Move move)
    {
        // Iniitiert eine neue Zeilen mit Informationen zu einem Zug
        GameObject newMoveInformation = Instantiate(moveInformationPrefab, moveInformationHolder.transform);

        Text[] prefabTexts = newMoveInformation.GetComponentsInChildren<Text>();
        Image piece = newMoveInformation.transform.GetChild(1).GetComponent<Image>();
        int moveCount = Board.GetMoveCount();

        prefabTexts[0].text = moveCount.ToString();
        prefabTexts[1].text = Board.DesignateSquare(Board.ConvertIndex64To120(move.StartSquare));
        prefabTexts[2].text = Board.DesignateSquare(Board.ConvertIndex64To120(move.TargetSquare));

        piece.sprite = BoardGeneration.Instance.pieces[Board.PieceOnSquare(Board.ConvertIndex64To120(move.TargetSquare))];

        // Setzt den Anker des UI-Elements um, wenn es mehr als 7 Elemente beinhaltet.
        if (moveCount > 7) (moveInformationHolder.transform as RectTransform).pivot = new Vector2(0.5f, 0);

        // Aktualisiert die Visualisierung des letzten Zuges
        UpdateLastMoveVisualization(move);
    }

    /// <summary>
    /// Die Funktion <c>UpdateLastMoveVisualization</c> aktualisiert die Visualisierung der zuletzt gespielten Zuge auf dem Brett.
    /// </summary>
    /// <param name="move">Der gespielte Zug</param>
    private void UpdateLastMoveVisualization(Move move)
    {
        if (Board.GetWhiteToMove())
        {
            // Deaktiviert die letzten Zug-Visualisierungen
            if (lastWhiteMoveFromGO != null) lastWhiteMoveFromGO.SetActive(false);
            if (lastBlackMoveFromGO != null) lastBlackMoveFromGO.SetActive(false);
            if (lastBlackMoveToGO != null) lastBlackMoveToGO.SetActive(false);

            // Weist die neuen Felder zu
            lastBlackMoveFromGO = BoardGeneration.Instance.squaresGO[move.StartSquare].transform.GetChild(2).gameObject;
            lastBlackMoveToGO = BoardGeneration.Instance.squaresGO[move.TargetSquare].transform.GetChild(2).gameObject;

            // Visualisiert das Ursprungsfeld des Zuges
            lastBlackMoveFromGO.GetComponent<Image>().color = lastBlackMoveColor;
            lastBlackMoveFromGO.SetActive(true);

            // Aktualisiert das Zielfeld des Zuges
            lastBlackMoveToGO.GetComponent<Image>().color = lastBlackMoveColor;
            lastBlackMoveToGO.SetActive(true);
        }
        else
        {
            // Deaktiviert die letzten Zug-Visualisierungen
            if (lastBlackMoveFromGO != null) lastBlackMoveFromGO.SetActive(false);
            if (lastWhiteMoveFromGO != null) lastWhiteMoveFromGO.SetActive(false);
            if (lastWhiteMoveToGO != null) lastWhiteMoveToGO.SetActive(false);

            // Weist die neuen Felder zu
            lastWhiteMoveFromGO = BoardGeneration.Instance.squaresGO[move.StartSquare].transform.GetChild(2).gameObject;
            lastWhiteMoveToGO = BoardGeneration.Instance.squaresGO[move.TargetSquare].transform.GetChild(2).gameObject;

            // Visualisiert das Ursprungsfeld des Zuges
            lastWhiteMoveFromGO.GetComponent<Image>().color = lastWhiteMoveColor;
            lastWhiteMoveFromGO.SetActive(true);

            // Aktualisiert das Zielfeld des Zuges
            lastWhiteMoveToGO.GetComponent<Image>().color = lastWhiteMoveColor;
            lastWhiteMoveToGO.SetActive(true);
        }
    }

    /// <summary>
    /// Die Funktion <c>SetEvaluationBar</c> aktualisiert den Wert des UI-Bewertungselement neben dem Spielbrett (Evaluation Bar)
    /// </summary>
    /// <param name="evaluation"></param>
    public void SetEvaluationBar(int evaluation)
    {
        float side = Board.GetPlayerColor() == Piece.WHITE ? 1f : -1f;
        float value = -side * 150f + (side * evaluation / 82f * 18.75f);
        float min = side == 1f ? -300 : 0f;
        float max = side == 1f ? 0 : 300;
        float posY = Mathf.Clamp(value, min, max);
        evaluationBar.localPosition = new(0, posY);
    }

    /// <summary>
    /// Die Funktion <c>ActivatePromotionVisuals</c> oeffnet ein Fenster fuer 
    /// die Nutzenden, wenn ein Bauer die gegnerische Grundreihe erreicht hat.
    /// </summary>
    /// <param name="move">Der gespielte Zug</param>
    public void ActivatePromotionVisuals(Move move)
    {
        bool whiteToMove = Board.GetWhiteToMove();

        promotionWindow.SetActive(true);
        historyWindow.SetActive(false);
        debugWindow.SetActive(false);

        if (whiteToMove) promotionWindow.transform.GetChild(1).gameObject.SetActive(true);
        else promotionWindow.transform.GetChild(2).gameObject.SetActive(true);

        curMove = move;
    }

    /// <summary>
    /// Die Funktion <c>PromotionPiece</c> wird aufgerufen, wenn ein Bauer die letzte gegnerische Reihe 
    /// erreicht und der User eine Figur in der UI ausgewaehlt hat (Springer, Laeufer, Turm oder Dame).
    /// </summary>
    /// <param name="strPiece">Die ausgewaehlte Figur als char - N,n;B,b;R,r;Q,q</param>
    public void PromotionPiece(string strPiece)
    {
        promotionWindow.transform.GetChild(1).gameObject.SetActive(false);
        promotionWindow.transform.GetChild(2).gameObject.SetActive(false);
        promotionWindow.SetActive(false);

        char symbol = strPiece.ToCharArray()[0];

        int pieceType = FENManager.pieceTypeFromSymbol[char.ToLower(symbol)];
        int pieceColor = char.IsUpper(symbol) ? Piece.WHITE : Piece.BLACK;

        Move move = Move.PromotionMoveWithPiece(curMove.StartSquare, curMove.TargetSquare, pieceType);

        if (move == null) Debug.LogWarning("Returned move shouldn't be null");

        int sqIndex = (Board.GetPlayerColor() == Piece.WHITE) ? move.TargetSquare : 63 - move.TargetSquare;

        SquareSlot sqSlot = boardWindow.transform.GetChild(sqIndex).GetComponentInChildren<SquareSlot>();

        sqSlot.MakeMove(sqSlot.curPromotionPointerDrag, move);

        DevisualizePossibleMoves();
    }

    /// <summary>
    /// Die Funktion <c>OnCheckMate</c> wird beim Spielende aufgerufen.
    /// </summary>
    public void OnCheckMate()
    {
        diagnosticsWindow.SetActive(false);
        checkMateWindow.SetActive(true);
        bool isWhite = Board.GetPlayerColor() == Piece.WHITE;

        Text text = checkMateWindow.GetComponentInChildren<Text>();

        if (isWhite && !Board.GetWhiteToMove() || isWhite && Board.GetWhiteToMove())
        {
            text.text = "You won!";
        }
        else
        {
            text.text = "You lost!";
        }
       
    }

    public void SubmitFEN()
    {
        string fen = fenInputField.text;
        
        NewGame(fen);
    }

    public void ToggleDebugMode()
    {
        DebugMode = !DebugMode;

        Diagnostics.Instance.UpdateDebugInformation(DebugMode);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
