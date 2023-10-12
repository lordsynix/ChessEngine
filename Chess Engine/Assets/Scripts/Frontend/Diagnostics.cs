using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

public class Diagnostics : MonoBehaviour
{
    public static Diagnostics Instance;

    [Header("Transposition Table")]
    public Text usagePercentage;
    public Text entryCount;

    private void Awake()
    {
        Instance = this;
    }

    #region Visuals

    public void UpdateTranspositionTableVisuals(double _usagePercentage, int _entryCount)
    {
        usagePercentage.text = $"{_usagePercentage}% used";
        entryCount.text = $"{_entryCount} entries";
    }

    public void UpdateDebugInformation(bool debugMode = true)
    {
        List<GameObject> squaresGO = BoardGeneration.instance.squaresGO;

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
