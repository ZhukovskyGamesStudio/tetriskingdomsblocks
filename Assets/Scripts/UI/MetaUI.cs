using UnityEngine;
using TMPro;

public class MetaUI : MonoBehaviour {
    public static MetaUI Instance;

    [SerializeField]
    private Transform[] _healthImages;

    [SerializeField]
    private TMP_Text _healthTimerText;

    private void Awake() {
        Instance = this;
    }

    public TMP_Text HealthTimerText => _healthTimerText;

    public void SetHealthImageActive(int index, bool active) {
        if (_healthImages != null && index >= 0 && index < _healthImages.Length && _healthImages[index] != null)
            _healthImages[index].gameObject.SetActive(active);
    }

    public void SetHealthTimerText(string text) {
        if (_healthTimerText != null)
            _healthTimerText.text = text;
    }

    public void SetHealthTimerActive(bool active) {
        if (_healthTimerText != null)
            _healthTimerText.gameObject.SetActive(active);
    }
}