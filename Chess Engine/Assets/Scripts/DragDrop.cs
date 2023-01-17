using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class DragDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler,
                                       IDragHandler, IInitializePotentialDragHandler
{

    public bool foundSquare = false;

    private GameObject canvas;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Vector2 startPosition;

    private void Awake()
    {
        canvas = GameObject.Find("Canvas");
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        startPosition = rectTransform.anchoredPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Pointer Down");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("End Drag");
        //canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        if (!foundSquare)
            rectTransform.anchoredPosition = startPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Begin Dragging");
        //canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false; 
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.GetComponent<Canvas>().scaleFactor;
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        eventData.useDragThreshold = false;
    }

}
