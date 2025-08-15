using System;
using TMPro;
using UnityEngine;

public class BuyPieceForCoinsOffer : MonoBehaviour {
    [SerializeField]
    private TextMeshProUGUI _costText;
    public void SetData(int cost) {
        _costText.text = cost.ToString();
    }
}
