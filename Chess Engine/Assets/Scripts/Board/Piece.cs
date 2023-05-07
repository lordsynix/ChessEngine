using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Die Klasse <c>Piece</c> beinhaltet die int-Konstanten für alle Schachfiguren
/// sowie wichtige Überprüfungsmethoden für Figur und Farbe.
/// Ein Figurenwert setzt sich als Binäraddition zwischen Farbe und Typ zusammen.
/// </summary>
public static class Piece
{
    
    public const int NONE = 0;
    public const int KING = 1;
    public const int PAWN = 2;
    public const int KNIGHT = 3;
    public const int BISHOP = 4;
    public const int ROOK = 5;
    public const int QUEEN = 6;

    public const int WHITE = 8;
    public const int BLACK = 16;

    /// <summary>
    /// Die Funktion <c>IsType</c> überprüft, ob eine übergebene 
    /// Figur mit einer ausgewählten Figurenart übereinstimmt.
    /// </summary>
    /// <param name="piece">Die zu überprüfende Figur.</param>
    /// <param name="pieceType">Die Figurenart, auf die Figur überprüft werden soll.</param>
    /// <returns>Gibt zurück, ob die übergebene Figur mit 
    /// der ausgewählten Figurenart übereinstimmt.</returns>
    public static bool IsType(int piece, int pieceType)
    {
        switch (pieceType)
        {
            case NONE:
                Debug.LogError("Invalid piece type");
                return false;

            case KING:
                if (piece == 9 || piece == 17)
                    return true;
                else return false;

            case PAWN:
                if (piece == 10 || piece == 18)
                    return true;
                else return false;

            case KNIGHT:
                if (piece == 11 || piece == 19)
                    return true;
                else return false;

            case BISHOP:
                if (piece == 12 || piece == 20)
                    return true;
                else return false;

            case ROOK:
                if (piece == 13 || piece == 21)
                    return true;
                else return false;

            case QUEEN:
                if (piece == 14 || piece == 22)
                    return true;
                else return false;

            default:
                Debug.LogError("Invalid piece type");
                return false;
        }
    }

    /// <summary>
    /// Die Funktion <c>IsColor</c> überprüft, ob eine übergebene 
    /// Figur einer ausgewählten Figurenfarbe angehört.
    /// </summary>
    /// <param name="piece">Die zu überprüfende Figur.</param>
    /// <param name="pieceColor">Die Firgurenfarbe, auf die Figur überprüft werden soll.</param>
    /// <returns>Gibt zurück, ob die übergebene Figur mit 
    /// der ausgewählten Figurenfarbe übereinstimmt.</returns>
    public static bool IsColor(int piece, int pieceColor)
    {
        if (pieceColor == WHITE)
        {
            if (piece < BLACK && piece > 0)
                return true;
            else return false;
        }
        else if (pieceColor == BLACK)
        {
            if (piece > BLACK)
                return true;
            else return false;
        }
        else
        {
            Debug.LogError("Invalid color value");
            return false;
        }
    }

    /// <summary>
    /// Die Funktion <c>IsSlidingPiece</c> überprüft, ob 
    /// sich eine Figur schräg über das Brett bewegen kann.
    /// </summary>
    /// <param name="piece">Die zu überprüfende Figur</param>
    /// <returns>Gibt zurück, ob die Funktion auf die übergebene Figur zutrifft</returns>
    public static bool IsSlidingPiece(int piece)
    {
        // Figur ist ein Läufer
        if (piece == 12 || piece == 20)
            return true;

        // Figur ist ein Turm
        if (piece == 13 || piece == 21)
            return true;

        // Figur ist eine Dame
        if (piece == 14 || piece == 22)
            return true;

        return false;
    }

    /// <summary>
    /// Die Funktion <c>IsKnight</c> überprüft, ob es
    /// sich bei einer Figur um einen Springer handelt.
    /// </summary>
    /// <param name="piece">Die zu überprüfende Figur</param>
    /// <returns>Gibt zurück, ob die übergebene Figur ein Springer ist</returns>
    public static bool IsKnight(int piece)
    {
        // Figur ist ein Springer
        if (piece == 11 || piece == 19)
            return true;

        return false;
    }

    /// <summary>
    /// Die Funktion <c>IsKing</c> überprüft, ob es
    /// sich bei einer Figur um einen König handelt.
    /// </summary>
    /// <param name="piece">Die zu überprüfende Figur</param>
    /// <returns>Gibt zurück, ob die übergebene Figur ein König ist</returns>
    public static bool IsKing(int piece)
    {
        // Figur ist ein König
        if (piece == 9 || piece == 17)
            return true;

        return false;
    }

    /// <summary>
    /// Die Funktion <c>IsPawn</c> überprüft, ob es
    /// sich bei einer Figur um einen Bauern handelt.
    /// </summary>
    /// <param name="piece">Die zu überprüfende Figur</param>
    /// <returns>Gibt zurück, ob die übergebene Figur ein Bauer ist</returns>
    public static bool IsPawn(int piece)
    {
        // Figur ist ein Bauer
        if (piece == 10 || piece == 18)
            return true;

        return false;
    }
}
