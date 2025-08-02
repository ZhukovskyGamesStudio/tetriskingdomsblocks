using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RealShopResourceOffer : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _priceText, _countText;

    [SerializeField]
    private Image _offerIcon;

    private Action<ResourceType, int> _buyResource;
    private ResourceOfferData _offerData;
    
    public void SetData(ResourceOfferData data, Action<ResourceType, int> buyResource) {
        _offerData = data;
        
        _priceText.text = data.Price + " RUB";
        _countText.text = "x" + data.ResourceCount;
        _offerIcon.sprite = data.Icon;

        _offerIcon.transform.localScale = new Vector3(data.ImageScale, data.ImageScale, 1);

        _buyResource = buyResource;
    }

    public void Buy() {
        _buyResource.Invoke(_offerData.Resource, _offerData.ResourceCount);
    }
}
