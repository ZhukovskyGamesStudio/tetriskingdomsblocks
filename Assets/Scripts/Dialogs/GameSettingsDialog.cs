using System;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GameSettingsDialog : DialogBase {
    [SerializedDictionary]
    public SerializedDictionary<ToggleType, Toggle> _toggles;
    
    [SerializedDictionary]
    public SerializedDictionary<ToggleType, Image> _togglesImages;
    
    [SerializedDictionary]
    public SerializedDictionary<ToggleType, Sprite> _togglesOnSprites;
    
    [SerializedDictionary]
    public SerializedDictionary<ToggleType, Sprite> _togglesOffSprites;
    
    private Action _goToMeta;
    private Action<bool> _changeMusic, _changeSound, _changeVibration;

    public override void SetData(object data) {
        Data dialogData = data as Data;

        _goToMeta = dialogData.GoToMeta;
        _changeMusic = dialogData.ChangeMusic;
        _changeSound = dialogData.ChangeSound;
        _changeVibration = dialogData.ChangeVibration;

        _toggles[ToggleType.Music].isOn = dialogData.IsMusicOn;
        _toggles[ToggleType.Sound].isOn = dialogData.IsSoundOn;
        _toggles[ToggleType.Vibration].isOn = dialogData.IsVibrationOn;
        
        ToggleImage(ToggleType.Music, dialogData.IsMusicOn);
        ToggleImage(ToggleType.Sound, dialogData.IsSoundOn);
        ToggleImage(ToggleType.Vibration, dialogData.IsVibrationOn);
    }

    private void ToggleImage(ToggleType toggle, bool isOn) {
        _togglesImages[toggle].sprite = isOn ? _togglesOnSprites[toggle] : _togglesOffSprites[toggle];
    }

    public void ToggleVibrations(bool isOn) {
        _changeVibration.Invoke(isOn);
        ToggleImage(ToggleType.Vibration, isOn);
    }

    public void ToggleMusic(bool isOn) {
        _changeMusic.Invoke(isOn);
        ToggleImage(ToggleType.Music, isOn);
    }

    public void ToggleSfx(bool isOn) {
        _changeSound.Invoke(isOn);
        ToggleImage(ToggleType.Sound, isOn);
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
    
    public enum ToggleType {
        Sound,
        Music,
        Vibration
    }
}