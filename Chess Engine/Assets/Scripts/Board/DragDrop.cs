using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Importiert verschiedene Klassen von Unity, um den Drag and Drop Mechanismus zu vereinfachen
public class DragDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler,
                                       IDragHandler, IInitializePotentialDragHandler
{
    public Board board;

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
        canvas = GameObject.Find("Canvas");
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        startPosition = rectTransform.anchoredPosition;
        myCanvas = GetComponent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        int[] square = Board.instance.GetSquare();
        slotNum = int.Parse(transform.parent.name.Split(' ')[1]);

        // Verhindert, dass ein leeres Feld ausgew?hlt werden kann
        if (square[slotNum - 1] == 0)
            validData = false;            
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Stellt sicher, dass sich die ausgew?hlte Figur
        // ?ber den anderen UI-Elementen befindet
        canvasGroup.blocksRaycasts = false;
        myCanvas.sortingOrder += 1;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        myCanvas.sortingOrder -= 1;
        if (!foundSquare)
            rectTransform.anchoredPosition = startPosition;
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
