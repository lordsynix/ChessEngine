using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

/// <summary>
/// Die Klasse <c>SquareSlot</c> stellt ein Feld auf dem Spielbrett dar.
/// Beinhaltet die grafische Darstellung der Figuren auf dem Feld.
/// </summary>
public class SquareSlot : MonoBehaviour, IDropHandler
{
    [HideInInspector] public GameObject curPromotionPointerDrag = null;

    private Image slot;
    private int startSquare120;
    private int targetSquare120;

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
    public void VerifyMove(GameObject pointerDrag, bool engineMove = false)
    {
        // Weist den Referenzen die entsprechenden Komponenten zu.
        slot = GetComponent<Image>();
        startSquare120 = (int)Variables.Object(pointerDrag).Get("SquareNum");
        targetSquare120 = (int)Variables.Object(gameObject).Get("SquareNum");
        int startSquare64 = Board.ConvertIndex120To64(startSquare120);
        int targetSquare64 = Board.ConvertIndex120To64(targetSquare120);
        
        // Stellt sicher, dass der Spieler nur seine Figuren bewegen kann.
        if (GameManager.GameMode != GameManager.Mode.Testing)
        {
            if (!engineMove)
            {
                int piece = Board.PieceOnSquare(startSquare120);
                int friendlyColor = Board.GetWhiteToMove() ? Piece.WHITE : Piece.BLACK;
                if (!Piece.IsColor(piece, friendlyColor)) return;
            }
        }

        // Der eingegebene Zug
        Move curMove = new(startSquare120, targetSquare120);
        
        // Ueberprueft, ob dieser moeglich ist
        List<Move> possibleMoves = GameManager.Instance.PossibleMoves;
        if (!possibleMoves.Any(m => m.StartSquare == curMove.StartSquare && m.TargetSquare == curMove.TargetSquare))
        {
            if (engineMove)
            {
                Debug.LogWarning("Engine move doesn't exists");
                GameManager.Instance.MakeEngineMove(possibleMoves[Random.Range(0, possibleMoves.Count)]);
            }
            return;
        }

        // Deaktiviert die Visualisierung der moeglichen Zuege.
        GameManager.Instance.DevisualizePossibleMoves();

        // Laedt den gegebenen Zug
        Move move = possibleMoves.Find(m => m.StartSquare == startSquare64 && m.TargetSquare == targetSquare64);
        
        // Initiiert eine Umwandlung, falls es sich um einen Menschen handelt.
        if (move.IsPromotion && !engineMove)
        {
            GameManager.Instance.ActivatePromotionVisuals(move);
            curPromotionPointerDrag = pointerDrag;
        }
        // Initiiert einen normalen Zug.
        else MakeMove(pointerDrag, move);
    }

    /// <summary>
    /// Die Funktion <c>Move</c> taetigt einen physischen Zug auf dem Spielbrett. Sie aktualisiert 
    /// alle physischen Brettvariablen wie das Sprite der Figur, die Farbe, der SFX usw.
    /// </summary>
    /// <param name="pointerDrag">Das GameObject der bewegten Figur.</param>
    /// <param name="move">Der ueberpruefte, eingegebene Zug.</param>
    public void MakeMove(GameObject pointerDrag, Move move)
    {
        // Setzt den Positionsursprung der Figur auf das neue Feld
        pointerDrag.GetComponent<DragDrop>().foundSquare = true;
        pointerDrag.GetComponent<RectTransform>().anchoredPosition =
                    GetComponent<RectTransform>().anchoredPosition;

        // Aktualisiert das neue Feld
        if (move.IsPromotion)
        {
            int playerToMove = Board.GetWhiteToMove() ? Piece.WHITE : Piece.BLACK;
            slot.sprite = BoardGeneration.instance.pieces[move.PromotionPieceType | playerToMove];
        }
        else
        {
            slot.sprite = pointerDrag.GetComponentInChildren<Image>().sprite;
        }
        slot.color = new Color32(255, 255, 255, 255);

        // Aktualisiert das alte Feld
        pointerDrag.GetComponentInChildren<Image>().sprite = null;
        pointerDrag.GetComponentInChildren<Image>().color = new Color32(255, 255, 255, 0);

        // Loescht den Bauer, der mit EnPssant geschlagen wurde
        if (move.MoveFlag == Move.EnPassantCaptureFlag) EnPassant(move);

        // Rochiert den Turm
        if (move.MoveFlag == Move.CastleFlag)
        {
            if (move.TargetSquare == 6 || move.TargetSquare == 62)
            {
                Castle(move, true);
            }
            else
            {
                Castle(move, false);
            }
        }

        // SFX
        if (move.MoveFlag == Move.EnPassantCaptureFlag)
        {
            FindObjectOfType<AudioManager>().Play("move_enpassant");
        }
        else
        {
            if (Board.PieceOnSquare(targetSquare120) == 0)
            {
                FindObjectOfType<AudioManager>().Play("move_normal");
            }
            else
            {
                FindObjectOfType<AudioManager>().Play("move_capture");
            }
        }

        // Aktualisiert die Brett-Variablen
        Board.MakeMove(move);

        // Aktualisiert die Move History
        GameManager.Instance.UpdateMoveHistory(move);
    }

    void EnPassant(Move move)
    {
        int enPasSq = Board.ConvertIndex120To64(move.TargetSquare);
        enPasSq += (move.StartSquare - move.TargetSquare > 0) ? 8 : -8;
        
        GameObject go = BoardGeneration.instance.squaresGO[enPasSq].transform.GetChild(0).gameObject;
        go.GetComponent<Image>().sprite = null;
        go.GetComponent<Image>().color = new Color32(255, 255, 255, 0);
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

        GameObject newSq = BoardGeneration.instance.squaresGO[newRookSq].transform.GetChild(0).gameObject;
        GameObject oldSq = BoardGeneration.instance.squaresGO[oldRookSq].transform.GetChild(0).gameObject;

        // Aktualisiert das neue Feld des Turms
        newSq.GetComponent<Image>().sprite = oldSq.GetComponent<Image>().sprite;
        newSq.GetComponent<Image>().color = new Color32(255, 255, 255, 255);

        // Aktualisiert das alte Feld des Turms
        oldSq.GetComponent<Image>().sprite = null;
        oldSq.GetComponent<Image>().color = new Color32(255, 255, 255, 0);
    }
}
