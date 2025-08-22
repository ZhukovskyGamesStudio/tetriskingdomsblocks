using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class BlockCameraDragForElementView : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        MetaFieldManager.Instance.DraggingInventoryScroll = true;
        Debug.Log("OnPointerDown");
    }
}
