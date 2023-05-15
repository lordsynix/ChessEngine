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
    public static readonly int[] KnightOffsets = { -12, -21, -19, -8, 12, 21, 19, 8 };
    public static readonly int[] WhitePawnOffsets = { -10, -20, -11, -9 };
    public static readonly int[] BlackPawnOffsets = { 10, 20, 11, 9 };
    public struct Move
    {
        public int StartSquare { get; set; }
        public int TargetSquare { get; set; }
        public bool Capture;
        public int Promotion;

        // Constructor für einen Zug
        public Move(int startSquare, int targetSquare, bool capture = false, int promotion = -1)
        {
            StartSquare = startSquare;
            TargetSquare = targetSquare;
            Capture = capture;
            Promotion = promotion;
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
        else if (Piece.IsKnight(piece))
        {
            GenerateKnightMoves(startSquare, piece);
        } 
        else if (Piece.IsPawn(piece))
        {
            GeneratePawnMoves(startSquare, piece);
        } 
        else if (Piece.IsKing(piece))
        {
            GenerateKingMoves(startSquare, piece);
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

                if (!LegitimateMove(pieceOnTargetSquare, startSquare, targetSquare, piece))
                    break;
            }
        }
    }

    void GenerateKnightMoves(int startSquare, int piece)
    {
        for (int dirIndex = 0; dirIndex < 8; dirIndex++)
        {
            int targetSquare = startSquare + KnightOffsets[dirIndex];
            int pieceOnTargetSquare = square120[targetSquare];

            LegitimateMove(pieceOnTargetSquare, startSquare, targetSquare, piece);
        }
    }

    void GeneratePawnMoves(int startSquare, int piece)
    {
        for (int dirIndex = 0; dirIndex < 4; dirIndex++)
        {
            int offset = Piece.IsColor(friendlyColor, Piece.WHITE) ? WhitePawnOffsets[dirIndex] : BlackPawnOffsets[dirIndex];
            int targetSquare = startSquare + offset;
            int pieceOnTargetSquare = square120[targetSquare];

            // Normaler Bauernzug
            if (dirIndex == 0)
            {
                LegitimateMove(pieceOnTargetSquare, startSquare, targetSquare, piece);
            }

            // Bauernzug für 2 Feldern
            else if (dirIndex == 1)
            {
                if (Piece.IsColor(friendlyColor, Piece.WHITE))
                {
                    if (startSquare >= 81 && startSquare <= 88) 
                        LegitimateMove(pieceOnTargetSquare, startSquare, targetSquare, piece);
                }
                else
                {
                    if (startSquare >= 31 && startSquare <= 38)
                        LegitimateMove(pieceOnTargetSquare, startSquare, targetSquare, piece);
                }
            }

            // Züge nur möglich, wenn diagonal geschlagen wird
            else CanCapture(pieceOnTargetSquare, startSquare, targetSquare, piece);
        }
    }

    void GenerateKingMoves(int startSquare, int piece)
    {
        for (int dirOffset = 0; dirOffset < 8; dirOffset++)
        {
            int targetSquare = startSquare + DirectionOffsets[dirOffset];
            int pieceOnTargetSquare = square120[targetSquare];

            LegitimateMove(pieceOnTargetSquare, startSquare, targetSquare, piece);
        }
    }

    bool LegitimateMove (int pieceOnTargetSquare, int startSquare, int targetSquare, int piece)
    {
        // Spielfeldrand erreicht
        if (pieceOnTargetSquare == -1)
        {
            //Debug.Log("Move invalid because square " + targetSquare + " is over the edge.");
            return false;
        }

        // Blockiert von eigenen Figur
        if (Piece.IsColor(pieceOnTargetSquare, friendlyColor))
        {
            //Debug.Log("Move invalid because friendly piece at " + targetSquare + " : " + pieceOnTargetSquare + " : " + startSquare);
            return false;
        }

        if (!CanCapture(pieceOnTargetSquare, startSquare, targetSquare, piece))
            moves.Add(new Move(startSquare, targetSquare, false, CanPromote(targetSquare, piece)));
        else
        {
            if (Piece.IsPawn(piece)) moves.RemoveAt(moves.Count - 1);
            return false;
        }

        return true;
    }

    bool CanCapture (int pieceOnTargetSquare, int startSquare, int targetSquare, int piece)
    {
        // Blockiert von gegnerischen Figur
        if (Piece.IsColor(pieceOnTargetSquare, opponentColor))
        {
            moves.Add(new Move(startSquare, targetSquare, true, CanPromote(targetSquare, piece)));
            return true;
        }

        return false;
    }

    int CanPromote(int targetSquare, int piece)
    {
        if (!Piece.IsPawn(piece)) return -1;

        if (Piece.IsColor(friendlyColor, Piece.WHITE))
        {
            if (targetSquare >= 21 && targetSquare <= 28)
                return 0;
        }
        else
        {
            if (targetSquare >= 91 && targetSquare <= 98)
                return 0;
        }
        return -1;
    }
}
