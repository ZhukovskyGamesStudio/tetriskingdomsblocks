using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class LoseDialog : DialogBase {
    [SerializeField]
    private TextMeshProUGUI _hpText;
    
    private Action _clickContinue;

    public override void SetData(object data) {
        Data dialogData = data as Data;

        _clickContinue = dialogData.ClickContinue;
        _hpText.text = _hpText.text.Replace("{hp}", dialogData.Hp.ToString());
    }

    public void ClickContinue() {
        _clickContinue.Invoke();
        Hide().Forget();
    }

    [Serializable]
    public class Data {
        public Action ClickContinue;
        public int Hp;
    }
}