using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class RealShopDialog : DialogBase {
    [SerializeField]
    private TextMeshProUGUI _balanceText, _titlePrefab;

    [SerializeField]
    private Transform _offersContainer;

    [SerializeField]
    private RealShopOffer _offerPrefab;

    [SerializeField]
    private GameObject _balancePanel;

    [SerializeField]
    private ShopOffersConfig _offersConfig;

    private Action _clickClose;

    public override void SetData(object data) {
        Data dialogData = data as Data;
        
        if(dialogData.Balance != null) _balanceText.text = dialogData.Balance.ToString();
        else _balancePanel.SetActive(false);
        
        _clickClose = dialogData.ClickClose;
        foreach (OffersGroupData offersGroup in _offersConfig.OffersGroups) {
            Instantiate(_titlePrefab, _offersContainer).text = offersGroup.Title;
            foreach (OfferData offerData in offersGroup.Offers) {
                Instantiate(_offerPrefab, _offersContainer).SetData(offerData);
            }
        }
    }

    public void ClickClose() {
        Hide().Forget();
        _clickClose.Invoke();
    }

    [Serializable]
    public class Data {
        public Action ClickClose;
        public int? Balance = null;
    }
}