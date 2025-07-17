using UnityEngine;
using TMPro;

public class MetaUI : MonoBehaviour {
    public static MetaUI Instance;

    [field: SerializeField]
    public HealthView HealthView { get; private set; }

    [field: SerializeField]
    public CountersPanelView CountersPanelView { get; private set; }

    [SerializeField]
    private TMP_Text _getPieceTimerText;

    [SerializeField]
    private TMP_Text _destroyPieceText;

    [SerializeField]
    private GameObject _ruleState, _buildState;

    [SerializeField]
    private GameObject _ruleCamera, _buildCamera;

    private void Awake() {
        Instance = this;
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
        MetaWorldCanvasView.Instance.gameObject.SetActive(false);
    }

    public void OpenRuleState() {
        _buildState.SetActive(false);
        _ruleState.SetActive(true);
        _ruleCamera.SetActive(true);
        _buildCamera.SetActive(false);
        MetaWorldCanvasView.Instance.gameObject.SetActive(true);
    }

    public void OpenSettings() {
        SettingsManager.Instance.ShowMetaSettingsDialog();
    }
}