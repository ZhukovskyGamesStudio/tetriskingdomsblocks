using UnityEngine;
using UnityEngine.UI;

public class InventoryCellView : MonoBehaviour {
    public PieceData Data { get; private set; }

    public Image IconImage;

    public void SetPieceInfo(PieceData data) {
        Data = data;
    }

    public void OnBeginDrag() {
        MetaBuildManager.Instance.CreatePieceInMeta(this);
    }
}