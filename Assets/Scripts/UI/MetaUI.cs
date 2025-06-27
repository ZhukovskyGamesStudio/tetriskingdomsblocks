using UnityEngine;
using TMPro;

public class MetaUI : MonoBehaviour {
    public static MetaUI Instance;

    [SerializeField]
    private Transform[] _healthImages;

    [SerializeField]
    private TMP_Text _healthTimerText;

    [SerializeField]
    private TMP_Text _magicCubeCounterText;
    [SerializeField]
    private TMP_Text _goldCounterText;
    [SerializeField]
    private TMP_Text[] _resourcesCountText;
    [SerializeField]
    private TMP_Text _getPieceTimerText;
    [SerializeField]
    private TMP_Text _destroyPieceText;
    [SerializeField]
    private Transform _hummerContainer;
    [SerializeField]
    private Transform _hummerContainerStart;
    [SerializeField]
    private Transform _hummerContainerEnd;
    [SerializeField]
    private Transform _resourcesMarksContainer;
    [SerializeField]
    private ResourceMarkView resourceMarkViewPrefab;

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

    public void SetMagicCubes(int value) {
        if (_magicCubeCounterText != null)
            _magicCubeCounterText.text = value.ToString();
    }

    public void SetGold(int value) {
        if (_goldCounterText != null)
            _goldCounterText.text = value.ToString();
    }

    public void SetResourceCount(int index, int value) {
        if (_resourcesCountText != null && index >= 0 && index < _resourcesCountText.Length && _resourcesCountText[index] != null)
            _resourcesCountText[index].text = value.ToString();
    }

    public void SetGetPieceTimer(string text) {
        if (_getPieceTimerText != null)
            _getPieceTimerText.text = text;
    }

    public void SetDestroyPieceText(string text) {
        if (_destroyPieceText != null)
            _destroyPieceText.text = text;
    }

    public Transform HummerContainer => _hummerContainer;
    public Transform HummerContainerStart => _hummerContainerStart;
    public Transform HummerContainerEnd => _hummerContainerEnd;
    public Transform ResourcesMarksContainer => _resourcesMarksContainer;
    public ResourceMarkView ResourceMarkViewPrefab => resourceMarkViewPrefab;
}