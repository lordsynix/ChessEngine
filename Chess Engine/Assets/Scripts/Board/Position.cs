using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MoveGenerator;

public class Position
{
    public bool GameOver = false;

    // Position
    public bool WhiteToMove;
    public int EnPassantSquare;

    public bool WhiteCastleKingside;
    public bool WhiteCastleQueenside;
    public bool BlackCastleKingside;
    public bool BlackCastleQueenside;

    public List<Position> ChildPositions;

    public List<Move> PossibleMoves;
    public List<int[]> PiecesList;

    public Position(List<int[]> piecesList, bool whiteToMove, int enPassantSquare)
    {
        PiecesList = piecesList;

        WhiteToMove = whiteToMove;
        EnPassantSquare = enPassantSquare;

        bool[] permissions = Board.GetCastlePermissions();

        WhiteCastleKingside = permissions[0];
        WhiteCastleQueenside = permissions[1];
        BlackCastleKingside = permissions[2];
        BlackCastleQueenside = permissions[3];

        PossibleMoves = GetPossibleMoves();
        GameOver = IsOver();
    }

    public List<Move> GetPossibleMoves()
    {
        if (PossibleMoves == null)
        {
            var moves = GenerateMoves();
            GameOver = moves.Count == 0;

            return moves;
        }
        
        return PossibleMoves;
    }

    public List<Position> GetChildPositions()
    {
        if (ChildPositions != null) return ChildPositions;

        ChildPositions = new();

        foreach (Move move in GetPossibleMoves())
        {
            Board.MakeMove(move, true, this);

            ChildPositions.Add(new(Board.GetPieceLocation(), Board.GetWhiteToMove(), move.EnPassant));

            Board.UnmakeMove(move, this);
        }

        return ChildPositions;
    }

    private bool IsOver()
    {
        return false;
    }

}
