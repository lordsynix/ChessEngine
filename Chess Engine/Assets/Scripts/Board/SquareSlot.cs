using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using static MoveGenerator;

/// <summary>
/// Die Klasse <c>SquareSlot</c> stellt ein Feld auf dem Spielbrett dar.
/// Beinhaltet die grafische Darstellung der Figuren auf dem Feld.
/// </summary>
public class SquareSlot : MonoBehaviour, IDropHandler
{
    private int[] square120;
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

            // Weist den Referenzen die entsprechenden Komponenten zu
            slot = GetComponent<Image>();
            slotNum = (int)Variables.Object(gameObject).Get("SquareNum");
            oldSlotNum = (int)Variables.Object(pointerDrag).Get("SquareNum");
            board = Board.instance;
            square120 = board.GetSquare120();
            bool whiteToMove = board.GetWhiteToMove();

            // Überprüft, ob der Spieler bereits den letzten Zug gemacht hat
            if (square120[oldSlotNum] < Piece.BLACK && !whiteToMove)
                return;
            if (square120[oldSlotNum] > Piece.BLACK && whiteToMove)
                return;

            // Überprüft, ob das anvisierte Feld leer ist
            if (square120[slotNum] == 0)
            {
                Move(pointerDrag, whiteToMove);
            }

            // Überprüft, ob das anvisierte Feld eine eigene Figur beinhaltet
            if (square120[slotNum] < Piece.BLACK && whiteToMove)
                return;
            if (square120[slotNum] > Piece.BLACK && !whiteToMove)
                return;

            // Überprüft, ob das anvisierte Feld eine gegnerische Figur beinhaltet
            if (square120[slotNum] > Piece.BLACK && whiteToMove)
                CapturePiece(pointerDrag, whiteToMove);
            if (square120[slotNum] < Piece.BLACK && !whiteToMove)
                CapturePiece(pointerDrag, whiteToMove);
        }
    }

    void Move(GameObject pointerDrag, bool whiteToMove)
    {
        Move curMove = new Move(oldSlotNum, slotNum);

        if (!GameManager.instance.possibleMoves.Contains(curMove))
            return;

        // Setzt den Positionsursprung der Figur auf das neue Feld
        pointerDrag.GetComponent<DragDrop>().foundSquare = true;
        pointerDrag.GetComponent<RectTransform>().anchoredPosition =
                    GetComponent<RectTransform>().anchoredPosition;

        // Aktualisiert das neue Feld
        slot.sprite = pointerDrag.GetComponentInChildren<Image>().sprite;
        slot.color = new Color32(255, 255, 255, 255);
        square120[slotNum] = square120[oldSlotNum];

        // Aktualisiert das alte Feld
        pointerDrag.GetComponentInChildren<Image>().sprite = null;
        pointerDrag.GetComponentInChildren<Image>().color = new Color32(255, 255, 255, 0);
        square120[oldSlotNum] = 0;

        // SFX
        FindObjectOfType<AudioManager>().Play("move_normal");

        // Aktualisiert die Brett-Variablen
        board.SetWhiteToMove(!whiteToMove);
        board.SetSquare120(square120);

        // Stellt sicher, dass der DebugMode verlassen wird
        GameManager.instance.Debug();
    }

    void CapturePiece(GameObject newPiece, bool whiteToMove)
    {
        Move curMove = new Move(oldSlotNum, slotNum);

        if (!GameManager.instance.possibleMoves.Contains(curMove))
            return;

        // Setzt den Positionsursprung der Figur auf das neue Feld
        newPiece.GetComponent<DragDrop>().foundSquare = true;
        newPiece.GetComponent<RectTransform>().anchoredPosition =
                    GetComponent<RectTransform>().anchoredPosition;

        // Aktualisiert das neue Feld
        slot.sprite = newPiece.GetComponentInChildren<Image>().sprite;
        square120[slotNum] = square120[oldSlotNum];

        // Aktualisiert das alte Feld
        newPiece.GetComponentInChildren<Image>().sprite = null;
        newPiece.GetComponentInChildren<Image>().color = new Color32(0, 0, 0, 0);
        square120[oldSlotNum] = 0;

        // SFX
        FindObjectOfType<AudioManager>().Play("move_capture");

        // Aktualisiert die Brett-Variablen
        board.SetWhiteToMove(!whiteToMove);
        board.SetSquare120(square120);

        // Stellt sicher, dass der DebugMode verlassen wird
        GameManager.instance.Debug();
    }

}
