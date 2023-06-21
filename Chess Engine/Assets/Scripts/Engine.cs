using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MoveGenerator;

public static class Engine
{
    [HideInInspector] public static string positionFEN = "";

    public static int friendlyColor;
    public static int opponentColor;

    const int pawnValue = 100;
    const int knightValue = 300;
    const int bishopValue = 300;
    const int rookValue = 500;
    const int queenValue = 900;

    public static int Evaluate()
    {
        int material = 0;
        List<int[]> piecesList = Board.GetPieceLocation();

        if (Board.GetWhiteToMove()) friendlyColor =  Piece.WHITE;
        else friendlyColor = Piece.BLACK;
        opponentColor = Piece.OpponentColor(friendlyColor);

        int[] sides = { friendlyColor, opponentColor };

        foreach(int color in sides)
        {
            int multiplier = (color == friendlyColor) ? -1 : 1;
            for (int pieceType = color + 1; pieceType < color + 7; pieceType++)
            {
                if (piecesList[pieceType].Length == 0) continue;
                if (Piece.IsType(pieceType, Piece.KING)) continue;

                int count = 0;
                foreach (int piece in piecesList[pieceType])
                {
                    if (piece == 0) continue;
                    count++;
                }

                if (count == 0) continue;

                if (Piece.IsType(pieceType, Piece.PAWN)) material -= count * pawnValue * multiplier;
                else if (Piece.IsType(pieceType, Piece.KNIGHT)) material -= count * knightValue * multiplier;
                else if (Piece.IsType(pieceType, Piece.BISHOP)) material -= count * bishopValue * multiplier;
                else if (Piece.IsType(pieceType, Piece.ROOK)) material -= count * rookValue * multiplier;
                else if (Piece.IsType(pieceType, Piece.QUEEN)) material -= count * queenValue * multiplier;
                //Debug.Log($"Color: {color} Count: {count} Type: {pieceType} Material: {material}");
            }
        }
        return material;
    }
}
