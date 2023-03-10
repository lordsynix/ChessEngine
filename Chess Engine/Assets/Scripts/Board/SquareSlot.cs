using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Die Klasse <c>SquareSlot</c> stellt ein Feld auf dem Spielbrett dar.
/// Beinhaltet die grafische Darstellung der Figuren auf dem Feld.
/// </summary>
public class SquareSlot : MonoBehaviour, IDropHandler
{
    private int[] square;
    private Board board;
    private Image slot;
    private int oldSlotNum;
    private int slotNum;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            var pointerDrag = eventData.pointerDrag;

            // Überprüft ob das ausgewählte Objekt eine Figur ist
            if (!pointerDrag.GetComponent<DragDrop>().validData)
                return;

            // Setzt den Positionsursprung der Figur auf das neue Feld
            pointerDrag.GetComponent<DragDrop>().foundSquare = true;
            pointerDrag.GetComponent<RectTransform>().anchoredPosition =
                        GetComponent<RectTransform>().anchoredPosition;

            // Weist den Refernzen die entsprechenden Komponenten zu
            slot = GetComponent<Image>();
            slotNum = int.Parse(transform.parent.name.Split(' ')[1]);
            oldSlotNum = int.Parse(pointerDrag.transform.parent.name.Split(' ')[1]);
            board = Board.instance;
            square = board.GetSquare();
            bool whiteToMove = board.GetWhiteToMove();

            // Überprüft, ob der Spieler bereits den letzten Zug gemacht hat
            if (square[oldSlotNum - 1] < Piece.Black && !whiteToMove)
                return;
            if (square[oldSlotNum - 1] > Piece.Black && whiteToMove)
                return;

            // TODO Überprüfen, ob das ausgewählte Feld nicht auf dem Brett ist mit Square120

            // Überprüft, ob das anvisierte Feld leer ist
            if (square[slotNum - 1] == 0)
            {
                Move(pointerDrag, whiteToMove);
            }

            // Überprüft, ob das anvisierte Feld eine eigene Figur beinhaltet
            if (square[slotNum - 1] < Piece.Black && whiteToMove)
                return;
            if (square[slotNum - 1] > Piece.Black && !whiteToMove)
                return;

            // Überprüft, ob das anvisierte Feld eine gegnerische Figur beinhaltet
            if (square[slotNum - 1] > Piece.Black && whiteToMove)
                CapturePiece(pointerDrag, whiteToMove);
            if (square[slotNum - 1] < Piece.Black && !whiteToMove)
                CapturePiece(pointerDrag, whiteToMove);
        }
    }

    void Move(GameObject pointerDrag, bool whiteToMove)
    {
        // Aktualisiert das neue Feld
        slot.sprite = pointerDrag.GetComponentInChildren<Image>().sprite;
        slot.color = new Color32(255, 255, 255, 255);
        square[slotNum - 1] = square[oldSlotNum - 1];

        // Aktualisiert das alte Feld
        pointerDrag.GetComponentInChildren<Image>().sprite = null;
        pointerDrag.GetComponentInChildren<Image>().color = new Color32(255, 255, 255, 0);
        square[oldSlotNum - 1] = 0;

        // SFX
        FindObjectOfType<AudioManager>().Play("move_normal");

        // Aktualisiert die Brett-Variablen
        board.SetWhiteToMove(!whiteToMove);
        board.SetSquare(square);

        // Stellt sicher, dass der DebugMode verlassen wird
        GameManager.instance.ExitDebug();
    }

    void CapturePiece(GameObject newPiece, bool whiteToMove)
    {
        // Aktualisiert das neue Feld
        slot.sprite = newPiece.GetComponentInChildren<Image>().sprite;
        square[slotNum - 1] = square[oldSlotNum - 1];

        // Aktualisiert das alte Feld
        newPiece.GetComponentInChildren<Image>().sprite = null;
        newPiece.GetComponentInChildren<Image>().color = new Color32(0, 0, 0, 0);
        square[oldSlotNum - 1] = 0;

        // SFX
        FindObjectOfType<AudioManager>().Play("move_capture");

        // Aktualisiert die Brett-Variablen
        board.SetWhiteToMove(!whiteToMove);
        board.SetSquare(square);

        // Stellt sicher, dass der DebugMode verlassen wird
        GameManager.instance.ExitDebug();
    }

}
