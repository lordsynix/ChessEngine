using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board
{
    public static Board instance;

    public static int[] Square;

    public static bool WhiteToMove = true;
    
    #region SETTER AND GETTER

    // Square Array, speichert Piece.Type und Piece.Color als int-Wert für jedes Feld
    public int[] GetSquare()
    {
        return Square;
    }

    public void SetSquare(int[] s)
    {
        if (s.Length != 64)
        {
            Debug.LogError("The Value of Board.Square must have 64 Int elements");
            return;
        }
        Square = s;
    }

    // Verwaltet den Spieler, der als nächstes spielen kann
    public bool GetWhiteToMove()
    {
        return WhiteToMove;
    }

    public void SetWhiteToMove(bool w)
    {
        WhiteToMove = w;
    }

    #endregion

    public void Initialize()
    {
        instance = this;

        Square = new int[64];
    }
}
