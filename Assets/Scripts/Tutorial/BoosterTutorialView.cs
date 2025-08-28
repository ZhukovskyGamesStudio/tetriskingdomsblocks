using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

public abstract class BoosterTutorialView : MonoBehaviour {
    [SerializeField]
    private RectTransform _boosterContainer;

    private Tween _currentTween;

    [SerializeField]
    private SpotlightAnimConfig _stepConfig;

    void Start() {
        SetHolesPositions();
        ShowBoosterStepTutorial();

        Init();
    }

    protected abstract void SendTutorialEventStep();
    protected abstract void Init();

    protected abstract Button BoosterButton { get; }

    private void SetHolesPositions() {
        var boosterContainer = BoosterButton.transform;
        _boosterContainer.transform.SetParent(boosterContainer);

        _boosterContainer.transform.position = boosterContainer.position;
    }

    private void ShowBoosterStepTutorial() {
        DragManager.IsDragDisabled = true;
        GameUI.Instance.GoalView.Witch.gameObject.SetActive(false);
        SpotlightsManager.Instance.SpotlightWithText.ShowSpotlightOnButton(BoosterButton, _stepConfig, HideTutorial);
        SpotlightsManager.Instance.HideFinger();
        ShowFingerWithDelay().Forget();
        SendTutorialEventStep();
    }

    private async UniTask ShowFingerWithDelay() {
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: this.GetCancellationTokenOnDestroy());
        SpotlightsManager.Instance.StartFingerClickAnimation(BoosterButton.transform.position);
    }

    private void HideTutorial() {
        HideBoosterStepTutorial().Forget();
    }

    private async UniTask HideBoosterStepTutorial() {
        SpotlightsManager.Instance.HideFinger();
        GameFieldManager.Instance.ClearAllLockedCells();
        _currentTween.Kill();
        _boosterContainer.gameObject.SetActive(false);

        await SpotlightsManager.Instance.SpotlightWithText.HideSpotlight();
        GameUI.Instance.GoalView.ShowWitchWithAnimation();
        DestroyTutorial();
    }

    private void DestroyTutorial() {
        DragManager.IsDragDisabled = false;
        NextPiecesView.Instance.SetTinyPortalActive(true);
        _currentTween.Kill();

        Destroy(_boosterContainer.gameObject);
        Destroy(gameObject);
    }
}