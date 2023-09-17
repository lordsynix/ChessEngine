using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MoveGenerator;
using static PieceSquareTables;

public static class Engine
{
    [HideInInspector] public static string positionFEN = "";
    public const int searchDepth = 2;

    public static Position currentPosition;

    public static int friendlyColor;
    public static int opponentColor;

    const int pawnValue = 82;
    const int knightValue = 337;
    const int bishopValue = 365;
    const int rookValue = 477;
    const int queenValue = 1025;
    const int kingValue = 12000;

    public static Position Search()
    {
        Log.Message($"Starting search... Depth: {searchDepth}");

        friendlyColor = Board.GetWhiteToMove() ? Piece.WHITE : Piece.BLACK;

        currentPosition = new(false, true);

        ValidateMoves();

        // Ueberprueft, ob noch Zuege moeglich sind.
        if (currentPosition.GameOver)
        {
            Log.Message($"Checkmate or Stalemate: 0 Possible Moves");
            GameManager.instance.CheckMate();
            return currentPosition;
        }

        int evaluation = AlphaBeta(currentPosition, searchDepth, int.MinValue, int.MaxValue, friendlyColor == Piece.WHITE);
        Debug.Log($"Evaluation: {evaluation} Stored Positions: {TranspositionTable.transpositionTable.Count}");

        int responses = -1;
        GameManager.instance.SetPositionStats(currentPosition.GetPossibleMoves().Count, responses, currentPosition.Evaluation);

        return currentPosition;
    }

    private static void ValidateMoves()
    {
        // Stellt sicher, dass der Koenig in einem Folgezug nicht geschlagen werden kann.
        List<Move> illegalMoves = new();
        List<Position> illegalPositions = new();

        List<Move> possibleMoves = currentPosition.GetPossibleMoves();
        List<Position> childPositions = currentPosition.GetChildPositions();

        foreach (Position childPos in childPositions)
        {
            int kingSq = Board.GetPieceLocation()[Piece.KING | friendlyColor][0];
            int index = childPositions.IndexOf(childPos);

            Move m = possibleMoves[index];

            if (m.StartSquare == kingSq) kingSq = m.TargetSquare;
            // TODO Check if castling squares are attacked. (m.Type == 3 || m.Type == 4)

            foreach (Move move in childPos.GetPossibleMoves())
            {
                if (move.TargetSquare == kingSq)
                {
                    illegalMoves.Add(currentPosition.GetPossibleMoves()[index]);
                    illegalPositions.Add(childPos);
                }
            }
        }

        currentPosition.RemoveInvalidItems(illegalMoves, illegalPositions);
    }

    public static int Minimax(Position pos, int depth, bool isWhite)
    {
        if (depth == 0 || pos.GameOver)
        {
            int evaluation = pos.Evaluation;
            return evaluation;
        }

        if (isWhite)
        {
            int maxEvaluation = int.MinValue;
            foreach (Position childPos in pos.GetChildPositions())
            {
                int evaluation = Minimax(childPos, depth - 1, false);
                if (evaluation > maxEvaluation) maxEvaluation = evaluation;
            }
            return maxEvaluation;
        }
        else
        {
            int minEvaluation = int.MaxValue;
            foreach (Position childPos in pos.GetChildPositions())
            {
                int evaluation = Minimax(childPos, depth - 1, true);
                if (evaluation < minEvaluation) minEvaluation = evaluation;
            }
            return minEvaluation;
        }
    }

    public static int AlphaBeta(Position pos, int depth, int alpha, int beta, bool isWhite)
    {
        if (depth == 0 || pos.GameOver)
            return Evaluate();

        if (isWhite)
        {
            int maxEvaluation = int.MinValue;
            foreach (Move move in pos.GetPossibleMoves())
            {
                Position childPos = GeneratePosition(move);

                int evaluation = AlphaBeta(childPos, depth - 1, alpha, beta, false);
                if (evaluation > maxEvaluation) maxEvaluation = evaluation;
                if (alpha > evaluation) alpha = evaluation;
                if (beta <= alpha) break;
            }
            return maxEvaluation;
        }
        else
        {
            int minEvaluation = int.MaxValue;
            foreach (Move move in pos.GetPossibleMoves())
            {
                Position childPos = GeneratePosition(move);

                int evaluation = AlphaBeta(childPos, depth - 1, alpha, beta, true);
                if (evaluation < minEvaluation) minEvaluation = evaluation;
                if (beta < evaluation) beta = evaluation;
                if (beta <= alpha) break;
            }
            return minEvaluation;
        }
    }

    private static Position GeneratePosition(Move move)
    {
        Board.MakeMove(move, true);
        Position pos = new();
        Board.UnmakeMove(move);

        return pos;
    }

    public static int Evaluate()
    {
        int evaluation = 0;
        var piecesList = Board.GetPieceLocation();

        int[] sides = { Piece.WHITE, Piece.BLACK };

        foreach(int color in sides)
        {
            // Positive Evaluation bedeutet Vorteil fuer Weiss, negativ fuer Schwarz.
            int colorMultiplier = (color == Piece.WHITE) ? 1 : -1;
            
            // Evaluiert den Wert der Figuren fuer beide Seiten.
            for (int pieceType = color + 1; pieceType < color + 7; pieceType++)
            {
                if (piecesList[pieceType].Length == 0) continue;

                int count = 0;
                foreach (int piecePosition in piecesList[pieceType])
                {
                    // Zaehlt die Anzahl der Figuren pro Typ.
                    if (piecePosition == 0) continue;
                    count++;

                    // Schaetzt die Position der jeweiligen Figur auf dem Brett ein.
                    evaluation += EvaluatePiecePosition(Board.ConvertIndex120To64(piecePosition), color, pieceType) * colorMultiplier;
                }

                if (count == 0) continue;

                if (Piece.IsType(pieceType, Piece.KING)) evaluation += kingValue * colorMultiplier;
                else if (Piece.IsType(pieceType, Piece.PAWN)) evaluation += count * pawnValue * colorMultiplier;
                else if (Piece.IsType(pieceType, Piece.KNIGHT)) evaluation += count * knightValue * colorMultiplier;
                else if (Piece.IsType(pieceType, Piece.BISHOP)) evaluation += count * bishopValue * colorMultiplier;
                else if (Piece.IsType(pieceType, Piece.ROOK)) evaluation += count * rookValue * colorMultiplier;
                else if (Piece.IsType(pieceType, Piece.QUEEN)) evaluation += count * queenValue * colorMultiplier;
            }
        }
        return evaluation;
    }

    private static int EvaluatePiecePosition(int posIndex, int color, int pieceType)
    {
        if (Piece.IsType(pieceType, Piece.KING))
            return color == Piece.WHITE ? WhiteKingValues[posIndex] : BlackKingValues[posIndex];
        else if (Piece.IsType(pieceType, Piece.PAWN))
            return color == Piece.WHITE ? WhitePawnValues[posIndex] : BlackPawnValues[posIndex];
        else if (Piece.IsType(pieceType, Piece.KNIGHT))
            return color == Piece.WHITE ? WhiteKnightValues[posIndex] : BlackKnightValues[posIndex];
        else if (Piece.IsType(pieceType, Piece.BISHOP))
            return color == Piece.WHITE ? WhiteBishopValues[posIndex] : BlackBishopValues[posIndex];
        else if (Piece.IsType(pieceType, Piece.ROOK))
            return color == Piece.WHITE ? WhiteRookValues[posIndex] : BlackRookValues[posIndex];
        else if (Piece.IsType(pieceType, Piece.QUEEN))
            return color == Piece.WHITE ? WhiteQueenValues[posIndex] : BlackQueenValues[posIndex];
        else
        {
            Debug.LogError("Couldn't find a pieceType for the given piece");
            return 0;
        }
    }
}
