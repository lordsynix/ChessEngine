using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public static class FENManager
{
    public static string startFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 0"; // Ausgangsposition als FEN-String
    //private const string startFEN = "r1bk3r/p2pBpNp/n4n2/1p1NP2P/6P1/3P4/P1P1K3/q5b1 b Qk ";
    //private const string startFEN = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 6";

    private static bool WhiteCastleKingside;
    private static bool WhiteCastleQueenside;
    private static bool BlackCastleKingside;
    private static bool BlackCastleQueenside;

    public static readonly Dictionary<char, int> pieceTypeFromSymbol = new()
    {
        ['k'] = Piece.KING,
        ['p'] = Piece.PAWN,
        ['n'] = Piece.KNIGHT,
        ['b'] = Piece.BISHOP,
        ['r'] = Piece.ROOK,
        ['q'] = Piece.QUEEN
    };

    /// <summary>
    /// Die Funktion <c>LoadFenPosition</c> generiert mit einem FEN-String eine 
    /// beliebige Schachposition unter Beruecksichtigung aller bestehender Regeln.
    /// </summary>
    /// <param name="fen">Der zu generierende FEN-String.</param>
    public static void LoadFenPosition()
    {
        string fen = startFEN;
        Log.Message($"Loading FEN-String: {fen}");

        try
        {
            // vgl. mit https://www.youtube.com/watch?v=U4ogK0MIzqk&t=160s 2:40s
            string fenBoard = fen.Split(' ')[0];
            string fenToMove = fen.Split(' ')[1];
            string fenCastle = fen.Split(' ')[2];
            string fenEnPassant = fen.Split(' ')[3];
            string fenFiftyMoveRule = fen.Split(' ')[4];
            string fenMoveCount = fen.Split(' ')[5];
            int file = 0, rank = 0;
            var square64 = new int[64];
            var bitboards = new ulong[23];

            // Weist der internen Brettdarstellung (8x8-Darstellung) die int-Werte der Position zu.
            foreach (char symbol in fenBoard)
            {
                if (symbol == '/')
                {
                    file = 0;
                    rank++;
                }
                else
                {
                    if (char.IsDigit(symbol))
                    {
                        file += (int)char.GetNumericValue(symbol);
                    }
                    else
                    {
                        int pieceColor = char.IsUpper(symbol) ? Piece.WHITE : Piece.BLACK;
                        int pieceType = pieceTypeFromSymbol[char.ToLower(symbol)];

                        square64[rank * 8 + file] = pieceColor | pieceType;

                        file++;
                    }
                }
            }
            Board.SetSquare64(square64);

            // Definiert den Spieler, welcher als naechstes Spielen darf.
            Board.SetWhiteToMove(fenToMove == "w");
            Board.SetPlayerColor(fenToMove.ToCharArray()[0]);

            BoardGeneration.instance.Generate(square64);

            SetCastlePermissions(fenCastle);

            int enPassantSq = -1;
            if (fenEnPassant != "-")
            {
                enPassantSq = Board.SquareToIndex(fenEnPassant);   
            }

            if (fenMoveCount != "-")
            {
                Board.SetMoveCount(int.Parse(fenMoveCount));
            }

            // Kreiert Position State
            int whiteCastle = (WhiteCastleKingside ? 1 << 0 : 0) | (WhiteCastleQueenside ? 1 << 1 : 0);
            int blackCastle = (BlackCastleKingside ? 1 << 2 : 0) | (BlackCastleQueenside ? 1 << 3 : 0);
            int castleRights = whiteCastle | blackCastle;

            ulong zobristKey = Zobrist.GenerateHash(enPassantSq, castleRights);
            PositionState currentPositionState = new(Piece.NONE, enPassantSq, castleRights, int.Parse(fenFiftyMoveRule), zobristKey);
            Board.SetCurrentPositionState(currentPositionState);

            Board.PositionStateHistory.Push(currentPositionState);

        }
        catch (Exception ex)
        {
            Log.Message($"Exception while loading FEN-String: {ex}");
            Debug.LogException(ex);

            Console.Instance.AddToConsole($"Exception while loading FEN-String: {ex}");
        }
    }

    private static void SetCastlePermissions(string s)
    {
        WhiteCastleKingside = false; WhiteCastleQueenside = false;
        BlackCastleKingside = false; BlackCastleQueenside = false;

        if (s == "") return;

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
        }
    }

}
