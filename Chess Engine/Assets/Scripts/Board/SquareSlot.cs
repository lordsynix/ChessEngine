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

    /// <summary>
    /// Die Funktion <c>SimulateDragDropMove</c> initiiert einen Zug, welcher nur durch klicken 
    /// auf die jeweiligen Felder gespielt wurde und nicht per "Drag and Drop".
    /// </summary>
    /// <param name="startSquare">Im GameManager abgespeichert. Das GameObject der bewegten Figur</param>
    public void SimulateDragDropMove(GameObject startSquare)
    {
        VerifyMove(startSquare);
    }

    /// <summary>
    /// Die Funktion <c>OnDrop</c> wird von Unity beim beenden des "Drag and Drop" aufgerufen.
    /// Initiiert einen Zug.
    /// </summary>
    /// <param name="eventData">Standartmaessig von Unity uebergeben, 
    /// dient zur Ermittlung des GameObjects der bewegten Figur</param>
    public void OnDrop(PointerEventData eventData)
    {
        GameObject pointerDrag = eventData.pointerDrag;

        VerifyMove(pointerDrag);
    }

    /// <summary>
    /// Die Funktion <c>VerifyMove</c> ueberprueft, ob die aktuelle Zugeingabe ein moeglicher Zug ist.
    /// </summary>
    /// <param name="pointerDrag">Das GameObject der bewegten Figur.</param>
    public void VerifyMove(GameObject pointerDrag)
    {
        // Weist den Referenzen die entsprechenden Komponenten zu
        slot = GetComponent<Image>();
        slotNum = (int)Variables.Object(gameObject).Get("SquareNum");
        oldSlotNum = (int)Variables.Object(pointerDrag).Get("SquareNum");

        // Der eingegebene Zug.
        Move curMove = new(oldSlotNum, slotNum);

        // Ueberprueft, ob dieser moeglich ist (Normaler- sowie Schlagzug).
        List<Move> possibleMoves = GameManager.instance.GetPossibleMoves();
        if (!possibleMoves.Any(m => m.StartSquare == curMove.StartSquare 
                                 && m.TargetSquare == curMove.TargetSquare)) return;

        // Deaktiviert die Visualisierung der moeglichen Zuege.
        GameManager.instance.DeactivateMoveVisualisation();

        // Laedt den gegebenen Zug
        Move move = possibleMoves.Find(m => m.StartSquare == oldSlotNum && m.TargetSquare == slotNum);

        // Initiiert einen Zug, falls es sich um eine Umwandlung handelt.
        if (move.Promotion != -1)
        {
            GameManager.instance.ActivatePromotionVisuals(move);
            curPromotionPointerDrag = pointerDrag;
        }
        // Initiiert einen normalen Zug.
        else Move(pointerDrag, move);
    }

    /// <summary>
    /// Die Funktion <c>Move</c> taetigt einen physischen Zug auf dem Spielbrett. Sie aktualisiert 
    /// alle physischen Brettvariablen wie das Sprite der Figur, die Farbe, der SFX usw.
    /// </summary>
    /// <param name="pointerDrag">Das GameObject der bewegten Figur.</param>
    /// <param name="move">Der ueberpruefte, eingegebene Zug.</param>
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
        Board.MakeMove(move, true);

        // Stellt sicher, dass der DebugMode verlassen wird
        GameManager.instance.ActivateDebugMode();

        // Aktualisiert die Move History
        GameManager.instance.UpdateMoveHistory(move);
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
