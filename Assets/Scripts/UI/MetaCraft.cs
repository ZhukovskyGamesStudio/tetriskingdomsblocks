using TMPro;
using UnityEngine;

public class MetaCraft : MonoBehaviour {
    [SerializeField]
    private MetaCraftResource _resourcePrefab;
    
    [SerializeField]
    private Transform _resourcesContainer;

    [SerializeField]
    private TextMeshProUGUI _nameText, _descriptionText;
    
    public void SetData(MetaCraftInfo craftInfo) {
        _nameText.text = craftInfo.CraftName;
        _descriptionText.text = craftInfo.Description;

        foreach (var resource in craftInfo.NeededResources) {
            MetaCraftResource _newResource = Instantiate(_resourcePrefab, _resourcesContainer);
            
            _newResource.SetData(resource.Key, (int)StorageManager.GameDataMain.GetResource(resource.Key), resource.Value);
        }
    }
}
