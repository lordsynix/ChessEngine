using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Die Klasse <c>Board</c> ist für die interne Brettdarstellung des Spielfelds zuständig. 
/// Verwaltet alle wichtigen Informationen über das Schachspiel.
/// </summary>
public class Board
{
    public static Board instance;

    private static int[] Square64;
    private static int[] Square120;

    public static bool WhiteToMove = true;
    public static int EnPassantSquare = -1;

    #region SETTER AND GETTER

    /// <summary>
    /// Die Methode <c>GetSquare64</c> gibt einen Square Array zurück, speichert Piece.Type 
    /// und Piece.Color als int-Wert für jedes Feld in der 8x8-Darstellung.
    /// </summary>
    /// <returns>Gibt die 8x8-Darstellung des Spielfelds als int[64] zurück.</returns>
    public int[] GetSquare64()
    {
        return Square64;
    }

    public void SetSquare64(int[] s)
    {
        if (s.Length != 64)
        {
            Debug.LogError("The Value of Board.Square must have 64 Int elements");
            return;
        }
        Square64 = s;
        Get120From64();
    }

    /// <summary>
    /// Die Methode <c>GetSquare120</c> gibt einen Square Array zurück, speichert Piece.Type 
    /// und Piece.Color als int-Wert für jedes Feld in der 12x10-Darstellung.
    /// </summary>
    /// <returns>Gibt die 12x10-Darstellung des Spielfelds als int[120] zurück.</returns>
    public int[] GetSquare120()
    {
        return Square120;
    }

    public void SetSquare120(int[] s)
    {
        if (s.Length != 120)
        {
            Debug.LogError("The Value of Board.Square must have 120 Int elements");
            return;
        }
        Square120 = s;
        Get64From120();
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

    public int GetEnPassantSquare()
    {
        return EnPassantSquare;
    }

    public void SetEnPassantSquare(int e)
    {
        EnPassantSquare = e;
    }

    #endregion

    /// <summary>
    /// Die Methode <c>Initialize</c> wird ausgeführt, 
    /// wenn eine neue Instanz der Klasse geschaffen wird.
    /// </summary>
    public void Initialize()
    {
        instance = this;

        Square64 = new int[64];
        Square120 = new int[120];
    }

    /// <summary>
    /// Die Methode <c>Get120From64</c> verändert, ausgehend von der 8x8-Darstellung 
    /// die Werte für die 12x10-Darstellung.
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
            
            Square120[j] = Square64[i];
            j++;
        }
    }

    /// <summary>
    /// Die Methode <c>Get64From120</c> verändert, ausgehend von der 12x10-Darstellung 
    /// die Werte für die 8x8-Darstellung.
    /// </summary>
    public void Get64From120()
    {
        Square64 = new int[64];

        int curSq = 0;

        foreach (int sq in Square120)
        {
            if (sq != -1)
            {
                Square64[curSq] = sq;
                curSq++;
            }
        }
    }

    public int[] Square64From120()
    {
        
        Square64 = new int[64];

        int curSq = 0;

        foreach (int sq in Square120)
        {
            if (sq != -1)
            {
                Square64[curSq] = sq;
                curSq++;
            }
        }
        return Square64;
    }

    /// <summary>
    /// Die Methode <c>Convert64To120</c> gibt, ausgehend eines Index der 8x8-Darstellung 
    /// den entsprechenden Index in der 12x10-Darstellung zurück.
    /// </summary>
    /// <returns>Gibt den Index der 12x10-Darstellung zurück.</returns>
    public int ConvertIndex64To120(int index)
    {
        int file = index % 8;
        int rank = (index - file) / 8;
        return 21 + rank * 10 + file;
    }

    public int ConvertIndex120To64(int index)
    {
        int file = (index % 10) - 1;
        int rank = ((index - (index % 10)) / 10) - 2;
        return rank * 8 + file;
    }

}
