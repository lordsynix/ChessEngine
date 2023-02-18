using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardGeneration : MonoBehaviour
{
    public GameObject squarePrefab;

    public Sprite[] pieces = new Sprite[23];

    [HideInInspector] public List<GameObject> squaresGO;

    private float squareWidth = 37.5f;
    private int squareNumber = 0;

    [Header("Colors")]
    [SerializeField] private Color lightCol = Color.white; // #DDC39C
    [SerializeField] private Color darkCol = Color.black; // #936D4B

    #region Instance

    public static BoardGeneration instance;

    private void Awake()
    {
        instance = this;    
    }

    #endregion

    private void Start()
    {
        CreateGraphicalBoard();
    }

    void CreateGraphicalBoard()
    {
        // Erstellt das Muster des Schachbretts für alle 64 Felder
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                bool isLightSquare = (rank + file) % 2 == 0;
                Vector2 position = new Vector2(file * squareWidth + squareWidth / 2, 
                                             -(rank * squareWidth + squareWidth / 2));

                GenerateSquare(position, isLightSquare);
            }
        }
        squareNumber = 0;
    }

    void GenerateSquare (Vector2 position, bool isLightSquare)
    {
        // Definiert für jedes Feld eine Farbe
        var squareColor = (isLightSquare) ? lightCol : darkCol;

        // Initiiert ein Feld an der angegebenen Position als child des Board Panel.
        // Die Feldposition muss mit der Hälfte der Breite und negativen Hälfte der
        // Höhe des parent transform subtrahiert werden um korrekt abgebildet zu werden.
        GameObject newSquare = Instantiate(squarePrefab, transform);
        
        RectTransform rtParent = (RectTransform)transform;
        newSquare.transform.localPosition = position - new Vector2(rtParent.rect.width / 2, -rtParent.rect.height / 2);
        newSquare.GetComponent<Image>().color = squareColor;
        squareNumber++;
        newSquare.name = "Square " + squareNumber;

        squaresGO.Add(newSquare);
    }

    public void GeneratePieces(int[] squares)
    {
        // Weist jedem Feld aufgrund seines int-Wertes eine Figur zu
        foreach (GameObject sq in squaresGO)
        {
            GameObject go = sq.transform.GetChild(0).gameObject;
            int index = squaresGO.IndexOf(sq);
            if (pieces[squares[index]] != null)
            {
                go.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            } 
            go.GetComponent<Image>().sprite = pieces[squares[index]];
        }
    }
}