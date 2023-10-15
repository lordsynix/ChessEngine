using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Die Klasse <c>MoveGenerator</c> ist fuer die Generierung von Zuegen zustaendig.
/// </summary>
public static class MoveGenerator
{
    public static readonly int[] DirectionOffsets = { 10, -10, 1, -1, 11, -11, 9, -9 };
    public static readonly int[] KnightOffsets = { -12, -21, -19, -8, 12, 21, 19, 8 };
    public static readonly int[] WhitePawnOffsets = { -10, -20, -11, -9 };
    public static readonly int[] BlackPawnOffsets = { 10, 20, 11, 9 };
    public static readonly int[] PromotionPieces = { Piece.QUEEN, Piece.ROOK, Piece.BISHOP, Piece.KNIGHT };

    private static List<Move> moves;
    private static int[] square120;
    private static int friendlyColor;
    private static int opponentColor;
    private static PositionState currentPositionState;

    /// <summary>
    /// Die Funktion <c>GenerateMovesForPiece</c> generiert alle moeglichen Zuege fuer eine Figur.
    /// </summary>
    /// <param name="startSquare">Das Ursprungsfeld in der 12x10-Darstellung</param>
    /// <param name="piece">Der Figurentyp, fuer den Zuege generiert werden</param>
    /// <returns>Liste mit allen moeglichen Zuegen fuer diese Figur</returns>
    public static List<Move> GenerateMovesForPiece(int startSquare, int piece)
    {
        moves = new();
        square120 = Board.GetSquare120();
        currentPositionState = Board.GetCurrentPositionState();

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

    /// <summary>
    /// Die Funktion <c>GenerateMoves</c> generiert alle moeglichen Zuege in einer Brettstellung fuer die spielende Partei.
    /// </summary>
    /// <returns>Liste mit allen moeglichen Zuegen fuer die spielende Partei</returns>
    public static List<Move> GenerateMoves()
    {
        square120 = Board.GetSquare120();
        ulong[] bitboards = Board.GetBitboards();
        ulong allPiecesMap = new();
        List<Move> moves = new();

        friendlyColor = Board.GetWhiteToMove() ? Piece.WHITE : Piece.BLACK;
        
        for (int pieceType = friendlyColor + 1; pieceType < friendlyColor + 7; pieceType++)
        {
            allPiecesMap |= bitboards[pieceType];
        }

        for (int sq = 0; sq < 64; sq++)
        {
            ulong mask = 1ul << sq;

            if ((allPiecesMap & mask) != 0)
            {
                // Spielfigur des Spielers auf diesem Feld (8x8-Darstellung)
                int sqIndex = Board.ConvertIndex64To120(sq);
                int piece = square120[sqIndex];

                if (Piece.IsPieceType(piece) != Piece.NONE)
                {
                    moves.AddRange(GenerateMovesForPiece(sqIndex, square120[sqIndex]));
                }
            }
        }

        return moves;
    }

    /// <summary>
    /// Die Funktion <c>GenerateSlidingMoves</c> generiert alle moeglichen Zuege fuer ein Laeufer, Turm oder eine Dame.
    /// </summary>
    /// <param name="startSquare">Das Ursprungsfeld in der 12x10-Darstellung</param>
    /// <param name="piece">Die Figurenart</param>
    static void GenerateSlidingMoves(int startSquare, int piece)
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

    /// <summary>
    /// Die Funktion <c>GenerateKnightMoves</c> generiert alle moeglichen Zuege fuer ein Pferd.
    /// </summary>
    /// <param name="startSquare">Das Ursprungsfeld in der 12x10-Darstellung</param>
    /// <param name="piece">Die Figurenart</param>
    static void GenerateKnightMoves(int startSquare, int piece)
    {
        for (int dirIndex = 0; dirIndex < 8; dirIndex++)
        {
            int targetSquare = startSquare + KnightOffsets[dirIndex];
            int pieceOnTargetSquare = square120[targetSquare];

            LegitimateMove(pieceOnTargetSquare, startSquare, targetSquare, piece);
        }
    }

    /// <summary>
    /// Die Funktion <c>GeneratePawnMoves</c> generiert alle moeglichen Zuege fuer ein Bauer.
    /// </summary>
    /// <param name="startSquare">Das Ursprungsfeld in der 12x10-Darstellung</param>
    /// <param name="piece">Die Figurenart</param>
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

    /// <summary>
    /// Die Funktion <c>GenerateKingMoves</c> generiert alle moeglichen Zuege fuer Koenig.
    /// </summary>
    /// <param name="startSquare">Das Ursprungsfeld in der 12x10-Darstellung</param>
    /// <param name="piece">Die Figurenart</param>
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

    /// <summary>
    /// Ueberprueft, ob ein Zug legal ist.
    /// </summary>
    /// <param name="pieceOnTargetSquare">Der Wert der internen Brettdarstellung auf dem Zielfeld des Zuges</param>
    /// <param name="startSquare">Das Ursprungsfeld in der 12x10-Darstellung</param>
    /// <param name="targetSquare">Das Zielfeld in der 12x10-Darstellung</param>
    /// <param name="piece">Die Figurenart</param>
    /// <param name="doubleAdvance">Ist der Zug ein doppelter Bauerzug?</param>
    /// <returns>Gibt zurueck, ob der Zug gespielt werden darf</returns>
    static bool LegitimateMove(int pieceOnTargetSquare, int startSquare, int targetSquare, int piece, bool doubleAdvance = false)
    {
        // Spielfeldrand erreicht
        if (pieceOnTargetSquare == -1)
        {
            return false;
        }

        // Blockiert von eigenen Figur
        if (Piece.IsColor(pieceOnTargetSquare, friendlyColor))
        {
            return false;
        }

        bool promotion = CanPromote(targetSquare, piece);

        // Freies Feld
        if (!CanCapture(pieceOnTargetSquare, startSquare, targetSquare, piece))
        {
            if (promotion) GeneratePromotionMoves(startSquare, targetSquare, false);
            else
            {
                // Doppelter Bauerzug, kreiert ein mögliches EnPassant Schlagfeld.
                if (doubleAdvance)
                {
                    moves.Add(new Move(startSquare, targetSquare, Move.PawnTwoUpFlag));
                }
                else moves.Add(new Move(startSquare, targetSquare));
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

    /// <summary>
    /// Die Funktion <c>CanCapture</c> ueberprueft, ob eine gegnerische Figur mit dem Zug geschlagen werden kann.
    /// </summary>
    /// <param name="pieceOnTargetSquare">Der Wert der internen Brettdarstellung auf dem Zielfeld des Zuges</param>
    /// <param name="startSquare">Das Ursprungsfeld in der 12x10-Darstellung</param>
    /// <param name="targetSquare">Das Zielfeld in der 12x10-Darstellung</param>
    /// <param name="piece">Die Figurenart</param>
    /// <returns>Gibt zurueck, ob eine Figur geschlagen werden kann</returns>
    static bool CanCapture(int pieceOnTargetSquare, int startSquare, int targetSquare, int piece)
    {
        // Blockiert von gegnerischen Figur
        if (Piece.IsColor(pieceOnTargetSquare, opponentColor))
        {
            if (CanPromote(targetSquare, piece)) GeneratePromotionMoves(startSquare, targetSquare, true);
            else moves.Add(new Move(startSquare, targetSquare));

            return true;
        }
        // Ueberprueft bei einem Bauer, ob ein EnPassant-Schlag moeglich ist
        else if (Piece.IsPawn(piece))
        {
            if (targetSquare == currentPositionState.enPassantSquare)
            {
                moves.Add(new Move(startSquare, targetSquare, Move.EnPassantCaptureFlag));
            }
        }

        return false;
    }

    /// <summary>
    /// Die Funktion <c>CanPromote</c> ueberprueft, ob eine Umwandlung stattfinden kann.
    /// </summary>
    /// <param name="targetSquare">Das Zielfeld in der 12x10-Darstellung</param>
    /// <param name="piece"></param>
    /// <returns></returns>
    static bool CanPromote(int targetSquare, int piece)
    {
        if (!Piece.IsPawn(piece))
            return false;

        if (targetSquare >= 21 && targetSquare <= 28 || targetSquare >= 91 && targetSquare <= 98)
            return true;

        return false;
    }

    /// <summary>
    /// Die Funktion <c>GeneratePromotionMoves</c> generiert alle Umwandlungszuege.
    /// </summary>
    /// <param name="startSquare">Das Ursprungsfeld in der 12x10-Darstellung</param>
    /// <param name="targetSquare">Das Zielfeld in der 12x10-Darstellung</param>
    /// <param name="capture">Wird dabei eine Figur geschlagen?</param>
    static void GeneratePromotionMoves(int startSquare, int targetSquare, bool capture)
    {
        if (capture)
        {
            if (targetSquare == startSquare + WhitePawnOffsets[0] || targetSquare == startSquare + BlackPawnOffsets[0])
                return;
        }

        moves.Add(new Move(startSquare, targetSquare, Move.PromoteToQueenFlag));
        moves.Add(new Move(startSquare, targetSquare, Move.PromoteToRookFlag));
        moves.Add(new Move(startSquare, targetSquare, Move.PromoteToBishopFlag));
        moves.Add(new Move(startSquare, targetSquare, Move.PromoteToKnightFlag));
    }

    /// <summary>
    /// Die Funktion <c>GenerateCastlingMoves</c> generiert alle Rochadezuege.
    /// </summary>
    /// <param name="startSquare">Das Ursprungsfeld in der 12x10-Darstellung</param>
    static void GenerateCastlingMoves(int startSquare)
    {
        int[] CastleOffsets = { 2, -2 };

        foreach (int offset in CastleOffsets)
        {
            bool kingside;
            bool permission;

            kingside = offset > 0;

            // Stellt sicher, dass der Spieler rochieren darf.
            if (Piece.IsColor(friendlyColor, Piece.WHITE))
            {
                if (kingside) if (!currentPositionState.HasKingsideCastleRight(true)) continue;
                if (!kingside) if (!currentPositionState.HasQueensideCastleRight(true)) continue;
            }
            else
            {
                if (kingside) if (!currentPositionState.HasKingsideCastleRight(false)) continue;
                if (!kingside) if (!currentPositionState.HasQueensideCastleRight(false)) continue;
            }

            // Stellt sicher, dass der Spieler rochieren kann.
            permission = GenerateCastlingPermission(startSquare, offset, kingside);

            if (permission)
            {
                int targetSquare = startSquare + offset;

                moves.Add(new Move(startSquare, targetSquare, Move.CastleFlag));
            }
        }
    }

    /// <summary>
    /// Die Funktion <c>GenerateCastlingPermission</c> ueberprueft, ob ein Koenig rochieren darf.
    /// </summary>
    /// <param name="startSquare">Das Ursprungsfeld in der 12x10-Darstellung</param>
    /// <param name="offset">Offset fuer den Koenig (2 oder -2)</param>
    /// <param name="kingside">Handelt es sich um eine kurze oder lange Rochade?</param>
    /// <returns>Gibt zurueck, ob der Koenig diesen Rochadezug ausfuehren darf</returns>
    static bool GenerateCastlingPermission(int startSquare, int offset, bool kingside)
    {
        // Ueberprueft, ob die Felder zwischen Koenig und Turm leer sind.
        for (int i = 1; i < 4; i++)
        {
            int square = startSquare + (i * (offset / 2));

            if (square != 28 && square != 98)
            {
                if (square120[square] != Piece.NONE)
                {
                    return false;
                }
            }
        }

        // Stellt sicher, dass die mitrochierende Figur ein eigener Turm ist.
        if (kingside)
        {
            if (square120[startSquare + 3] != (Piece.ROOK | friendlyColor))
            {
                return false;
            }

        }
        else
        {
            if (square120[startSquare - 4] != (Piece.ROOK | friendlyColor))
            {
                return false;
            }

        }

        return true;
    }
}
