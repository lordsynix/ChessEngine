using System.Collections.Generic;
using static PieceSquareTables;
using System.Linq;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Xml.Schema;

public static class Engine
{
    public static TranspositionTable transpositionTable;

    public static int searchDepth = 3;
    public static int tableSizeMB = 8;

    private static int friendlyColor = -1;
    private static long positionsEvaluated = 0;

    private static Stopwatch stopwatch;

    public struct SearchResult
    {
        public int Evaluation;
        public List<Move> SearchPath;

        public SearchResult(int evaluation, List<Move> searchPath)
        {
            Evaluation = evaluation;
            SearchPath = searchPath;
        }
    }

    const int pawnValue = 100;
    const int knightValue = 300;
    const int bishopValue = 320;
    const int rookValue = 500;
    const int queenValue = 900;
    const int kingValue = 12000;

    public static void Search()
    {
        Log.Message($"Started search with depth {searchDepth}...");

        stopwatch.Start();

        // Generiert und validiert alle moeglichen Zuege
        List<Move> possibleMoves = GetPossibleMoves();
        
        // Eine Seite hat keine moeglichen Zuege mehr
        if (possibleMoves.Count == 0)
        {
            GameManager.Instance.OnCheckMate();
            GameManager.Instance.PossibleMoves = possibleMoves;
            return;
        }

        GameManager.Instance.PossibleMoves = possibleMoves;

        // Initialisiert die Suche des bestmoeglichen Zuges.
        positionsEvaluated = 0;

        // Startet die Alpha-Beta Suche, um einen Vorteil bei bester Antwort des Gegenspielenden zu finden
        SearchResult result = AlphaBeta(possibleMoves, searchDepth, int.MinValue, int.MaxValue, Board.GetWhiteToMove());

        // Aktualisiert den Wert des UI-Elements neben dem Spielbrett (Evaluation Bar)
        GameManager.Instance.SetEvaluationBar(result.Evaluation);

        Debug.Log("Alpha-Beta search: " + result.Evaluation);

        stopwatch.Stop();

        foreach (Move move in result.SearchPath)
        {
            if (move != null) Debug.Log(Board.DesignateMove(move));
        }

        //Log.Message($"Bestmove: {Board.DesignateMove(searchPath[0])}");
        Debug.Log($"Depth {searchDepth}: Positions evaluated: {positionsEvaluated}: Time: {stopwatch.ElapsedMilliseconds} ms");
        Debug.Log("--------------------------------");

        if (Board.GetPlayerColor() == Piece.WHITE != Board.GetWhiteToMove())
        {
            GameManager.Instance.MakeEngineMove(result.SearchPath[0]);
        }
    }

    public static List<Move> GetPossibleMoves()
    {
        friendlyColor = Board.GetWhiteToMove() ? Piece.WHITE : Piece.BLACK;

        List<Move> possibleMoves = MoveGenerator.GenerateMoves();
        List<Move> illegalMoves = new();

        int prevKingSq = -1;

        foreach (Move move in possibleMoves)
        {
            Board.MakeMove(move, true);

            int kingSq = GetKingSquare(prevKingSq);

            // Aktualisiert das Königfeld, falls der König sich bewegt hat
            if (move.StartSquare == kingSq) kingSq = move.TargetSquare;
            
            List<Move> responses = MoveGenerator.GenerateMoves();
            
            foreach (Move response in responses)
            {
                // Wenn der König geschlagt werden kann, stand der König im Schach
                if (response.TargetSquare == kingSq)
                {
                    illegalMoves.Add(move);
                }
            }

            Board.UnmakeMove(move, true);
        }
        possibleMoves.RemoveAll(move => illegalMoves.Contains(move));
        
        return possibleMoves;
    }

    private static int GetKingSquare(int prevKingSq)
    {
        ulong kingBitboard = Board.GetBitboards()[Piece.KING | friendlyColor];

        if (prevKingSq != -1)
        {
            ulong prevMask = 1ul << prevKingSq;
            if ((kingBitboard & prevMask) != 0) return prevKingSq;
        }

        ulong mask = 1ul;

        for (int sq = 0; sq < 64; sq++)
        {
            if ((kingBitboard & mask) != 0)
            {
                return sq;
            }
            mask <<= 1;
        }

        Debug.LogWarning("King bitboard doesn't contain a position for the king");
        return -1;
    }

    public static SearchResult AlphaBeta(List<Move> possibleMoves, int depth, int alpha, int beta, bool isWhite, Move parentMove = null)
    {
        if (depth == 0 || possibleMoves.Count == 0)
        {
            var searchPath = new List<Move>() { parentMove };
            return new SearchResult(Evaluate(depth), searchPath);
        }

        if (isWhite)
        {
            int maxEvaluation = int.MinValue;
            SearchResult bestResult = new();
            foreach (Move move in possibleMoves)
            {
                Board.MakeMove(move, true);
                SearchResult result = AlphaBeta(MoveGenerator.GenerateMoves(), depth - 1, alpha, beta, false, move);
                Board.UnmakeMove(move, true);

                if (result.Evaluation > maxEvaluation)
                {
                    maxEvaluation = result.Evaluation;
                    bestResult = result;
                }

                if (alpha > result.Evaluation) alpha = result.Evaluation;
                if (beta <= alpha) break;
            }
            bestResult.SearchPath.Add(parentMove);
            return new(maxEvaluation, bestResult.SearchPath);
        }
        else
        {
            int minEvaluation = int.MaxValue;
            SearchResult bestResult = new();
            foreach (Move move in possibleMoves)
            {
                Board.MakeMove(move, true);
                SearchResult result = AlphaBeta(MoveGenerator.GenerateMoves(), depth - 1, alpha, beta, true, move);
                Board.UnmakeMove(move, true);

                if (result.Evaluation < minEvaluation)
                {
                    minEvaluation = result.Evaluation;
                    bestResult = result;
                }

                if (beta < result.Evaluation) beta = result.Evaluation;
                if (beta <= alpha) break;
            }
            bestResult.SearchPath.Add(parentMove);
            return new(minEvaluation, bestResult.SearchPath);
        }
    }

    public static int Evaluate(int depth = -1)
    {
        // Ueberprueft in der Transpositionstabelle, ob die Bewertung fuer diese Position bereits existiert
        PositionState currentPositionState = Board.GetCurrentPositionState();
        Entry entry = transpositionTable.Lookup(currentPositionState.zobristKey);

        if (entry != null) return entry.Evaluation;

        // Evaluiert die Position, die momentan in der internen Brettdarstellung dargestellt wird
        int evaluation = 0;
        int whiteEvaluation = 0;
        int blackEvaluation = 0;
        ulong[] bitboards = Board.GetBitboards();

        int[] sides = { Piece.WHITE, Piece.BLACK };

        foreach (int color in sides)
        {
            // Positive Evaluation bedeutet Vorteil fuer Weiss, negativ fuer Schwarz.
            int colorMultiplier = (color == Piece.WHITE) ? 1 : -1;

            for (int pieceType = color + 1; pieceType < color + 7; pieceType++)
            {
                ulong bitboard = bitboards[pieceType];
                if (bitboard == 0) continue;

                var piecePositions = Board.GetPiecePositions(bitboard);
                foreach (int square in piecePositions)
                {
                    evaluation += EvaluatePiecePosition(square, color, pieceType);
                }

                int count = piecePositions.Count();

                if (Piece.IsType(pieceType, Piece.KING)) evaluation += kingValue;
                else if (Piece.IsType(pieceType, Piece.PAWN)) evaluation += count * pawnValue;
                else if (Piece.IsType(pieceType, Piece.KNIGHT)) evaluation += count * knightValue;
                else if (Piece.IsType(pieceType, Piece.BISHOP)) evaluation += count * bishopValue;
                else if (Piece.IsType(pieceType, Piece.ROOK)) evaluation += count * rookValue;
                else if (Piece.IsType(pieceType, Piece.QUEEN)) evaluation += count * queenValue;
                
            }
            whiteEvaluation += color == Piece.WHITE ? evaluation : 0;
            blackEvaluation += color == Piece.BLACK ? evaluation : 0;
            evaluation = 0;
        }
        //Debug.Log("White " + whiteEvaluation + " Black " + blackEvaluation);

        positionsEvaluated++;
        evaluation = whiteEvaluation - blackEvaluation;

        transpositionTable.Store(currentPositionState.zobristKey, evaluation, depth);

        return evaluation;
    }

    private static int EvaluatePiecePosition(int square, int color, int pieceType)
    {
        try
        {
            if (Piece.IsType(pieceType, Piece.KING))
                return color == Piece.WHITE ? WhiteKingValues[square] : BlackKingValues[square];
            else if (Piece.IsType(pieceType, Piece.PAWN))
                return color == Piece.WHITE ? WhitePawnValues[square] : BlackPawnValues[square];
            else if (Piece.IsType(pieceType, Piece.KNIGHT))
                return color == Piece.WHITE ? WhiteKnightValues[square] : BlackKnightValues[square];
            else if (Piece.IsType(pieceType, Piece.BISHOP))
                return color == Piece.WHITE ? WhiteBishopValues[square] : BlackBishopValues[square];
            else if (Piece.IsType(pieceType, Piece.ROOK))
                return color == Piece.WHITE ? WhiteRookValues[square] : BlackRookValues[square];
            else if (Piece.IsType(pieceType, Piece.QUEEN))
                return color == Piece.WHITE ? WhiteQueenValues[square] : BlackQueenValues[square];
            else
            {
                Debug.LogError("Couldn't find a pieceType for the given piece");
                return 0;
            }
        }
        catch
        {
            return 0;
        }
        
    }

    public static void Initialize()
    {
        int tableSize = 1024 * 1024 * tableSizeMB;

        transpositionTable = new TranspositionTable(tableSize);
        stopwatch = new Stopwatch();
    }
}
