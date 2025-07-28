using System;
using TMPro;
using UnityEngine;

public class ProfileDialog : DialogBase {
    [SerializeField]
    private TextMeshProUGUI _winsText, _levelsText, _bestText, _builtText, _playerNameText;

    private Action _clickEditPic;
    
    public override void SetData(object data) {
        Data dialogData = data as Data;

        _winsText.text = dialogData.Wins.ToString();
        _levelsText.text = dialogData.Levels.ToString();
        _bestText.text = dialogData.WeeksBest.ToString();
        _builtText.text = dialogData.BuiltCells.ToString();

        _playerNameText.text = dialogData.PlayerName;

        _clickEditPic = dialogData.ClickEditPic;
    }

    public void ClickEditPic() {
        _clickEditPic.Invoke();
    }

    [Serializable]
    public class Data {
        public int Wins;
        public int Levels;
        public int WeeksBest;
        public int BuiltCells;
        public string PlayerName;
        public Action ClickEditPic;
    }
}