using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class BlockCameraDragForElementView : MonoBehaviour, IBeginDragHandler, IPointerDownHandler {
    [SerializeField]
    private bool _useDragCheck;

    public void OnBeginDrag(PointerEventData eventData) {
        if (_useDragCheck)
            MetaFieldManager.Instance.DraggingInventoryScroll = true;
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (!_useDragCheck)
            MetaFieldManager.Instance.DraggingInventoryScroll = true;
    }


}
