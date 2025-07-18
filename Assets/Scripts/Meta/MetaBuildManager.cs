using Cysharp.Threading.Tasks;
using UnityEngine;

public class MetaBuildManager : MonoBehaviour {
    public static MetaBuildManager Instance;

    private void Awake() {
        Instance = this;
    }

    private PieceView CreatePiece(PieceData nextPiece) {
        PieceView go = Instantiate(PiecesViewTable.Instance.PieceViewPrefab);
        go.SetData(nextPiece);
        return go;
    }

    public void CreatePieceInMeta(InventoryCellView inventoryCell) {
        PieceView pieceView = CreatePiece(inventoryCell.Data);
        pieceView.AppearFromInventoryAsync().Forget();
        MetaFieldManager.Instance.SpawnPieceFromInventory(pieceView, inventoryCell);
    }

    public void SetInventoryCellIcon(InventoryCellView inventoryCell) {
        PieceView pieceView = Instance.CreatePiece(inventoryCell.Data);
        IconRendererManager.Instance.GetIconAsSprite(pieceView.gameObject, texture => { inventoryCell.IconImage.sprite = texture; });
    }
}