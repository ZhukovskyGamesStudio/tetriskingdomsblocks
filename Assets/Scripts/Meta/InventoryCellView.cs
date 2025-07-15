using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryCellView : MonoBehaviour, IPointerDownHandler 
{
    public PieceData Data { get; private set; }
    
    public void SetPieceInfo(PieceData data) {
        Data = data;
    }
    
    public void OnPointerDown(PointerEventData eventData) {
        NextPiecesView.Instance.CreatePieceInMeta(this);
    }

}
