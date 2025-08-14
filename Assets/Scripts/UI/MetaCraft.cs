using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MetaCraft : MonoBehaviour {
    [SerializeField]
    private MetaCraftResource _resourcePrefab;
    
    [SerializeField]
    private Transform _resourcesContainer;

    [SerializeField]
    private TextMeshProUGUI _nameText, _descriptionText, _bonusText;

    [SerializeField]
    private Image _bonusResourceIcon;

    [SerializeField]
    private Button _craftButton;

    [SerializeField]
    private int _pieceLayer;
    
    [SerializeField]
    private Transform _pieceContainer;

    private Action<CellView> _craft;
    private CellView _craftingPiece;
    
    public void SetData(MetaCraftInfo craftInfo, Action<CellView> craft, bool hasCell) {
        if(!hasCell) _craftButton.interactable = false;
        
        ApplyResultCell(craftInfo.ResultPrefab);
        _nameText.text = craftInfo.CraftName;
        _descriptionText.text = craftInfo.Description;
        _craft = craft;
        _bonusText.text = $"+{craftInfo.BonusPercents}%";
        _bonusResourceIcon.sprite = SpritesManager.Instance.GetSprite(craftInfo.BonusResource);

        MetaCraftResource _cellResource = Instantiate(_resourcePrefab, _resourcesContainer);
        _cellResource.SetData(craftInfo.NeededCell, hasCell);
        
        foreach (var resource in craftInfo.NeededResources) {
            MetaCraftResource _newResource = Instantiate(_resourcePrefab, _resourcesContainer);
            float resourceCount = StorageManager.GameDataMain.GetResource(resource.Key);
            if (resourceCount < resource.Value) _craftButton.interactable = false;
            
            _newResource.SetData(resource.Key, (int)resourceCount, resource.Value);
        }
    }

    private void ApplyResultCell(CellView cellPrefab) {
        CellView instance = Instantiate(cellPrefab, _pieceContainer);
        instance.ApplyCenterPivot();
        instance.CenterPivot.localPosition = Vector3.zero;
        _craftingPiece = instance;
        ApplyLayerToChildren(instance.CenterPivot, _pieceLayer);
    }

    private void ApplyLayerToChildren(Transform obj, int layer) {
        obj.gameObject.layer = layer;
        foreach (Transform child in obj) {
            ApplyLayerToChildren(child, layer);
        }
    }

    public void Craft() {
        _craft.Invoke(_craftingPiece);
    }
}
