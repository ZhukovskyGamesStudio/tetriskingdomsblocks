using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryCellView : MonoBehaviour {
    public PieceData Data { get; private set; }

    public Image IconImage;

    [field:SerializeField]
    public EventTrigger EventTrigger;
    
    public void SetPieceInfo(PieceData data) {
        Data = data;
    }

    public void OnBeginDrag() {
        MetaBuildManager.Instance.CreatePieceInMeta(this);
    }
}