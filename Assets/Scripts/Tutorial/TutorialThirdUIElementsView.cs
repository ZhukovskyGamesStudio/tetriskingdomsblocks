using Cysharp.Threading.Tasks;
using DG.Tweening;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

public class TutorialThirdUIElementsView : MonoBehaviour {
    [SerializeField]
    private RectTransform _goalViewContainer;

    [SerializeField]
    private Tween _currentTween;

    [SerializeField]
    private SpotlightAnimConfig _step1Config;

    void Start() {
        GameUI.Instance.GoalView.UltimateProgressBar.maxValue = 5;
        GameFieldManager.Instance.OnCellPlaced += ShowUltimateStepTutorial;
        SetHolesPositions();
        NextPiecesView.Instance.SetTinyPortalActive(false);
    }

    public void SetHolesPositions() {
        var ultimateContainer = GameUI.Instance._ultimateContainer;
        _goalViewContainer.transform.SetParent(ultimateContainer);
    }

    private void ShowUltimateStepTutorial(Vector2Int pos, bool[,] cells) {
        GameUI.Instance.GoalView.Witch.gameObject.SetActive(false);
        SpotlightsManager.Instance.SpotlightWithText.ShowSpotlightOnButton(GameUI.Instance.GoalView.UltimateButton, _step1Config,
            () => { HideUltimateStepTutorial().Forget(); });
        SpotlightsManager.Instance.StartFingerClickAnimation(GameUI.Instance.GoalView.UltimateButton.transform.position);

        GameFieldManager.Instance.OnCellPlaced -= ShowUltimateStepTutorial;
    }

    public async UniTask HideUltimateStepTutorial() {
        SpotlightsManager.Instance.HideFinger();
        GameFieldManager.Instance.ClearAllLockedCells();
        _currentTween.Kill();
        await SpotlightsManager.Instance.SpotlightWithText.HideSpotlight();
        NextPiecesView.Instance.SetTinyPortalActive(true);
        GameUI.Instance.GoalView.ShowWitchWithAnimation();
        DestroyTutorial();
    }

    public void DestroyTutorial() {
        _currentTween.Kill();

        Destroy(_goalViewContainer.gameObject);
        Destroy(gameObject);
    }
}