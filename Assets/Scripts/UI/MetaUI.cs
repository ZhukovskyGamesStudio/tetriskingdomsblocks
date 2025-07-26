using System;
using UnityEngine;
using TMPro;

public class MetaUI : MonoBehaviour {
    public static MetaUI Instance;

    [field: SerializeField]
    public HealthView HealthView { get; private set; }

    [field: SerializeField]
    public CountersPanelView CountersPanelView { get; private set; }

    [SerializeField]
    private GameObject _getPieceTimer;
    
    [SerializeField]
    private TMP_Text _getPieceTimerText;

    [SerializeField]
    private TMP_Text _destroyPieceText;

    [SerializeField]
    private TMP_Text _playText;

    [SerializeField]
    private GameObject _ruleState, _buildState;

    [SerializeField]
    private GameObject _ruleCamera, _buildCamera;

    [SerializeField]
    private GameObject _getPieceButton, _buyPieceButton;

    private Vector3 _buildCameraShift;

    private void Awake() {
        Instance = this;
        InitBuildCameras();
    }

    private void InitBuildCameras() {
        var ray = new Ray(_buildCamera.transform.position, _buildCamera.transform.forward);
        var hit = Physics.Raycast(ray, out RaycastHit hitinfo, 100, LayerMask.GetMask("Ground"));

        if (hit) {
            _buildCameraShift =  _buildCamera.transform.position - hitinfo.point;
        }
    }

    public void OpenResources() {
        
    }

    public void SetPlayText(string text) {
        _playText.text = text;
    }

    public void SetGetPieceButtonActive(bool isActive) {
        _getPieceTimer.SetActive(!isActive);
        _buyPieceButton.SetActive(!isActive);
        
        _getPieceButton.SetActive(isActive);
    }

    public void UpdateGetPieceTimer(TimeSpan timeLeft) {
        if (timeLeft.TotalSeconds > 0) {
            _getPieceTimerText.text = TimeConverter.ConvertToTimeString(timeLeft);
        }
        else if (_getPieceTimer.activeSelf) {
            SetGetPieceButtonActive(true);
        }
    }

    public void SetGetPieceTimer(string text) {
        if (_getPieceTimerText != null) {
            _getPieceTimerText.text = text;
        }
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

        var ray = new Ray(_ruleCamera.transform.position, _ruleCamera.transform.forward);
        var hit = Physics.Raycast(ray, out RaycastHit hitinfo, 100, LayerMask.GetMask("Ground"));

        if (hit) {
            _buildCamera.transform.position = hitinfo.point + _buildCameraShift;
        }

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

    private void OnDrawGizmos() {
        Gizmos.color = Color.white;
        Gizmos.DrawLine(_ruleCamera.transform.position, _ruleCamera.transform.position + _ruleCamera.transform.forward * 30);
        Gizmos.DrawLine(_buildCamera.transform.position, _buildCamera.transform.position + _buildCamera.transform.forward * 30);
    }
}