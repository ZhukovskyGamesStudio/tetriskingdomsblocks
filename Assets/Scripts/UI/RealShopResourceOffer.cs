using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RealShopResourceOffer : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _priceText, _countText;

    [SerializeField]
    private Image _offerIcon;
    
    public void SetData(ResourceOfferData data) {
        _priceText.text = data.Price + " RUB";
        _countText.text = "x" + data.ResourceCount;
        _offerIcon.sprite = data.Icon;
    }
}
