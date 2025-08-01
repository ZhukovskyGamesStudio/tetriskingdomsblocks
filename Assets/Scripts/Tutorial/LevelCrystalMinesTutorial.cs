using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelCrystalMinesTutorial : MonoBehaviour {

    [SerializeField]
    private RectTransform _blackBGImage;

    [SerializeField]
    private TMP_Text _tutorialText;

    [SerializeField]
    private Transform _fingerImageContainer;

    [SerializeField]
    private string _firstTutorialText;

    [SerializeField]
    private string _secondTutorialText;

    [SerializeField]
    private TutorialHoleHelper _holeHelper;

    [SerializeField]
    private List<Vector3Int> _firstStepCells, _secondStepCells;

    [SerializeField]
    private bool _canSkipTutorial;

    private int _tutorialStep = 0;

    void Start() {
        GameFieldManager.Instance.OnCellPlaced += HideFirstStepTutorial;
        ShowFirstStepTutorial();
    }

    private void ShowFirstStepTutorial() {
        TutorialHoleHelper.DestroyHoles();
        TutorialHoleHelper.SpawnHoles(_firstStepCells);
        _tutorialText.text = _firstTutorialText;
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
        
        Invoke("ShowThirdStepTutorial", 0.5f); 
    }

  /*  public void ShowSecondStepTutorial() {
        TutorialHoleHelper.HighlightObjects(new List<GameObject> { _pieceCellsContainer });
        TutorialHoleHelper.SpawnHoles(_secondStepCells);
        _tutorialStep = 2;
        Time.timeScale = 0;
        _tutorialText.text = _secondTutorialText;
        _holeImages[2].gameObject.SetActive(true);
    }

    public void HideSecondStepTutorial(Vector2Int pos, bool[,] cells) {
        TutorialHoleHelper.DestroyHoles();
        _holeImages[2].gameObject.SetActive(false);
        Time.timeScale = 1;
        _canSkipTutorial = false;
        _tutorialText.gameObject.SetActive(false);
       
      
    }*/
    public void ShowThirdStepTutorial() {
        TutorialHoleHelper.SpawnHoles(_secondStepCells);
        _tutorialStep = 1;
        _tutorialText.text = _secondTutorialText;
       
        HighlightCurrentPiece();
    }

    public void HideThirdStepTutorial() {
        TutorialHoleHelper.DestroyHoles();
        Time.timeScale = 1;
        GameFieldManager.Instance.OnCellPlaced -= HideFirstStepTutorial;
        DestroyTutorial();
    }

    public void DestroyTutorial() {

        Destroy(_blackBGImage.gameObject);

        Destroy(gameObject);
    }
}