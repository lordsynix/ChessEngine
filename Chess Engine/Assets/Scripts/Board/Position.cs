using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Position
{
    public int[] Square64;
    public int[] Square120;

    public bool WhiteToMove = true;
    public int EnPassantSquare = -1;

    public bool WhiteCastleKingside = false;
    public bool WhiteCastleQueenside = false;
    public bool BlackCastleKingside = false;
    public bool BlackCastleQueenside = false;

    public List<int[]> PiecesList = new();

    public Position(int[] square64, int[] square120, bool whiteToMove, int enPassantSquare, bool whiteCastleKingside,
                    bool whiteCastleQueenside, bool blackCastleKingside, bool blackCastleQueenside, List<int[]> piecesList)
    {
        Square64 = square64;
        Square120 = square120;
        WhiteToMove = whiteToMove;
        EnPassantSquare = enPassantSquare;
        WhiteCastleKingside = whiteCastleKingside;
        WhiteCastleQueenside = whiteCastleQueenside;
        BlackCastleKingside = blackCastleKingside;
        BlackCastleQueenside = blackCastleQueenside;
        PiecesList = piecesList;
    }
}
