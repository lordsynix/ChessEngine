using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Die Klasse <c>Piece</c> beinhaltet die int-Konstanten f�r alle Schachfiguren.
/// Ein Figurenwert setzt sich als Bin�raddition zwischen Farbe und Typ zusammen.
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
    /// Die Funktion <c>IsSlidingPiece</c> �berpr�ft, ob 
    /// sich eine Figur schr�g �ber das Brett bewegen kann.
    /// </summary>
    /// <param name="piece">Die zu �berpr�fende Figur</param>
    /// <returns>Gibt zur�ck, ob die Funktion auf die �bergebene Figur zutrifft</returns>
    public static bool IsSlidingPiece (int piece)
    {
        // Figur ist ein L�ufer
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
    /// Die Funktion <c>IsType</c> �berpr�ft, ob eine �bergebene 
    /// Figur mit einer ausgew�hlten Figurenart �bereinstimmt.
    /// </summary>
    /// <param name="piece">Die zu �berpr�fende Figur.</param>
    /// <param name="pieceType">Die Figurenart, auf die Figur �berpr�ft werden soll.</param>
    /// <returns>Gibt zur�ck, ob die �bergebene Figur mit 
    /// der ausgew�hlten Figurenart �bereinstimmt.</returns>
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
    /// Die Funktion <c>IsColor</c> �berpr�ft, ob eine �bergebene 
    /// Figur einer ausgew�hlten Figurenfarbe angeh�rt.
    /// </summary>
    /// <param name="piece">Die zu �berpr�fende Figur.</param>
    /// <param name="pieceType">Die Firgurenfarbe, auf die Figur �berpr�ft werden soll.</param>
    /// <returns>Gibt zur�ck, ob die �bergebene Figur mit 
    /// der ausgew�hlten Figurenfarbe �bereinstimmt.</returns>
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
    
}
