using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsDialog : DialogBase {
    [SerializeField]
    private Toggle _musicToggle, _soundToggle, _vibrationToggle;

    private Action<bool> _changeMusic, _changeSound, _changeVibration;
    private Action _goToMeta;

    public override void SetData(object data) {
        Data settingsData = data as Data;
        _musicToggle.SetIsOnWithoutNotify(settingsData.data.IsMusicOn);
        _soundToggle.SetIsOnWithoutNotify(settingsData.data.IsSoundOn);
        _vibrationToggle.SetIsOnWithoutNotify(settingsData.data.IsVibrationOn);

        _changeMusic = settingsData.changeMusic;
        _changeSound = settingsData.changeSound;
        _changeVibration = settingsData.changeVibration;
        _goToMeta = settingsData.goToMeta;
    }

    public void ToggleVibrations(bool isOn) {
        _changeVibration?.Invoke(isOn);
    }

    public void ToggleMusic(bool isOn) {
        _changeMusic?.Invoke(isOn);
    }

    public void ToggleSfx(bool isOn) {
        _changeSound?.Invoke(isOn);
    }

    public void GoToMeta() {
        _goToMeta?.Invoke();
    }

    [Serializable]
    public class Data {
        public SettingsData data;
        public Action<bool> changeMusic;
        public Action<bool> changeSound;
        public Action<bool> changeVibration;
        public Action goToMeta;
    }
}