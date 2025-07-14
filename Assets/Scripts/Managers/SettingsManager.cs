using UnityEngine;

public class SettingsManager : MonoBehaviour {
    [SerializeField]
    private Transform _settingsContainer;

    [SerializeField]
    private SettingsDialog _settingsDialog;

    public static SettingsManager Instance;

    public void SetSettings() {
        _settingsDialog.SetData(StorageManager.GameDataMain.SettingsData, ChangeToggleMusic, ChangeToggleSound, ChangeToggleVibration,
            GoToMeta);

        BackgroundMusicManager.Instance.ChangeIsPlayingMusic(StorageManager.GameDataMain.SettingsData.IsMusicOn);
    }

    private void Awake() {
        Instance = this;
        SetSettings();
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

    public void CloseSettings() => _settingsContainer.gameObject.SetActive(false);
    public void OpenSettings() => _settingsContainer.gameObject.SetActive(true);

    private void GoToMeta() {
        MainManager.Instance.GoToMeta();
    }
}