using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

using Debug = UnityEngine.Debug;

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

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        EngineManager.InitializeUCI();
        FENManager.LoadFenPosition();
        Engine.Search();

        // Richtet die Evaluation Bar fuer die richtige Seite aus
        evaluationBar.localPosition = Board.GetPlayerColor() == Piece.WHITE ? new(0, -150f) : new(0f, 150f);
    }

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

    public void MakeEngineMove(Move move)
    {
        StartCoroutine(ExecuteMove(move));
    }

    IEnumerator ExecuteMove(Move move)
    {
        yield return new WaitForSeconds(0.5f);

        // Weist fuer alle beteiligten Felder die GameObjects zu
        GameObject startSquare = BoardGeneration.instance.squaresGO[move.StartSquare];
        GameObject targetSquare = BoardGeneration.instance.squaresGO[move.TargetSquare];

        GameObject startSquarePiece = startSquare.transform.GetChild(0).gameObject;

        // Spielt den Zug
        targetSquare.GetComponentInChildren<SquareSlot>().VerifyMove(startSquarePiece, true);
    }

    public void VisualizePossibleMoves(int startSquare)
    {
        DevisualizePossibleMoves();

        // Aktiviert die grafische Visualisierung der moeglichen Felder
        if (PossibleMoves.Count == 0 || PossibleMoves == null) Debug.LogWarning("No possible moves assigned");

        foreach (Move move in PossibleMoves)
        {
            if (move.StartSquare != Board.ConvertIndex120To64(startSquare)) continue;

            int targetSquare = move.TargetSquare;
            GameObject targetSquareGO = BoardGeneration.instance.squaresGO[targetSquare];

            VisualizedMoves.Add(targetSquareGO);
            targetSquareGO.transform.GetChild(2).gameObject.SetActive(true);
        }
    }

    public void DevisualizePossibleMoves()
    {
        // Deaktiviert die grafische Visualisierung der moeglichen Felder
        foreach (GameObject go in VisualizedMoves)
        {
            go.transform.GetChild(2).gameObject.SetActive(false);
        }
        VisualizedMoves = new List<GameObject>();
    }

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

        piece.sprite = BoardGeneration.instance.pieces[Board.PieceOnSquare(Board.ConvertIndex64To120(move.TargetSquare))];

        if (moveCount > 7) (moveInformationHolder.transform as RectTransform).pivot = new Vector2(0.5f, 0);
    }

    public void SetEvaluationBar(int evaluation)
    {
        float side = Board.GetPlayerColor() == Piece.WHITE ? 1f : -1f;
        float value = -side * 150f + (side * evaluation / 82f * 18.75f);
        float min = side == 1f ? -300 : 0f;
        float max = side == 1f ? 0 : 300;
        float posY = Mathf.Clamp(value, min, max);
        evaluationBar.localPosition = new(0, posY);
    }

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
    /// erreicht und der User eine Figur ausgewaehlt hat (Springer, Laeufer, Turm oder Dame).
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

    public void OnCheckMate()
    {
        diagnosticsWindow.SetActive(false);
        checkMateWindow.SetActive(true);
        bool isWhite = Board.GetPlayerColor() == Piece.WHITE;

        Text text = checkMateWindow.GetComponentInChildren<Text>();

        if (isWhite && !Board.GetWhiteToMove())
        {
            text.text = "You won!";
        }
        else
        {
            text.text = "You lost!";
        }
       
    }

    public void ToggleDebugMode()
    {
        DebugMode = !DebugMode;

        Diagnostics.Instance.UpdateDebugInformation(DebugMode);
    }
}
