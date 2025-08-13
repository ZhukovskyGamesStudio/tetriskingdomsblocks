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
    private TextMeshProUGUI _nameText, _descriptionText, _cellLevelText;

    [SerializeField]
    private Image _neededCellImage;

    [SerializeField]
    private Button _craftButton;

    [SerializeField]
    private GameObject _hasCellState, _cellAbsentState;

    [SerializeField]
    private int _pieceLayer;
    
    [SerializeField]
    private Transform _pieceContainer;

    private Action _craft;
    private MetaCraftInfo _craftInfo;
    
    public void SetData(MetaCraftInfo craftInfo, Action craft, bool hasCell) {
        _craftInfo = craftInfo;
        
        _hasCellState.SetActive(hasCell);
        _cellAbsentState.SetActive(!hasCell);
        if(!hasCell) _craftButton.interactable = false;
        
        ApplyResultCell(craftInfo.ResultPrefab);
        _nameText.text = craftInfo.CraftName;
        _descriptionText.text = craftInfo.Description;
        _craft = craft;
        _neededCellImage.sprite = SpritesManager.Instance.GetSprite(craftInfo.NeededCell);
        _cellLevelText.text = $"{craftInfo.NeededCellLevel}+";
        
        foreach (var resource in craftInfo.NeededResources) {
            MetaCraftResource _newResource = Instantiate(_resourcePrefab, _resourcesContainer);
            float resourceCount = StorageManager.GameDataMain.GetResource(resource.Key);
            if (resourceCount < resource.Value) _craftButton.interactable = false;
            
            _newResource.SetData(resource.Key, (int)resourceCount, resource.Value);
        }
    }

    private void ApplyResultCell(GameObject cellPrefab) {
        GameObject instance = Instantiate(cellPrefab, _pieceContainer);
        ApplyLayerToChildren(instance.transform, _pieceLayer);
    }

    private void ApplyLayerToChildren(Transform obj, int layer) {
        obj.gameObject.layer = layer;
        foreach (Transform child in obj) {
            ApplyLayerToChildren(child, layer);
        }
    }

    public void Craft() {
        _craft.Invoke();
    }
}
