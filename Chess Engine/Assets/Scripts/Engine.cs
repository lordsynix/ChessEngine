using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MoveGenerator;

public static class Engine
{
    [HideInInspector] public static string positionFEN = "";

    public static Position currentPosition;

    public static Dictionary<int, int> positionEvaluation = new();

    public static int friendlyColor;
    public static int opponentColor;

    private static int positionIndex = 0;

    const int pawnValue = 100;
    const int knightValue = 300;
    const int bishopValue = 300;
    const int rookValue = 500;
    const int queenValue = 900;

    public static Position Search()
    {
        float startTime = Time.realtimeSinceStartup;

        friendlyColor = Board.GetWhiteToMove() ? Piece.WHITE : Piece.BLACK;

        currentPosition = new(Board.GetPieceLocation(), Board.GetWhiteToMove(), Board.GetEnPassantSquare());

        // Stellt sicher, dass der Koenig nicht geschlagen werden kann.
        List<Move> illegalMoves = new();
        List<Position> illegalPositions = new();

        int responses = 0;
        foreach (Position pos in currentPosition.GetChildPositions())
        {
            responses += pos.PossibleMoves.Count;
            int kingSq = pos.PiecesList[Piece.KING | friendlyColor][0];

            int index = currentPosition.ChildPositions.IndexOf(pos);
            Move m = currentPosition.PossibleMoves[index];

            if (m.StartSquare == kingSq) kingSq = m.TargetSquare;
            // TODO Check if castling squares are attacked. (m.Type == 3 || m.Type == 4)

            foreach (Move move in pos.PossibleMoves)
            {
                if (move.TargetSquare == kingSq)
                {
                    illegalMoves.Add(currentPosition.PossibleMoves[index]);
                    illegalPositions.Add(pos);
                }
            }
        }

        // Filtert die illegalen Zuege aus den Moeglichen heraus.
        currentPosition.PossibleMoves.RemoveAll(move => illegalMoves.Contains(move));
        currentPosition.ChildPositions.RemoveAll(pos => illegalPositions.Contains(pos));

        // Ueberprueft, ob noch Zuege moeglich sind.
        if (currentPosition.PossibleMoves.Count == 0)
        {
            currentPosition.GameOver = true;
            GameManager.instance.CheckMate();
            return currentPosition;
        }

        // Evaluiert die beste Position nach einer Zugabfolge.
        positionEvaluation = new();
        int evaluation = Minimax(currentPosition, 1, friendlyColor == Piece.WHITE);
        int posIndex = positionEvaluation.FirstOrDefault(x => x.Value == evaluation).Key;
        currentPosition.bestMove = currentPosition.PossibleMoves[posIndex];

        /*Debug.Log($"{currentPosition.PossibleMoves.Count} possible moves and {responses} possible responses -------- " +
                  $"Time: {(Time.realtimeSinceStartup - startTime) * 1000} ms");*/

        GameManager.instance.SetPositionStats(currentPosition.PossibleMoves.Count, responses);
        positionIndex = 0;

        return currentPosition;
    }

    public static int Minimax(Position pos, int depth, bool isWhite)
    {
        if (depth == 0 || pos.GameOver)
        {
            int evaluation = Evaluate(pos);
            positionEvaluation.Add(positionIndex, evaluation);
            positionIndex++;
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
            return Evaluate(pos);

        if (isWhite)
        {
            int maxEvaluation = int.MinValue;
            foreach (Position childPos in pos.GetChildPositions())
            {
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
            foreach (Position childPos in pos.GetChildPositions())
            {
                int evaluation = AlphaBeta(childPos, depth - 1, alpha, beta, true);
                if (evaluation < minEvaluation) minEvaluation = evaluation;
                if (beta < evaluation) beta = evaluation;
                if (beta <= alpha) break;
            }
            return minEvaluation;
        }
    }

    public static int Evaluate(Position pos)
    {
        int material = 0;

        int[] sides = { Piece.WHITE, Piece.BLACK };

        foreach(int color in sides)
        {
            // Positive Evaluation bedeutet Vorteil fuer Weiss, negativ fuer Schwarz.
            int colorMultiplier = (color == Piece.WHITE) ? 1 : -1;

            // Evaluiert den Wert der Figuren fuer beide Seiten.
            for (int pieceType = color + 1; pieceType < color + 7; pieceType++)
            {
                if (pos.PiecesList[pieceType].Length == 0) continue;
                if (Piece.IsType(pieceType, Piece.KING)) continue;

                int count = 0;
                foreach (int piece in pos.PiecesList[pieceType])
                {
                    if (piece == 0) continue;
                    count++;
                }

                if (count == 0) continue;

                if (Piece.IsType(pieceType, Piece.PAWN)) material += count * pawnValue * colorMultiplier;
                else if (Piece.IsType(pieceType, Piece.KNIGHT)) material += count * knightValue * colorMultiplier;
                else if (Piece.IsType(pieceType, Piece.BISHOP)) material += count * bishopValue * colorMultiplier;
                else if (Piece.IsType(pieceType, Piece.ROOK)) material += count * rookValue * colorMultiplier;
                else if (Piece.IsType(pieceType, Piece.QUEEN)) material += count * queenValue * colorMultiplier;
            }
        }
        return material;
    }
}
