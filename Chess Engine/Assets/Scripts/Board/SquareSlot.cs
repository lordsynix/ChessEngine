using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [HideInInspector] public GameObject curPromotionPointerDrag = null;

    private Image slot;
    private int oldSlotNum;
    private int slotNum;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            var pointerDrag = eventData.pointerDrag;

            // Weist den Referenzen die entsprechenden Komponenten zu
            slot = GetComponent<Image>();
            slotNum = (int)Variables.Object(gameObject).Get("SquareNum");
            oldSlotNum = (int)Variables.Object(pointerDrag).Get("SquareNum");


            Move curMove = new(oldSlotNum, slotNum);

            // Ueberprueft, ob der eingegebene Zug moeglich ist (Normaler- sowie Schlagzug)
            if (!GameManager.instance.possibleMoves.Any(
                m => m.StartSquare == curMove.StartSquare && m.TargetSquare == curMove.TargetSquare)) return;

            // Laedt den gegebenen Zug
            Move move = GameManager.instance.possibleMoves.Find(m => m.StartSquare == oldSlotNum && m.TargetSquare == slotNum);
            
            if (move.Promotion != -1)
            {
                GameManager.instance.ActivatePromotionVisuals(move);
                curPromotionPointerDrag = pointerDrag;
            }    
            else Move(pointerDrag, move);
        }
    }

    public void Move(GameObject pointerDrag, Move move)
    {
        // Setzt den Positionsursprung der Figur auf das neue Feld
        pointerDrag.GetComponent<DragDrop>().foundSquare = true;
        pointerDrag.GetComponent<RectTransform>().anchoredPosition =
                    GetComponent<RectTransform>().anchoredPosition;

        // Aktualisiert das neue Feld
        if (move.Promotion == -1) slot.sprite = pointerDrag.GetComponentInChildren<Image>().sprite;
        else slot.sprite = BoardGeneration.instance.pieces[move.Promotion];
        slot.color = new Color32(255, 255, 255, 255);

        // Aktualisiert das alte Feld
        pointerDrag.GetComponentInChildren<Image>().sprite = null;
        pointerDrag.GetComponentInChildren<Image>().color = new Color32(255, 255, 255, 0);

        // Loescht den Bauer, der mit EnPssant geschlagen wurde
        if (move.Type == 2) EnPassant(move);

        // Rochiert den Turm
        if (move.Type == 3) Castle(move, true);
        if (move.Type == 4) Castle(move, false);

        // SFX
        if (move.Type == 0) FindObjectOfType<AudioManager>().Play("move_normal");
        else if (move.Type == 1) FindObjectOfType<AudioManager>().Play("move_capture");

        // Aktualisiert die Brett-Variablen
        Board.MakeMove(move);

        // Stellt sicher, dass der DebugMode verlassen wird
        GameManager.instance.Debug();
    }

    void EnPassant(Move move)
    {
        int enPasSq = Board.ConvertIndex120To64(Board.GetEnPassantSquare());
        enPasSq += (move.StartSquare - move.TargetSquare > 0) ? 8 : -8;

        GameObject go = BoardGeneration.instance.squaresGO[enPasSq].transform.GetChild(0).gameObject;
        go.GetComponent<Image>().sprite = null;
        go.GetComponent<Image>().color = new Color32(255, 255, 255, 0);

        FindObjectOfType<AudioManager>().Play("move_enpassant");
    }

    void Castle(Move move, bool kingside)
    {
        int oldRookSq;
        int newRookSq;

        if (kingside)
        {
            oldRookSq = move.StartSquare + 3;
            newRookSq = move.StartSquare + 1;
        }
        else
        {
            oldRookSq = move.StartSquare - 4;
            newRookSq = move.StartSquare - 1;
        }

        GameObject newSq = BoardGeneration.instance.squaresGO[Board.ConvertIndex120To64(newRookSq)].transform.GetChild(0).gameObject;
        GameObject oldSq = BoardGeneration.instance.squaresGO[Board.ConvertIndex120To64(oldRookSq)].transform.GetChild(0).gameObject;

        // Aktualisiert das neue Feld des Turms
        newSq.GetComponent<Image>().sprite = oldSq.GetComponent<Image>().sprite;
        newSq.GetComponent<Image>().color = new Color32(255, 255, 255, 255);

        // Aktualisiert das alte Feld des Turms
        oldSq.GetComponent<Image>().sprite = null;
        oldSq.GetComponent<Image>().color = new Color32(255, 255, 255, 0);

        FindObjectOfType<AudioManager>().Play("move_normal");
    }
}
