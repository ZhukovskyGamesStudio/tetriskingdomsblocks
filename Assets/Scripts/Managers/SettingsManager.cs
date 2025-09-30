using System;
using UnityEngine;

public class SettingsManager : MonoBehaviour {

    private const string _privacy_url = "https://eightforce.com/privacy/";
    
    public static SettingsManager Instance;

    private void Awake() {
        Instance = this;
    }

    public void SetSettings() {
        BackgroundMusicManager.Instance.ChangeIsPlayingMusic(StorageManager.GameDataMain.SettingsData.IsMusicOn);
    }

    public void ShowGameSettingsDialog() {
        var passingData = new DialogWithData {
            DialogType = typeof(GameSettingsDialog),
            Data = new GameSettingsDialog.Data {
                ChangeMusic = ChangeToggleMusic,
                ChangeSound = ChangeToggleSound,
                ChangeVibration = ChangeToggleVibration,
                GoToMeta = AskGoToMeta,
                IsMusicOn = StorageManager.GameDataMain.SettingsData.IsMusicOn,
                IsSoundOn = StorageManager.GameDataMain.SettingsData.IsSoundOn,
                IsVibrationOn = StorageManager.GameDataMain.SettingsData.IsVibrationOn
            }
        };

        DialogsManager.Instance.ShowDialogWithData(passingData);
    }

    public void ShowMetaSettingsDialog() {
        var passingData = new DialogWithData {
            DialogType = typeof(MetaSettingsDialog),
            Data = new MetaSettingsDialog.Data {
                ChangeMusic = ChangeToggleMusic,
                ChangeSound = ChangeToggleSound,
                ChangeVibration = ChangeToggleVibration,
                ChangeNotifications = isOn => print("notifications: " + isOn),
                IsMusicOn = StorageManager.GameDataMain.SettingsData.IsMusicOn,
                IsSoundOn = StorageManager.GameDataMain.SettingsData.IsSoundOn,
                IsVibrationOn = StorageManager.GameDataMain.SettingsData.IsVibrationOn,
                IsNotificationsOn = false,
                ClickSupport = () => throw new NotImplementedException(), // TODO: убрать заглушки
                ClickTerms = () => Application.OpenURL(_privacy_url)
            }
        };

        DialogsManager.Instance.ShowDialogWithData(passingData);
    }

    public void ChangeToggleVibration(bool isOn) => StorageManager.GameDataMain.SettingsData.IsVibrationOn = isOn;

    public void ChangeToggleMusic(bool isOn) {
        StorageManager.GameDataMain.SettingsData.IsMusicOn = isOn;
        BackgroundMusicManager.Instance.ChangeIsPlayingMusic(isOn);
    }

    public void ChangeToggleSound(bool isOn) {
        StorageManager.GameDataMain.SettingsData.IsSoundOn = isOn;
    }

    private void AskGoToMeta() {
        DialogsManager.Instance.ShowDialogWithData(new DialogWithData {
            DialogType = typeof(ExitGameDialog),
            Data = new ExitGameDialog.Data {
                ClickExit = MainManager.Instance.RemoveHealthAndGoToMeta
            }
        });
    }
}