using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MoveGenerator;

public class Position
{
    public bool GameOver = false;

    public List<Position> ChildPositions;

    public List<Move> PossibleMoves;
    public List<int[]> PiecesList;

    public int FriendlyColor;
    public int OpponentColor;

    public Position(List<int[]> piecesList, int friendlyColor, int opponentColor)
    {
        PiecesList = piecesList;
        FriendlyColor = friendlyColor;
        OpponentColor = opponentColor;

        PossibleMoves = GetPossibleMoves();
        GameOver = IsOver();
    }

    public List<Move> GetPossibleMoves()
    {
        if (PossibleMoves == null)
        {
            return GenerateMoves();
        }
        
        return PossibleMoves;
    }

    public List<Position> GetChildPositions()
    {
        if (ChildPositions != null) return ChildPositions;

        ChildPositions = new();

        foreach (Move move in PossibleMoves)
        {
            Board.MakeMove(move);

            FriendlyColor = Board.GetWhiteToMove() ? Piece.WHITE : Piece.BLACK;
            OpponentColor = Piece.OpponentColor(FriendlyColor);
            ChildPositions.Add(new(Board.GetPieceLocation(), FriendlyColor, OpponentColor));

            Board.UnmakeMove(move);
        }

        return ChildPositions;
    }

    private bool IsOver()
    {
        return false;
    }

}
