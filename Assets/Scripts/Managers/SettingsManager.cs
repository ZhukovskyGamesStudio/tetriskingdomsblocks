using UnityEngine;

public class SettingsManager : MonoBehaviour {
    public static SettingsManager Instance;

    private void Awake() {
        Instance = this;
    }

    public void SetSettings() {
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
                goToMeta = GoToMeta
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

    private void GoToMeta() {
        MainManager.Instance.GoToMeta();
    }
}