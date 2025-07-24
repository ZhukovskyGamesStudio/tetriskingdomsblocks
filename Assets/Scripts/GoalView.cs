using DG.Tweening;
using TMPro;
using UnityEngine;

public class GoalView : MonoBehaviour {
    [field: SerializeField]
    private TextMeshProUGUI _movesCountText;

    [SerializeField]
    private RectTransform _taskState;

    [field: SerializeField]
    public TaskUIView[] TaskUIViews { get; private set; }
    
    private Tween _currentTween;

    public void SetMovesCount(int count) {
        _movesCountText.text = count.ToString();
    }

    public void SetTasksActive(bool active) {
        foreach (var taskUI in TaskUIViews) {
            taskUI.gameObject.SetActive(active);
        }
    }

    public void ExitGame() {
        MainManager.Instance.GoToMeta();
    }

    private void OnDestroy() {
        _currentTween.Kill();
    }
}