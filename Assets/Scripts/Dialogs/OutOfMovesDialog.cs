using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class OutOfMovesDialog : DialogBase {
    [SerializeField]
    private TextMeshProUGUI _balanceText, _costText;
    
    private Action _clickAdd, _clickClose, _clickBalance;

    public override void SetData(object data) {
        Data dialogData = data as Data;

        _clickAdd = dialogData.ClickAdd;
        _clickClose = dialogData.ClickClose;
        _clickBalance = dialogData.ClickBalance;
        _balanceText.text = dialogData.Balance.ToString();
        _costText.text = _costText.text.Replace("{cost}", dialogData.Cost.ToString());
    }

    public void ClickAdd() {
        _clickAdd.Invoke();
        Hide().Forget();
    }

    public void ClickClose() {
        _clickClose.Invoke();
        Hide().Forget();
    }

    public void ClickBalance() {
        _clickBalance.Invoke();
    }

    [Serializable]
    public class Data {
        public Action ClickAdd;
        public Action ClickClose;
        public Action ClickBalance;
        public int Balance;
        public int Cost;
    }
}