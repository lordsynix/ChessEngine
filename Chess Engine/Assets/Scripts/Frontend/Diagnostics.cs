using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Die Klasse <c>Diagnostics</c> regelt die Debug-Funktionen, um mich als Entwickler 
/// zu unterstuetzen. Sie ist fuer das Verstaendnis der Anwendung irrelevant.
/// </summary>
public class Diagnostics : MonoBehaviour
{
    public static Diagnostics Instance;

    [Header("Transposition Table")]
    public Text usagePercentageText;
    public Text entryCount;

    private void Awake()
    {
        Instance = this;
    }

    #region Visuals

    /// <summary>
    /// Die Funktion <c>UpdateTranspositionTableVisuals</c> aktualisiert die geschaetzten Werte fuer die Transpositionstabelle.
    /// </summary>
    public void UpdateTranspositionTableVisuals()
    {
        long usedMemory = TranspositionTable.SizeOfEntry * TranspositionTable.EntryCount;

        usagePercentageText.text = $"{Math.Round((double)usedMemory / TranspositionTable.AvailableMemory, 3)}% used";
        entryCount.text = $"{TranspositionTable.EntryCount} entries";
    }

    /// <summary>
    /// Die Funktion <c>UpdateDebugInformation</c> aktualisiert die Debug-Werte fuer alle Felder.
    /// </summary>
    /// <param name="debugMode">Ist die Anwendung im Debug-Modus?</param>
    public void UpdateDebugInformation(bool debugMode = true)
    {
        List<GameObject> squaresGO = BoardGeneration.instance.squaresGO;

        // Schaltet die UI-Elemente zum Debugen aus
        if (!debugMode)
        {
            GameManager.Instance.diagnosticsWindow.SetActive(false);

            foreach (GameObject go in squaresGO)
            {
                go.transform.GetChild(1).gameObject.SetActive(false);
            }
            return;
        }

        GameManager.Instance.diagnosticsWindow.SetActive(true);
        ulong[] bitboards = Board.GetBitboards();

        // Laedt die Informationen fuer jedes Feld auf dem Brett
        foreach (GameObject go in squaresGO)
        {
            int sqIndex64 = squaresGO.IndexOf(go);
            int sqIndex120 = Board.ConvertIndex64To120(sqIndex64);
            int piece = Board.PieceOnSquare(Board.ConvertIndex64To120(sqIndex64));

            Text[] texts = go.transform.GetChild(1).GetComponentsInChildren<Text>();
            texts[0].text = sqIndex64.ToString();
            texts[1].text = piece == Piece.NONE ? "-" : piece.ToString();
            texts[2].text = sqIndex120.ToString();
            texts[3].text = "-";

            go.transform.GetChild(1).gameObject.SetActive(true);
        }

        // Werte gemaess den Bitboards (Positionsrepraesentierung aller Figuren)
        for (int pieceType = Piece.WHITE + 1; pieceType < Piece.BLACK + 7; pieceType++)
        {
            ulong bitboard = bitboards[pieceType];
            if (bitboard == 0) continue;

            var piecePositions = Board.GetPiecePositions(bitboard);
            foreach (int square in piecePositions)
            {
                string pieceFromBitboard = Piece.CharFromPieceValue(pieceType).ToString();
                squaresGO[square].transform.GetChild(1).GetComponentsInChildren<Text>()[3].text = pieceFromBitboard;
            }
        }
    }

    #endregion

}
