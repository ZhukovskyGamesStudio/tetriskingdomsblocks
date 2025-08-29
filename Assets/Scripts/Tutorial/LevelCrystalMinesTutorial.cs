using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ScriptableObjects;
using UnityEngine;

public class LevelCrystalMinesTutorial : MonoBehaviour {
    [SerializeField]
    private SpotlightAnimConfig _nearCrystalMineAnimConfig, _onCrystalAnimConfig;

    [SerializeField]
    private List<Vector3Int> _firstStepCells, _secondStepCells;

    [SerializeField]
    private bool _canSkipTutorial;

    private int _tutorialStep;

    void Start() {
        GameFieldManager.Instance.OnCellPlaced += HideFirstStepTutorial;
        ShowFirstStepTutorial();
    }

    private void ShowFirstStepTutorial() {
        GameUI.Instance.GoalView.Witch.gameObject.SetActive(false);
        GameUI.Instance.GoalView.SettingsButton.gameObject.SetActive(false);
        GameUI.Instance.GoalView.gameObject.SetActive(false);
        
        SpotlightsManager.Instance.SpotlightWithText.ShowSpotlight(SpotlightsManager.Instance.CenterScreenAnchor, _nearCrystalMineAnimConfig);
        TutorialHoleHelper.DestroyHoles();
        TutorialHoleHelper.SpawnHoles(_firstStepCells);
        HighlightCurrentPiece();
    }

    private GameObject _pieceCellsContainer;

    private void HighlightCurrentPiece() {
        PieceView piece = FindAnyObjectByType<PieceView>();
        _pieceCellsContainer = piece._cellsContainer.gameObject;
        TutorialHoleHelper.HighlightObjects(new List<GameObject> { _pieceCellsContainer });
    }

    public void HideFirstStepTutorial(Vector2Int pos, bool[,] cells) {
        if (_tutorialStep == 1) {
            HideThirdStepTutorial();
            return;
        }

        TutorialHoleHelper.DestroyHoles();
        SpotlightsManager.Instance.SpotlightWithText.HideSpotlight().Forget();
        Invoke(nameof(ShowThirdStepTutorial), 0.5f);
    }

    public void ShowThirdStepTutorial() {
        TutorialHoleHelper.SpawnHoles(_secondStepCells);
        _tutorialStep = 1;

        HighlightCurrentPiece();
        SpotlightsManager.Instance.SpotlightWithText.ShowSpotlight(SpotlightsManager.Instance.CenterScreenAnchor, _onCrystalAnimConfig);
    }

    public void HideThirdStepTutorial() {
        GameUI.Instance.GoalView.ShowWitchWithAnimation();
        GameUI.Instance.GoalView.gameObject.SetActive(true);
        GameUI.Instance.GoalView.SettingsButton.gameObject.SetActive(true);
        SpotlightsManager.Instance.SpotlightWithText.HideSpotlight().Forget();
        TutorialHoleHelper.DestroyHoles();
        GameFieldManager.Instance.OnCellPlaced -= HideFirstStepTutorial;
        DestroyTutorial();
    }

    public void DestroyTutorial() {
        Destroy(gameObject);
    }
}