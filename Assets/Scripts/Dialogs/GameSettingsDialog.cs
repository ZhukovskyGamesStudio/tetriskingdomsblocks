using System;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GameSettingsDialog : DialogBase {
    [SerializeField]
    private UIToggle _musicToggle, _soundToggle, _vibrationToggle;
    
    private Action _goToMeta;
    private Action<bool> _changeMusic, _changeSound, _changeVibration;

    [SerializeField]
    private Button _exitFromGameButton;
    [SerializeField]
    private Transform _exitLockedButtonObject;

    public override void SetData(object data) {
        Data dialogData = data as Data;

        _goToMeta = dialogData.GoToMeta;
        _changeMusic = dialogData.ChangeMusic;
        _changeSound = dialogData.ChangeSound;
        _changeVibration = dialogData.ChangeVibration;
        
        _musicToggle.Init(dialogData.IsMusicOn);
        _soundToggle.Init(dialogData.IsSoundOn);
        _vibrationToggle.Init(dialogData.IsVibrationOn);

        if (StorageManager.GameDataMain.CurMaxLevel <= 2) {
             _exitFromGameButton.gameObject.SetActive(false);
             Instantiate(_exitLockedButtonObject, _exitFromGameButton.transform.parent);
        }
       
    }

    public void ToggleVibrations(bool isOn) {
        _changeVibration.Invoke(isOn);
    }

    public void ToggleMusic(bool isOn) {
        _changeMusic.Invoke(isOn);
    }

    public void ToggleSfx(bool isOn) {
        _changeSound.Invoke(isOn);
    }

    public void GoToMeta() {
        Hide().Forget();
        _goToMeta.Invoke();
    }

    [Serializable]
    public class Data {
        public Action<bool> ChangeMusic, ChangeSound, ChangeVibration;
        public Action GoToMeta;
        public bool IsMusicOn, IsSoundOn, IsVibrationOn;
    }
}