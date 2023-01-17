using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SquareSlot : MonoBehaviour, IDropHandler
{

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("Drop");
        if (eventData.pointerDrag != null)
        {
            eventData.pointerDrag.GetComponent<DragDrop>().foundSquare = true;
            eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition =
                                  GetComponent<RectTransform>().anchoredPosition;

            Debug.Log(gameObject.name);
        }
    }

}
