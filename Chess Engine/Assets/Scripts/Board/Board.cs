using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor.PackageManager;
using UnityEngine;

public static class Board 
{
    private static int PlayerColor = -1;

    private static int MoveCount = 0;

    private static int[] Square64;
    private static int[] Square120;

    private static ulong[] Bitboards;

    private static bool WhiteToMove = true;

    private static PositionState CurrentPositionState;
    public static Stack<PositionState> PositionStateHistory;
    public static Stack<ulong> RepetitionPositionHistroy;
    public static List<Move> AllGameMoves;

    
    #region ZUG

    public static void MakeMove(Move move, bool engineSearch = false)
    {
        try
        {
            int startSquare120 = ConvertIndex64To120(move.StartSquare);
            int targetSquare120 = ConvertIndex64To120(move.TargetSquare);
            int moveFlag = move.MoveFlag;
            bool isPromotion = move.IsPromotion;
            bool isEnPassant = moveFlag is Move.EnPassantCaptureFlag;

            int moveColor = WhiteToMove ? Piece.WHITE : Piece.BLACK;
            int movedPiece = Square120[startSquare120];
            int capturedPiece = isEnPassant ? Piece.PAWN | Piece.OpponentColor(moveColor) : Square120[targetSquare120];
            int capturedPieceType = Piece.IsPieceType(capturedPiece);

            int prevCastlingRights = CurrentPositionState.castlingRights;
            int prevEnPassantSquare = CurrentPositionState.enPassantSquare;
            ulong newZobristKey = CurrentPositionState.zobristKey;
            int newCastlingRights = CurrentPositionState.castlingRights;
            int newEnPassantSquare = -1;

            if (moveFlag == Move.CastleFlag && !Piece.IsType(movedPiece, Piece.KING)) Debug.LogError("Castle Error");

            // Aktualisiert die bewegte Figur
            MovePiece(movedPiece, startSquare120, targetSquare120);

            // Schlagen einer Figur
            if (capturedPiece != Piece.NONE)
            {
                int captureSquare = targetSquare120;

                // En Passant Schlag
                if (isEnPassant)
                {
                    captureSquare = targetSquare120 + (WhiteToMove ? -10 : 10);
                    Square120[captureSquare] = Piece.NONE;
                }

                // Aktualisiert das bitboard der geschlagenen Figur
                ToggleSquare(capturedPiece, captureSquare);
                newZobristKey ^= Zobrist.pieceKeys[ConvertIndex120To64(captureSquare), capturedPiece];
            }

            // Rochade
            if (Piece.IsType(movedPiece, Piece.KING))
            {
                newCastlingRights &= WhiteToMove ? 0b1100 : 0b0011;

                if (moveFlag == Move.CastleFlag)
                {
                    int rook = Piece.ROOK | moveColor;
                    bool kingside = targetSquare120 == 27 || targetSquare120 == 97;
                    int oldRookSq = kingside ? startSquare120 + 3 : startSquare120 - 4;
                    int newRookSq = kingside ? startSquare120 + 1 : startSquare120 - 1;

                    // Aktualisiert die Position des Turms
                    MovePiece(rook, oldRookSq, newRookSq);

                    newZobristKey ^= Zobrist.pieceKeys[ConvertIndex120To64(oldRookSq), rook];
                    newZobristKey ^= Zobrist.pieceKeys[ConvertIndex120To64(newRookSq), rook];
                }
            }

            // Verwandlung
            if (isPromotion)
            {
                int promotionPieceType = moveFlag switch
                {
                    Move.PromoteToQueenFlag => Piece.QUEEN,
                    Move.PromoteToRookFlag => Piece.ROOK,
                    Move.PromoteToKnightFlag => Piece.KNIGHT,
                    Move.PromoteToBishopFlag => Piece.BISHOP,
                    _ => 0
                };

                int promotionPiece = promotionPieceType | moveColor;

                // Ersetzt den Bauern, welcher sich bereits in der letzten 
                // Reihe befindet, mit der Figur, in die er sich verwandelt. 
                ToggleSquare(movedPiece, targetSquare120);
                ToggleSquare(promotionPiece, targetSquare120);
                Square120[targetSquare120] = promotionPiece;
            }

            // En Passant
            if (moveFlag == Move.PawnTwoUpFlag)
            {
                newEnPassantSquare = targetSquare120 + (WhiteToMove ? 10 : -10);
                newZobristKey ^= Zobrist.enPassantKeys[Zobrist.enPassantSquares.IndexOf(newEnPassantSquare)];
            }

            // Aktualisiert die Rochaderechte.
            if (prevCastlingRights != 0)
            {
                if (targetSquare120 == 98 || startSquare120 == 98)
                {
                    newCastlingRights &= PositionState.ClearWhiteKingsideMask;
                }
                else if (targetSquare120 == 91 || startSquare120 == 91)
                {
                    newCastlingRights &= PositionState.ClearWhiteQueensideMask;
                }
                else if (targetSquare120 == 28 || startSquare120 == 28)
                {
                    newCastlingRights &= PositionState.ClearBlackKingsideMask;
                }
                else if (targetSquare120 == 21 || startSquare120 == 21)
                {
                    newCastlingRights &= PositionState.ClearBlackQueensideMask;
                }
            }

            // Aktualisiere Zobrist-Hash mit den neuen Figurenpositionen und dem ziehenden Spieler
            newZobristKey ^= Zobrist.whiteToMoveKey;
            newZobristKey ^= Zobrist.pieceKeys[move.StartSquare, movedPiece];
            newZobristKey ^= Zobrist.pieceKeys[move.TargetSquare, Square120[targetSquare120]];
            newZobristKey ^= Zobrist.enPassantKeys[Zobrist.enPassantSquares.IndexOf(prevEnPassantSquare)];

            if (newCastlingRights != prevCastlingRights)
            {
                newZobristKey ^= Zobrist.castlingKeys[prevCastlingRights]; // loescht alte Rochaderechte
                newZobristKey ^= Zobrist.castlingKeys[newCastlingRights]; // fuegt neue Rochaderechte hinzu
            }

            // Player to move
            WhiteToMove = !WhiteToMove;

            // Erhoeht die Anzahl der gespielten Zuege
            MoveCount++;
            int newFiftyMoveCounter = CurrentPositionState.fiftyMoveCounter + 1;

            // Bauernzuege oder das Schlagen einer Figur erneuert die 50-Zuege-Regel. Ausserdem
            // erneuern diese Bedingungen das Unentschieden durch dreifache Zugwiederholung.
            if (Piece.IsType(movedPiece, Piece.PAWN) || capturedPieceType != Piece.NONE)
            {
                if (!engineSearch)
                {
                    RepetitionPositionHistroy.Clear();
                }
                newFiftyMoveCounter = 0;
            }

            // Neuer Position State
            PositionState newState = new(capturedPieceType, newEnPassantSquare, newCastlingRights, newFiftyMoveCounter, newZobristKey);
            PositionStateHistory.Push(newState);
            CurrentPositionState = newState;

            if (!engineSearch)
            {
                AllGameMoves.Add(move);

                RepetitionPositionHistroy.Push(newState.zobristKey);

                Diagnostics.Instance.UpdateDebugInformation(GameManager.Instance.DebugMode);
                Engine.Search();
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"Error while making move {DesignateMove(move)}");
            Debug.LogException(ex);
        }
    }

    public static void UnmakeMove(Move move, bool engineSearch = false)
    {
        try
        {
            // Swap colour to move
            WhiteToMove = !WhiteToMove;

            bool undoingWhiteMove = WhiteToMove;

            // Get move info
            int movedFrom = ConvertIndex64To120(move.StartSquare);
            int movedTo = ConvertIndex64To120(move.TargetSquare);
            int moveFlag = move.MoveFlag;
            int moveColor = undoingWhiteMove ? Piece.WHITE : Piece.BLACK;

            bool undoingEnPassant = moveFlag == Move.EnPassantCaptureFlag;
            bool undoingPromotion = move.IsPromotion;
            bool undoingCapture = CurrentPositionState.capturedPieceType != Piece.NONE;

            int movedPiece = undoingPromotion ? Piece.PAWN | moveColor : Square120[movedTo];
            int movedPieceType = Piece.IsPieceType(movedPiece);
            int capturedPieceType = CurrentPositionState.capturedPieceType;

            // If undoing promotion, then remove piece from promotion square and replace with pawn
            if (undoingPromotion)
            {
                int promotedPiece = Square120[movedTo];

                ToggleSquare(promotedPiece, movedTo);
                ToggleSquare(movedPiece, movedTo);
            }

            MovePiece(movedPiece, movedTo, movedFrom);

            // Undo capture
            if (undoingCapture)
            {
                int captureSquare = movedTo;
                int capturedPiece = capturedPieceType | Piece.OpponentColor(moveColor);

                if (undoingEnPassant)
                {
                    captureSquare = movedTo + (undoingWhiteMove ? -10 : 10);
                }

                // Add back captured piece
                ToggleSquare(capturedPiece, captureSquare);

                Square120[captureSquare] = capturedPiece;
            }


            // Update king
            if (movedPieceType is Piece.KING)
            {
                // Undo castling
                if (moveFlag is Move.CastleFlag)
                {
                    int rookPiece = Piece.ROOK | moveColor;
                    bool kingside = movedTo == 97 || movedTo == 27;
                    int rookSquareBeforeCastling = kingside ? movedTo + 1 : movedTo - 2;
                    int rookSquareAfterCastling = kingside ? movedTo - 1 : movedTo + 1;

                    // Undo castling by returning rook to original square
                    MovePiece(rookPiece, rookSquareAfterCastling, rookSquareBeforeCastling);
                }
            }

            if (!engineSearch && RepetitionPositionHistroy.Count > 0)
            {
                RepetitionPositionHistroy.Pop();
            }
            if (!engineSearch)
            {
                AllGameMoves.RemoveAt(AllGameMoves.Count - 1);
            }

            // Go back to previous state
            PositionStateHistory.Pop();
            CurrentPositionState = PositionStateHistory.Peek();
            MoveCount--;

        }
        catch (Exception ex)
        {
            Debug.Log($"Error while unmaking move {DesignateMove(move)}");
            Debug.LogException(ex);
        }
    }

    private static void MovePiece(int piece, int startSquare, int targetSquare)
    {
        ToggleSquares(piece, startSquare, targetSquare);

        Square120[startSquare] = Piece.NONE;
        Square120[targetSquare] = piece;
    }

    private static void ToggleSquare(int piece, int squareIndex)
    {
        Bitboards[piece] ^= 1ul << ConvertIndex120To64(squareIndex);
    }

    private static void ToggleSquares(int piece, int squareA, int squareB)
    {
        Bitboards[piece] ^= (1ul << ConvertIndex120To64(squareA) | 1ul << ConvertIndex120To64(squareB));
    }

    #endregion

    #region Getter

    public static int[] GetSquare120()
    {
        return Square120;
    }

    public static int[] GetSquare64()
    {
        return GetSquare64From120();
    }

    public static bool GetWhiteToMove()
    {
        return WhiteToMove;
    }

    public static int GetPlayerColor()
    {
        return PlayerColor;
    }

    public static PositionState GetCurrentPositionState()
    {
        return CurrentPositionState;
    }

    public static ulong[] GetBitboards()
    {
        return Bitboards;
    }

    public static int PieceOnSquare(int sqIndex120)
    {
        return Square120[sqIndex120];
    }

    public static IEnumerable<int> GetPiecePositions(ulong bitboard)
    {
        for (int i = 0; i < 64; i++)
        {
            if (((bitboard >> i) & 1UL) == 1UL)
            {
                yield return i;
            }
        }
    }

    public static int GetMoveCount()
    {
        return MoveCount;
    }

    #endregion

    #region Setter

    public static void SetSquare64(int[] square64)
    {
        if (square64.Length == 64)
        {
            Square64 = square64;
            Square120 = GetSquare120From64();
            SetBitboards();
        }
    }

    public static void SetSquare120(int[] square120)
    {
        if (square120.Length == 120)
        {
            Square120 = square120;
            Square64 = GetSquare64From120();
            SetBitboards();
        }
    }

    public static void SetBitboards()
    {
        Bitboards = new ulong[23];

        for (int sqIndex = 0; sqIndex < 64; sqIndex++)
        {
            int piece = Square64[sqIndex];
            if (piece != Piece.NONE)
            {
                Bitboards[piece] |= 1ul << sqIndex;
            }
        }
    }

    public static void SetWhiteToMove(bool whiteToMove)
    {
        WhiteToMove = whiteToMove;
    }

    public static void SetPlayerColor(char symbol)
    {
        if (symbol == '-') PlayerColor = UnityEngine.Random.Range(0, 2) == 0 ? Piece.WHITE : Piece.BLACK;
        else if (symbol == 'w') PlayerColor = Piece.WHITE;
        else if (symbol == 'b') PlayerColor = Piece.BLACK;
    }

    public static void SetMoveCount(int moveCount)
    {
        MoveCount =  Math.Max(0, (moveCount - 1) * 2 + (WhiteToMove ? 0 : 1));
    }

    public static void SetCurrentPositionState(PositionState state)
    {
        CurrentPositionState = state;

        Log.Message($"Position State updated - Captured Piece: {state.capturedPieceType} EnPasSq: {state.enPassantSquare} " +
                    $"CastlingRights: {state.castlingRights} FiftyMoves: {state.fiftyMoveCounter} ZobristHash: {state.zobristKey}");
    }

    #endregion

    #region Brettdarstellung

    /// Die Funktion <c>Get120From64</c> gibt, ausgehend von der 8x8-Darstellung,
    /// die Werte fuer die 12x10-Darstellung zurueck.
    /// </summary>
    /// <returns>Gibt den entsprechenden Square120 Array zurueck.</returns>
    public static int[] GetSquare120From64()
    {
        Square120 = new int[120];

        // Setzt alle Felder auf den Wert -1
        for (int i = 0; i < Square120.Length; i++)
        {
            Square120[i] = -1;
        }

        // Liest die Werte der 8x8-Darstellung fuer die 12x10-Darstellung.
        int j = 19;
        for (int i = 0; i < 64; i++)
        {
            if (i % 8 == 0)
            {
                j += 2;
                Square120[j] = -1;
            }

            Square120[j] = Square64[i];
            j++;
        }

        return Square120;
    }

    /// <summary>
    /// Die Funktion <c>Get64From120</c> gibt, ausgehend von der 12x10-Darstellung,
    /// die Werte fuer die 8x8-Darstellung zurueck.
    /// </summary>
    /// <returns>Gibt den entsprechenden Square64 Array zurueck.</returns>
    public static int[] GetSquare64From120()
    {

        Square64 = new int[64];

        int curSq = 0;

        foreach (int sq in Square120)
        {
            if (sq != -1)
            {
                Square64[curSq] = sq;
                curSq++;
            }
        }
        return Square64;
    }

    /// <summary>
    /// Die Funktion <c>ConvertIndex64To120</c> gibt, ausgehend eines Index der 
    /// 8x8-Darstellung, den entsprechenden Index in der 12x10-Darstellung zurueck.
    /// </summary>
    /// <returns>Gibt den entsprechenden Index der 12x10-Darstellung zurueck.</returns>
    public static int ConvertIndex64To120(int index)
    {
        int file = index % 8;
        int rank = (index - file) / 8;
        return 21 + rank * 10 + file;
    }

    /// <summary>
    /// Die Funktion <c>ConvertIndex120To64</c> gibt, ausgehend eines Index der 12x10-Darstellung,
    /// den entsprechenden Index in der 8x8-Darstellung zurueck.
    /// </summary>
    /// <returns>Gibt den entsprechenden Index der 8x8-Darstellung zurueck.</returns>
    public static int ConvertIndex120To64(int index)
    {
        int file = (index % 10) - 1;
        int rank = ((index - (index % 10)) / 10) - 2;
        return rank * 8 + file;
    }

    /// <summary>
    /// Die Funktion <c>DesignateMove</c> beschriftet einen Zug in der algebraischen Notation.
    /// </summary>
    /// <param name="move">Den zu beschriftenden Zug.</param>
    /// <returns>Verstaendlichere Beschriftung als String.</returns>
    public static string DesignateMove(Move move)
    {
        return DesignateSquare(ConvertIndex64To120(move.StartSquare)) + DesignateSquare(ConvertIndex64To120(move.TargetSquare));
    }

    /// <summary>
    /// Die Funktion <c>DesignateSquare</c> beschriftet ein Feld auf dem Brett in der algebraischen Notation.
    /// </summary>
    /// <param name="move">Das zu beschriftende Feld als Index in der 10x12-Darstellung.</param>
    /// <returns>Algebraische Notation des Feldes als String.</returns>
    public static string DesignateSquare(int index)
    {
        string s = "";

        int file = (index % 10) - 1;
        int rank = 8 - (((index - (index % 10)) / 10) - 2);

        s += (char)(65 + file);
        s += rank;
        return s;
    }

    public static int SquareToIndex(string algebraicNotation)
    {
        if (algebraicNotation.Length != 2)
        {
            Debug.LogWarning("Input must be in the algebraic chess notation for one square");
            return -1;
        }

        char fileChar = algebraicNotation[0];
        char rankChar = algebraicNotation[1];

        if (fileChar < 'a' || fileChar > 'h' || rankChar < '1' || rankChar > '8')
        {
            Debug.LogWarning("Input must be in the algebraic chess notation for one square");
            return -1;
        }

        // Convert file and rank to corresponding integers
        int fileIndex = fileChar - 'a';
        int rankIndex = '8' - rankChar;

        // Calculate the index in a 12x10 board representation
        int index = 21 + fileIndex + rankIndex * 10;
        Debug.Log(algebraicNotation + " : " + index + " : " + fileIndex + " : " + rankIndex);
        return index;
    }

    #endregion

    public static void Initialize()
    {
        AllGameMoves = new();

        Square64 = new int[64];
        Square120 = new int[120];

        PositionStateHistory = new Stack<PositionState>(capacity: 64);
        RepetitionPositionHistroy = new Stack<ulong>(capacity: 64);

        MoveCount = 0;

        Bitboards = new ulong[23];
    }
}
