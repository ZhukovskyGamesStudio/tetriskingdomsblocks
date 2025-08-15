using System;
using TMPro;
using UnityEngine;

public class BuyPieceForCoinsOffer : MonoBehaviour {
    [SerializeField]
    private TextMeshProUGUI _costText;

    private Action _onBuy;

    public void SetData(int cost, Action onBuy) {
        _onBuy = onBuy;
        _costText.text = cost.ToString();
    }

    public void Buy() {
        _onBuy?.Invoke();
    }
}