using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class WinDialog : DialogBase {
    [SerializeField]
    private TextMeshProUGUI _coinsText, _cubesText;
    
    private Action _clickClaim;

    public override void SetData(object data) {
        Data dialogData = data as Data;

        _clickClaim = dialogData.ClickClaim;
        _coinsText.text = dialogData.Coins.ToString();
        _cubesText.text = dialogData.Cubes.ToString();
    }

    public void ClickClaim() {
        _clickClaim.Invoke();
        Hide().Forget();
    }

    [Serializable]
    public class Data {
        public Action ClickClaim;
        public int Coins;
        public int Cubes;
    }
}