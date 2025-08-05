using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class OutOfMovesDialog : DialogBase {
    [SerializeField]
    private TextMeshProUGUI _costText;

    private Action _buyMoves, _clickClose;

    public override void SetData(object data) {
        Data dialogData = data as Data;

        _buyMoves = dialogData.BuyMoves;
        _clickClose = dialogData.ClickClose;
        _costText.text = dialogData.Cost.ToString();
    }

    public void ClickBuy() {
        Hide().Forget();
        _buyMoves.Invoke();
    }

    public void ClickCLose() {
        Hide().Forget();
        _clickClose.Invoke();
    }

    [Serializable]
    public class Data {
        public int Cost;
        public Action BuyMoves, ClickClose;
    }
}