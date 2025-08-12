using System;
using TMPro;
using UnityEngine;

public class MetaCraft : MonoBehaviour {
    [SerializeField]
    private MetaCraftResource _resourcePrefab;
    
    [SerializeField]
    private Transform _resourcesContainer;

    [SerializeField]
    private TextMeshProUGUI _nameText, _descriptionText;

    private Action _craft;
    
    public void SetData(MetaCraftInfo craftInfo, Action craft) {
        _nameText.text = craftInfo.CraftName;
        _descriptionText.text = craftInfo.Description;
        _craft = craft;

        foreach (var resource in craftInfo.NeededResources) {
            MetaCraftResource _newResource = Instantiate(_resourcePrefab, _resourcesContainer);
            
            _newResource.SetData(resource.Key, (int)StorageManager.GameDataMain.GetResource(resource.Key), resource.Value);
        }
    }

    public void Craft() {
        _craft.Invoke();
    }
}
