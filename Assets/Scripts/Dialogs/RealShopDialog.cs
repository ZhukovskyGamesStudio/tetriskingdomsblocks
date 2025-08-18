using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RealShopDialog : DialogBase {
    [SerializeField]
    private TextMeshProUGUI _balanceText;

    [SerializeField]
    private Transform _specialOffersContainer, _bundleOffersContainers, _resourceOffersContainer, _buyPieceContainer;

    [SerializeField]
    private RealShopOffer _specialOfferPrefab;

    [SerializeField]
    private RealShopResourceOffer _resourceOfferPrefab;

    [SerializeField]
    private GameObject _coreState;

    [SerializeField]
    private ShopOffersConfig _offersConfig;

    [SerializeField]
    private ScrollRect _scrollRect;

    private BuyPieceForCoinsOffer _buyPieceOffer;
    
    private Action _clickClose;
    private Data _data;

    public override void SetData(object data) {
        Data dialogData = data as Data;
        _data = dialogData;

        if (dialogData.IsCore) {
            _balanceText.text = dialogData.Balance.ToString();
        } else _coreState.SetActive(false);

        _clickClose = dialogData.ClickClose;

        foreach (SpecialOfferData specialOffer in _offersConfig.SpecialOffers) {
            RealShopOffer newOffer = Instantiate(specialOffer.Prefab, _specialOffersContainer);
            newOffer.SetData(specialOffer, BuyOffer);
        }

        foreach (SpecialOfferData bundleOffer in _offersConfig.BundleOffers) {
            RealShopOffer newOffer = Instantiate(bundleOffer.Prefab, _bundleOffersContainers);
            newOffer.SetData(bundleOffer, BuyOffer);
        }

        foreach (ResourceOfferData resourceOffer in _offersConfig.ResourceOffers) {
            RealShopResourceOffer newOffer = Instantiate(_resourceOfferPrefab, _resourceOffersContainer);
            newOffer.SetData(resourceOffer, BuyResource);
        }

        _buyPieceOffer = Instantiate(_offersConfig.BuyPieceForCoinsOffer, _buyPieceContainer);
        _buyPieceOffer.SetData(_offersConfig.BuyPieceForCoinsCost, BuyPieceForCoins);
        
    }

    public override UniTask Show(Action onClose) {
        if (_data.OnPiece) {
            _scrollRect.normalizedPosition = Vector2.down;
        }
        return base.Show(onClose);
    }

    public void BuyOffer(SpecialOfferData data) {
        _data.BuyOffer(data);
    }

    public void BuyResource(ResourceOfferData data) {
        _data.BuyResource(data);
    }

    public void BuyPieceForCoins() {
        _data.BuyPieceForCoins(_offersConfig.BuyPieceForCoinsCost);
    }

    public void ClickClose() {
        Hide().Forget();
        _clickClose?.Invoke();
    }

    [Serializable]
    public class Data {
        public Action ClickClose;
        public int Balance;
        public bool IsCore;
        public Action<int> BuyPieceForCoins;
        public Action<SpecialOfferData> BuyOffer;
        public Action<ResourceOfferData> BuyResource;
        public bool OnPiece;
    }
}

public static class ScrollRectExtensions
{
    public static Vector2 GetSnapToPositionToBringChildIntoView(this ScrollRect instance, RectTransform child)
    {
        Canvas.ForceUpdateCanvases();
        Vector2 viewportLocalPosition = instance.viewport.localPosition;
        Vector2 childLocalPosition   = child.localPosition;
        Vector2 result = new Vector2(
            0 - (viewportLocalPosition.x + childLocalPosition.x),
            0 - (viewportLocalPosition.y + childLocalPosition.y)
        );
        return result;
    }
}