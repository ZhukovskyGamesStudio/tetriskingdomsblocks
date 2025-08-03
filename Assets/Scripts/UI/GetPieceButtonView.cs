using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GetPieceButtonView : MonoBehaviour {
    [SerializeField]
    private GameObject _readyState, _timerState;

    [SerializeField]
    private MetaFieldManager _metaManager;

    [SerializeField]
    private TextMeshProUGUI _timerText;

    public Button GetPieceButton;

    public void GetPiece() {
        _metaManager.GetPiece();
    }

    public void UpdateGetPieceTimer(TimeSpan timeLeft) {
        bool isTimeLeft = timeLeft.TotalSeconds > 0;
        SetGetPieceButtonActive(!isTimeLeft);
        if (isTimeLeft) {
            _timerText.text = TimeConverter.ConvertToTimeString(timeLeft);
        }
    }

    public void SetGetPieceButtonActive(bool isActive) {
        _timerState.SetActive(!isActive);
        _readyState.SetActive(isActive);
    }
}