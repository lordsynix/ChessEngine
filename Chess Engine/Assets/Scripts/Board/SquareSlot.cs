using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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

            // Check if selected piece isn't null
            if (!pointerDrag.GetComponent<DragDrop>().validData)
                return;

            pointerDrag.GetComponent<DragDrop>().foundSquare = true;
            pointerDrag.GetComponent<RectTransform>().anchoredPosition =
                        GetComponent<RectTransform>().anchoredPosition;

            // Initialize
            slot = GetComponent<Image>();
            slotNum = int.Parse(transform.parent.name.Split(' ')[1]);
            oldSlotNum = int.Parse(pointerDrag.transform.parent.name.Split(' ')[1]);
            board = Board.instance;
            square = board.GetSquare();
            bool whiteToMove = board.GetWhiteToMove();

            // Player already did the last move
            if (square[oldSlotNum - 1] < Piece.Black && !whiteToMove)
                return;
            if (square[oldSlotNum - 1] > Piece.Black && whiteToMove)
                return;
            
            // Targeted Square is empty
            if (square[slotNum - 1] == 0)
            {
                slot.sprite = pointerDrag.GetComponentInChildren<Image>().sprite;
                slot.color = new Color32(255, 255, 255, 255);
                square[slotNum - 1] = square[oldSlotNum - 1];

                pointerDrag.GetComponentInChildren<Image>().sprite = null;
                pointerDrag.GetComponentInChildren<Image>().color = new Color32(255, 255, 255, 0);
                square[oldSlotNum - 1] = 0;

                FindObjectOfType<AudioManager>().Play("move_normal"); // Sound Effect

                board.SetWhiteToMove(!whiteToMove);
                board.SetSquare(square);
            }
            
            // Targeted Square contains a friendly piece
            if (square[slotNum - 1] < Piece.Black && whiteToMove)
                return;
            if (square[slotNum - 1] > Piece.Black && !whiteToMove)
                return;

            // Targeted Square contains an enemy piece
            if (square[slotNum - 1] > Piece.Black && whiteToMove)
                CapturePiece(pointerDrag, whiteToMove);
            if (square[slotNum - 1] < Piece.Black && !whiteToMove)
                CapturePiece(pointerDrag, whiteToMove);
        }
    }

    void CapturePiece(GameObject newPiece, bool whiteToMove)
    {
        slot.sprite = newPiece.GetComponentInChildren<Image>().sprite;
        square[slotNum - 1] = square[oldSlotNum - 1];

        newPiece.GetComponentInChildren<Image>().sprite = null;
        newPiece.GetComponentInChildren<Image>().color = new Color32(0, 0, 0, 0);
        square[oldSlotNum - 1] = 0;

        FindObjectOfType<AudioManager>().Play("move_capture"); // Sound Effect

        board.SetWhiteToMove(!whiteToMove);
        board.SetSquare(square);
    }

}
