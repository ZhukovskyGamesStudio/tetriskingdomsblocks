using System;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoalView : MonoBehaviour {
    [SerializeField]
    private TextMeshProUGUI _taskText, _sliderText;

    [SerializeField]
    private Slider _slider;

    [SerializeField]
    private RectTransform _taskState, _winState, _loseState;

    public static GoalView Instance;
    private bool _isGameEnded;
    private Tween _currentTween;

    private void Awake() {
        Instance = this;
    }

    /*public void InitTask(GameData data) {
        SetTaskState();
        var kvp = data.TaskData.GoalToCollect.First();
        _taskText.text = $"Collect {kvp.Value} {kvp.Key}";
        _slider.maxValue = kvp.Value;
        UpdateTask(data);
    }

    public void UpdateTask(GameData data) {
        var kvp = data.TaskData.GoalToCollect.First();
        data.CollectedResources.TryGetValue(kvp.Key, out int hasAmount);
        _slider.value = hasAmount;
        _sliderText.text = $"{hasAmount}/{kvp.Value}";
    }*/

    private void SetTaskState() {
        if (_isGameEnded) {
            return;
        }

        _winState.gameObject.SetActive(false);
        _loseState.gameObject.SetActive(false);
        //_taskState.gameObject.SetActive(true);
    }

    public void SetWinState()
    {
        if(_isGameEnded)return;
        WinAnimation();
        _isGameEnded = true;
        _winState.gameObject.SetActive(true);
        //_taskState.gameObject.SetActive(false);
        _loseState.gameObject.SetActive(false);
    }
    
    public void SetLoseState()
    {
        if(_isGameEnded)return;
        LoseAnimation();
        _isGameEnded = true;
        _winState.gameObject.SetActive(false);
        //_taskState.gameObject.SetActive(false);
        _loseState.gameObject.SetActive(true);
    }

    private void WinAnimation()
    {
        _currentTween = DOTween.Sequence()
            .Append(GameManager.Instance.BgTasksImage.DOAnchorPosY(
                GameManager.Instance.BgTasksImage.anchoredPosition.y + 370, 1f))

            .Append(GameManager.Instance.OpenedDoorEndGame.DOMoveY(GameManager.Instance.OpenedDoorEndGame.position.y + 2.1f, 0.7f))
            .Append(GameManager.Instance.OpenedDoorEndGame.DOMoveY(GameManager.Instance.OpenedDoorEndGame.position.y + 2f, 0.07f))
            .Append(GameManager.Instance.OpenedDoorEndGame.DOMoveY(GameManager.Instance.OpenedDoorEndGame.position.y + 2.25f, 0.1f))
            .Join(_winState.DOAnchorPosY(_winState.anchoredPosition.y - 500, 0.6f))

            .Append(GameManager.Instance.CameraContainer.DOMoveZ(GameManager.Instance.CameraContainer.position.z + 5, 3f));
    }

    private void LoseAnimation()
    {
        _currentTween = DOTween.Sequence()
            .Append(GameManager.Instance.BgTasksImage.DOAnchorPosY(
                GameManager.Instance.BgTasksImage.anchoredPosition.y + 370, 1f))
            .Append(_loseState.DOAnchorPosY(_loseState.anchoredPosition.y - 500, 0.6f));
    }
    
    public void StartAgain() {
        GameManager.Instance.Restart();
    }
    public void ExitGame() {
        GameManager.Instance.GoToMeta();
    }

    private void OnDestroy()
    {
        _currentTween.Kill();
    }
}