using AYellowpaper.SerializedCollections;
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

    [SerializeField]
    private SerializedDictionary<ResourceType, TextMeshProUGUI> _resourcesText;

    public void SetData(SpecialOfferData data) {
        _priceText.text = data.Price + " RUB";
        //_offerIcon.sprite = data.Icon;

        foreach (var resource in data.Resources) {
            if (_resourcePrefab) {
                ResourceCount newResource = Instantiate(_resourcePrefab, _resourcesContainer);
                newResource.SetData(resource.Key, resource.Value.ToString());
            }

            if (_resourcesText.TryGetValue(resource.Key, out TextMeshProUGUI ugui)) {
                ugui.text = $"x{resource.Value}";
            }
        }
    }
}