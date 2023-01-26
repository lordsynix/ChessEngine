using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public const string startFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";

    public static GameManager instance;
    public Board board = new();

    [Header("References")]
    public GameObject squareInformationPrefab;
    public GameObject squareInformationHolder;
    public InputField fenInputField;
    
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
        int file = 0, rank = 0;

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
    }

    // UI Buttons
    public void OnGeneratePosition()
    {
        if (fenInputField != null)
        {
            board.SetSquare(new int[64]);
            LoadFenPosition(fenInputField.text);
        }
    }

    public void OnDebug()
    {
        square = board.GetSquare();

        int i = 0;
        foreach (int sq in square)
        {
            i++;
            GameObject newSquareInformation = Instantiate(squareInformationPrefab,
                                              squareInformationHolder.transform);

            Text[] prefabTexts = newSquareInformation.GetComponentsInChildren<Text>();
            
            newSquareInformation.transform.SetParent(squareInformationHolder.transform);
            prefabTexts[0].text = i.ToString();
            prefabTexts[1].text = sq.ToString();
        }
        
    }


}
