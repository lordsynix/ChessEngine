using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The class <c>Move</c> preserves the memory during search by compacting a move into 16bits
/// with the format "fffftttttttsssssss": Bits 0-5 startSquare, Bits 6-11 targetSquare, Bits 12-15 move flags 
/// </summary>
public class Move
{
    // 18 Bit Zugwert
    readonly ushort moveValue;

    // Flags
    public const int NoFlag = 0b0000;
    public const int EnPassantCaptureFlag = 0b0001;
    public const int CastleFlag = 0b0010;
    public const int PawnTwoUpFlag = 0b0011;

    public const int PromoteToQueenFlag = 0b0100;
    public const int PromoteToKnightFlag = 0b0101;
    public const int PromoteToRookFlag = 0b0110;
    public const int PromoteToBishopFlag = 0b0111;

    // Masken
    const ushort startSquareMask = 0b0000000000111111;
    const ushort targetSquareMask = 0b0000111111000000;
    const ushort flagMask = 0b1111000000000000;

    // Konstruktoren
    public Move(ushort moveValue)
    {
        this.moveValue = moveValue;
    }

    public Move(int startSquare, int targetSquare)
    {
        int _startSquare = Board.ConvertIndex120To64(startSquare);
        int _targetSquare = Board.ConvertIndex120To64(targetSquare);

        moveValue = (ushort)(_startSquare | _targetSquare << 6 | NoFlag << 12);
    }

    public Move(int startSquare, int targetSquare, int flag)
    {
        startSquare = Board.ConvertIndex120To64(startSquare);
        targetSquare = Board.ConvertIndex120To64(targetSquare);

        moveValue = (ushort)(startSquare | targetSquare << 6 | flag << 12);
    }

    public ushort Value => moveValue;
    public bool IsNull => moveValue == 0;

    public int StartSquare => moveValue & startSquareMask;
    public int TargetSquare => (moveValue & targetSquareMask) >> 6;
    public bool IsPromotion => MoveFlag >= PromoteToQueenFlag;
    public int MoveFlag => moveValue >> 12;

    public int PromotionPieceType
    {
        get
        {
            switch (MoveFlag)
            {
                case PromoteToRookFlag:
                    return Piece.ROOK;
                case PromoteToKnightFlag:
                    return Piece.KNIGHT;
                case PromoteToBishopFlag:
                    return Piece.BISHOP;
                case PromoteToQueenFlag:
                    return Piece.QUEEN;
                default:
                    return Piece.NONE;
            }
        }
    }

    public static Move PromotionMoveWithPiece(int startSquare, int targetSquare, int piece)
    {
        return piece switch
        {
            Piece.ROOK => new Move(Board.ConvertIndex64To120(startSquare), Board.ConvertIndex64To120(targetSquare), PromoteToRookFlag),
            Piece.KNIGHT => new Move(Board.ConvertIndex64To120(startSquare), Board.ConvertIndex64To120(targetSquare), PromoteToKnightFlag),
            Piece.BISHOP => new Move(Board.ConvertIndex64To120(startSquare), Board.ConvertIndex64To120(targetSquare), PromoteToBishopFlag),
            Piece.QUEEN => new Move(Board.ConvertIndex64To120(startSquare), Board.ConvertIndex64To120(targetSquare), PromoteToQueenFlag),
            _ => null,
        };
    }

    public static Move NullMove => new Move(0);

}
