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
        var squareColor = (isLightSquare) ? lightCol : darkCol;

        // Instantiates a square by the given position as a child of the board panel.
        // The Squares position has to be subtracted with the half of the width and negative
        // half of the height of the parents transform to be correctly displayed.
        GameObject newSquare = Instantiate(squarePrefab, transform);
        
        RectTransform rt_parent = (RectTransform)transform;
        newSquare.transform.localPosition = position - new Vector2(rt_parent.rect.width / 2, -rt_parent.rect.height / 2);
        newSquare.GetComponent<Image>().color = squareColor;
        squareNumber++;
        newSquare.name = "Square " + squareNumber;

        squaresGO.Add(newSquare);
    }

    public void GeneratePieces(int[] squares)
    {
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