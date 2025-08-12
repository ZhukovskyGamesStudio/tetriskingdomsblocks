using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ScriptableObjects;
using UnityEngine;

public class LevelOneTutorial : MonoBehaviour {
    [SerializeField]
    private RectTransform _pieceContainer, _goalViewContainer;

    [SerializeField]
    private List<Vector3Int> _firstStepCells, _secondStepCells;

    [SerializeField]
    private SpotlightAnimConfig _step1Config, _step2Config;

    [SerializeField]
    private bool _canSkipTutorial;

    private int _tutorialStep = 1;

    void Start() {
        GameFieldManager.Instance.OnCellPlaced += HideFirstStepTutorial;
        ShowFirstStepTutorial();
        SetHolesPositions();

        _pieceContainer.gameObject.SetActive(true);
        _goalViewContainer.gameObject.SetActive(false);
        SpotlightsManager.Instance.StartFingerDragAnimation(_pieceContainer.transform.position,
            (Vector2)Camera.main!.WorldToScreenPoint(new Vector3(3.5f, 0, 3.5f)));
    }

    private void Update() {
        if (Input.touchCount > 0) {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
                TrySkipStep();
        }

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
            TrySkipStep();
#endif
    }

    private void TrySkipStep() {
        if (_canSkipTutorial) {
            if (_tutorialStep == 3)
                HideThirdStepTutorial().Forget();
        }
    }

    public void SetHolesPositions() {
        var posHoleSecond = (Vector2)Camera.main.WorldToScreenPoint(NextPiecesView.Instance._piecesContainers[0].transform.position);
        _pieceContainer.transform.position = posHoleSecond;
    }

    private void ShowFirstStepTutorial() {
        NextPiecesView.Instance.SetTinyPortalActive(false);
        GameUI.Instance.GoalView.gameObject.SetActive(false);
        SpotlightsManager.Instance.SpotlightWithText.ShowSpotlight(SpotlightsManager.Instance.CenterScreenAnchor, _step1Config);
        TutorialHoleHelper.DestroyHoles();
        TutorialHoleHelper.SpawnHoles(_firstStepCells);
        HighlightCurrentPiece();
    }

    private GameObject _nextPiecesContainer;

    private void HighlightCurrentPiece() {
        PieceView piece = FindAnyObjectByType<PieceView>();
        _nextPiecesContainer = piece._cellsContainer.gameObject;
        TutorialHoleHelper.HighlightObjects(new List<GameObject> { _nextPiecesContainer });
    }

    public void HideFirstStepTutorial(Vector2Int pos, bool[,] cells) {
        HideFirstStepTutorialAsync().Forget();
    }

    public async UniTask HideFirstStepTutorialAsync() {
        SpotlightsManager.Instance.HideFinger();
        GameUI.Instance.GoalView.gameObject.SetActive(true);
        GameUI.Instance.GoalView.Witch.gameObject.SetActive(false);
        GameUI.Instance.GoalView.SettingsButton.gameObject.SetActive(false);
        await SpotlightsManager.Instance.SpotlightWithText.HideSpotlight();
        TutorialHoleHelper.DestroyHoles();
        GameFieldManager.Instance.OnCellPlaced -= HideFirstStepTutorial;
        GameFieldManager.Instance.ClearAllLockedCells();
        _canSkipTutorial = true;
        _pieceContainer.gameObject.SetActive(false);

        //TODO дождаться анимации перелёта ресурсов от клеток в счётчики
        await UniTask.Delay(TimeSpan.FromSeconds(1));
        ShowThirdStepTutorial();
    }

    public void ShowThirdStepTutorial() {
        SpotlightsManager.Instance.SpotlightWithText.ShowSpotlight(GameUI.Instance.GoalView.transform, _step2Config);
        _tutorialStep = 3;
        _canSkipTutorial = true;
        _goalViewContainer.gameObject.SetActive(true);
    }

    public async UniTask HideThirdStepTutorial() {
        TutorialHoleHelper.DestroyHoles();
        Time.timeScale = 1;
        _goalViewContainer.gameObject.SetActive(false);
        DestroyTutorial();
        await SpotlightsManager.Instance.SpotlightWithText.HideSpotlight();
        ShowWitch();
        GameUI.Instance.GoalView.SettingsButton.gameObject.SetActive(true);
        NextPiecesView.Instance.SetTinyPortalActive(true);
    }

    private static void ShowWitch() {
        GameUI.Instance.GoalView.ShowWitchWithAnimation();
    }

    public void DestroyTutorial() {
        Destroy(_nextPiecesContainer.gameObject);
        Destroy(_goalViewContainer.gameObject);

        Destroy(gameObject);
    }
}