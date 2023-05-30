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

    public static bool WhiteCastleKingside = false;
    public static bool WhiteCastleQueenside = false;
    public static bool BlackCastleKingside = false;
    public static bool BlackCastleQueenside = false;

    public List<int[]> piecesList = new();

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
        Square120 = GetSquare120From64();
        GetPieceLocation();
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
        Square64 = GetSquare64From120();
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

    public bool[] GetCastlePermissions()
    {
        return new bool[]
        {
            WhiteCastleKingside,
            WhiteCastleQueenside,
            BlackCastleKingside,
            BlackCastleQueenside
        };
    }

    public void SetCastlePermissions(string s)
    {
        WhiteCastleKingside = false; WhiteCastleQueenside = false;
        BlackCastleKingside = false; BlackCastleQueenside = false;
        
        foreach (char symbol in s)
        {
            if (char.ToLower(symbol) == 'k')
            {
                if (char.IsUpper(symbol)) WhiteCastleKingside = true;
                else BlackCastleKingside = true;
            }
            else if (char.ToLower(symbol) == 'q')
            {
                if (char.IsUpper(symbol)) WhiteCastleQueenside = true;
                else BlackCastleQueenside = true;
            }
            else Error.instance.OnError();
        }
    }

    #endregion

    #region BRETTDARSTELLUNG

    /// <summary>
    /// Die Methode <c>GetSquare120From64</c> ver�ndert, ausgehend von der 8x8-Darstellung 
    /// die Werte f�r die 12x10-Darstellung.
    /// </summary>
    /// <returns>Gibt den entsprechenden Square120 Array zurück.</returns>
    public int[] GetSquare120From64()
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

        return Square120;
    }

    /// <summary>
    /// Die Methode <c>GetSquare64From120</c> ver�ndert, ausgehend von der 12x10-Darstellung 
    /// die Werte f�r die 8x8-Darstellung.
    /// </summary>
    /// <returns>Gibt den entsprechenden Square64 Array zurück.</returns>
    public int[] GetSquare64From120()
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

    /// <summary>
    /// Die Methode <c>GetPieceLocation</c> speichert, ausgehend von der 8x8-Darstellung
    /// die Position für alle Figuren auf dem Brett in Form eines int-Wertes.
    /// </summary>
    public void GetPieceLocation()
    {
        piecesList = Piece.GeneratePiecesList();

        for (int sq = 0; sq < Square64.Length; sq++)
        {
            int value = Square64[sq];
            if (value == Piece.NONE) continue;

            for (int i = 0; i < piecesList[value].Length; i++)
            {
                if (piecesList[value][i] == 0)
                {
                    piecesList[value][i] = ConvertIndex64To120(sq);
                    break;
                }
            }

        }
    }

    public void DebugPieceLocation()
    {
        for (int i = 0; i < piecesList.Count; i++)
        {
            int[] array = piecesList[i];
            Debug.Log("-------- Piece " + i + " --------");
            for (int k = 0; k < array.Length; k++)
            {
                if (array[k] == 0) continue;
                Debug.Log(k + " value: " + array[k]);
            }
        }
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
        // Update piece position
        int piece = Square120[move.StartSquare];
        int position = Array.IndexOf(piecesList[piece], move.StartSquare);
        piecesList[piece][position] = move.TargetSquare;

        if (move.Capture == 1)
        {
            int enemyPiece = Square120[move.TargetSquare];
            int enemyPosition = Array.IndexOf(piecesList[enemyPiece], move.TargetSquare);
            piecesList[enemyPiece][enemyPosition] = 0;
        }

        if (move.Promotion != -1)
        {
            // Setzt die Position des umwandelnden Bauern zurück.
            piecesList[piece][position] = 0;

            // Setzt die Position für die umwandelte Figur.
            for (int i = 0; i < piecesList[move.Promotion].Length; i++)
            {
                if (piecesList[move.Promotion][i] == 0)
                {
                    piecesList[move.Promotion][i] = move.TargetSquare;
                    break;
                }
            }
        }

        // Target Square
        Square120[move.TargetSquare] = (move.Promotion == -1) ? Square120[move.StartSquare] : move.Promotion;

        // Start Square
        Square120[move.StartSquare] = 0;

        // En Passant Capture
        if (move.Capture == 2)
        {
            int enPasSq = EnPassantSquare;
            enPasSq += (move.StartSquare - move.TargetSquare > 0) ? 10 : -10;

            // Update captured pawn position
            int pos = Array.IndexOf(piecesList[Square120[enPasSq]], enPasSq);
            piecesList[Square120[enPasSq]][pos] = 0;

            Square120[enPasSq] = 0;
        }

        // En Passant Square
        EnPassantSquare = move.EnPassant;

        // Player to move
        WhiteToMove = !WhiteToMove;
    }
}
