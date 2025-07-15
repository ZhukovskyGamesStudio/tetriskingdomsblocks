using System;
using TMPro;
using UnityEngine;

public class RealShopDialog : DialogBase {
    [SerializeField]
    private TextMeshProUGUI _balanceText, _titlePrefab;

    [SerializeField]
    private Transform _offersContainer;

    [SerializeField]
    private RealShopOffer _offerPrefab;

    public override void SetData(object data) {
        Data dialogData = data as Data;

        _balanceText.text = dialogData.Balance.ToString();
        foreach (OffersGroupData offersGroup in dialogData.OffersGroups) {
            Instantiate(_titlePrefab, _offersContainer).text = offersGroup.Title;
            foreach (OfferData offerData in offersGroup.Offers) {
                Instantiate(_offerPrefab, _offersContainer).SetData(offerData);
            }
        }
    }

    [Serializable]
    public class Data {
        public OffersGroupData[] OffersGroups;
        public int Balance;
    }
}