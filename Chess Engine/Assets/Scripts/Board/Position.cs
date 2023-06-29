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

    public Position(List<Move> moves, List<int[]> piecesList, int friendlyColor, int opponentColor)
    {
        PossibleMoves = moves;
        PiecesList = piecesList;
        FriendlyColor = friendlyColor;
        OpponentColor = opponentColor;

        GameOver = IsOver();
    }

    public List<Position> GetChildPositions()
    {
        ChildPositions = new();

        foreach (Move move in PossibleMoves)
        {
            Board.MakeMove(move);

            FriendlyColor = Board.GetWhiteToMove() ? Piece.WHITE : Piece.BLACK;
            OpponentColor = Piece.OpponentColor(FriendlyColor);
            ChildPositions.Add(new(GenerateMoves(), Board.GetPieceLocation(), FriendlyColor, OpponentColor));

            Board.UnmakeMove(move);
        }

        return ChildPositions;
    }

    private bool IsOver()
    {
        return false;
    }

}
