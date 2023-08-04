using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using static MoveGenerator;

/// <summary>
/// Die Klasse <c>GameManager</c> ist fuer die Verwaltung des Spielablaufs zustaendig.
/// Initiiert alle anderen Klassen und dient als Fundament des Programms.
/// </summary>
public class GameManager : MonoBehaviour
{
    public string[] puzzlesData;
    public GameObject gameTreeRoot;
    public GameObject movePrefab;

    private const string startFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq"; // Ausgangsposition als FEN-String
    // private const string testFEN = "r1bk3r/p2pBpNp/n4n2/1p1NP2P/6P1/3P4/P1P1K3/q5b1 b Qk";
    // private const string testFEN2 = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w -";

    private List<int> usedPuzzlesId = new();

    public static GameManager instance;

    [Header("Windows")]
    public GameObject debugWindow;
    public GameObject promotionWindow;
    public GameObject boardWindow;
    public GameObject historyWindow;
    public GameObject checkMateWindow;

    [Header("Board")]
    public GameObject moveInformationPrefab;
    public GameObject moveInformationHolder;
    public Text sideToMove;
    public Text movesCounter;
    public Text responsesCounter;

    [Header("Debug Tools")]
    public GameObject squareInformationPrefab;
    public GameObject squareInformationHolder;
    public InputField fenInputField;
    public Text debugSideToMove;
    public Text castlePermissions;
    public Text enPassantSquare;

    [HideInInspector] public int latestSlotNum;
    [HideInInspector] public GameObject startSquare;

    public bool DebugMode { get; private set; }
    
    private int[] square64 = null;
    private int[] square120 = null;

    private List<GameObject> highlightedMoves;
    private List<Move> possibleMoves;

    private Move curMove;
    private readonly Dictionary<char, int> pieceTypeFromSymbol = new()
    {
        ['k'] = Piece.KING,
        ['p'] = Piece.PAWN,
        ['n'] = Piece.KNIGHT,
        ['b'] = Piece.BISHOP,
        ['r'] = Piece.ROOK,
        ['q'] = Piece.QUEEN
    };

    #region ESSENTIALS

    private void Awake()
    {
        instance = this;

        //Board.SetGameMode(Board.Mode.Testing);
    }

    private void Start()
    {
        Board.Initialize();
        square120 = Board.GetSquare120();

        ResetHighlightedMoves();

        if (SceneManager.GetActiveScene().name == "ChessBoard") LoadFenPosition(startFEN);
        else if (SceneManager.GetActiveScene().name == "Puzzles") LoadFenPosition(GetPuzzleFen());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) MainMenu();
    }

    #endregion

    #region GETTER AND SETTER

    public void ResetHighlightedMoves()
    {
        highlightedMoves = new();
    }

    public List<Move> GetPossibleMoves()
    {
        return possibleMoves;
    }

    public void SetPossibleMoves()
    {
        Position currentPosition = Engine.Search();

        if (currentPosition.GameOver) return;

        possibleMoves = currentPosition.GetPossibleMoves();

        // Ueberprueft, ob die Engine den naechsten Zug macht.
        if (Board.GetGameMode() == Board.Mode.HumanComputer)
        {
            if (Board.GetPlayerColor() != (Board.GetWhiteToMove() ? Piece.WHITE : Piece.BLACK) && !currentPosition.GameOver)
            {
                StartCoroutine(MakeEngineMove(currentPosition.bestMove));
            }
            else
            {
                if (DebugMode) UpdateGameTree(currentPosition);
            }
        }
        else
        {
            if (DebugMode) UpdateGameTree(currentPosition);
        }
    }

    #endregion

    #region FEN

    public void LoadFenPosition(string fen)
    {
        try
        {
            string fenBoard = fen.Split(' ')[0];
            string fenToMove = fen.Split(' ')[1];
            string fenCastle = fen.Split(' ')[2];
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
                        int pieceColor = char.IsUpper(symbol) ? Piece.WHITE : Piece.BLACK;
                        int pieceType = pieceTypeFromSymbol[char.ToLower(symbol)];
                        square64[rank * 8 + file] = pieceColor | pieceType;
                        file++;
                    }
                }
            }
            Board.SetSquare64(square64);
            BoardGeneration.instance.GeneratePieces(square64);

            // Definiert den Spieler, welcher als nächstes Spielen darf.
            if (fenToMove == "w") Board.SetWhiteToMove(true);
            else if (fenToMove == "b") Board.SetWhiteToMove(false);
            else Debug.LogWarning("Please enter a valid FEN-String");

            // Verarbeitet die Rochaderechte der Position.
            Board.SetCastlePermissions(fenCastle);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            fenInputField.text = "Invalid position";
            Error.instance.OnError();
            LoadFenPosition(startFEN);
        }

        SetPossibleMoves();
    }

    public void StoreFenPosition()
    {
        square64 = Board.GetSquare64();

        int emptySquares = 0;
        int sq = -1;
        string position = "";

        foreach (int square in square64)
        {
            sq++;

            if (sq != 0 && sq % 8 == 0)
            {
                if (emptySquares != 0)
                {
                    position += emptySquares.ToString();
                    emptySquares = 0;
                }
                position += '/';
            }

            if (square == 0)
            {
                emptySquares++;
                continue;
            }

            char piece = Piece.CharFromPieceValue(square);
            if (piece != ' ')
            {
                if (emptySquares != 0)
                {
                    position += emptySquares.ToString();
                    emptySquares = 0;
                }
                position += piece;

            }
        }
        if (emptySquares != 0)
        {
            position += emptySquares.ToString();
        }
        position += ' ';

        position += Board.GetWhiteToMove() ? 'w' : 'b';
        position += ' ';

        bool[] castlePermissions = Board.GetCastlePermissions();
        if (castlePermissions[0]) position += 'K';
        if (castlePermissions[1]) position += 'Q';
        if (castlePermissions[2]) position += 'k';
        if (castlePermissions[3]) position += 'q';
    }

    #endregion

    #region BUTTONS

    /// <summary>
    /// Die Methode <c>OnGeneratePosition</c> übermittelt den eingegebenen 
    /// FEN-String und generiert die grafische Repräsentierung der Position.
    /// </summary>
    public void OnGeneratePosition()
    {
        if (fenInputField.text == "") // TODO: wenn das fenInputField null ist, wird hier eine exception geworfen werden.
        // es waere vielleicht sinnvoller, das if von zeile 131 vor diesem if einzufuegen...?
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
        DebugMode = !DebugMode; // Mehrfaches Klicken fuehrt zum Ausschalten des Debug Modus.

        if (DebugMode) ActivateDebugMode();
        else DeactivateDebugMode();
    }
    
    /// <summary>
    /// Die Methode <c>MainMenu</c> lädt das Startmenu.
    /// </summary>
    public void MainMenu()
    {
        StoreFenPosition();
        SceneManager.LoadScene(0);
    }

    public void PromotionPiece(string strPiece)
    {
        promotionWindow.transform.GetChild(1).gameObject.SetActive(false);
        promotionWindow.transform.GetChild(2).gameObject.SetActive(false);
        promotionWindow.SetActive(false);

        char symbol = strPiece.ToCharArray()[0];

        int pieceType = pieceTypeFromSymbol[char.ToLower(symbol)];
        int pieceColor = char.IsUpper(symbol) ? Piece.WHITE : Piece.BLACK;

        curMove.Promotion = pieceColor | pieceType;

        int targetSq = Board.ConvertIndex120To64(curMove.TargetSquare);
        int sqIndex = (Board.GetPlayerColor() == Piece.WHITE) ? targetSq : 63 - targetSq;

        SquareSlot sqSlot = boardWindow.transform.GetChild(sqIndex).GetComponentInChildren<SquareSlot>();

        sqSlot.Move(sqSlot.curPromotionPointerDrag, curMove);

        DeactivateMoveVisualisation();
    }

    #endregion

    #region VISUALS

    /// <summary>
    /// Die Methode <c>ActivateDebugMode</c> startet den Debug Mode für Entwickler.
    /// </summary>
    public void ActivateDebugMode()
    {
        if (!DebugMode) return;

        square64 = Board.GetSquare64From120();
        List<int[]> piecesList = Board.GetPieceLocation();

        // Loescht die alten Zeilen mit Informationen zu einem Feld
        for (int i = 0; i < squareInformationHolder.transform.childCount; i++)
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

        // Zeigt den Spieler an, welcher als naechstes Spielen kann.
        debugSideToMove.text = "Player to move: <b>" + (Board.GetWhiteToMove() ? "White </b>" : "Black </b>");

        // Listet die Rochaderechte auf.
        string s = "Castle Permissions: <b>";
        bool[] permissions = Board.GetCastlePermissions();
        if (permissions[0]) s += 'K';
        if (permissions[1]) s += 'Q';
        if (permissions[2]) s += 'k';
        if (permissions[3]) s += 'q';
        castlePermissions.text = s + "</b>";

        // Zeigt das aktuelle EnPassant Feld an.
        enPassantSquare.text = Board.GetEnPassantSquare().ToString();

        historyWindow.SetActive(false);
        debugWindow.SetActive(true);

        // Visualisiert Informationen ueber ein Feld auf dem Brett.
        List<GameObject> squaresGO = BoardGeneration.instance.squaresGO;
        int k = 0;
        foreach (GameObject go in squaresGO)
        {
            Text[] texts = go.transform.GetChild(1).GetComponentsInChildren<Text>();
            texts[0].text = k.ToString();
            texts[1].text = square64[k].ToString();
            texts[2].text = Board.ConvertIndex64To120(k).ToString();
            texts[3].text = 0.ToString();
            go.transform.GetChild(1).gameObject.SetActive(true);
            k++;
        }

        for (int pieceType = Piece.WHITE + 1; pieceType < Piece.BLACK + 7; pieceType++)
        {
            if (piecesList[pieceType].Length == 0) continue;

            foreach (int startSquare in piecesList[pieceType])
            {
                if (startSquare == 0) continue;
                Text[] texts = BoardGeneration.instance.squaresGO[Board.ConvertIndex120To64(startSquare)].transform.GetChild(1).
                    GetComponentsInChildren<Text>();
                texts[3].text = Piece.CharFromPieceValue(pieceType).ToString();
            }
        }
    }

    public void DeactivateDebugMode()
    {
        // Deaktiviert Details Scrollbar
        debugWindow.SetActive(false);
        historyWindow.SetActive(true);

        // Deaktiviert die grafischen Informationen fuer ein Feld
        List<GameObject> squaresGO = BoardGeneration.instance.squaresGO;
        foreach (GameObject go in squaresGO)
        {
            go.transform.GetChild(1).gameObject.SetActive(false);
        }
    }

    public void UpdateMoveHistory(Move move)
    {
        square120 = Board.GetSquare120();

        // Iniitiert eine neue Zeilen mit Informationen zu einem Zug
        GameObject newMoveInformation = Instantiate(moveInformationPrefab, moveInformationHolder.transform);

        Text[] prefabTexts = newMoveInformation.GetComponentsInChildren<Text>();
        Image piece = newMoveInformation.transform.GetChild(1).GetComponent<Image>();
        int moveCount = Board.GetMoveCount();

        prefabTexts[0].text = moveCount.ToString();
        prefabTexts[1].text = Board.DesignateSquare(move.StartSquare);
        prefabTexts[2].text = Board.DesignateSquare(move.TargetSquare);

        piece.sprite = BoardGeneration.instance.pieces[square120[move.TargetSquare]];

        if (moveCount > 7) (moveInformationHolder.transform as RectTransform).pivot = new Vector2(0.5f, 0);

        sideToMove.text = Board.GetWhiteToMove() ? "Side to move: <b>White</b>" : "Side to move: <b>Black</b>";
    }

    /// <summary>
    /// Die Methode <c>ActivateMoveVisualization</c> aktiviert die grafische 
    /// Visualisierung aller verfügbaren Züge für die ausgewählte Figur.
    /// </summary>
    public void ActivateMoveVisualization(int startSquare)
    {
        if (possibleMoves.Count == 0 || possibleMoves == null) Debug.LogWarning("No possible moves assigned");

        foreach (Move move in possibleMoves)
        {
            if (move.StartSquare != startSquare) continue;

            // Aktiviert die grafische Visualisierung der moeglichen Felder
            int targetSquare = move.TargetSquare;
            GameObject targetSquareGO = BoardGeneration.instance.squaresGO [Board.ConvertIndex120To64(targetSquare)];

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
        // Deaktiviert die grafische Visualisierung der moeglichen Felder
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
        bool whiteToMove = Board.GetWhiteToMove();

        promotionWindow.SetActive(true);
        historyWindow.SetActive(false);
        debugWindow.SetActive(false);

        if (whiteToMove) promotionWindow.transform.GetChild(1).gameObject.SetActive(true);
        else promotionWindow.transform.GetChild(2).gameObject.SetActive(true);

        curMove = move;
    }

    public void DeactivatePromotionVisuals()
    {
        promotionWindow.transform.GetChild(1).gameObject.SetActive(false);
        promotionWindow.transform.GetChild(2).gameObject.SetActive(false);

        promotionWindow.SetActive(false);
        if (DebugMode) debugWindow.SetActive(true);
        else historyWindow.SetActive(true);
    }

    public void SetPositionStats(int moves, int responses)
    {
        movesCounter.text = "Possible Moves: " + moves.ToString();
        responsesCounter.text = "Possible Responses: " + responses.ToString();
    }

    public void UpdateGameTree(Position pos)
    {
        if (pos.PossibleMoves.Count != pos.ChildPositions.Count)
            Debug.LogError("Possible Positions can't differ from Possible Moves");

        // Loescht alte Informationen.
        foreach (Transform child in gameTreeRoot.transform)
        {
            Destroy(child.gameObject);
        }

        foreach(Position childPos in pos.ChildPositions)
        {
            int index = pos.ChildPositions.IndexOf(childPos);

            var moveGO = Instantiate(movePrefab, gameTreeRoot.transform);
            moveGO.name = Board.DesignateMove(pos.PossibleMoves[index]) + " - " + Engine.positionEvaluation[index];

            foreach (Move responseMove in childPos.PossibleMoves)
            {
                var childMoveGO = Instantiate(movePrefab, moveGO.transform);
                childMoveGO.name = Board.DesignateMove(responseMove) + " - ";
            }
        }
    }

    #endregion
    
    void ResetBoard()
    {
        // Brett-Variablen zuruecksetzen
        Board.ResetBoard();

        // Setzt die grafische Repraesentierung der Figuren zurueck.
        BoardGeneration.instance.ResetBoard();

        // Setzt die Move History zurueck
        for (int i = 0; i < moveInformationHolder.transform.childCount; i++)
        {
            Destroy(moveInformationHolder.transform.GetChild(i).gameObject);
        }

        // Setzt die Benutzeroberflaeche zurueck.
        DeactivateDebugMode();
        DeactivateMoveVisualisation();
        DeactivatePromotionVisuals();

        ResetHighlightedMoves();
    }

    public void CheckMate()
    {
        int winner = Piece.OpponentColor(Board.GetWhiteToMove() ? Piece.WHITE : Piece.BLACK);

        if (winner == Board.GetPlayerColor())
            checkMateWindow.GetComponentInChildren<Text>().text = "You Win!";
        else checkMateWindow.GetComponentInChildren<Text>().text = "You Lose!";

        checkMateWindow.SetActive(true);
        movesCounter.gameObject.SetActive(false);
        responsesCounter.gameObject.SetActive(false);
    }
    
    public void MakePhysicalMove(GameObject pointerDrag, int targetSlotNum)
    {
        // Simuliert einen Drag and Drop, falls nicht das selbe Feld angewählt wurde.
        if (targetSlotNum != latestSlotNum)
        {
            if (startSquare != null)
            {
                pointerDrag.GetComponent<SquareSlot>().VerifyMove(startSquare);
            }

            startSquare = pointerDrag;
            latestSlotNum = targetSlotNum;
        }
    }

    IEnumerator MakeEngineMove(Move move)
    {
        // TODO use delay time to calculate.
        yield return new WaitForSeconds(1);

        // Weist fuer alle beteiligten Felder die GameObjects zu.
        GameObject startSquare = BoardGeneration.instance.squaresGO[Board.ConvertIndex120To64(move.StartSquare)];
        GameObject targetSquare = BoardGeneration.instance.squaresGO[Board.ConvertIndex120To64(move.TargetSquare)];

        GameObject startSquarePiece = startSquare.transform.GetChild(0).gameObject;

        // Spielt den Zug aus.
        targetSquare.GetComponentInChildren<SquareSlot>().VerifyMove(startSquarePiece, true);
    }

    private string GetPuzzleFen()
    {
        int id = UnityEngine.Random.Range(0, puzzlesData.Length - 1);
        
        string[] randomPuzzle = puzzlesData[id].Split(',');

        string fen = randomPuzzle[0];

        if (usedPuzzlesId.Contains(id)) return GetPuzzleFen();

        usedPuzzlesId.Add(id);

        return fen;
    }

}
