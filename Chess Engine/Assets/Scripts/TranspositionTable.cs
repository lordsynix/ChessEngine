using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TranspositionTable
{
    public static Dictionary<ulong, Entry> transpositionTable = new();

    public class Entry
    {
        public int Evaluation;

        public Entry(int evaluation)
        {
            Evaluation = evaluation;
        }
    }

    // Zobrist Hashing
    private static ulong[][] pieceSquareKeys;
    private static ulong sideToMoveKey;
    private static ulong[] castlingKeys;
    private static ulong enPassantKey;

    private static System.Random random;

    public static void Initialize()
    {
        transpositionTable = new();
        random = new();

        // Initialisiert die Figur-Feld Schluessel
        pieceSquareKeys = new ulong[12][];
        for (int i = 0; i < 12; i++)
        {
            pieceSquareKeys[i] = new ulong[64];
            for (int j = 0;  j < 64; j++)
            {
                pieceSquareKeys[i][j] = GenerateRandomKey();
            }
        }

        // Initialisiert die uebrigen Schluessel
        sideToMoveKey = GenerateRandomKey();
        enPassantKey = GenerateRandomKey();
        castlingKeys = new ulong[16];
        for (int i = 0; i < 16; i++)
        {
            castlingKeys[i] = GenerateRandomKey();
        }
    }

    private static ulong GenerateRandomKey() 
    {
        byte[] buffer = new byte[8];
        random.NextBytes(buffer);
        return BitConverter.ToUInt64(buffer, 0);
    }

    public static ulong CalculateHash()
    {
        ulong hash = 0;
        int[] square64 = Board.GetSquare64();

        for (int sq = 0; sq < 64; sq++)
        {
            int piece = square64[sq];

            if (piece != Piece.NONE)
            {
                int pieceType = Piece.IsPieceType(piece);

                hash ^= pieceSquareKeys[pieceType][sq];
            }
        }

        if (Board.GetWhiteToMove() == true)
        {
            hash ^= sideToMoveKey;
        }

        if (Board.GetEnPassantSquare() != -1)
        {
            hash ^= enPassantKey;
        }

        bool[] permissions = Board.GetCastlePermissions();
        for (int i = 0; i < 4; i++)
        {
            if (permissions[i])
            {
                hash ^= castlingKeys[i];
            }
        }

        return hash;
    }

    public static Entry Lookup()
    {
        ulong hash = CalculateHash();

        if (transpositionTable.TryGetValue(hash, out Entry entry))
        {
            return entry;
        }

        return null;
    }

    public static void Store(Entry entry)
    {
        ulong hash = CalculateHash();

        if (!transpositionTable.ContainsKey(hash))
        {
            transpositionTable.Add(hash, entry);
            Debug.Log($"Stored position{GameManager.instance.StoreFenPosition()} at {hash}");
        }
        else
        {
            Debug.LogWarning($"Transposition table already contains an entry for the given " +
                $"hash with the position {GameManager.instance.StoreFenPosition()}");
        }
    }
}
