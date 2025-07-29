using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditAvatarDialog : DialogBase {
    [SerializeField]
    private TextMeshProUGUI _playerNameText;

    [SerializeField]
    private GameObject _avatarPrefab;

    [SerializeField]
    private Transform _avatarsContainer;

    [SerializeField]
    private Image _avatarImage;

    private Action _clickClose;
    private Action<int> _clickChangeAvatar;

    private List<Sprite> _possibleAvatars;
    
    public override void SetData(object data) {
        Data dialogData = data as Data;

        _playerNameText.text = dialogData.PlayerName;

        _clickClose = dialogData.ClickClose;
        _clickChangeAvatar = dialogData.ClickChangeAvatar;

        _possibleAvatars = dialogData.PossibleAvatars;
        _avatarImage.sprite = _possibleAvatars[dialogData.CurrentAvatar];
        
        for (int i = 0; i < _possibleAvatars.Count; i++) {
            GameObject avatarObject = Instantiate(_avatarPrefab, _avatarsContainer);
            
            int avatarId = i;
            avatarObject.GetComponent<Image>().sprite = _possibleAvatars[i];
            avatarObject.GetComponent<Button>().onClick.AddListener(() => ClickChangeAvatar(avatarId));
        }
    }

    public void ClickChangeAvatar(int avatarId) {
        _avatarImage.sprite = _possibleAvatars[avatarId];
        _clickChangeAvatar.Invoke(avatarId);
    }

    public void ClickClose() {
        Hide().Forget();
        _clickClose.Invoke();
    }

    [Serializable]
    public class Data {
        public string PlayerName;
        public Action ClickClose;
        public Action<int> ClickChangeAvatar;
        public List<Sprite> PossibleAvatars;
        public int CurrentAvatar;
    }
}