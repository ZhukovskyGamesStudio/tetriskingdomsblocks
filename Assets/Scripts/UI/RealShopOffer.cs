using System;
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

    private SpecialOfferData _specialOfferData;
    private Action<SpecialOfferData> _buyClick;

    public void SetData(SpecialOfferData data, Action<SpecialOfferData> onBuy) {
        _specialOfferData = data;
        _buyClick = onBuy;
        _priceText.text = InAppsManager.Instance.InAppsProvider.GetPrice(data.Type);
        //_offerIcon.sprite = data.Icon;

        foreach (var resource in data.Resources) {
            if (_resourcePrefab) {
                ResourceCount newResource = Instantiate(_resourcePrefab, _resourcesContainer);
                newResource.SetData(resource.Key, resource.Value.ToString());
            }

            if (_resourcesText.TryGetValue(resource.Key, out TextMeshProUGUI ugui)) {
                if (resource.Key == ResourceType.InfiniteHPMinutes) {
                    var time = TimeSpan.FromMinutes(resource.Value);
                    if (time.TotalHours > 0) {
                        ugui.text = TimeSpan.FromMinutes(resource.Value).ToString(@"hh\:mm");
                    } else {
                        ugui.text = TimeSpan.FromMinutes(resource.Value).ToString(@"mm\:ss");
                    }
                } else {
                    ugui.text = $"x{resource.Value}";
                }
            }
        }
    }

    public void BuyClick() {
        _buyClick?.Invoke(_specialOfferData);
    }
}