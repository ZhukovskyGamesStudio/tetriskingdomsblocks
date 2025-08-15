using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

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

    private Action _clickClose;
    private Action<ResourceType, int> _buyResource;

    public override void SetData(object data) {
        Data dialogData = data as Data;

        _buyResource = dialogData.BuyResource;
        
        if (dialogData.IsCore) {
            _balanceText.text = dialogData.Balance.ToString();
        } else _coreState.SetActive(false);
        
        _clickClose = dialogData.ClickClose;

        foreach (SpecialOfferData specialOffer in _offersConfig.SpecialOffers) {
            RealShopOffer newOffer = Instantiate(specialOffer.Prefab, _specialOffersContainer);
            newOffer.SetData(specialOffer);
        }
        foreach (SpecialOfferData bundleOffer in _offersConfig.BundleOffers) {
            RealShopOffer newOffer = Instantiate(bundleOffer.Prefab, _bundleOffersContainers);
            newOffer.SetData(bundleOffer);
        }
        
        foreach (ResourceOfferData resourceOffer in _offersConfig.ResourceOffers) {
            RealShopResourceOffer newOffer = Instantiate(_resourceOfferPrefab, _resourceOffersContainer);
            newOffer.SetData(resourceOffer, BuyResource);
        }
        BuyPieceForCoinsOffer buyPieceOffer = Instantiate(_offersConfig.BuyPieceForCoinsOffer, _buyPieceContainer);
        buyPieceOffer.SetData(_offersConfig.BuyPieceForCoinsCost);
    }

    public void BuyResource(ResourceType resource, int count) {
        _buyResource.Invoke(resource, count);
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
        public Action<ResourceType, int> BuyResource;
    }
}