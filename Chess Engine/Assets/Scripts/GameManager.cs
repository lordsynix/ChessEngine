using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public const string startFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w";
    // public const string testFEN = "r1bk3r/p2pBpNp/n4n2/1p1NP2P/6P1/3P4/P1P1K3/q5b1 b";

    public static GameManager instance;
    public Board board = new();

    [Header("Debug Tools")]
    public GameObject details;
    public GameObject squareInformationPrefab;
    public GameObject squareInformationHolder;
    public InputField fenInputField;
    public Text sideToMove;
    
    [HideInInspector] public int[] square = null;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        board.Initialize();
        square = board.GetSquare();

        LoadFenPosition(startFEN);
    }

    // FEN String
    public void LoadFenPosition(string fen)
    {
        var pieceTypeFromSymbol = new Dictionary<char, int>()
        {
            ['k'] = Piece.King,
            ['p'] = Piece.Pawn,
            ['n'] = Piece.Knight,
            ['b'] = Piece.Bishop,
            ['r'] = Piece.Rook,
            ['q'] = Piece.Queen
        };

        string fenBoard = fen.Split(' ')[0];
        string fenToMove = fen.Split(' ')[1];
        int file = 0, rank = 0;
        square = new int[64];

        // Weist den Figuren in der Position ihre int-Werte zu
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
                    int pieceColor = (char.IsUpper(symbol)) ? Piece.White : Piece.Black;
                    int pieceType = pieceTypeFromSymbol[char.ToLower(symbol)];
                    square[rank * 8 + file] = pieceColor | pieceType;
                    file++;
                }
            }
        }
        board.SetSquare(square);
        BoardGeneration.instance.GeneratePieces(square);

        // Definiert den Spieler, welcher als nächstes Spielen kann
        if (fenToMove == "w")
            board.SetWhiteToMove(true);
        else if (fenToMove == "b")
            board.SetWhiteToMove(false);
        else
            Debug.LogWarning("Please enter a valid FEN-String");
    }

    // UI Buttons
    public void OnGeneratePosition()
    {
        if (fenInputField.text == "")
        {
            Debug.LogWarning("Please enter a valid FEN-String");
            return;
        }
        if (fenInputField != null)
        {
            
            ResetBoard();
            LoadFenPosition(fenInputField.text);
        }
    }

    void ResetBoard()
    {
        // Brett-Variablen zurücksetzten
        board.SetSquare(new int[64]);

        // Grafisches Brett zurücksetzten
        List<GameObject> squaresGO = BoardGeneration.instance.squaresGO;

        foreach (GameObject go in squaresGO)
        {
            Image piece = go.transform.GetChild(0).GetComponent<Image>();
            piece.color = new Color32(255, 255, 255, 0);
            piece.sprite = null;
        }

    }

    public void OnDebug()
    {
        square = board.GetSquare();

        // Löscht die alten Zeilen mit Informationen zu einem Feld
        for(int i = 0; i < squareInformationHolder.transform.childCount; i++)
        {
            Destroy(squareInformationHolder.transform.GetChild(i).gameObject);
        }

        // Iniitiert neue Zeilen mit Informationen zu einem Feld
        int j = 0;
        foreach (int sq in square)
        {
            j++;

            GameObject newSquareInformation = Instantiate(squareInformationPrefab,
                                              squareInformationHolder.transform);

            Text[] prefabTexts = newSquareInformation.GetComponentsInChildren<Text>();
            GameObject piece = newSquareInformation.transform.GetChild(2).gameObject;
            Image pieceImage = piece.GetComponent<Image>();
            
            newSquareInformation.transform.SetParent(squareInformationHolder.transform);
            prefabTexts[0].text = j.ToString();
            prefabTexts[1].text = sq.ToString();

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

        // Definiert den Spieler, welcher als nächstes Spielen kann
        sideToMove.text = "Player to move: <b>" + (board.GetWhiteToMove() ? "White </b>" : "Black </b>");

        details.SetActive(true);

        // Visualisiert Informationen über ein Feld auf dem Brett
        List<GameObject> squaresGO = BoardGeneration.instance.squaresGO;
        int k = 0;
        foreach (GameObject go in squaresGO)
        {
            k++;
            Text[] texts = go.transform.GetChild(1).GetComponentsInChildren<Text>();
            texts[0].text = k.ToString();
            texts[1].text = square[k - 1].ToString();
            go.transform.GetChild(1).gameObject.SetActive(true);
        }
    }

    public void ExitDebug()
    {
        // Deaktiviert Details Scrollbar
        details.SetActive(false);

        // Deaktiviert die grafischen Informationen für ein Feld
        List<GameObject> squaresGO = BoardGeneration.instance.squaresGO;
        foreach (GameObject go in squaresGO)
        {
            go.transform.GetChild(1).gameObject.SetActive(false);
        }
    }

}
