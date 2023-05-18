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
    [SerializeField] public GameObject curPromotionPointerDrag = null;

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

            // Weist den Referenzen die entsprechenden Komponenten zu
            slot = GetComponent<Image>();
            slotNum = (int)Variables.Object(gameObject).Get("SquareNum");
            oldSlotNum = (int)Variables.Object(pointerDrag).Get("SquareNum");


            Move curMove = new(oldSlotNum, slotNum);

            // Überprüft, ob der eingegebene Zug möglich ist (Normaler- sowie Schlagzug)
            if (!GameManager.instance.possibleMoves.Any(
                m => m.StartSquare == curMove.StartSquare && m.TargetSquare == curMove.TargetSquare)) return;

            // Lädt den gegebenen Zug
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
        GameManager.instance.DeactivatePromotionVisuals();

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

        // Löscht den Bauer, der mit EnPssant geschlagen wurde
        if (move.Capture == 2)
        {
            int enPasSq = Board.instance.ConvertIndex120To64(Board.instance.GetEnPassantSquare());
            enPasSq += (move.StartSquare - move.TargetSquare > 0) ? 8 : -8;

            GameObject go = BoardGeneration.instance.squaresGO[enPasSq].transform.GetChild(0).gameObject;
            go.GetComponent<Image>().sprite = null;
            go.GetComponent<Image>().color = new Color32(255, 255, 255, 0);

            FindObjectOfType<AudioManager>().Play("move_enpassant");
        }

        // SFX
        if (move.Capture == 0) FindObjectOfType<AudioManager>().Play("move_normal");
        else if (move.Capture == 1) FindObjectOfType<AudioManager>().Play("move_capture");

        // Aktualisiert die Brett-Variablen
        Board.instance.MakeMove(move);

        // Stellt sicher, dass der DebugMode verlassen wird
        GameManager.instance.Debug();
    }
}
