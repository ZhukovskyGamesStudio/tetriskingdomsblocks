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
    private bool _canSkipTutorial;

    private int _tutorialStep;

    [SerializeField]
    private SpotlightAnimConfig _step1Config;

    void Start() {
        GameFieldManager.Instance.OnCellPlaced += ShowUltimateStepTutorial;
        SetHolesPositions();
        NextPiecesView.Instance.SetTinyPortalActive(false);
    }

    private void Update() {
        if (Input.touchCount > 0 && _tutorialStep == 1) {
            Touch touch = Input.GetTouch(0);

            //if (touch.phase == TouchPhase.Began)
            // HideUltimateStepTutorial();
        }

#if UNITY_EDITOR
        //if (Input.GetMouseButtonDown(0) && _tutorialStep == 1)
        //  HideUltimateStepTutorial();
#endif
    }

    public void SetHolesPositions() {
        var ultimateContainer = GameUI.Instance._ultimateContainer;
        _goalViewContainer.transform.SetParent(ultimateContainer);
    }

    private void ShowUltimateStepTutorial(Vector2Int pos, bool[,] cells) {
        SpotlightsManager.Instance.SpotlightWithText.ShowSpotlightOnButton(GameUI.Instance.GoalView.UltimateButton, _step1Config,
            () => { HideUltimateStepTutorial().Forget(); });
        SpotlightsManager.Instance.StartFingerClickAnimation(GameUI.Instance.GoalView.UltimateButton.transform.position);

        _tutorialStep = 1;
        GameFieldManager.Instance.OnCellPlaced -= ShowUltimateStepTutorial;
    }

    public async UniTask HideUltimateStepTutorial() {
        SpotlightsManager.Instance.HideFinger();
        GameFieldManager.Instance.ClearAllLockedCells();
        _currentTween.Kill();
        _canSkipTutorial = true;
        _goalViewContainer.gameObject.SetActive(false);
        DestroyTutorial();
        await SpotlightsManager.Instance.SpotlightWithText.HideSpotlight();
    }

    public void DestroyTutorial() {
        NextPiecesView.Instance.SetTinyPortalActive(true);
        _currentTween.Kill();

        Destroy(_goalViewContainer.gameObject);
        Destroy(gameObject);
    }
}