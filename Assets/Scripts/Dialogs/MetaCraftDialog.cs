using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class MetaCraftDialog : DialogBase {
    [SerializeField]
    private MetaCraft _craftPrefab;

    [SerializeField]
    private Transform _craftsContainer;
    
    [SerializeField]
    private CanvasGroup _panelCanvasGroup;

    [SerializeField]
    private Transform _craftingCellAnchor, _buildButtonAnchor;
    
    [SerializeField]
    private GameObject _claimButton;

    [SerializeField]
    private AnimationClip _hidePanelClip;

    private Action<MetaCraftInfo> _craft;
    private CellView _craftingCell;
    private float _cellRotateSpeed = 8;
    
    public override void SetData(object data) {
        Data dialogData = data as Data;
        _craft = dialogData.Craft;

        foreach (MetaCraftInfo craft in dialogData.Crafts) {
            MetaCraft newCraft = Instantiate(_craftPrefab, _craftsContainer);
            MetaCraftInfo craftInfo = craft;
            bool hasCell = MetaFieldManager.Instance.HasPieceInInventory(craftInfo.NeededCell);
            newCraft.SetData(craft, craftingCell => Craft(craftInfo, craftingCell), hasCell);
        }
    }

    private void Craft(MetaCraftInfo craftInfo, CellView craftingCell) {
        _craft.Invoke(craftInfo);
        _panelCanvasGroup.interactable = false;
        _craftingCell = craftingCell;
        CraftAnimation(craftingCell).Forget();
    }

    private async UniTask CraftAnimation(CellView cell) {
        cell.CenterPivot.SetParent(transform);
        CellIdleRotate(cell).Forget();
        
        await DOTween.Sequence()
            .Append(cell.CenterPivot.DOScale(cell.CenterPivot.localScale * 1.4f, 0.2f))
            .Append(cell.CenterPivot.DOScale(cell.CenterPivot.localScale * 1.35f, 0.2f))
            .AppendInterval(0.1f)
            .AsyncWaitForCompletion();
        
        GetComponent<Animation>().Play(_hidePanelClip.name);
        
        await DOTween.Sequence()
            .Append(cell.CenterPivot.DOMove(_craftingCellAnchor.position, 0.8f))
            .Join(cell.CenterPivot.DOScale(cell.CenterPivot.localScale * 3, 0.8f))
            .AsyncWaitForCompletion();
        _claimButton.SetActive(true);
    }
    
    private async UniTask CellIdleRotate(CellView cell) {
        var token = this.GetCancellationTokenOnDestroy();
        while (true) {
            cell.CenterPivot.Rotate(Vector3.up * _cellRotateSpeed * Time.deltaTime);
            await UniTask.WaitForEndOfFrame(token);
        }
    }
    
    public void ExitFromDialog() {
        MetaFieldManager.Instance.CanInteractWithField(true);
    }

    public void ClickClaimPiece() {
        ClaimAndClose().Forget();
    }

    private async UniTask ClaimAndClose() {
        HideAnimation().Forget();
        _cellRotateSpeed *= 5;
        
        await DOTween.Sequence()
            .Append(_craftingCell.CenterPivot.DOMove(_buildButtonAnchor.position, 0.6f))
            .Join(_craftingCell.CenterPivot.DOScale(Vector3.zero, 0.6f))
            .AsyncWaitForCompletion();

        CloseInstant();
    }

    [Serializable]
    public class Data {
        public List<MetaCraftInfo> Crafts;
        public Action<MetaCraftInfo> Craft;
    }
}