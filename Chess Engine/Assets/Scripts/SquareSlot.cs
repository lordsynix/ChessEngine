using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SquareSlot : MonoBehaviour, IDropHandler
{

    private Image square;

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("Drop");
        if (eventData.pointerDrag != null)
        {
            var pointerDrag = eventData.pointerDrag;

            pointerDrag.GetComponent<DragDrop>().foundSquare = true;
            pointerDrag.GetComponent<RectTransform>().anchoredPosition =
                        GetComponent<RectTransform>().anchoredPosition;

            square = GetComponent<Image>();
            if (square.sprite == null)
            {
                // Targeted Square is empty
                square.sprite = pointerDrag.GetComponentInChildren<Image>().sprite;
                square.color = new Color32(255, 255, 255, 255);

                pointerDrag.GetComponentInChildren<Image>().sprite = null;
                pointerDrag.GetComponentInChildren<Image>().color = new Color32(0, 0, 0, 0);
            }
        }
    }

}
