using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RealShopOffer : MonoBehaviour {
    [SerializeField]
    private Transform _resourcesContainer;

    [SerializeField]
    private ResourceCount _resourcePrefab;

    [SerializeField]
    private TextMeshProUGUI _priceText;

    [SerializeField]
    private Image _offerIcon;
    
    public void SetData(SpecialOfferData data) {
        _priceText.text = data.Price + " RUB";
        _offerIcon.sprite = data.Icon;

        foreach (var resource in data.Resources) {
            ResourceCount newResource = Instantiate(_resourcePrefab, _resourcesContainer);
            newResource.SetData(resource.Key, resource.Value.ToString());
        }
    }
}
