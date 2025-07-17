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
    private Transform _resourcesMarksContainer;

    [SerializeField]
    private ResourceMarkView resourceMarkViewPrefab;

    [SerializeField]
    private Transform _unlockCellUIContainer;

    [SerializeField]
    private TMP_Text _unlockCellText;

    [SerializeField]
    private Transform _upgradeCellUIContainer;

    [SerializeField]
    private TMP_Text _upgradeCellText;

    [SerializeField]
    private TMP_Text _cellInfoText;

    [SerializeField]
    private GameObject _ruleState, _buildState;

    [SerializeField]
    private GameObject _ruleCamera, _buildCamera;

    [SerializeField]
    private GameObject _worldCanvas;

    public Transform ResourcesMarksContainer => _resourcesMarksContainer;
    public ResourceMarkView ResourceMarkViewPrefab => resourceMarkViewPrefab;

    private void Awake() {
        Instance = this;
    }

    public TMP_Text HealthTimerText => _healthTimerText;

    public void SetHealthImageActive(int index, bool active) {
        if (_healthImages != null && index >= 0 && index < _healthImages.Length && _healthImages[index] != null)
            _healthImages[index].gameObject.SetActive(active);
    }

    public void SetActiveUnlockUI(bool active) => _unlockCellUIContainer.gameObject.SetActive(active);

    public void SetPositionUnlockUI(Vector3 pos) => _unlockCellUIContainer.transform.position = pos;

    public void SetActiveUpgradeUI(bool active) => _upgradeCellUIContainer.gameObject.SetActive(active);

    public void SetPositionUpgradeUI(Vector3 pos) => _upgradeCellUIContainer.transform.position = pos;

    public void UnlockCellText(string text) {
        if (_unlockCellText != null)
            _unlockCellText.text = text;
    }

    public void SetUpgradeCellText(string textInfo, string textButton) {
        _upgradeCellText.text = textButton;
        _cellInfoText.text = textInfo;
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

    public void OpenBuildState() {
        _buildState.SetActive(true);
        _ruleState.SetActive(false);
        _ruleCamera.SetActive(false);
        _buildCamera.SetActive(true);
        _worldCanvas.SetActive(false);
    }

    public void OpenRuleState() {
        _buildState.SetActive(false);
        _ruleState.SetActive(true);
        _ruleCamera.SetActive(true);
        _buildCamera.SetActive(false);
        _worldCanvas.SetActive(true);
    }
}