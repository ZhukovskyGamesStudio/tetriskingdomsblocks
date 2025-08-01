using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class LoseDialog : DialogBase {
    [SerializeField]
    private TextMeshProUGUI _hpText;
    
    private Action _clickRetry, _clickExit;

    public override void SetData(object data) {
        Data dialogData = data as Data;

        _clickRetry = dialogData.ClickRetry;
        _clickExit = dialogData.ClickExit;
        _hpText.text = _hpText.text.Replace("{hp}", dialogData.Hp.ToString());
    }

    public void ClickRetry() {
        _clickRetry.Invoke();
        Hide().Forget();
    }

    public void ClickExit() {
        _clickExit.Invoke();
        Hide().Forget();
    }

    [Serializable]
    public class Data {
        public Action ClickRetry, ClickExit;
        public int Hp;
    }
}