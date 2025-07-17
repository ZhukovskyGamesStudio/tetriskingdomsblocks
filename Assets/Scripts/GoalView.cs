using DG.Tweening;
using UnityEngine;

public class GoalView : MonoBehaviour {
    [SerializeField]
    private RectTransform _taskState, _winState, _loseState;

    public bool _isGameEnded { get; private set; }
    private Tween _currentTween;

    public void SetWinState() {
        if (_isGameEnded) return;
        WinAnimation();

        _isGameEnded = true;
    }

    public void SetLoseState() {
        if (_isGameEnded) {
            return;
        }

        _isGameEnded = true;
    }

    private void WinAnimation() {
        _currentTween = DOTween.Sequence()
            .Append(GameUI.Instance.BgTasksImage.DOAnchorPosY(GameUI.Instance.BgTasksImage.anchoredPosition.y + 370, 1f))
            .Append(GameUI.Instance.OpenedDoorEndGame.DOMoveY(GameUI.Instance.OpenedDoorEndGame.position.y + 2.3f, 0.7f))
            .Append(GameUI.Instance.OpenedDoorEndGame.DOMoveY(GameUI.Instance.OpenedDoorEndGame.position.y + 2.2f, 0.07f))
            .Append(GameUI.Instance.OpenedDoorEndGame.DOMoveY(GameUI.Instance.OpenedDoorEndGame.position.y + 2.45f, 0.1f))
            .Append(GameFieldManager.Instance.CameraContainer.DOMoveZ(GameFieldManager.Instance.CameraContainer.position.z + 5, 3f))
            .OnComplete(() => GameUI.Instance.ShowWinDialog());
    }

    private void LoseAnimation() {
        _currentTween = DOTween.Sequence()
            .Append(GameUI.Instance.BgTasksImage.DOAnchorPosY(GameUI.Instance.BgTasksImage.anchoredPosition.y + 370, 1f))
            .Append(_loseState.DOAnchorPosY(_loseState.anchoredPosition.y - 800, 0.6f));
    }

    public void ExitGame() {
        MainManager.Instance.GoToMeta();
    }

    private void OnDestroy() {
        _currentTween.Kill();
    }
}