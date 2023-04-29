using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static MoveGenerator;

// Importiert verschiedene Klassen von Unity, um den Drag and Drop Mechanismus zu vereinfachen
public class DragDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler,
                                       IDragHandler, IInitializePotentialDragHandler
{
    private Board board;
    public MoveGenerator moveGenerator = new();

    public bool foundSquare = false;
    public bool validData = true;

    private GameObject canvas;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas myCanvas;

    private Vector2 startPosition;
    private int slotNum;

    private void Awake()
    {
        // Weist den Referenzen die entsprechenden Komponenten zu
        board = Board.instance;
        canvas = GameObject.Find("Canvas");
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        startPosition = rectTransform.anchoredPosition;
        myCanvas = GetComponent<Canvas>();
        GameManager.instance.highlightedMoves = new();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        int[] square120 = Board.instance.GetSquare120();
        slotNum = (int)Variables.Object(gameObject).Get("SquareNum");

        // Verhindert, dass ein leeres Feld ausgewählt werden kann
        if (square120[slotNum] == 0)
            validData = false;

        // Generiert alle Züge für die ausgewählte Figur
        List<Move> moves = moveGenerator.GenerateMovesForPiece(slotNum, square120[slotNum]);
        if (moves != null)
        {
            // Überprüft, ob die Visualisierung für mögliche Felder bereits aktiv ist
            GameManager.instance.DeactivateMoveVisualisation();

            GameManager.instance.ActivateMoveVisualization(moves);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Stellt sicher, dass sich die ausgewählte Figur
        // über den anderen UI-Elementen befindet
        canvasGroup.blocksRaycasts = false;
        myCanvas.sortingOrder += 1;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        myCanvas.sortingOrder -= 1;
        if (!foundSquare)
            rectTransform.anchoredPosition = startPosition;

        GameManager.instance.DeactivateMoveVisualisation();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Verschiebt die Figur mit der Maus
        rectTransform.anchoredPosition += eventData.delta / canvas.GetComponent<Canvas>().scaleFactor;
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        // Deaktiviert das eingebaute Drag and Drop Verhalten von Unity
        eventData.useDragThreshold = false;
    }

    
}
