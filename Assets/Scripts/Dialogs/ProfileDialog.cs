using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileDialog : DialogBase {
    [SerializeField]
    private TextMeshProUGUI _winsText, _levelsText, _bestText, _builtText, _playerNameText;

    [SerializeField]
    private Image _avatarImage;

    private Action _clickEditAvatar;
    
    public override void SetData(object data) {
        Data dialogData = data as Data;

        _winsText.text = dialogData.Wins.ToString();
        _levelsText.text = dialogData.Levels.ToString();
        _bestText.text = dialogData.WeeksBest.ToString();
        _builtText.text = dialogData.BuiltCells.ToString();

        _playerNameText.text = dialogData.PlayerName;
        _avatarImage.sprite = dialogData.AvatarSprite;

        _clickEditAvatar = dialogData.ClickEditAvatar;
    }

    public void ClickEditAvatar() {
        Hide().Forget();
        _clickEditAvatar.Invoke();
    }

    [Serializable]
    public class Data {
        public int Wins;
        public int Levels;
        public int WeeksBest;
        public int BuiltCells;
        public string PlayerName;
        public Sprite AvatarSprite;
        public Action ClickEditAvatar;
    }
}