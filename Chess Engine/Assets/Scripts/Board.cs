using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board
{
    public static int[] Square;
    
    #region SETTER AND GETTER

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
        Debug.Log("Successfully updated square array");
    }

    #endregion

    public void Initialize()
    {
        Square = new int[64];
    }
}
