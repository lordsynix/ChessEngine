using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static MoveGenerator;

// Importiert verschiedene Klassen von Unity, um den Drag and Drop Mechanismus zu vereinfachen.
public class DragDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler,
                                       IDragHandler, IInitializePotentialDragHandler
{
    public bool foundSquare = false;

    private GameObject canvas;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas myCanvas;

    private bool isWhite;

    private Vector2 startPosition;
    private int slotNum;

    private void Awake()
    {
        // Weist den Referenzen die entsprechenden Komponenten zu.
        canvas = GameObject.Find("Canvas");
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        startPosition = rectTransform.anchoredPosition;
        myCanvas = GetComponent<Canvas>();
        isWhite = Board.GetPlayerColor() == Piece.WHITE;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Board.GetGameMode() == Board.Mode.HumanComputer)
        {
            // Laesst die Visualisierung der Zuege nur bei eigenen Figuren zu.
            slotNum = (int)Variables.Object(gameObject).Get("SquareNum");
            int piece = Board.PieceOnSquare(slotNum);
            int friendlyColor = Board.GetPlayerColor();

            if (Piece.IsColor(piece, friendlyColor))
            {
                // Aktiviert die Visualisierung aller möglichen Züge.
                GameManager.instance.DeactivateMoveVisualisation();
                GameManager.instance.ActivateMoveVisualization(slotNum);
            }
            else
            {
                // Gegnerische Figur.
                ExitDragDrop();
            }
        } 
        else if (Board.GetGameMode() == Board.Mode.Testing)
        {
            slotNum = (int)Variables.Object(gameObject).Get("SquareNum");

            GameManager.instance.DeactivateMoveVisualisation();
            GameManager.instance.ActivateMoveVisualization(slotNum);
        }
        
        GameManager.instance.MakePhysicalMove(gameObject, slotNum);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Stellt sicher, dass sich die ausgewaehlte Figur
        // ueber den anderen UI-Elementen befindet.
        canvasGroup.blocksRaycasts = false;
        myCanvas.sortingOrder += 1;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        myCanvas.sortingOrder -= 1;

        if (!foundSquare) ExitDragDrop();

        // Zurücksetzten der Variablen.
        foundSquare = false;
    }

    void ExitDragDrop()
    {
        rectTransform.anchoredPosition = startPosition;
        rectTransform.SetPositionAndRotation(transform.parent.position, Quaternion.identity);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Verschiebt die Figur mit der Maus.
        rectTransform.anchoredPosition += eventData.delta / canvas.GetComponent<Canvas>().scaleFactor;
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        // Deaktiviert das eingebaute Drag and Drop Verhalten von Unity.
        eventData.useDragThreshold = false;
    }

    
}
