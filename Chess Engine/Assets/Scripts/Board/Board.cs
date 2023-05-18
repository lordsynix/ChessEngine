using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MoveGenerator;

/// <summary>
/// Die Klasse <c>Board</c> ist f�r die interne Brettdarstellung des Spielfelds zust�ndig. 
/// Verwaltet alle wichtigen Informationen �ber das Schachspiel.
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
    /// Die Methode <c>GetSquare64</c> gibt einen Square Array zur�ck, speichert Piece.Type 
    /// und Piece.Color als int-Wert f�r jedes Feld in der 8x8-Darstellung.
    /// </summary>
    /// <returns>Gibt die 8x8-Darstellung des Spielfelds als int[64] zur�ck.</returns>
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
    /// Die Methode <c>GetSquare120</c> gibt einen Square Array zur�ck, speichert Piece.Type 
    /// und Piece.Color als int-Wert f�r jedes Feld in der 12x10-Darstellung.
    /// </summary>
    /// <returns>Gibt die 12x10-Darstellung des Spielfelds als int[120] zur�ck.</returns>
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
    /// Die Methode <c>GetWhiteToMove</c> verwaltet den Spieler, der als n�chstes spielen kann.
    /// </summary>
    /// <returns>Gibt einen bool zur�ck, ob weiss am Zug ist.</returns>
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

    #region BRETTDARSTELLUNG

    /// <summary>
    /// Die Methode <c>Get120From64</c> ver�ndert, ausgehend von der 8x8-Darstellung 
    /// die Werte f�r die 12x10-Darstellung.
    /// </summary>
    public void Get120From64()
    {
        Square120 = new int[120];

        // Setzt alle Felder auf den Wert -1
        for (int i = 0; i < Square120.Length; i++)
        {
            Square120[i] = -1;
        }

        // Liest die Werte der 8x8-Darstellung f�r die 12x10-Darstellung.
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
    /// Die Methode <c>Get64From120</c> ver�ndert, ausgehend von der 12x10-Darstellung 
    /// die Werte f�r die 8x8-Darstellung.
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
    /// Die Methode <c>ConvertIndex64To120</c> gibt, ausgehend eines Index der 
    /// 8x8-Darstellung den entsprechenden Index in der 12x10-Darstellung zur�ck.
    /// </summary>
    /// <returns>Gibt den entsprechenden Index der 12x10-Darstellung zur�ck.</returns>
    public int ConvertIndex64To120(int index)
    {
        int file = index % 8;
        int rank = (index - file) / 8;
        return 21 + rank * 10 + file;
    }

    /// <summary>
    /// Die Methode <c>ConvertIndex120To64</c> gibt, ausgehend eines Index der 12x10-Darstellung 
    /// den entsprechenden Index in der 8x8-Darstellung zur�ck.
    /// </summary>
    /// <returns>Gibt den entsprechenden Index der 8x8-Darstellung zur�ck.</returns>
    public int ConvertIndex120To64(int index)
    {
        int file = (index % 10) - 1;
        int rank = ((index - (index % 10)) / 10) - 2;
        return rank * 8 + file;
    }

    #endregion

    /// <summary>
    /// Die Methode <c>Initialize</c> wird ausgef�hrt, 
    /// wenn eine neue Instanz der Klasse geschaffen wird.
    /// </summary>
    public void Initialize()
    {
        instance = this;

        Square64 = new int[64];
        Square120 = new int[120];
    }

    /// <summary>
    /// Die Methode <c>ResetBoard</c> setzt alle Brett-Variablen auf ihre Default-Werte zurück.
    /// </summary>
    public void ResetBoard()
    {
        SetSquare64(new int[64]);
        SetWhiteToMove(true);
        SetEnPassantSquare(-1);
    }

    public void MakeMove(Move move)
    {
        // Target Square
        Square120[move.TargetSquare] = (move.Promotion == -1) ? Square120[move.StartSquare] : move.Promotion;

        // Start Square
        Square120[move.StartSquare] = 0;

        // En Passant Capture
        if (move.Capture == 2)
        {
            int enPasSq = EnPassantSquare;
            enPasSq += (move.StartSquare - move.TargetSquare > 0) ? 10 : -10;
            Square120[enPasSq] = 0;
        }

        // En Passant Square
        EnPassantSquare = move.EnPassant;

        // Player to move
        WhiteToMove = !WhiteToMove;
    }
}
