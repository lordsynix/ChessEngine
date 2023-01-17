using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board
{
    public static Board instance;

    public static int[] Square;

    public static bool WhiteToMove = true;
    
    #region SETTER AND GETTER

    // Square Array, stores the Piece.Type and Piece.Color as an int for each square
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
