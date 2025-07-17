using UnityEngine;

public class SettingsManager : MonoBehaviour {
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

    public void ChangeToggleVibration(bool isOn) => StorageManager.GameDataMain.SettingsData.IsVibrationOn = isOn;

    public void ChangeToggleMusic(bool isOn) {
        StorageManager.GameDataMain.SettingsData.IsMusicOn = isOn;
        BackgroundMusicManager.Instance.ChangeIsPlayingMusic(isOn);
    }

    public void ChangeToggleSound(bool isOn) {
        StorageManager.GameDataMain.SettingsData.IsSoundOn = isOn;
        Debug.Log(StorageManager.GameDataMain.SettingsData.IsSoundOn);
    }

    public void OpenSettings() => ShowGameSettingsDialog();

    private void AskGoToMeta() {
        DialogsManager.Instance.ShowDialogWithData(new DialogWithData() {
            DialogType = typeof(ExitGameDialog),
            Data = new ExitGameDialog.Data {
                СlickYes = MainManager.Instance.GoToMeta
            }
        });
    }
}