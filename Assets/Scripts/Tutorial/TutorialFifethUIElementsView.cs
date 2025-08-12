using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ScriptableObjects;
using UnityEngine;

public class TutorialFifthUIElementsView : MonoBehaviour {
    [SerializeField]
    private RectTransform _boosterContainer;

    private Tween _currentTween;

    [SerializeField]
    private SpotlightAnimConfig _stepConfig;

    private List<Vector3Int> _firstStepCells;

    void Start() {
        GameFieldManager.Instance.OnPieceDestroyedByHammer += () => { HideBoosterStepTutorial().Forget(); };

        SetHolesPositions();
        ShowBoosterStepTutorial();

        StorageManager.GameDataMain.HummerCount = 5;

        if (BoostersManager.Instance != null) {
            GameUI.Instance.GameBoostersButtons.UpdateCounters(StorageManager.GameDataMain);
        }
    }

    public void SetHolesPositions() {
        var boosterContainer = GameUI.Instance.GameBoostersButtons._hummerButton.transform;
        _boosterContainer.transform.SetParent(boosterContainer);

        _firstStepCells = new List<Vector3Int>();
        for (int i = 0; i < 8; i++) {
            for (int j = 0; j < 8; j++) {
                _firstStepCells.Add(new Vector3Int(i, 0, j));
            }
        }

        _boosterContainer.transform.position = boosterContainer.position;
    }

    private void ShowBoosterStepTutorial() {
        GameUI.Instance.GoalView.Witch.gameObject.SetActive(false);
        SpotlightsManager.Instance.SpotlightWithText.ShowSpotlightOnButton(GameUI.Instance.GameBoostersButtons._hummerButton, _stepConfig,
            () => { HideBoosterStepTutorial().Forget(); });
        SpotlightsManager.Instance.StartFingerClickAnimation(GameUI.Instance.GoalView.UltimateButton.transform.position);
        TutorialHoleHelper.SpawnHoles(_firstStepCells);
    }

    private async UniTask HideBoosterStepTutorial() {
        SpotlightsManager.Instance.HideFinger();
        GameFieldManager.Instance.ClearAllLockedCells();
        _currentTween.Kill();
        _boosterContainer.gameObject.SetActive(false);

        TutorialHoleHelper.DestroyHoles();
        await SpotlightsManager.Instance.SpotlightWithText.HideSpotlight();
        GameUI.Instance.GoalView.ShowWitchWithAnimation();
        DestroyTutorial();
    }

    private void DestroyTutorial() {
        NextPiecesView.Instance.SetTinyPortalActive(true);
        _currentTween.Kill();

        Destroy(_boosterContainer.gameObject);
        Destroy(gameObject);
    }
}