using System;
using UnityEngine;
using UnityEngine.UI;

public class MetaSettingsDialog : DialogBase {
    [SerializeField]
    private UIToggle _musicToggle, _soundToggle, _vibrationToggle, _notificationsToggle;
    
    private Action<bool> _changeMusic, _changeSound, _changeVibration, _changeNotifications;
    private Action _clickSupport, _clickTerms;

    public override void SetData(object data) {
        Data dialogData = data as Data;

        _clickSupport = dialogData.ClickSupport;
        _clickTerms = dialogData.ClickTerms;

        _changeMusic = dialogData.ChangeMusic;
        _changeSound = dialogData.ChangeSound;
        _changeVibration = dialogData.ChangeVibration;
        _changeNotifications = dialogData.ChangeNotifications;

        _musicToggle.Init(dialogData.IsMusicOn);
        _soundToggle.Init(dialogData.IsSoundOn);
        _vibrationToggle.Init(dialogData.IsVibrationOn);
        _notificationsToggle.Init(dialogData.IsNotificationsOn);
    }

    public void ToggleVibrations(bool isOn) {
        _changeVibration.Invoke(isOn);
    }

    public void CloseSettings() {
        if(MetaTabsPanel.Instance.SelectedTab != MetaTab.Rule)return;
        
        MetaFieldManager.Instance.CanDragCamera = true;
        MetaFieldManager.Instance.CanOpenLockedZones = true;
    }
    public void ToggleMusic(bool isOn) {
        _changeMusic.Invoke(isOn);
    }

    public void ToggleSfx(bool isOn) {
        _changeSound.Invoke(isOn);
    }

    public void ToggleNotifications(bool isOn) {
        _changeNotifications.Invoke(isOn);
    }

    public void ClickSupport() {
        _clickSupport.Invoke();
    }
    
    public void ClickTerms() {
        _clickTerms.Invoke();
    }

    [Serializable]
    public class Data {
        public Action<bool> ChangeMusic, ChangeSound, ChangeVibration, ChangeNotifications;
        public Action ClickSupport, ClickTerms;
        public bool IsMusicOn, IsSoundOn, IsVibrationOn, IsNotificationsOn;
    }
}