using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MoveGenerator;

public class Position
{
    // Position Informations
    public bool GameOver = false;
    public int Evaluation = -1;

    public Move BestMove;

    private List<Move> PossibleMoves;
    private List<Position> ChildPositions;

    public Position(bool store = true, bool generateChilds = false)
    {
        PossibleMoves = GenerateMoves();
        GameOver = PossibleMoves.Count == 0;

        Evaluation = Engine.Evaluate();

        if (generateChilds)
        {
            GetChildPositions();
        }
        else
        {
            // Add position to the transposition table
            if (store) TranspositionTable.Store(new(Evaluation));
        }
    }

    public List<Move> GetPossibleMoves()
    {
        if (PossibleMoves == null)
        {
            Debug.LogError("PossibleMoves aren't generated yet");
        }
        return PossibleMoves;
    }

    public List<Position> GetChildPositions()
    {
        if (ChildPositions == null)
        {
            ChildPositions = new();

            foreach (Move move in PossibleMoves)
            {
                Board.MakeMove(move, true);

                Position childPosition = new(false);
                ChildPositions.Add(childPosition);

                Board.UnmakeMove(move);
            }
        }

        return ChildPositions;
    }

    public void RemoveInvalidItems(List<Move> illegalMoves, List<Position> illegalPositions)
    {
        // Filtert die illegalen Zuege aus den Moeglichen heraus.
        PossibleMoves.RemoveAll(move => illegalMoves.Contains(move));
        ChildPositions.RemoveAll(pos => illegalPositions.Contains(pos));
    }
}
