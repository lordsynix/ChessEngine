using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Die Klasse <c>Zobrist</c> ist fuer das nahezu einzigartige und ressourcensparende Hashing fuer eine Brettdarstellung 
/// verantwortlich. Der Zobrist-Key einer Position kann zu deren Abspeicherung in der Transpositionstabelle verwendet werden.
/// </summary>
public static class Zobrist
{
    // Uebernommen von: https://github.com/SebLague/Chess-Coding-Adventure/blob/Chess-V2-UCI/Chess-Coding-Adventure/src/Core/Board/Zobrist.cs

    public static ulong[,] pieceKeys = new ulong[64, 23];
    public static ulong[] enPassantKeys = new ulong[17];
    public static ulong whiteToMoveKey;
    public static ulong[] castlingKeys = new ulong[16];

    public static readonly List<int> enPassantSquares = new()
    {
        -1, 71, 72, 73, 74, 75, 76, 77, 78, 41, 42, 43, 44, 45, 46, 47, 48
    };

    public static void Initialize()
    {
        System.Random random = new();

        // Initializiert die Zufallsschluessel fuer die Schachfiguren auf allen Feldern
        for (int sq = 0; sq < 64; sq++)
        {
            for (int pieceType = 0; pieceType < 23; pieceType++)
            {
                pieceKeys[sq, pieceType] = (ulong)random.Next();
            }
        }

        // Initializiert die Zufallsschluessel fuer alle En-Passant-Felder
        for (int i = 0; i < 17; i++)
        {
            enPassantKeys[i] = (ulong)random.Next();
        }

        // Initializiert den Zufallsschluessel fuer die Seite, die als naechstes Spielt
        whiteToMoveKey = (ulong)random.Next();

        // Initializiert die Zufallsschluessel fuer alle moeglichen Rochaderechte
        for (int i = 0; i < 16; i++)
        {
            castlingKeys[i] = (ulong)random.Next();
        }
    }

    /// <summary>
    /// Die Funktion <c>GenerateHash</c> generiert fuer eine Ausgangsposition den Zobrist-Schluessel. 
    /// Ressourcenaufwendig, weshalb sie nur einmalig vom FENManger aufgerufen wird. 
    /// </summary>
    /// <param name="_enPassantSquare"></param>
    /// <param name="_castlingRights"></param>
    /// <returns></returns>
    public static ulong GenerateHash(int _enPassantSquare, int _castlingRights)
    {
        int[] square64 = Board.GetSquare64();

        ulong hash = 0;

        for (int sq = 0; sq < 64; sq++)
        {
            int piece = square64[sq];

            if (piece != Piece.NONE)
            {
                hash ^= pieceKeys[sq, piece];
            }
        }

        int enPassantSquare = _enPassantSquare;
        if (enPassantSquare != -1)
        {
            hash ^= enPassantKeys[enPassantSquares.IndexOf(enPassantSquare)];
        }

        if (Board.GetWhiteToMove() == true)
        {
            hash ^= whiteToMoveKey;
        }

        hash ^= castlingKeys[_castlingRights];

        return hash;
    }
}
