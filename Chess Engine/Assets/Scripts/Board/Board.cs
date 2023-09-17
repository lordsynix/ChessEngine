using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static MoveGenerator;

/// <summary>
/// Die Klasse <c>Board</c> ist fuer die interne Brettdarstellung des Spielfelds zustaendig.
/// Verwaltet alle wichtigen Informationen ueber das Schachspiel.
/// </summary>
public static class Board
{
    public enum Mode
    {
        HumanComputer,
        HumanHuman,
        ComputerComputer,
        Testing
    }

    private static Mode GameMode;

    private static int PlayerColor = -1;

    private static int MoveCount = 0;

    private static int[] Square64;
    private static int[] Square120;

    private static bool WhiteToMove = true;
    private static int EnPassantSquare = -1;

    private static bool WhiteCastleKingside = false;
    private static bool WhiteCastleQueenside = false;
    private static bool BlackCastleKingside = false;
    private static bool BlackCastleQueenside = false;

    private static List<int[]> PiecesList = new();

    private static List<int> LastCaptures = new();
    private static List<int> LastEnPassant = new();

    private static bool _WhiteCastleKingside;
    private static bool _WhiteCastleQueenside;
    private static bool _BlackCastleKingside;
    private static bool _BlackCastleQueenside;

    #region SETTER AND GETTER

    public static int GetPlayerColor()
    {
        if (PlayerColor == -1) return SetPlayerColor('-');
        else return PlayerColor;
    }

    /// <summary>
    /// Die Funktion <c>SetPlayerColor</c> gibt ausgehend von einem uebergebenen Symbol die Farbe des Spielers zurueck.
    /// </summary>
    /// <param name="symbol">Die uebergebene Farbe als char-Wert.</param>
    public static int SetPlayerColor(char symbol)
    {
        int color;

        // Waelt zufaellige Farbe fuer den Spieler aus.
        if (symbol == '-') color = UnityEngine.Random.Range(0, 2) == 0 ? Piece.WHITE : Piece.BLACK;

        else if (symbol == 'w') color = Piece.WHITE;
        else if (symbol == 'b') color = Piece.BLACK;
        else
        {
            Debug.LogError("Color symbol not valid");
            color = -1;
        } 
            
        PlayerColor = color;

        return PlayerColor;
    }

    public static Mode GetGameMode()
    {
        return GameMode;
    }

    public static void SetGameMode(Mode mode)
    {
        GameMode = mode;

        Log.Message($"Enter GameMode: {mode}");
    }

    /// <summary>
    /// Die Methode <c>GetSquare64</c> gibt einen Square Array zurueck, speichert Piece.Type
    /// und Piece.Color als int-Wert fuer jedes Feld in der 8x8-Darstellung.
    /// </summary>
    /// <returns>Gibt die 8x8-Darstellung des Spielfelds als int[64] zurueck.</returns>
    public static int[] GetSquare64()
    {
        return Square64;
    }

    public static void SetSquare64(int[] s)
    {
        if (s.Length != 64)
        {
            Debug.LogError("The Value of Board.Square must have 64 Int elements");
            return;
        }
        Square64 = s;
        Square120 = GetSquare120From64();
        PiecesList = SetPieceLocation();
    }

    /// <summary>
    /// Die Methode <c>GetSquare120</c> gibt einen Square Array zurueck, speichert Piece.Type
    /// und Piece.Color als int-Wert fuer jedes Feld in der 12x10-Darstellung.
    /// </summary>
    /// <returns>Gibt die 12x10-Darstellung des Spielfelds als int[120] zurueck.</returns>
    public static int[] GetSquare120()
    {
        return Square120;
    }

    public static void SetSquare120(int[] s)
    {
        if (s.Length != 120)
        {
            Debug.LogError("The Value of Board.Square must have 120 Int elements");
            return;
        }
        Square120 = s;
        Square64 = GetSquare64From120();
        PiecesList = SetPieceLocation();
    }

    /// <summary>
    /// Die Methode <c>GetWhiteToMove</c> verwaltet den Spieler, der als naechstes spielen kann.
    /// </summary>
    /// <returns>Gibt einen bool zurueck, ob weiss am Zug ist.</returns>
    public static bool GetWhiteToMove()
    {
        return WhiteToMove;
    }

    public static void SetWhiteToMove(bool w)
    {
        WhiteToMove = w;
    }

    public static int GetEnPassantSquare()
    {
        return EnPassantSquare;
    }

    public static void SetEnPassantSquare(int e)
    {
        EnPassantSquare = e;
    }

    public static bool[] GetCastlePermissions()
    {
        return new bool[]
        {
            WhiteCastleKingside,
            WhiteCastleQueenside,
            BlackCastleKingside,
            BlackCastleQueenside
        };
    }

    public static string GetCastlePermissionsAsString()
    {
        string permissions = "";

        _ = WhiteCastleKingside ? "K" : "";
        _ = WhiteCastleQueenside ? "Q" : "";
        _ = BlackCastleKingside ? "k" : "";
        _ = BlackCastleQueenside ? "q" : "";

        return permissions == "" ? "-" : permissions;
    }

    public static void SetCastlePermissions(string s)
    {
        WhiteCastleKingside = false; WhiteCastleQueenside = false;
        BlackCastleKingside = false; BlackCastleQueenside = false;

        if (s == "") return;

        foreach (char symbol in s)
        {
            if (char.ToLower(symbol) == 'k')
            {
                if (char.IsUpper(symbol)) WhiteCastleKingside = true;
                else BlackCastleKingside = true;
            }
            else if (char.ToLower(symbol) == 'q')
            {
                if (char.IsUpper(symbol)) WhiteCastleQueenside = true;
                else BlackCastleQueenside = true;
            }
            else if (symbol != '-') Error.instance.OnError();
        }
    }

    public static void SetCastlePermissionsWithBools(bool whiteCastleKingside, bool whiteCastleQueenside, 
                                                     bool blackCastleKingside, bool blackCastleQueenside)
    {
        WhiteCastleKingside = whiteCastleKingside;
        WhiteCastleQueenside = whiteCastleQueenside;
        BlackCastleKingside = blackCastleKingside;
        BlackCastleQueenside = blackCastleQueenside;
    }

    public static void SetWhiteCastleKingside(bool whiteCastleKingside)
    {
        WhiteCastleKingside = whiteCastleKingside;
    }

    public static void SetWhiteCastleQueenside(bool whiteCastleQueenside)
    {
        WhiteCastleQueenside = whiteCastleQueenside;
    }

    public static void SetBlackCastleKingside(bool blackCastleKingside)
    {
        BlackCastleKingside = blackCastleKingside;
    }

    public static void SetBlackCastleQueenside(bool blackCastleQueenside)
    {
        BlackCastleQueenside = blackCastleQueenside;
    }

    public static int GetMoveCount()
    {
        return MoveCount;
    }

    /// <summary>
    /// Die Methode <c>GetPieceLocation</c> gibt die Position für alle Figuren auf dem Brett in Form eines int-Wertes zurueck.
    /// </summary>
    /// <returns>Gibt die Position aller Figuren auf dem Brett zurueck</returns>
    public static List<int[]> GetPieceLocation()
    {
        return PiecesList;
    }

    /// <summary>
    /// Die Methode <c>SetPieceLocation</c> speichert, ausgehend von der 8x8-Darstellung
    /// die Position für alle Figuren auf dem Brett in Form eines int-Wertes.
    /// </summary>
    public static List<int[]> SetPieceLocation()
    {
        // Generiert eine Liste von int[] fuer jede Figurenart, in welchen die Position der jeweiligen Figur gespeichert werden.
        PiecesList = Piece.GeneratePiecesList();

        for (int sq = 0; sq < Square64.Length; sq++)
        {
            int value = Square64[sq];
            if (value == Piece.NONE) continue;

            for (int i = 0; i < PiecesList[value].Length; i++)
            {
                // Ueberschreibt den default Zustand, bei welchem alle Positionen 0 sind.
                if (PiecesList[value][i] == 0)
                {
                    PiecesList[value][i] = ConvertIndex64To120(sq);
                    break;
                }
            }
        }

        return PiecesList;
    }

    public static void SetPieceLocationWithList(List<int[]> pieces)
    {
        PiecesList = pieces;
    }

    public static int PieceOnSquare(int square)
    {
        return Square120[square];
    }

    #endregion

    #region BRETTDARSTELLUNG

    /// <summary>
    /// Die Methode <c>Get120From64</c> veraendert, ausgehend von der 8x8-Darstellung
    /// die Werte fuer die 12x10-Darstellung.
    /// </summary>
    /// <returns>Gibt den entsprechenden Square120 Array zurück.</returns>
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
    /// Die Methode <c>Get64From120</c> veraendert, ausgehend von der 12x10-Darstellung
    /// die Werte fuer die 8x8-Darstellung.
    /// </summary>
    /// <returns>Gibt den entsprechenden Square64 Array zurück.</returns>
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
    /// Die Methode <c>ConvertIndex64To120</c> gibt, ausgehend eines Index der 
    /// 8x8-Darstellung den entsprechenden Index in der 12x10-Darstellung zurueck.
    /// </summary>
    /// <returns>Gibt den entsprechenden Index der 12x10-Darstellung zurueck.</returns>
    public static int ConvertIndex64To120(int index)
    {
        int file = index % 8;
        int rank = (index - file) / 8;
        return 21 + rank * 10 + file;
    }

    /// <summary>
    /// Die Methode <c>ConvertIndex120To64</c> gibt, ausgehend eines Index der 12x10-Darstellung 
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
    /// Die Funktion <c>DesignateMove</c> beschriftet einen Zug in einer intuitiveren Form.
    /// </summary>
    /// <param name="move">Den zu beschriftenden Zug.</param>
    /// <returns>Verstaendlichere Beschriftung als String.</returns>
    public static string DesignateMove(Move move)
    {
        return DesignateSquare(move.StartSquare) + DesignateSquare(move.TargetSquare);
    }

    /// <summary>
    /// Die Funktion <c>DesignateSquare</c> beschriftet ein Feld auf dem Brett in der algebraischen Notation.
    /// </summary>
    /// <param name="move">Das zu beschriftende Feld als Index in der 12x12-Darstellung.</param>
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

    #endregion

    #region ESSENTIELL

    /// <summary>
    /// Die Methode <c>Initialize</c> wird ausgefuehrt,
    /// wenn eine neue Instanz der Klasse geschaffen wird.
    /// </summary>
    public static void Initialize()
    {
        Square64 = new int[64];
        Square120 = new int[120];
    }

    /// <summary>
    /// Die Methode <c>ResetBoard</c> setzt alle Brett-Variablen auf ihre Default-Werte zurück.
    /// </summary>
    public static void ResetBoard()
    {
        SetSquare64(new int[64]);
        SetWhiteToMove(true);
        SetEnPassantSquare(-1);
        MoveCount = 0;
        PlayerColor = -1;
    }

    #endregion

    #region ZUG

    public static void MakeMove(Move move, bool calculation = false)
    {
        // Aktualisiert die Rochaderechte.
        SetCastlePermissionsWithBools(_WhiteCastleKingside, _WhiteCastleQueenside, _BlackCastleKingside, _BlackCastleQueenside);

        // Position der Figuren
        int piece = Square120[move.StartSquare];
        int position = Array.IndexOf(PiecesList[piece], move.StartSquare);
        PiecesList[piece][position] = move.TargetSquare;

        if (move.Type == 0 || move.Type == 3 || move.Type == 4)
        {
            LastCaptures.Add(Piece.NONE);
        }

        else if (move.Type == 1)
        {
            int enemyPiece = Square120[move.TargetSquare];
            int enemyPos = Array.IndexOf(PiecesList[enemyPiece], move.TargetSquare);

            //Debug.Log($"Move: {DesignateMove(move)} Piece: {enemyPiece} Positions: {move.TargetSquare} Count: {PiecesList[enemyPiece].Length}");
            
            PiecesList[enemyPiece][enemyPos] = 0;

            // Speichert die geschlagenen Figuren.
            LastCaptures.Add(enemyPiece);
        }

        // Aktualisiert die Rochaderechte
        UpdateCastlePermissions(move, piece);

        // Rochade
        if (move.Type == 3) Castle(move, true);
        if (move.Type == 4) Castle(move, false);

        // Verwandlung
        if (move.Promotion != -1)
        {
            Promotion(move, piece, position);
        }

        // Zielfeld
        if (move.Promotion == -1)
        {
            Square120[move.TargetSquare] = Square120[move.StartSquare];
            Square64[ConvertIndex120To64(move.TargetSquare)] = Square120[move.StartSquare];
        }
        else
        {
            LastCaptures.Add(Square120[move.TargetSquare]);
            Square120[move.TargetSquare] = move.Promotion;
            Square64[ConvertIndex120To64(move.TargetSquare)] = move.Promotion;
        }

        // Startfeld
        Square120[move.StartSquare] = 0;
        Square64[ConvertIndex120To64(move.StartSquare)] = 0;

        // En Passant Schlag
        if (move.Type == 2)
        {
            EnPassant(move);
        }

        // En Passant
        EnPassantSquare = move.EnPassant;
        LastEnPassant.Add(move.EnPassant);

        // Player to move
        WhiteToMove = !WhiteToMove;

        // Erhoeht die Anzahl der gespielten Zuege
        MoveCount++;

        // Generiert die naechsten Zuege
        if (!calculation)
        {
            LastCaptures.RemoveAt(0);
            GameManager.instance.SetPossibleMoves();
        }
    }

    public static void UnmakeMove(Move move)
    {
        // Verwendet die jetzigen Werte.
        int lastCapture = LastCaptures.Last();
        int lastEnPassant = LastEnPassant.Last();

        // Aktualisiert die Rochaderechte.
        SetCastlePermissionsWithBools(_WhiteCastleKingside, _WhiteCastleQueenside, _BlackCastleKingside, _BlackCastleQueenside);

        // Position der Figuren
        int piece = Square120[move.TargetSquare];
        int position = Array.IndexOf(PiecesList[piece], move.TargetSquare);
        PiecesList[piece][position] = move.StartSquare;

        if (move.Type == 1)
        {
            for (int i = 0; i < PiecesList[lastCapture].Length; i++)
            {
                // Speichert die Figurenposition bei der naechsten freien Stelle.
                if (PiecesList[lastCapture][i] == 0)
                {
                    PiecesList[lastCapture][i] = move.TargetSquare;
                    break;
                }
            }
        }

        // Aktualisiert die Rochaderechte
        UpdateCastlePermissions(move, piece);

        // Rochade
        if (move.Type == 3) Castle(move, true, true);
        if (move.Type == 4) Castle(move, false, true);

        // Verwandlung
        if (move.Promotion != -1)
        {
            Promotion(move, piece, position, true);

            // Startfeld
            int friendlyColor = (move.Promotion < Piece.BLACK) ? Piece.WHITE : Piece.BLACK;

            Square120[move.StartSquare] = friendlyColor | Piece.PAWN;
            Square64[ConvertIndex120To64(move.StartSquare)] = Square120[friendlyColor | Piece.PAWN];
        }
        else
        {
            // Startfeld
            Square120[move.StartSquare] = Square120[move.TargetSquare];
            Square64[ConvertIndex120To64(move.StartSquare)] = Square120[move.TargetSquare];
        }

        // Zielfeld
        Square120[move.TargetSquare] = lastCapture;
        Square64[ConvertIndex120To64(move.TargetSquare)] = lastCapture;


        // En Passant Schlag
        if (move.Type == 2)
        {
            EnPassant(move, true);
        }

        // En Passant
        EnPassantSquare = LastEnPassant.Last();
        LastEnPassant.RemoveAt(LastEnPassant.Count - 1);

        // Loescht die Information zur zuletzt gespielten Figur.
        LastCaptures.RemoveAt(LastCaptures.Count - 1);

        // Player to move
        WhiteToMove = !WhiteToMove;

        // Senkt die Anzahl der gespielten Zuege
        MoveCount--;

    }

    private static void UpdateCastlePermissions(Move move, int piece)
    {
        // Speichert die jetztige Position.
        _WhiteCastleKingside = WhiteCastleKingside;
        _WhiteCastleQueenside = WhiteCastleQueenside;
        _BlackCastleKingside = BlackCastleKingside;
        _BlackCastleQueenside = BlackCastleQueenside;

        // Loescht die Rochaderechte, wenn sich der Koenig bewegt hat.
        if (Piece.IsType(piece, Piece.KING))
        {
            if (Piece.IsColor(piece, Piece.WHITE))
            {
                WhiteCastleKingside = false;
                WhiteCastleQueenside = false;
            }
            else
            {
                BlackCastleKingside = false;
                BlackCastleQueenside = false;
            }
        }
        // Loescht die Rochaderechte fuer die jeweilige Seite, wenn sich ein Turm bewegt hat.
        if (Piece.IsType(piece, Piece.ROOK))
        {
            if (Piece.IsColor(piece, Piece.WHITE))
            {
                if (move.StartSquare == 91)
                    WhiteCastleQueenside = false;
                else if (move.StartSquare == 98)
                    WhiteCastleKingside = false;
            }
            else
            {
                if (move.StartSquare == 21)
                    BlackCastleQueenside = false;
                else if (move.StartSquare == 28)
                    BlackCastleKingside = false;
            }
        }
    }

    private static void Castle(Move move, bool kingside, bool undo = false)
    {
        int oldRookSq;
        int newRookSq;

        if (kingside)
        {
            oldRookSq = undo ? move.StartSquare + 1 : move.StartSquare + 3;
            newRookSq = undo ? move.StartSquare + 3 : move.StartSquare + 1;
        }
        else
        {
            oldRookSq = undo ? move.StartSquare - 1 : move.StartSquare - 4;
            newRookSq = undo ? move.StartSquare - 4 : move.StartSquare - 1;
        }

        // Figurenposition des Turms
        int rook = Square120[oldRookSq];
        int position = Array.IndexOf(PiecesList[rook], oldRookSq);
        PiecesList[rook][position] = newRookSq;

        // Zielfeld des Turms
        Square120[newRookSq] = rook;
        Square64[ConvertIndex120To64(newRookSq)] = rook;

        // Startfeld des Turms
        Square120[oldRookSq] = 0;
        Square64[ConvertIndex120To64(oldRookSq)] = 0;  
    }

    private static void Promotion(Move move, int piece, int position, bool undo = false)
    {
        if (!undo)
        {
            // Setzt die Position des umgewandelten Bauerns.
            PiecesList[piece][position] = undo ? move.StartSquare : 0;

            // Setzt die Position fuer die umgewandelte Figur.
            for (int i = 0; i < PiecesList[move.Promotion].Length; i++)
            {
                if (PiecesList[move.Promotion][i] == 0)
                {
                    PiecesList[move.Promotion][i] = move.TargetSquare;
                    break;
                }
            }
        }
        else
        {
            // Setzt die Position des umgewandelten Bauerns.
            int _piece = (move.Promotion < Piece.BLACK) ? Piece.WHITE | Piece.PAWN : Piece.BLACK | Piece.PAWN;

            for (int i = 0; i < PiecesList[_piece].Length; i++)
            {
                if (PiecesList[_piece][i] == 0) PiecesList[_piece][i] = move.StartSquare;
            }

            // Setzt die Position fuer die umgewandelte Figur zurueck.
            PiecesList[move.Promotion][Array.IndexOf(PiecesList[move.Promotion], move.StartSquare)] = 0;

        }
        
    }

    private static void EnPassant(Move move, bool undo = false)
    {
        int pawnSq = move.TargetSquare;
        pawnSq += (move.StartSquare - move.TargetSquare > 0) ? 10 : -10;

        // Werte bei undo sind doppelt gedreht, da WhiteToMove noch nicht
        // aktualisiert wurde und es sich um den gegnerischen Bauern handelt.
        int pawnValue = undo ? (WhiteToMove ? 10 : 18) : (WhiteToMove ? 18 : 10);
        
        if (!undo)
        {
            // Loescht den indirekt geschlagenen Bauer.
            int pos = Array.IndexOf(PiecesList[Square120[pawnSq]], pawnSq);
            PiecesList[Square120[pawnSq]][pos] = 0;

            Square120[pawnSq] = 0;
            Square64[ConvertIndex120To64(pawnSq)] = 0;

            LastCaptures.Add(pawnValue);
        }
        else
        {
            for (int i = 0; i < PiecesList[pawnValue].Length; i++)
            {
                // Speichert den indirekt geschlagenen Bauer bei der naechsten freien Stelle.
                if (PiecesList[pawnValue][i] == 0)
                {
                    PiecesList[pawnValue][i] = pawnSq;
                    break;
                }
            }

            Square120[pawnSq] = pawnValue;
            Square64[ConvertIndex120To64(pawnSq)] = pawnValue;
        }
    }

    #endregion

}
