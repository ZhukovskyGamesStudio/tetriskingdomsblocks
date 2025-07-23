using System;
using TMPro;
using UnityEngine;

public class HealthView : MonoBehaviour {
    [SerializeField]
    private TMP_Text _healthCountText;

    [SerializeField]
    private TMP_Text _healthTimerText;

    public TMP_Text HealthTimerText => _healthTimerText;

    public void SetHealthCountText(int index) {
        if (_healthCountText != null)
            _healthCountText.text = index.ToString();
    }

    public void SetHealthTimerText(string text) {
        if (_healthTimerText != null)
            _healthTimerText.text = text;
    }

    public void SetHealthTimerActive(bool active) {
        if (_healthTimerText != null)
            _healthTimerText.gameObject.SetActive(active);
    }
    
    public void UpdateHealthTimerUI(TimeSpan time) {
        SetHealthTimerActive(true);
        SetHealthTimerText(TimeConverter.ConvertToTimeString(time));
    }

    public void SetNoConnection() {
        SetHealthTimerText("No internet connection");
    }
}