using System;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GoalView : MonoBehaviour {
    [SerializeField]
    private TextMeshProUGUI _taskText, _sliderText;

    [SerializeField]
    private Slider _slider;

    [SerializeField]
    private RectTransform _taskState, _winState, _loseState;

    public static GoalView Instance;
    public bool _isGameEnded { get; private set; }
    private Tween _currentTween;

    [SerializeField]
    private TMP_Text _loseRestartText;

    private void Awake() {
        Instance = this;
    }

    public void SetWinState() {
        if (_isGameEnded) return;
        WinAnimation();
        
        _isGameEnded = true;
    }

    public void SetLoseState() {
        if (_isGameEnded) {
            return;
        }

        StorageManager.GameDataMain.HealthCount--;
        var passingData = new DialogWithData() {
            DialogType = typeof(LoseDialog),
            Data = new LoseDialog.Data() {
                ClickContinue = ExitGame,
                Hp = StorageManager.GameDataMain.HealthCount
            }
        };
        DialogsManager.Instance.ShowDialogWithData(passingData);
        
        _isGameEnded = true;
    }

    private void WinAnimation() {
        var passingData = new DialogWithData() {
            DialogType = typeof(WinDialog),
            Data = new WinDialog.Data() {
                ClickClaim = ExitGame,
                Coins = -1,
                Cubes = -1
            }
        };
        
        _currentTween = DOTween.Sequence()
            .Append(GameUI.Instance.BgTasksImage.DOAnchorPosY(GameUI.Instance.BgTasksImage.anchoredPosition.y + 370, 1f))
            .Append(GameUI.Instance.OpenedDoorEndGame.DOMoveY(GameUI.Instance.OpenedDoorEndGame.position.y + 2.3f, 0.7f))
            .Append(GameUI.Instance.OpenedDoorEndGame.DOMoveY(GameUI.Instance.OpenedDoorEndGame.position.y + 2.2f, 0.07f))
            .Append(GameUI.Instance.OpenedDoorEndGame.DOMoveY(GameUI.Instance.OpenedDoorEndGame.position.y + 2.45f, 0.1f))
            .Append(GameFieldManager.Instance.CameraContainer.DOMoveZ(GameFieldManager.Instance.CameraContainer.position.z + 5, 3f))
            .OnComplete(() => DialogsManager.Instance.ShowDialogWithData(passingData));
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