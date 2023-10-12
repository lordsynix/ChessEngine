using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionState
{
    public readonly int capturedPieceType;
    public readonly int enPassantSquare;
    public readonly int castlingRights;
    public readonly int fiftyMoveCounter;
    public readonly ulong zobristKey;

    public const int ClearWhiteKingsideMask = 0b1110;
    public const int ClearWhiteQueensideMask = 0b1101;
    public const int ClearBlackKingsideMask = 0b1011;
    public const int ClearBlackQueensideMask = 0b0111;

    public PositionState(int capturedPieceType, int enPassantSquare, int castlingRights, int fiftyMoveCounter, ulong zobristKey)
    {
        this.capturedPieceType = capturedPieceType;
        this.enPassantSquare = enPassantSquare;
        this.castlingRights = castlingRights;
        this.fiftyMoveCounter = fiftyMoveCounter;
        this.zobristKey = zobristKey;
    }

    public bool HasKingsideCastleRight(bool white)
    {
        int mask = white ? 1 : 4;
        return (castlingRights & mask) != 0;
    }

    public bool HasQueensideCastleRight(bool white)
    {
        int mask = white ? 2 : 8;
        return (castlingRights & mask) != 0;
    }
}
