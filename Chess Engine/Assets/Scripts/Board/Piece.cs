using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Die Klasse <c>Piece</c> beinhaltet die int-Konstanten fuer alle Schachfiguren
/// sowie wichtige Ueberpruefungsmethoden fuer Figur und Farbe.
/// Ein Figurenwert setzt sich als Binaeraddition zwischen Farbe und Typ zusammen.
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
    /// Die Funktion <c>IsType</c> ueberprueft, ob eine uebergebene
    /// Figur mit einer ausgewaehlten Figurenart uebereinstimmt.
    /// </summary>
    /// <param name="piece">Die zu ueberpruefende Figur.</param>
    /// <param name="pieceType">Die Figurenart, auf die Figur ueberprueft werden soll.</param>
    /// <returns>Gibt zurueck, ob die uebergebene Figur mit
    /// der ausgewaehlten Figurenart uebereinstimmt.</returns>
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
    /// Die Funktion <c>IsColor</c> ueberprueft, ob eine uebergebene
    /// Figur einer ausgewaehlten Figurenfarbe angehoert.
    /// </summary>
    /// <param name="piece">Die zu ueberpruefende Figur.</param>
    /// <param name="pieceColor">Die Firgurenfarbe, auf die Figur ueberprueft werden soll.</param>
    /// <returns>Gibt zurueck, ob die uebergebene Figur mit
    /// der ausgewaehlten Figurenfarbe uebereinstimmt.</returns>
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
            if (piece >= BLACK)
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
    /// Die Funktion <c>OpponentColor</c> gibt ausgehend von einer Farbe die gegnerische Farbe zurueck.
    /// </summary>
    /// <param name="color">Die aktuelle Farbe</param>
    /// <returns>Gibt die gegnerische Farbe zurueck</returns>
    public static int OpponentColor(int color)
    {
        if (color == WHITE) return BLACK;
        else if (color == BLACK) return WHITE;
        else return -1;
    }

    /// <summary>
    /// Die Funktion <c>IsSlidingPiece</c> ueberprueft, ob
    /// sich eine Figur schraeg ueber das Brett bewegen kann.
    /// </summary>
    /// <param name="piece">Die zu ueberpruefende Figur</param>
    /// <returns>Gibt zurueck, ob die Funktion auf die uebergebene Figur zutrifft</returns>
    public static bool IsSlidingPiece(int piece)
    {
        // Figur ist ein Laeufer
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
    /// Die Funktion <c>IsKnight</c> ueberprueft, ob es
    /// sich bei einer Figur um einen Springer handelt.
    /// </summary>
    /// <param name="piece">Die zu ueberpruefende Figur</param>
    /// <returns>Gibt zurueck, ob die uebergebene Figur ein Springer ist</returns>
    public static bool IsKnight(int piece)
    {
        // Figur ist ein Springer
        if (piece == 11 || piece == 19)
            return true;

        return false;
    }

    /// <summary>
    /// Die Funktion <c>IsKing</c> ueberprueft, ob es
    /// sich bei einer Figur um einen Koenig handelt.
    /// </summary>
    /// <param name="piece">Die zu ueberpruefende Figur</param>
    /// <returns>Gibt zurueck, ob die uebergebene Figur ein Koenig ist</returns>
    public static bool IsKing(int piece)
    {
        // Figur ist ein Koenig
        if (piece == 9 || piece == 17)
            return true;

        return false;
    }

    /// <summary>
    /// Die Funktion <c>IsPawn</c> ueberprueft, ob es
    /// sich bei einer Figur um einen Bauern handelt.
    /// </summary>
    /// <param name="piece">Die zu ueberpruefende Figur</param>
    /// <returns>Gibt zurueck, ob die uebergebene Figur ein Bauer ist</returns>
    public static bool IsPawn(int piece)
    {
        // Figur ist ein Bauer
        if (piece == 10 || piece == 18)
            return true;

        return false;
    }

    public static char CharFromPieceValue(int piece)
    {
        bool isWhite = IsColor(piece, WHITE);

        if (isWhite) piece -= WHITE;
        else piece -= BLACK;

        return piece switch
        {
            KING => isWhite ? 'K' : 'k',
            PAWN => isWhite ? 'P' : 'p',
            KNIGHT => isWhite ? 'N' : 'n',
            BISHOP => isWhite ? 'B' : 'b',
            ROOK => isWhite ? 'R' : 'r',
            QUEEN => isWhite ? 'Q' : 'q',
            _ => ' ',
        };
    }

    public static List<int[]> GeneratePiecesList()
    {
        List<int[]> piecesList = new()
        {
            new int[0],  // 0  - UNDEFINED
            new int[0],  // 1  - UNDEFINED
            new int[0],  // 2  - UNDEFINED
            new int[0],  // 3  - UNDEFINED
            new int[0],  // 4  - UNDEFINED
            new int[0],  // 5  - UNDEFINED
            new int[0],  // 6  - UNDEFINED
            new int[0],  // 7  - UNDEFINED
            new int[0],  // 8  - NONE
            new int[1],  // 9  - WHITE KING
            new int[8],  // 10 - WHITE PAWN
            new int[10], // 11 - WHITE KNIGHT
            new int[10], // 12 - WHITE BISHOP
            new int[10], // 13 - WHITE ROOK
            new int[9],  // 14 - WHITE QUEEN
            new int[0],  // 15 - UNDEFINED
            new int[0],  // 16 - NONE
            new int[1],  // 17 - BLACK KING
            new int[8],  // 18 - BLACK PAWN
            new int[10], // 19 - BLACK KNIGHT
            new int[10], // 20 - BLACK BISHOP
            new int[10], // 21 - BLACK ROOK
            new int[9]   // 22 - BLACK QUEEN
        };

        return piecesList;
    }

}
