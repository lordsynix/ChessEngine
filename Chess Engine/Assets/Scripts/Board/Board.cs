using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MoveGenerator;

/// <summary>
/// Die Klasse <c>Board</c> ist fuer die interne Brettdarstellung des Spielfelds zustaendig.
/// Verwaltet alle wichtigen Informationen ueber das Schachspiel.
/// </summary>
public static class Board
{
    private static int[] Square64;
    private static int[] Square120;

    public static bool WhiteToMove = true;
    public static int EnPassantSquare = -1;

    public static bool WhiteCastleKingside = false;
    public static bool WhiteCastleQueenside = false;
    public static bool BlackCastleKingside = false;
    public static bool BlackCastleQueenside = false;

    public static List<int[]> piecesList = new();

    #region SETTER AND GETTER

    /// <summary>
    /// Die Methode <c>GetSquare64</c> gibt einen Square Array zurueck, speichert Piece.Type
    /// und Piece.Color als int-Wert fuer jedes Feld in der 8x8-Darstellung.
    /// </summary>
    /// <returns>Gibt die 8x8-Darstellung des Spielfelds als int[64] zurueck.</returns>
    public static int[] GetSquare64()
    {
        return Square64;
    }

    public static void SetSquare64(int[] s)
    {
        if (s.Length != 64)
        {
            Debug.LogError("The Value of Board.Square must have 64 Int elements");
            return;
        }
        Square64 = s;
        Square120 = GetSquare120From64();
        piecesList = GetPieceLocation();
    }

    /// <summary>
    /// Die Methode <c>GetSquare120</c> gibt einen Square Array zurueck, speichert Piece.Type
    /// und Piece.Color als int-Wert fuer jedes Feld in der 12x10-Darstellung.
    /// </summary>
    /// <returns>Gibt die 12x10-Darstellung des Spielfelds als int[120] zurueck.</returns>
    public static int[] GetSquare120()
    {
        return Square120;
    }

    public static void SetSquare120(int[] s)
    {
        if (s.Length != 120)
        {
            Debug.LogError("The Value of Board.Square must have 120 Int elements");
            return;
        }
        Square120 = s;
        Square64 = GetSquare64From120();
        piecesList = GetPieceLocation();
    }

    /// <summary>
    /// Die Methode <c>GetWhiteToMove</c> verwaltet den Spieler, der als naechstes spielen kann.
    /// </summary>
    /// <returns>Gibt einen bool zurueck, ob weiss am Zug ist.</returns>
    public static bool GetWhiteToMove()
    {
        return WhiteToMove;
    }

    public static void SetWhiteToMove(bool w)
    {
        WhiteToMove = w;
    }

    public static int GetEnPassantSquare()
    {
        return EnPassantSquare;
    }

    public static void SetEnPassantSquare(int e)
    {
        EnPassantSquare = e;
    }

    public static bool[] GetCastlePermissions()
    {
        return new bool[]
        {
            WhiteCastleKingside,
            WhiteCastleQueenside,
            BlackCastleKingside,
            BlackCastleQueenside
        };
    }

    public static void SetCastlePermissions(string s)
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
    /// Die Methode <c>Get120From64</c> veraendert, ausgehend von der 8x8-Darstellung
    /// die Werte fuer die 12x10-Darstellung.
    /// </summary>
    /// <returns>Gibt den entsprechenden Square120 Array zurück.</returns>
    public static int[] GetSquare120From64()
    {
        Square120 = new int[120];

        // Setzt alle Felder auf den Wert -1
        for (int i = 0; i < Square120.Length; i++)
        {
            Square120[i] = -1;
        }

        // Liest die Werte der 8x8-Darstellung fuer die 12x10-Darstellung.
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
    /// Die Methode <c>Get64From120</c> veraendert, ausgehend von der 12x10-Darstellung
    /// die Werte fuer die 8x8-Darstellung.
    /// </summary>
    /// <returns>Gibt den entsprechenden Square64 Array zurück.</returns>
    public static int[] GetSquare64From120()
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
    /// 8x8-Darstellung den entsprechenden Index in der 12x10-Darstellung zurueck.
    /// </summary>
    /// <returns>Gibt den entsprechenden Index der 12x10-Darstellung zurueck.</returns>
    public static int ConvertIndex64To120(int index)
    {
        int file = index % 8;
        int rank = (index - file) / 8;
        return 21 + rank * 10 + file;
    }

    /// <summary>
    /// Die Methode <c>ConvertIndex120To64</c> gibt, ausgehend eines Index der 12x10-Darstellung 
    /// den entsprechenden Index in der 8x8-Darstellung zurueck.
    /// </summary>
    /// <returns>Gibt den entsprechenden Index der 8x8-Darstellung zurueck.</returns>
    public static int ConvertIndex120To64(int index)
    {
        int file = (index % 10) - 1;
        int rank = ((index - (index % 10)) / 10) - 2;
        return rank * 8 + file;
    }

    /// <summary>
    /// Die Methode <c>GetPieceLocation</c> speichert, ausgehend von der 8x8-Darstellung
    /// die Position für alle Figuren auf dem Brett in Form eines int-Wertes.
    /// </summary>
    public static List<int[]> GetPieceLocation()
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

        return piecesList;
    }

    public static void DebugPieceLocation()
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
    /// Die Methode <c>Initialize</c> wird ausgefuehrt,
    /// wenn eine neue Instanz der Klasse geschaffen wird.
    /// </summary>
    public static void Initialize()
    {
        Square64 = new int[64];
        Square120 = new int[120];
    }

    /// <summary>
    /// Die Methode <c>ResetBoard</c> setzt alle Brett-Variablen auf ihre Default-Werte zurück.
    /// </summary>
    public static void ResetBoard()
    {
        SetSquare64(new int[64]);
        SetWhiteToMove(true);
        SetEnPassantSquare(-1);
    }

    public static void MakeMove(Move move)
    {
        // Position der Figuren
        int piece = Square120[move.StartSquare];
        int position = Array.IndexOf(piecesList[piece], move.StartSquare);
        piecesList[piece][position] = move.TargetSquare;

        if (move.Type == 1)
        {
            int enemyPiece = Square120[move.TargetSquare];
            int enemyPosition = Array.IndexOf(piecesList[enemyPiece], move.TargetSquare);
            piecesList[enemyPiece][enemyPosition] = 0;
        }

        // Aktualisiert die Rochaderechte
        UpdateCastlePermissions(move, piece);

        // Rochade
        if (move.Type == 3) Castle(move, true);
        if (move.Type == 4) Castle(move, false);

        // Verwandlung
        if (move.Promotion != -1)
        {
            Promotion(move, piece, position);
        }

        // Zielfeld
        Square120[move.TargetSquare] = (move.Promotion == -1) ? Square120[move.StartSquare] : move.Promotion;

        // Startfeld
        Square120[move.StartSquare] = 0;

        // En Passant Schlag
        if (move.Type == 2)
        {
            EnPassant(move);
        }

        // En Passant Square
        EnPassantSquare = move.EnPassant;

        // Player to move
        WhiteToMove = !WhiteToMove;
    }

    private static void UpdateCastlePermissions(Move move, int piece)
    {
        if (Piece.IsType(piece, Piece.KING))
        {
            if (Piece.IsColor(piece, Piece.WHITE))
            {
                WhiteCastleKingside = false;
                WhiteCastleQueenside = false;
            }
            else
            {
                BlackCastleKingside = false;
                BlackCastleQueenside = false;
            }
        }
        if (Piece.IsType(piece, Piece.ROOK))
        {
            if (Piece.IsColor(piece, Piece.WHITE))
            {
                if (move.StartSquare == 91)
                    WhiteCastleQueenside = false;
                else if (move.StartSquare == 98)
                    WhiteCastleKingside = false;
            }
            else
            {
                if (move.StartSquare == 21)
                    BlackCastleQueenside = false;
                else if (move.StartSquare == 28)
                    BlackCastleKingside = false;
            }
        }
    }

    private static void Castle(Move move, bool kingside)
    {
        int oldRookSq;
        int newRookSq;

        if (kingside)
        {
            oldRookSq = move.StartSquare + 3;
            newRookSq = move.StartSquare + 1;
        }
        else
        {
            oldRookSq = move.StartSquare - 4;
            newRookSq = move.StartSquare - 1;
        }

        // Figurenposition des Turms
        int rook = Square120[oldRookSq];
        int position = Array.IndexOf(piecesList[rook], oldRookSq);
        piecesList[rook][position] = newRookSq;

        // Zielfeld des Turms
        Square120[newRookSq] = Square120[oldRookSq];

        // Startfeld des Turms
        Square120[oldRookSq] = 0;
    }

    private static void Promotion(Move move, int piece, int position)
    {
        // Setzt die Position des umwandelnden Bauern zurück.
        piecesList[piece][position] = 0;

        // Setzt die Position fuer die umgewandelte Figur.
        for (int i = 0; i < piecesList[move.Promotion].Length; i++)
        {
            if (piecesList[move.Promotion][i] == 0)
            {
                piecesList[move.Promotion][i] = move.TargetSquare;
                break;
            }
        }
    }

    private static void EnPassant(Move move)
    {
        int enPasSq = EnPassantSquare;
        enPasSq += (move.StartSquare - move.TargetSquare > 0) ? 10 : -10;

        // Update captured pawn position
        int pos = Array.IndexOf(piecesList[Square120[enPasSq]], enPasSq);
        piecesList[Square120[enPasSq]][pos] = 0;

        Square120[enPasSq] = 0;
    }
}
