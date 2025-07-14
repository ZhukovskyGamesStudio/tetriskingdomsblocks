using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsDialog : MonoBehaviour {
    [SerializeField]
    private Toggle _musicToggle, _soundToggle, _vibrationToggle;

    private Action<bool> _changeMusic, _changeSound, _changeVibration;
    private Action _goToMeta;

    public void SetData(SettingsData data, Action<bool> changeMusic, Action<bool> changeSound, Action<bool> changeVibration, Action goToMeta) {
        _musicToggle.SetIsOnWithoutNotify(data.IsMusicOn);
        _soundToggle.SetIsOnWithoutNotify(data.IsSoundOn);
        _vibrationToggle.SetIsOnWithoutNotify(data.IsVibrationOn);

        _changeMusic = changeMusic;
        _changeSound = changeSound;
        _changeVibration = changeVibration;
        _goToMeta = goToMeta;
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

    public void Close() {
        gameObject.SetActive(false);
    }
}