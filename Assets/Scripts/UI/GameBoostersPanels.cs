using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

public class GameBoostersPanels : MonoBehaviour {
    [field: SerializeField]
    public GameObject RotateSelect { get; private set; }

    [field: SerializeField]
    public GameObject RotateUse { get; private set; }

    [SerializedDictionary]
    public SerializedDictionary<BoosterType, GameObject> BoostersWindows;


    public void ConfirmShuffle() {
        BoostersManager.Instance.UseRandomField();
    }
    
    public void SwitchShuffleWindowActive() {
        if (!BoostersManager.Instance.CanShuffle()) {
            return;
        }

        ToggleBoosterPanelActive(BoosterType.Shuffle);
    }
    
    public void ConfirmDynamite() {
        BoostersManager.Instance.UseDynamite();
    }
    public void SwitchBombWindowActive() {
        if (!BoostersManager.Instance.CanDynamite()) {
            return;
        }

        ToggleBoosterPanelActive(BoosterType.Bomb);
    }

    
    public void ConfirmHammer() {
        BoostersManager.Instance.UseHummer();
    }
    public void SwitchHammerWindowActive() {
        if (!BoostersManager.Instance.CanHammer()) {
            return;
        }

        ToggleBoosterPanelActive(BoosterType.Hammer);
    }

    public void ConfirmRotate() {
        BoostersManager.Instance.UseRotatePiece();
    }

    public void ApplyRotate() {
        BoostersManager.Instance.ApplyRotation();
    }

    public void RotateLeft() {
        BoostersManager.Instance.RotatePieceLeft();
    }
    
    public void RotateRight() {
        BoostersManager.Instance.RotatePieceRight();
    }
    
    public void SwitchRotateWindowActive() {
        if (!BoostersManager.Instance.CanRotate()) {
            return;
        }

        ToggleBoosterPanelActive(BoosterType.Rotate);
    }

    private void ToggleBoosterPanelActive(BoosterType booster) {
        SetBoosterActive(booster, !BoostersWindows[booster].activeSelf);
    }

    public void SetUseRotateActive() {
        RotateSelect.SetActive(false);
        RotateUse.SetActive(true);
    }

    public void SetBoosterActive(BoosterType booster, bool isActive) {
        if (booster != BoosterType.Bomb || !isActive) BoostersManager.Instance.CancelDynamite();
        if (booster == BoosterType.Rotate && isActive) {
            RotateSelect.SetActive(true);
            RotateUse.SetActive(false);
        }

        if (booster == BoosterType.Shuffle) {
            if (isActive) {
                TutorialHoleHelper.SpawnHoles(FieldManager.AllFieldCells());
            } else {
                TutorialHoleHelper.DestroyHoles();
            }
        }

        foreach (var boosterWindow in BoostersWindows) {
            boosterWindow.Value.SetActive(boosterWindow.Key == booster && isActive);
        }
    }
}