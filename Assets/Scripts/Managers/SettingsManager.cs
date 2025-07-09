using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{

    [SerializeField] private Toggle _musicToggle;
    [SerializeField] private Toggle _soundToggle;
    [SerializeField] private Toggle _vibrationToggle;
    [SerializeField]
    private Transform _settingsContainer;
        
    public static SettingsManager Instance;

    public void SetSettings()
    {
        _musicToggle.isOn = StorageManager.GameDataMain.IsMusicOn;
        _soundToggle.isOn = StorageManager.GameDataMain.IsSoundOn;
        _vibrationToggle.isOn = StorageManager.GameDataMain.IsVibrationOn;
        BackgroundMusicManager.Instance.ChangeIsPlayingMusic(StorageManager.GameDataMain.IsMusicOn);
    }
    private void Awake()
    {
        Instance = this;
        _musicToggle.onValueChanged.AddListener(ChangeToggleMusic);
        _soundToggle.onValueChanged.AddListener(ChangeToggleSound);
        _vibrationToggle.onValueChanged.AddListener(ChangeToggleVibration);
    }
    
    public void ChangeToggleVibration( bool isOn)=> StorageManager.GameDataMain.IsVibrationOn = isOn;
    public void ChangeToggleMusic( bool isOn)
    {
        StorageManager.GameDataMain.IsMusicOn = isOn;
        BackgroundMusicManager.Instance.ChangeIsPlayingMusic(isOn);
    }

    public void ChangeToggleSound( bool isOn)
    {
        StorageManager.GameDataMain.IsSoundOn = isOn;
        Debug.Log(StorageManager.GameDataMain.IsSoundOn);
    }

    public void CloseSettings()=>_settingsContainer.gameObject.SetActive(false);
    public void OpenSettings()=>_settingsContainer.gameObject.SetActive(true);
}
