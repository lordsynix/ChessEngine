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
    public static readonly int[] PromotionPieces = { Piece.QUEEN, Piece.ROOK, Piece.BISHOP, Piece.KNIGHT };

    public struct Move
    {
        public int StartSquare { get; set; }
        public int TargetSquare { get; set; }
        // 0 - Normaler Zug, 1 - Normales Schlagen, 2 - En Passant Schlag
        public int Capture;
        public int Promotion;
        public int EnPassant;

        // Constructor f�r einen Zug
        public Move(int startSquare, int targetSquare, int capture = 0, int promotion = -1, int enPassant = -1)
        {
            StartSquare = startSquare;
            TargetSquare = targetSquare;
            Capture = capture;
            Promotion = promotion;
            EnPassant = enPassant;
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

            // Bauernzug f�r 2 Feldern
            else if (dirIndex == 1)
            {
                if (Piece.IsColor(friendlyColor, Piece.WHITE) && startSquare >= 81 && startSquare <= 88)
                    LegitimateMove(pieceOnTargetSquare, startSquare, targetSquare, piece, true);
                
                else if (Piece.IsColor(friendlyColor, Piece.BLACK) && startSquare >= 31 && startSquare <= 38)
                    LegitimateMove(pieceOnTargetSquare, startSquare, targetSquare, piece, true);
            }

            // Z�ge nur m�glich, wenn diagonal geschlagen wird
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

    bool LegitimateMove (int pieceOnTargetSquare, int startSquare, int targetSquare, int piece, bool doubleAdvance = false)
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

        bool promotion = CanPromote(startSquare, targetSquare, piece);

        // Freies Feld
        if (!CanCapture(pieceOnTargetSquare, startSquare, targetSquare, piece))
        {
            if (promotion) GeneratePromotionMoves(startSquare, targetSquare, 0);
            else
            {
                // Doppelter Bauerzug, kreiert ein mögliches EnPassant Schlagfeld.
                if (doubleAdvance)
                {
                    int enPassantSqare = startSquare + (targetSquare - startSquare) / 2;
                    moves.Add(new Move(startSquare, targetSquare, 0, -1, enPassantSqare));
                } else moves.Add(new Move(startSquare, targetSquare));
            } 
        }
        // Blckiert von gegnerischer Figur
        else
        {
            if (!promotion) if (Piece.IsPawn(piece)) moves.RemoveAt(moves.Count - 1);

            return false;
        }

        return true;
    }

    bool CanCapture (int pieceOnTargetSquare, int startSquare, int targetSquare, int piece)
    {
        // Blockiert von gegnerischen Figur
        if (Piece.IsColor(pieceOnTargetSquare, opponentColor))
        {
            if (CanPromote(startSquare, targetSquare, piece)) GeneratePromotionMoves(startSquare, targetSquare, 1);
            else moves.Add(new Move(startSquare, targetSquare, 1));

            return true;
        }
        else if (Piece.IsPawn(piece))
        {
            if (targetSquare == Board.instance.GetEnPassantSquare())
                moves.Add(new Move(startSquare, targetSquare, 2));
        }

        return false;
    }

    bool CanPromote(int startSquare, int targetSquare, int piece)
    {
        if (!Piece.IsPawn(piece)) return false;

        if (targetSquare >= 21 && targetSquare <= 28 || targetSquare >= 91 && targetSquare <= 98) return true;

        return false;
    }

    void GeneratePromotionMoves(int startSquare, int targetSquare, int capture)
    {
        if (capture == 1)
        {
            if (targetSquare == startSquare + WhitePawnOffsets[0] || targetSquare == startSquare + BlackPawnOffsets[0])
                return;
        }

        foreach(int piece in PromotionPieces)
        {
            moves.Add(new Move(startSquare, targetSquare, capture, friendlyColor | piece));
        }
    }
}
