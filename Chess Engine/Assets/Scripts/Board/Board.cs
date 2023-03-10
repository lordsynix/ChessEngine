using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board
{
    public static Board instance;

    public static int[] Square;
    public static int[] Square120;

    public static bool WhiteToMove = true;

    #region SETTER AND GETTER

    /// <summary>
    /// Die Methode <c>GetSquare</c> gibt einen Square Array zurück, speichert Piece.Type 
    /// und Piece.Color als int-Wert für jedes Feld.
    /// </summary>
    /// <returns>Gibt die 8x8-Darstellung des Spielfelds als int[64] zurück.</returns>
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

    /// <summary>
    /// Die Methode <c>GetWhiteToMove</c> verwaltet den Spieler, der als nächstes spielen kann.
    /// </summary>
    /// <returns>Gibt einen bool zurück, ob weiss am Zug ist.</returns>
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

    /// <summary>
    /// Die Methode <c>Get120From64</c> gibt, ausgehend von der 8x8-Darstellung 
    /// die Werte für die 12x10-Darstellung zurück.
    /// </summary>
    public void Get120From64()
    {
        Square120 = new int[120];

        // Setzt alle Felder auf den Wert -1
        for (int i = 0; i < Square120.Length; i++)
        {
            Square120[i] = -1;
        }

        // Liest die Werte der 8x8-Darstellung für die 12x10-Darstellung
        int j = 19;
        for (int i = 0; i < 64; i++)
        {
            if (i % 8 == 0)
            {
                j += 2;
                Square120[j] = -1;
            }
            
            Square120[j] = Square[i];
            j++;
        }

        // TODO Debug Funktion entfernen
        int k = 0;
        foreach (int sq in Square120)
        {
            Debug.Log(k + " : " + sq);
            k++;
        }
    }

}
