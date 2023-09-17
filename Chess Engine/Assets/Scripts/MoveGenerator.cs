using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public static class MoveGenerator
{
    public static readonly int[] DirectionOffsets = { 10, -10, 1, -1, 11, -11, 9, -9 };
    public static readonly int[] KnightOffsets = { -12, -21, -19, -8, 12, 21, 19, 8 };
    public static readonly int[] WhitePawnOffsets = { -10, -20, -11, -9 };
    public static readonly int[] BlackPawnOffsets = { 10, 20, 11, 9 };
    public static readonly int[] PromotionPieces = { Piece.QUEEN, Piece.ROOK, Piece.BISHOP, Piece.KNIGHT };

    public struct Move
    {
        public int StartSquare { get; set; }
        public int TargetSquare { get; set; }
        // 0 - Normaler Zug, 1 - Normales Schlagen, 2 - En Passant Schlag, 3 - Castle Kingside, 4 - Castle Queenside
        public int Type;
        public int Promotion;
        public int EnPassant;

        // Constructor fuer einen Zug
        public Move(int startSquare, int targetSquare, int type = 0, int promotion = -1, int enPassant = -1)
        {
            StartSquare = startSquare;
            TargetSquare = targetSquare;
            Type = type;
            Promotion = promotion;
            EnPassant = enPassant;
        }
    }

    private static List<Move> moves;
    private static int[] square120;
    private static int friendlyColor;
    private static int opponentColor;
    private static List<int[]> piecesList;

    public static List<Move> GenerateMovesForPiece(int startSquare, int piece)
    {
        moves = new();
        square120 = Board.GetSquare120();

        if (Board.GetWhiteToMove())
        {
            friendlyColor = Piece.WHITE;
            opponentColor = Piece.BLACK;
            if (piece > Piece.BLACK)
                Debug.LogWarning("The given piece doesn't match with the current color");
        }  
        else
        {
            friendlyColor = Piece.BLACK;
            opponentColor = Piece.WHITE;
            if (piece < Piece.BLACK && piece > 0)
                Debug.LogWarning("The given piece doesn't match with the current color");
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

    public static List<Move> GenerateMoves()
    {
        square120 = Board.GetSquare120();
        piecesList = Board.GetPieceLocation();
        List<Move> moves = new();

        if (Board.GetWhiteToMove()) friendlyColor = Piece.WHITE;   
        else friendlyColor = Piece.BLACK;
            
        for (int pieceType = friendlyColor + 1; pieceType < friendlyColor + 7; pieceType++)
        {
            if (piecesList[pieceType].Length == 0) continue;

            foreach (int startSquare in piecesList[pieceType])
            {
                if (startSquare == 0) continue;

                moves.AddRange(GenerateMovesForPiece(startSquare, pieceType));
            }
        }

        return moves;
    }

    static void GenerateSlidingMoves (int startSquare, int piece)
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

    static void GenerateKnightMoves(int startSquare, int piece)
    {
        for (int dirIndex = 0; dirIndex < 8; dirIndex++)
        {
            int targetSquare = startSquare + KnightOffsets[dirIndex];
            int pieceOnTargetSquare = square120[targetSquare];

            LegitimateMove(pieceOnTargetSquare, startSquare, targetSquare, piece);
        }
    }

    static void GeneratePawnMoves(int startSquare, int piece)
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

            // Bauernzug fuer 2 Feldern
            else if (dirIndex == 1)
            {
                if (Piece.IsColor(friendlyColor, Piece.WHITE) && startSquare >= 81 
                    && startSquare <= 88 && square120[startSquare - 10] == Piece.NONE)
                    LegitimateMove(pieceOnTargetSquare, startSquare, targetSquare, piece, true);
                
                else if (Piece.IsColor(friendlyColor, Piece.BLACK) && startSquare >= 31 
                    && startSquare <= 38 && square120[startSquare + 10] == Piece.NONE)
                    LegitimateMove(pieceOnTargetSquare, startSquare, targetSquare, piece, true);
            }

            // Zuege nur moeglich, wenn diagonal geschlagen wird
            else CanCapture(pieceOnTargetSquare, startSquare, targetSquare, piece);
        }
    }

    static void GenerateKingMoves(int startSquare, int piece)
    {
        for (int dirOffset = 0; dirOffset < 8; dirOffset++)
        {
            int targetSquare = startSquare + DirectionOffsets[dirOffset];
            int pieceOnTargetSquare = square120[targetSquare];

            LegitimateMove(pieceOnTargetSquare, startSquare, targetSquare, piece);
        }
        GenerateCastlingMoves(startSquare);
    }

    static bool LegitimateMove (int pieceOnTargetSquare, int startSquare, int targetSquare, int piece, bool doubleAdvance = false)
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

    static bool CanCapture (int pieceOnTargetSquare, int startSquare, int targetSquare, int piece)
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
            if (targetSquare == Board.GetEnPassantSquare())
            {
                moves.Add(new Move(startSquare, targetSquare, 2));
            }
        }

        return false;
    }

    static bool CanPromote(int startSquare, int targetSquare, int piece)
    {
        if (!Piece.IsPawn(piece)) 
            return false;

        if (targetSquare >= 21 && targetSquare <= 28 || targetSquare >= 91 && targetSquare <= 98) 
            return true;

        return false;
    }

    static void GeneratePromotionMoves(int startSquare, int targetSquare, int capture)
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

    static void GenerateCastlingMoves(int startSquare)
    {
        int[] CastleOffsets = { 2, -2 };

        foreach (int offset in CastleOffsets)
        {
            bool kingside;
            bool permission;

            bool[] permissions = Board.GetCastlePermissions();

            bool WhiteCastleKingside = permissions[0];
            bool WhiteCastleQueenside = permissions[1];
            bool BlackCastleKingside = permissions[2];
            bool BlackCastleQueenside = permissions[3];

            if (offset > 0) kingside = true;
            else kingside = false;

            // Stellt sicher, dass der Spieler rochieren darf.
            if (Piece.IsColor(friendlyColor, Piece.WHITE))
            {
                if (kingside) if (!WhiteCastleKingside) continue;
                if (!kingside) if (!WhiteCastleQueenside) continue;
            }
            else
            {
                if (kingside) if (!BlackCastleKingside) continue;
                if (!kingside) if (!BlackCastleQueenside) continue;
            }
            
            // Stellt sicher, dass der Spieler rochieren kann.
            permission = GenerateCastlingPermission(startSquare, offset, kingside);
            
            if (permission)
            {
                int targetSquare = startSquare + offset;

                moves.Add(new Move(startSquare, targetSquare, kingside ? 3 : 4));
            }
        }        
    }

    static bool GenerateCastlingPermission(int startSquare, int offset, bool kingside, bool permission = true)
    {
        // Ueberprueft, ob die Felder zwischen Koenig und Turm leer sind.
        for (int i = 1; i < 4; i++)
        {
            int square = startSquare + i * (offset / 2);

            if (square != 28 && square != 98)
            {
                if (square120[square] != 0)
                {
                    permission = false;
                    break;
                }
            }
        }

        // Stellt sicher, dass die mitrochierende Figur ein eigener Turm ist.
        if (kingside)
        {
            if (square120[startSquare + 3] != (Piece.ROOK | friendlyColor))
            {
                if (friendlyColor == Piece.WHITE) Board.SetWhiteCastleKingside(false);
                else Board.SetBlackCastleKingside(false);
                permission = false;
            }

        }
        else
        {
            if (square120[startSquare - 4] != (Piece.ROOK | friendlyColor))
            {
                if (friendlyColor == Piece.WHITE) Board.SetWhiteCastleQueenside(false);
                else Board.SetBlackCastleQueenside(false);
                permission = false;
            }

        }

        return permission;
    }
}
