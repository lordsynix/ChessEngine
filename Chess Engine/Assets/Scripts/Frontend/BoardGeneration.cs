using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Die Klasse <c>BoardGeneration</c> ist fuer die Generierung der Benutzeroberflaeche
/// des Spielfelds beim Starten des Programms zustaendig. Beinhaltet Funktionen fuer die
/// Visualisierung der Felder und Figuren auf dem Brett.
/// </summary>
public class BoardGeneration : MonoBehaviour
{
    public static BoardGeneration instance;

    public GameObject squarePrefab;

    public Sprite[] pieces = new Sprite[23];

    [HideInInspector] public List<GameObject> squaresGO;

    private readonly float squareWidth = 37.5f;
    private int sqNum;
    private bool isWhite;

    [Header("Colors")]
    [SerializeField] private Color lightCol = Color.white; // #DDC39C
    [SerializeField] private Color darkCol = Color.black; // #936D4B

    private void Awake()
    {
        instance = this;    
    }

    public void Generate(int[] squares)
    {
        CreateGraphicalBoard();
        GeneratePieces(squares);
    }

    private void CreateGraphicalBoard()
    {
        isWhite = Board.GetWhiteToMove();
        sqNum = isWhite ? 0 : 63;

        // Erstellt das Muster des Schachbretts fuer alle 64 Felder
        // vgl. mit https://www.youtube.com/watch?v=U4ogK0MIzqk&t=27s 0:27s
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                bool isLightSquare = (rank + file) % 2 == 0;
                Vector2 position = new(file * squareWidth + squareWidth / 2, -(rank * squareWidth + squareWidth / 2));

                GenerateSquare(position, isLightSquare);
            }
        }
    }

    private void GenerateSquare (Vector2 position, bool isLightSquare)
    {
        // Initiiert ein Feld an der angegebenen Position als child des Board-Panel.
        // Die Feldposition muss mit der Haelfte der Breite und negativen Haelfte der
        // Hoehe des parent transform subtrahiert werden, um korrekt abgebildet zu werden.
        GameObject newSquare = Instantiate(squarePrefab, transform);
        
        RectTransform rtParent = (RectTransform)transform;
        newSquare.transform.localPosition = position - new Vector2(rtParent.rect.width / 2, -rtParent.rect.height / 2);

        // Setzt die Farbe fuer das Feld.
        var squareColor = isLightSquare ? lightCol : darkCol;
        newSquare.GetComponent<Image>().color = squareColor;

        // Definiert den Index fuer das Feld.
        int sq120 = Board.ConvertIndex64To120(sqNum);
        Variables.Object(newSquare.transform.GetChild(0)).Set("SquareNum", sq120);
        sqNum += isWhite ? 1 : -1;
        
        // Speichert das Feld als GameObject ab.
        squaresGO.Add(newSquare);
    }

    /// <summary>
    /// Die Methode <c>GeneratePieces</c> weist jedem Feld 
    /// aufgrund seines int-Wertes eine Figur zu.
    /// </summary>
    public void GeneratePieces(int[] squares)
    {
        if (!isWhite) squaresGO.Reverse();

        for (int i = 0; i < squaresGO.Count; i++)
        {
            GameObject go = squaresGO[i].transform.GetChild(0).gameObject;
            if (pieces[squares[i]] != null)
            {
                go.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            }
            go.GetComponent<Image>().sprite = pieces[squares[i]];
        }
        Log.Message($"Successfully generated pieces");
        Log.Message($"--------------------------------");
    }

    /// <summary>
    /// Die Methode <c>ResetBoard</c> setzt die grafische Repraesentierung der Figuren zurueck
    /// </summary>
    public void ResetBoard()
    {
        foreach (GameObject go in squaresGO)
        {
            Image piece = go.transform.GetChild(0).GetComponent<Image>();
            piece.color = new Color32(255, 255, 255, 0);
            piece.sprite = null;
        }
    }
}