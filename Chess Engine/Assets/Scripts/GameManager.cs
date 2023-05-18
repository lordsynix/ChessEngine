using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static MoveGenerator;

/// <summary>
/// Die Klasse <c>GameManager</c> ist f�r die Verwaltung des Spielablaufs zust�ndig.
/// Initiiert alle anderen Klassen und dient als Fundament des Programms.
/// </summary>
public class GameManager : MonoBehaviour
{
    public const string startFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w";
    // public const string testFEN = "r1bk3r/p2pBpNp/n4n2/1p1NP2P/6P1/3P4/P1P1K3/q5b1 b";

    public static GameManager instance;
    public Board board = new();

    [Header("Windows")]
    public GameObject details;
    public GameObject promotion;
    public GameObject boardContainer;

    [Header("Debug Tools")]
    public GameObject squareInformationPrefab;
    public GameObject squareInformationHolder;
    public InputField fenInputField;
    public Text sideToMove;
    
    [HideInInspector] public bool debugMode = false;
    
    [HideInInspector] public int[] square64 = null;
    [HideInInspector] public int[] square120 = null;

    [HideInInspector] public List<GameObject> highlightedMoves;
    [HideInInspector] public List<Move> possibleMoves;

    private Move curMove;
    private Dictionary<char, int> pieceTypeFromSymbol = new()
    {
        ['k'] = Piece.KING,
        ['p'] = Piece.PAWN,
        ['n'] = Piece.KNIGHT,
        ['b'] = Piece.BISHOP,
        ['r'] = Piece.ROOK,
        ['q'] = Piece.QUEEN
    };

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        board.Initialize();
        square120 = board.GetSquare120();

        LoadFenPosition(startFEN);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) MainMenu();
    }

    // FEN String
    public void LoadFenPosition(string fen)
    {
        try
        {
            string fenBoard = fen.Split(' ')[0];
            string fenToMove = fen.Split(' ')[1];
            int file = 0, rank = 0;
            square64 = new int[64];

            // Weist der internen Brettdarstellung (8x8-Darstellung) die int-Werte der Position zu.
            foreach (char symbol in fenBoard)
            {
                if (symbol == '/')
                {
                    file = 0;
                    rank++;
                }
                else
                {
                    if (char.IsDigit(symbol))
                    {
                        file += (int)char.GetNumericValue(symbol);
                    }
                    else
                    {
                        int pieceColor = (char.IsUpper(symbol)) ? Piece.WHITE : Piece.BLACK;
                        int pieceType = pieceTypeFromSymbol[char.ToLower(symbol)];
                        square64[rank * 8 + file] = pieceColor | pieceType;
                        file++;
                    }
                }
            }
            board.SetSquare64(square64);
            BoardGeneration.instance.GeneratePieces(square64);

            // Definiert den Spieler, welcher als nächstes Spielen darf.
            if (fenToMove == "w") board.SetWhiteToMove(true);
            else if (fenToMove == "b") board.SetWhiteToMove(false);
            else UnityEngine.Debug.LogWarning("Please enter a valid FEN-String");
        }
        catch
        {
            // TODO Send Exception
            fenInputField.text = "Invalid position";
            LoadFenPosition(startFEN);
        }
    }

    #region Buttons

    /// <summary>
    /// Die Methode <c>OnGeneratePosition</c> übermittelt den eingegebenen 
    /// FEN-String und generiert die grafische Repräsentierung der Position.
    /// </summary>
    public void OnGeneratePosition()
    {
        if (fenInputField.text == "")
        {
            UnityEngine.Debug.LogWarning("Please enter a valid FEN-String");
            return;
        }
        if (fenInputField != null)
        {
            ResetBoard();
            LoadFenPosition(fenInputField.text);
            fenInputField.text = "";
        }
    }

    /// <summary>
    /// Die Methode <c>DebugButton</c> startet den Debug Mode für eine Position.
    /// </summary>
    public void DebugButton()
    {
        debugMode = !debugMode;
        if (debugMode) Debug();
        else ExitDebug();
    }
    
    /// <summary>
    /// Die Methode <c>MainMenu</c> lädt das Startmenu.
    /// </summary>
    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }

    #endregion
    
    void ResetBoard()
    {
        // Brett-Variablen zur�cksetzten
        board.ResetBoard();

        // Setzt die grafische Repr�sentierung der Figuren zur�ck.
        BoardGeneration.instance.ResetBoard();
    }

    /// <summary>
    /// Die Methode <c>Debug</c> startet den Debug Mode für Entwickler.
    /// </summary>
    public void Debug()
    {
        if (!debugMode) return;
                
        square64 = board.Square64From120();
        
        // L�scht die alten Zeilen mit Informationen zu einem Feld
        for(int i = 0; i < squareInformationHolder.transform.childCount; i++)
        {
            Destroy(squareInformationHolder.transform.GetChild(i).gameObject);
        }

        // Iniitiert neue Zeilen mit Informationen zu einem Feld
        int j = 0;
        foreach (int sq in square64)
        {
            GameObject newSquareInformation = Instantiate(squareInformationPrefab,
                                              squareInformationHolder.transform);

            Text[] prefabTexts = newSquareInformation.GetComponentsInChildren<Text>();
            GameObject piece = newSquareInformation.transform.GetChild(2).gameObject;
            Image pieceImage = piece.GetComponent<Image>();
            
            newSquareInformation.transform.SetParent(squareInformationHolder.transform);
            prefabTexts[0].text = j.ToString();
            prefabTexts[1].text = sq.ToString();

            j++;

            // Leeres Feld
            if (sq == 0)
            {
                pieceImage.sprite = null;
                pieceImage.color = new Color32(0, 0, 0, 0);
            }
            else
            {
                pieceImage.sprite = BoardGeneration.instance.pieces[sq];
                pieceImage.color = new Color32(255, 255, 255, 255);
            }
        }

        // Definiert den Spieler, welcher als n�chstes Spielen kann
        sideToMove.text = "Player to move: <b>" + (board.GetWhiteToMove() ? "White </b>" : "Black </b>");

        details.SetActive(true);

        // Visualisiert Informationen �ber ein Feld auf dem Brett
        List<GameObject> squaresGO = BoardGeneration.instance.squaresGO;
        int k = 0;
        foreach (GameObject go in squaresGO)
        {
            Text[] texts = go.transform.GetChild(1).GetComponentsInChildren<Text>();
            texts[0].text = k.ToString();
            texts[1].text = square64[k].ToString();
            go.transform.GetChild(1).gameObject.SetActive(true);
            k++;
        }
    }

    public void ExitDebug()
    {
        // Deaktiviert Details Scrollbar
        details.SetActive(false);

        // Deaktiviert die grafischen Informationen f�r ein Feld
        List<GameObject> squaresGO = BoardGeneration.instance.squaresGO;
        foreach (GameObject go in squaresGO)
        {
            go.transform.GetChild(1).gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Die Methode <c>ActivateMoveVisualization</c> aktiviert die grafische 
    /// Visualisierung aller verfügbaren Züge für die ausgewählte Figur.
    /// </summary>
    public void ActivateMoveVisualization(List<Move> moves)
    {
        possibleMoves = moves;

        foreach (Move move in moves)
        {
            // Aktiviert die grafische Visualisierung der m�glichen Felder
            int targetSquare = move.TargetSquare;
            GameObject targetSquareGO = BoardGeneration.instance.squaresGO
                [Board.instance.ConvertIndex120To64(targetSquare)];

            highlightedMoves.Add(targetSquareGO);
            targetSquareGO.transform.GetChild(2).gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Die Methode <c>DeactivateMoveVisualization</c> deaktiviert die grafische 
    /// Visualisierung aller verfügbaren Züge für die losgelassene Figur.
    /// </summary>
    public void DeactivateMoveVisualisation()
    {
        // Deaktiviert die grafische Visualisierung der m�glichen Felder
        foreach (GameObject go in highlightedMoves)
        {
            go.transform.GetChild(2).gameObject.SetActive(false);
        }
        highlightedMoves = new List<GameObject>();
    }

    /// <summary>
    /// Öffnet ein Menü, im dem die Figur ausgewählt werden kann, 
    /// in die sich der Bauer in der letzten Reihe verwandelt.
    /// </summary>
    /// <param name="move">Den Zug, bei dem sich der Bauer verwandelt.</param>
    public void ActivatePromotionVisuals(Move move)
    {
        bool whiteToMove = board.GetWhiteToMove();

        promotion.SetActive(true);
         
        if (whiteToMove) promotion.transform.GetChild(1).gameObject.SetActive(true);
        else promotion.transform.GetChild(2).gameObject.SetActive(true);

        curMove = move;
    }

    public void DeactivatePromotionVisuals()
    {
        promotion.transform.GetChild(1).gameObject.SetActive(false);
        promotion.transform.GetChild(2).gameObject.SetActive(false);

        promotion.SetActive(false);
    }

    public void PromotionPiece(string strPiece)
    {
        promotion.transform.GetChild(1).gameObject.SetActive(false);
        promotion.transform.GetChild(2).gameObject.SetActive(false);
        promotion.SetActive(false);

        char symbol = strPiece.ToCharArray()[0];

        int pieceType = pieceTypeFromSymbol[char.ToLower(symbol)];
        int pieceColor = char.IsUpper(symbol) ? Piece.WHITE : Piece.BLACK;

        curMove.Promotion = pieceColor | pieceType;

        SquareSlot sqSlot = boardContainer.transform.GetChild(board.ConvertIndex120To64(curMove.TargetSquare))
                            .GetComponentInChildren<SquareSlot>();

        sqSlot.Move(sqSlot.curPromotionPointerDrag, curMove);
    }
}
