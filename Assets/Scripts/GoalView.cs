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
    public bool _isGameEnded { get;private set;}
    private Tween _currentTween;
    [SerializeField] private TMP_Text _loseRestartText;

    private void Awake() {
        Instance = this;
    }

    public void SetWinState() {
        if (_isGameEnded) return;
        WinAnimation();
       
        _isGameEnded = true;
       // _winState.gameObject.SetActive(true);
        _loseState.gameObject.SetActive(false);
    }

    public void SetLoseState() {
        if (_isGameEnded) return;
        if (StorageManager.GameDataMain.HealthCount <= 0)
            _loseRestartText.text = "Watch add and recovery 1 energy";
        else
            _loseRestartText.text = "Restart";
        LoseAnimation();
        _isGameEnded = true;
       // _winState.gameObject.SetActive(false);
        _loseState.gameObject.SetActive(true);
    }

    private void WinAnimation()
    {
        _currentTween = DOTween.Sequence()
            .Append(
                GameUI.Instance.BgTasksImage.DOAnchorPosY(GameUI.Instance.BgTasksImage.anchoredPosition.y + 370, 1f))
            .Append(
                GameUI.Instance.OpenedDoorEndGame.DOMoveY(GameUI.Instance.OpenedDoorEndGame.position.y + 2.3f, 0.7f))
            .Append(GameUI.Instance.OpenedDoorEndGame.DOMoveY(GameUI.Instance.OpenedDoorEndGame.position.y + 2.2f, 0.07f))
            .Append(GameUI.Instance.OpenedDoorEndGame.DOMoveY(GameUI.Instance.OpenedDoorEndGame.position.y + 2.45f,
                0.1f))
            .Append(GameFieldManager.Instance.CameraContainer.DOMoveZ(
                GameFieldManager.Instance.CameraContainer.position.z + 5, 3f))
            .OnComplete(() => SceneManager.LoadScene("MetaScene"));
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