using UnityEngine;

public class SettingsManager : MonoBehaviour {
    public static SettingsManager Instance;

    private void Awake() {
        Instance = this;
    }

    public void SetSettings() {
        Debug.Log(StorageManager.GameDataMain.SettingsData.IsMusicOn);
        BackgroundMusicManager.Instance.ChangeIsPlayingMusic(StorageManager.GameDataMain.SettingsData.IsMusicOn);
    }

    public void ShowSettingsDialog() {
        var passingData = new DialogWithData() {
            DialogType = typeof(SettingsDialog),
            Data = new SettingsDialog.Data() {
                data = StorageManager.GameDataMain.SettingsData,
                changeMusic = ChangeToggleMusic,
                changeSound = ChangeToggleSound,
                changeVibration = ChangeToggleVibration,
                goToMeta = AskGoToMeta
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

    public void OpenSettings() => ShowSettingsDialog();

    private void AskGoToMeta() {
        DialogsManager.Instance.ShowDialogWithData(new DialogWithData() {
            DialogType = typeof(ExitGameDialog),
            Data = new ExitGameDialog.Data {
                СlickYes = MainManager.Instance.GoToMeta
            }
        });
    }
}