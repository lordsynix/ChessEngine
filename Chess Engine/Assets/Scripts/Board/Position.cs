using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MoveGenerator;

public class Position
{
    // Position Informations
    public bool GameOver = false;
    public int Evaluation = -1;

    public Move bestMove;

    public List<Position> ChildPositions;

    public List<Move> PossibleMoves;

    public Position(List<Move> possibleMoves, int depth)
    {
        PossibleMoves = possibleMoves;
        ChildPositions = GetChildPositions(depth);

        Evaluation = Engine.Evaluate();
    }

    public List<Move> GetPossibleMoves()
    {
        if (PossibleMoves == null)
        {
            Debug.LogError("PossibleMoves aren't generated yet");
        }
        return PossibleMoves;
    }

    public List<Position> GetChildPositions(int depth)
    {
        if (depth == 0)
        {
            return null;
        }

        ChildPositions = new();

        foreach (Move move in PossibleMoves)
        {
            Board.MakeMove(move, true);

            Position childPosition = new(GenerateMoves(), depth - 1);
            ChildPositions.Add(childPosition);

            Board.UnmakeMove(move);
        }

        return ChildPositions;
    }
}
