using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Zobrist
{
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
