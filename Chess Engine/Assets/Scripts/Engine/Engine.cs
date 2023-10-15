using System.Collections.Generic;
using static PieceSquareTables;
using System.Linq;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

/// <summary>
/// Die Klasse <c>Engine</c> bildet unter anderem das Backend meiner Anwendung und ist 
/// fuer die Suche und Bewertung von Folgezuegen in einer Brettstellung zustaendig.
/// </summary>
public static class Engine
{
    public static TranspositionTable transpositionTable;

    public static int searchDepth = 3;
    public static int tableSizeMB = 8;

    private static int friendlyColor = -1;
    private static long positionsEvaluated = 0;

    private static Stopwatch stopwatch;

    // Das Resultat eines Suchvorgangs beinhaltet eine Evaluation sowie einen Suchpfad.
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

    // Ausgangswerte fuer die materielle Evaluation
    const int pawnValue = 100;
    const int knightValue = 300;
    const int bishopValue = 320;
    const int rookValue = 500;
    const int queenValue = 900;
    const int kingValue = 12000;

    /// <summary>
    /// Die Funktion <c>StartSearch</c> startet die Suche von Folgezuegen fuer eine gewisse Tiefe.
    /// </summary>
    public static void StartSearch()
    {
        Log.Message($"Started search with depth {searchDepth}...");

        stopwatch.Start();

        Search();
    }

    /// <summary>
    /// Die Funktion <c>Search</c> definiert die moeglichen Zuege fuer die Benutzeroberflaeche und sucht Folgezuege in einer Position.
    /// </summary>
    private static void Search()
    {
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

        // Initialisiert die Suche des bestmoeglichen Zuges
        positionsEvaluated = 0;

        // Startet die Alpha-Beta Suche, um einen Vorteil bei bester Antwort des Gegenspielenden zu finden
        SearchResult result = AlphaBeta(possibleMoves, searchDepth, int.MinValue, int.MaxValue, Board.GetWhiteToMove());

        // Formattiert den Suchpfad um
        result.SearchPath.Reverse();

        // Speichert die Resultate
        //FinishedSearch = true;
        //searchResult = result;

        //searchThread.Abort();
        StopSearch(result);
    }

    /// <summary>
    /// Die Funktion <c>StopSearch</c> verarbeitet die Resultate der Suche und spielt den besten Enginezug.
    /// </summary>
    /// <param name="result"></param>
    public static void StopSearch(SearchResult result)
    {
        // Aktualisiert den Wert des UI-Bewertungselement neben dem Spielbrett (Evaluation Bar)
        GameManager.Instance.SetEvaluationBar(result.Evaluation);

        stopwatch.Stop();

        string searchPath = "";
        foreach (Move move in result.SearchPath)
        {
            if (move != null) searchPath += Board.DesignateMove(move) + ": ";
        }

        Debug.Log($"Evaluation: {result.Evaluation}Depth {searchDepth}: " +
                  $"Positions evaluated: {positionsEvaluated}: Time: {stopwatch.ElapsedMilliseconds} ms");
        Debug.Log(searchPath);
        Debug.Log("--------------------------------");

        // Spielt den besten Zug, falls nicht die Anwendenden am Zug sind
        if (Board.GetPlayerColor() == Piece.WHITE != Board.GetWhiteToMove())
        {
            GameManager.Instance.MakeEngineMove(result.SearchPath[0]);
        }

        Diagnostics.Instance.UpdateTranspositionTableVisuals();
    }

    /// <summary>
    /// Die Funktion <c>GetPossibleMoves</c> gibt die moeglichen Zuege unter Beruecksichtigung der "Schach"-Regel zurueck.
    /// </summary>
    /// <returns></returns>
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
        
        return OrderMoves(possibleMoves);
    }

    /// <summary>
    /// Die Funktion <c>GetKingSquare</c> gibt den Feldindex des spielenden Koenigs in der 8x8-Darstellung zurueck.
    /// </summary>
    /// <param name="prevKingSq"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Die Funktion <c>OrderMoves</c> ist eine einfache Version des MoveOrdering, welches die Suche 
    /// beschleunigen sollte. Wertet Rochaden, Umwandlungen und Schlagzuege in der Sortierung auf.
    /// </summary>
    /// <param name="moves">Die zu sortierenden Zuege</param>
    /// <returns></returns>
    private static List<Move> OrderMoves(List<Move> moves)
    {
        List<Move> orderedMoves = new();

        // Priorisiert Rochaden und Umwandlungen
        foreach (Move move in moves)
        {
            if (move.MoveFlag == Move.CastleFlag || move.MoveFlag == Move.PromoteToQueenFlag)
            {
                orderedMoves.Add(move);
            }
        }
        moves = moves.Except(orderedMoves).ToList();

        // Sortiert die Schlagzuege bevorzugt
        foreach (Move move in moves)
        {
            if (Board.PieceOnSquare(Board.ConvertIndex64To120(move.TargetSquare)) != Piece.NONE)
            {
                orderedMoves.Add(move);
            }
            if (move.MoveFlag == Move.EnPassantCaptureFlag)
            {
                orderedMoves.Add(move);
            }
        }
        moves = moves.Except(orderedMoves).ToList();

        // Fuegt alle verbleibenden Zuege hinzu
        orderedMoves.AddRange(moves);

        return orderedMoves;
    }

    /// <summary>
    /// Die Funktion <c>AlphaBeta</c> bildet den rekursiven Alpha-Beta-Suchalgorithmus. Gibt 
    /// die bestmoegliche Position nach einer gewissen fuer die spielende Partei zurueck.
    /// </summary>
    /// <param name="possibleMoves">Moegliche Zuege in dieser Iteration</param>
    /// <param name="depth">Verbleibende Suchtiefe</param>
    /// <param name="alpha">Alpha-Wert: Start bei negativer Unentlichkeit</param>
    /// <param name="beta">Beta-Wert: Start bei positiver Unentlichkeit</param>
    /// <param name="isWhite">Maximierende oder minimierende Partei</param>
    /// <param name="moveStack">Stapel der Evaluierten Zuege, um den Suchpfad zu bilden</param>
    /// <returns></returns>
    public static SearchResult AlphaBeta(List<Move> possibleMoves, int depth, int alpha, int beta, bool isWhite, Stack<Move> moveStack = null)
    {
        // Erstellt einen neuen Zugstapel, wenn dieser null ist
        moveStack ??= new Stack<Move>();

        // Abbruchbedingungen: Erreichte Suchtiefe, Patt oder Schachmatt
        if (depth == 0 || possibleMoves.Count == 0)
        {
            return new SearchResult(Evaluate(), new List<Move>(moveStack));
        }

        SearchResult bestResult = new();

        // Maximierende Partei
        if (isWhite)
        {
            int maxEvaluation = int.MinValue;

            foreach (Move move in possibleMoves)
            {
                Board.MakeMove(move, true);
                moveStack.Push(move);

                SearchResult result = AlphaBeta(MoveGenerator.GenerateMoves(), depth - 1, alpha, beta, false, moveStack);

                Board.UnmakeMove(move, true);
                moveStack.Pop();

                // Besseres Ergebnis als zuvor
                if (result.Evaluation > maxEvaluation)
                {
                    maxEvaluation = result.Evaluation;
                    bestResult = result;
                }

                if (alpha > result.Evaluation) alpha = result.Evaluation;
                if (beta <= alpha) break;
            }

            return new SearchResult(maxEvaluation, new List<Move>(bestResult.SearchPath));
        }
        // Minimierende Partei
        else
        {
            int minEvaluation = int.MaxValue;

            foreach (Move move in possibleMoves)
            {
                Board.MakeMove(move, true);
                moveStack.Push(move);

                SearchResult result = AlphaBeta(MoveGenerator.GenerateMoves(), depth - 1, alpha, beta, true, moveStack);

                Board.UnmakeMove(move, true);
                moveStack.Pop();

                // Besseres Ergebnis als zuvor
                if (result.Evaluation < minEvaluation)
                {
                    minEvaluation = result.Evaluation;
                    bestResult = result;
                }

                if (beta < result.Evaluation) beta = result.Evaluation;
                if (beta <= alpha) break;
            }

            return new SearchResult(minEvaluation, new List<Move>(bestResult.SearchPath));
        }
    }

    /// <summary>
    /// Die Funktion <c>Evaluate</c> evaluiert eine Brettstellung nach Material und Figurenpositionen.
    /// </summary>
    /// <returns></returns>
    public static int Evaluate()
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

        positionsEvaluated++;
        evaluation = whiteEvaluation - blackEvaluation;

        transpositionTable.Store(currentPositionState.zobristKey, evaluation);

        return evaluation;
    }

    /// <summary>
    /// Die Funktion <c>EvaluatePiecePosition</c> bewertet die Figurenstellung einer Figur auf dem Brett.
    /// </summary>
    /// <param name="square">Feldindex in der 8x8-Darstellung</param>
    /// <param name="color">Die Farbe der Figur</param>
    /// <param name="pieceType">Die zu evaluierende Figurenart</param>
    /// <returns></returns>
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
