using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class BlockCameraDragForElementView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        MetaFieldManager.Instance.CanDragCamera = false;
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        MetaFieldManager.Instance.CanDragCamera = true;
    }
}
