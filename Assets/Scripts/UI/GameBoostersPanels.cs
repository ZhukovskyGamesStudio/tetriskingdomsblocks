using AYellowpaper.SerializedCollections;
using UnityEngine;

public class GameBoostersPanels : MonoBehaviour {
    [field: SerializeField]
    public GameObject RotateSelect { get; private set; }

    [field: SerializeField]
    public GameObject RotateUse { get; private set; }

    [SerializedDictionary]
    public SerializedDictionary<BoosterType, GameObject> BoostersWindows;

    public void OpenShuffle() {
        if (!BoostersManager.Instance.CanShuffle()) {
            return;
        }

        SetBoosterActive(BoosterType.Shuffle, true);
    }

    public void ConfirmShuffle() {
        BoostersManager.Instance.UseRandomField();
        CancelShuffle();
    }

    public void CancelShuffle() {
        SetBoosterActive(BoosterType.Shuffle, false);
    }

    public void OpenBomb() {
        if (!BoostersManager.Instance.CanDynamite()) {
            return;
        }

        BoostersManager.Instance.UseDynamite();
        SetBoosterActive(BoosterType.Bomb, true);
    }

    public void ConfirmDynamite() {
        BoostersManager.Instance.UseDynamite();
        SetBoosterActive(BoosterType.Bomb, false);
    }

    public void CancelBomb() {
        SetBoosterActive(BoosterType.Bomb, false);
    }

    public void OpenHammer() {
        if (!BoostersManager.Instance.CanHammer()) {
            return;
        }

        BoostersManager.Instance.UseHammer();
        SetBoosterActive(BoosterType.Hammer, true);
    }

    public void ConfirmHammer() {
        SetBoosterActive(BoosterType.Hammer, false);
    }

    public void CancelHammer() {
        SetBoosterActive(BoosterType.Hammer, false);
    }

    public void ConfirmRotate() {
        BoostersManager.Instance.UseRotatePiece();
    }
    
    public void CancelRotate() {
        SetBoosterActive(BoosterType.Rotate, false);
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

    public void OpenRotate() {
        if (!BoostersManager.Instance.CanRotate()) {
            return;
        }

        SetBoosterActive(BoosterType.Rotate, true);
    }

    private void ToggleBoosterPanelActive(BoosterType booster) {
        SetBoosterActive(booster, !BoostersWindows[booster].activeSelf);
    }

    public void SetUseRotateActive() {
        RotateSelect.SetActive(false);
        RotateUse.SetActive(true);
    }

    public void SetBoosterActive(BoosterType booster, bool isActive) {
       

        if (booster == BoosterType.Rotate && isActive) {
            RotateSelect.SetActive(true);
            RotateUse.SetActive(false);
        }
        

        if (booster == BoosterType.Hammer && isActive) {
            TutorialHoleHelper.HighlightCells(GameFieldManager.Instance.AllHammerableCells());
        }

        if (booster == BoosterType.Bomb && isActive) {
            TutorialHoleHelper.SpawnHoles(FieldManager.AllFieldCells());
        }

        GameUI.Instance.GoalView.gameObject.SetActive(!isActive);

        if (isActive) { } else {
            TutorialHoleHelper.DestroyHoles();
        }

        foreach (var boosterWindow in BoostersWindows) {
            boosterWindow.Value.SetActive(boosterWindow.Key == booster && isActive);
        }
    }
}