using System;
using TMPro;
using UnityEngine;

public class TextDialog : DialogBase {
    [SerializeField]
    private TextMeshProUGUI _titleText, _mainText, _buttonText;
    
    public override void SetData(object data) {
        Data dialogData = data as Data;

        _titleText.text = dialogData.Title;
        _mainText.text = dialogData.MainText;
        _buttonText.text = dialogData.ButtonText;
    }

    [Serializable]
    public class Data {
        public string Title;
        public string MainText;
        public string ButtonText = "Ok";
    }
}