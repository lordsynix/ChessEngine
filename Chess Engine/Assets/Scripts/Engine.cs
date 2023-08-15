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

    public static Dictionary<int, int> positionEvaluation = new();

    public static int friendlyColor;
    public static int opponentColor;

    private static int positionIndex = 0;

    const int pawnValue = 82;
    const int knightValue = 337;
    const int bishopValue = 365;
    const int rookValue = 477;
    const int queenValue = 1025;
    const int kingValue = 12000;

    public static Position Search()
    {
        LogManager.Instance.LogMessage($"Starting search... Depth: {searchDepth}");

        float startTime = Time.realtimeSinceStartup;

        friendlyColor = Board.GetWhiteToMove() ? Piece.WHITE : Piece.BLACK;

        currentPosition = new(GenerateMoves(), searchDepth);

        // Stellt sicher, dass der Koenig in einem Folgezug nicht geschlagen werden kann.
        List<Move> illegalMoves = new();
        List<Position> illegalPositions = new();

        int responses = 0;
        foreach (Position pos in currentPosition.ChildPositions)
        {
            responses += pos.GetPossibleMoves().Count;
            int kingSq = Board.GetPieceLocation()[Piece.KING | friendlyColor][0];

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
            LogManager.Instance.LogMessage($"Checkmate or Stalemate: 0 Possible Moves");
            currentPosition.GameOver = true;
            GameManager.instance.CheckMate();
            return currentPosition;
        }

        LogManager.Instance.LogMessage($"Generated {currentPosition.PossibleMoves.Count} Possible Moves");

        // Evaluiert die beste Position nach einer Zugabfolge.
        positionEvaluation = new();
        int evaluation = Minimax(currentPosition, searchDepth, friendlyColor == Piece.WHITE);
        int posIndex = positionEvaluation.FirstOrDefault(x => x.Value == evaluation).Key;
        currentPosition.bestMove = currentPosition.PossibleMoves[posIndex];

        //Debug.Log($"Evaluation: {evaluation} PositionEvaluation: {currentPosition.Evaluation}");
        /*Debug.Log($"{currentPosition.PossibleMoves.Count} possible moves and {responses} possible responses -------- " +
                  $"Time: {(Time.realtimeSinceStartup - startTime) * 1000} ms");*/

        GameManager.instance.SetPositionStats(currentPosition.PossibleMoves.Count, responses, currentPosition.Evaluation);
        positionIndex = 0;

        return currentPosition;
    }

    public static int Minimax(Position pos, int depth, bool isWhite)
    {
        if (depth == 0 || pos.GameOver)
        {
            int evaluation = pos.Evaluation;
            positionEvaluation.Add(positionIndex, evaluation);
            positionIndex++;
            return evaluation;
        }

        if (isWhite)
        {
            int maxEvaluation = int.MinValue;
            foreach (Position childPos in pos.ChildPositions)
            {
                int evaluation = Minimax(childPos, depth - 1, false);
                if (evaluation > maxEvaluation) maxEvaluation = evaluation;
            }
            return maxEvaluation;
        }
        else
        {
            int minEvaluation = int.MaxValue;
            foreach (Position childPos in pos.ChildPositions)
            {
                int evaluation = Minimax(childPos, depth - 1, true);
                if (evaluation < minEvaluation) minEvaluation = evaluation;
            }
            return minEvaluation;
        }
    }

    /*public static int AlphaBeta(Position pos, int depth, int alpha, int beta, bool isWhite)
    {
        if (depth == 0 || pos.GameOver)
            return Evaluate();

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
    }*/

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
