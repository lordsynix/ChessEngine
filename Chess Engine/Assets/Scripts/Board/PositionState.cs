using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Die Klasse <c>PositionState</c> speichert den Positionsstatus einer Brettdarstellung. Dieser beinhaltet die geschlagene Figur,
/// das en-passant Feld, die Rochaderechte, ein Wert zur 50-Zuege-Regel sowie einen nahezu einzigartigen Hash (Zobrist-Key) der Position.
/// </summary>
public class PositionState
{
    // Ubernommen von: https://github.com/SebLague/Chess-Coding-Adventure/blob/Chess-V2-UCI/Chess-Coding-Adventure/src/Core/Board/GameState.cs
    // In einigen Versionen in der Mitte der Arbeit habe ich mich an aehnliche
    // Moeglichkeiten hingetastet, welche allerdings nicht verwendbar waren.

    public readonly int capturedPieceType;
    public readonly int enPassantSquare;
    public readonly int castlingRights;
    public readonly int fiftyMoveCounter;
    public readonly ulong zobristKey;

    public const int ClearWhiteKingsideMask = 0b1110;
    public const int ClearWhiteQueensideMask = 0b1101;
    public const int ClearBlackKingsideMask = 0b1011;
    public const int ClearBlackQueensideMask = 0b0111;

    // Konstrukor
    public PositionState(int capturedPieceType, int enPassantSquare, int castlingRights, int fiftyMoveCounter, ulong zobristKey)
    {
        this.capturedPieceType = capturedPieceType;
        this.enPassantSquare = enPassantSquare;
        this.castlingRights = castlingRights;
        this.fiftyMoveCounter = fiftyMoveCounter;
        this.zobristKey = zobristKey;
    }

    // Funktionen fuer andere Klassen, um die Rochaderechte einer Position abzufragen.
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
