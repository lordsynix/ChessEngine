using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveGenerator
{
    #region INSTANCE

    public static MoveGenerator instance;

    private void Awake()
    {
        instance = this;
    }

    #endregion

    public static readonly int[] DirectionOffsets = { 10, -10, 1, -1, 11, -11, 9, -9 };
    public struct Move
    {
        public readonly int StartSquare;
        public readonly int TargetSquare;

        // Constructor für einen Zug
        public Move(int startSquare, int targetSquare)
        {
            StartSquare = startSquare;
            TargetSquare = targetSquare;
        }
    }

    List<Move> moves;
    private int[] square120;
    private int friendlyColor;
    private int opponentColor;
    
    public List<Move> GenerateMovesForPiece(int startSquare, int piece)
    {
        moves = new();
        square120 = Board.instance.GetSquare120();

        if (Board.instance.GetWhiteToMove())
        {
            friendlyColor = Piece.WHITE;
            opponentColor = Piece.BLACK;
            if (piece > Piece.BLACK)
                return null;
        }  
        else
        {
            friendlyColor = Piece.BLACK;
            opponentColor = Piece.WHITE;
            if (piece < Piece.BLACK && piece > 0)
                return null;
        }

        if (Piece.IsSlidingPiece(piece))
        {
            GenerateSlidingMoves(startSquare, piece);
        }

        return moves;
    }

    void GenerateSlidingMoves (int startSquare, int piece)
    {
        int startDirIndex = Piece.IsType(piece, Piece.BISHOP) ? 4 : 0;
        int endDirIndex = Piece.IsType(piece, Piece.ROOK) ? 4 : 8;

        for (int dirIndex = startDirIndex; dirIndex < endDirIndex; dirIndex++)
        {
            for (int n = 0; n < 8; n++)
            {
                int targetSquare = startSquare + DirectionOffsets[dirIndex] * (n + 1);
                int pieceOnTargetSquare = square120[targetSquare];

                // Spielfeldrand erreicht
                if (pieceOnTargetSquare == -1)
                {
                    //Debug.Log("Move invalid because square " + targetSquare + " is over the edge.");
                    break;
                }

                // Blockiert von eigenen Figur
                if (Piece.IsColor(pieceOnTargetSquare, friendlyColor))
                {
                    //Debug.Log("Move invalid because friendly piece at " + targetSquare + " : " + pieceOnTargetSquare + " : " + startSquare);
                    break;
                }

                moves.Add(new Move(startSquare, targetSquare));
                //Debug.Log("Added a move from " + startSquare + " to " + targetSquare);

                // Blockiert von gegnerischen Figur
                if (Piece.IsColor(pieceOnTargetSquare, opponentColor))
                {
                    //Debug.Log("Skipped direction because of an enemy piece at " + targetSquare);
                    break;
                }
            }
        }
    }
}
