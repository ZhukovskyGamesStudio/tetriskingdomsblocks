using TMPro;
using UnityEngine;

public class RealShopOffer : MonoBehaviour {
    [SerializeField]
    private Transform _resourcesObject;

    [SerializeField]
    private ResourceCount _resourcePrefab;

    [SerializeField]
    private TextMeshProUGUI _titleText, _priceText;
    
    public void SetData(OfferData data) {
        _titleText.text = data.Title;
        _priceText.text = data.Price + " RUB";

        foreach (var resource in data.Resources) {
            ResourceCount newResource = Instantiate(_resourcePrefab, _resourcesObject);
            newResource.SetData(resource.Key, resource.Value.ToString());
        }
    }
}
