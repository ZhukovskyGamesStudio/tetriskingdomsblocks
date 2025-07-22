using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalView : MonoBehaviour {
    [SerializeField]
    private RectTransform _taskState;

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
      Transform cameraContainer =  GameFieldManager.Instance != null ? GameFieldManager.Instance.CameraContainer : TutorialFieldManager.Instance.CameraContainer;
        _currentTween = DOTween.Sequence()
            .Append(GameUI.Instance.BgTasksImage.DOAnchorPosY(GameUI.Instance.BgTasksImage.anchoredPosition.y + 370, 1f))
            .Append(GameUI.Instance.OpenedDoorEndGame.DOMoveY(GameUI.Instance.OpenedDoorEndGame.position.y + 2.3f, 0.7f))
            .Append(GameUI.Instance.OpenedDoorEndGame.DOMoveY(GameUI.Instance.OpenedDoorEndGame.position.y + 2.2f, 0.07f))
            .Append(GameUI.Instance.OpenedDoorEndGame.DOMoveY(GameUI.Instance.OpenedDoorEndGame.position.y + 2.45f, 0.1f))
            .Append(cameraContainer.DOMoveZ(cameraContainer.position.z + 5, 3f))
            .OnComplete(() => {
                if (GameFieldManager.Instance != null) GameUI.Instance.ShowWinDialog();
                else SceneManager.LoadScene("GameScene");
            });
    }

    public void ExitGame() {
        MainManager.Instance.GoToMeta();
    }

    private void OnDestroy() {
        _currentTween.Kill();
    }
}