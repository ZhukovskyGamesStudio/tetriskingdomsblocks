using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoalView : MonoBehaviour {
    [field: SerializeField]
    private TextMeshProUGUI _movesCountText;

    [SerializeField]
    private RectTransform _taskState;

    [field: SerializeField]
    public TaskUIView[] TaskUIViews { get; private set; }
    
    [Header("Ultimate")]
    
    [field:SerializeField]
    public Slider UltimateProgressBar{ get; private set; }

    [field:SerializeField]
    public Button UltimateButton{ get; private set; }

    [field:SerializeField]
    public GameObject UseUltimateMenu{ get; private set; }
    
    [SerializeField]
    private Animation _ultimateAnimationUI;

    [SerializeField]
    private AnimationClip _hideUiClip;
    
    [field: SerializeField]
    public GameObject Witch { get; private set; }
    [field: SerializeField]
    public GameObject SettingsButton { get; private set; }
    
    [SerializeField]
    private Animation _witchAnimation;
    
    [SerializeField]
    private AnimationClip _witchShowClip;
    
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
    
    
    public void HideUltimateButton() {
        UseUltimateMenu.SetActive(false);
        UltimateProgressBar.gameObject.SetActive(true);
        //make animations(maybe scale from 0 to 1)
    }
    
    public void HideUltimateUI() {
        _ultimateAnimationUI.Play(_hideUiClip.name);
        var animationObject = UltimateButton.gameObject.activeInHierarchy ? UltimateButton.transform : UltimateProgressBar.transform;
        DOTween.Sequence().Append(animationObject.DOScale(1.1f, 0.2f).SetEase(Ease.OutBack))
            .Append(animationObject.DOScale(Vector3.zero, 0.7f).SetEase(Ease.InBack));
    }

    public void ShowWitchWithAnimation() {
        Witch.SetActive(true);
        _witchAnimation.Play(_witchShowClip.name);
    }
    
}