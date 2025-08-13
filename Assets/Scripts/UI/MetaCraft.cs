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
    private TextMeshProUGUI _nameText, _descriptionText;

    [SerializeField]
    private Image _neededCellImage;

    [SerializeField]
    private Button _craftButton;

    private Action _craft;
    private MetaCraftInfo _craftInfo;
    
    public void SetData(MetaCraftInfo craftInfo, Action craft) {
        _craftInfo = craftInfo;
        
        _nameText.text = craftInfo.CraftName;
        _descriptionText.text = craftInfo.Description;
        _craft = craft;
        _neededCellImage.sprite = SpritesManager.Instance.GetSprite(craftInfo.NeededCell);

        foreach (var resource in craftInfo.NeededResources) {
            MetaCraftResource _newResource = Instantiate(_resourcePrefab, _resourcesContainer);
            float resourceCount = StorageManager.GameDataMain.GetResource(resource.Key);
            if (resourceCount < resource.Value) _craftButton.interactable = false;
            
            _newResource.SetData(resource.Key, (int)resourceCount, resource.Value);
        }
    }

    public void Craft() {
        _craft.Invoke();
    }
}
