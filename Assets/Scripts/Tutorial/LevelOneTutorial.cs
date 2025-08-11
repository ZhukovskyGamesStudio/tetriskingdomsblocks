using System;
using System.Collections.Generic;
using DG.Tweening;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelOneTutorial : MonoBehaviour {
    [SerializeField]
    private RectTransform[] _holeImages;

    [SerializeField]
    private RectTransform _blackBGImage;

    [SerializeField]
    private TMP_Text _tutorialText;

    [SerializeField]
    private Image _fingerImage;

    [SerializeField]
    private string _firstTutorialText;

    [SerializeField]
    private string _secondTutorialText;

    [SerializeField]
    private string _thirdTutorialText;

    [SerializeField]
    private Tween _currentTween;

    [SerializeField]
    private TutorialHoleHelper _holeHelper;

    [SerializeField]
    private List<Vector3Int> _firstStepCells, _secondStepCells, _thirdStepCells;

    [SerializeField]
    private SpotlightAnimConfig _step1Config, _step2Config, _step3Config;

    [SerializeField]
    private bool _canSkipTutorial;

    private int _tutorialStep = 1;

    void Start() {
        GameFieldManager.Instance.OnCellPlaced += HideFirstStepTutorial;
        ShowFirstStepTutorial();
        SetHolesPositions();

        StartAnimation(_holeImages[0].transform.position, (Vector2)Camera.main!.WorldToScreenPoint(new Vector3(3.5f, 0, 3.5f)));
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
            /*  if (_tutorialStep == 2)
                  HideSecondStepTutorial();*/
            if (_tutorialStep == 3)
                HideThirdStepTutorial();
        }
    }

    public void SetHolesPositions() {
        var posHoleSecond = (Vector2)Camera.main.WorldToScreenPoint(NextPiecesView.Instance._piecesContainers[0].transform.position);
        // Присваиваем позицию UI-элементу
        //ar posHoleFirst = GameUI.Instance._tasksContainer.position;

        //_holeImages[1].transform.SetParent(_holeHelper._holesContainer,true);
        //_holeImages[0].transform.SetParent(_holeHelper._holesContainer,true);
        //_holeImages[2].transform.SetParent(_holeHelper._holesContainer,true);

        //_holeImages[1].position = posHoleFirst;
        _holeImages[0].transform.position = posHoleSecond;
        _holeImages[2].transform.position = (Vector2)Camera.main.WorldToScreenPoint(new Vector3(4, 0, 3.5f));
        _tutorialText.transform.position = (Vector2)Camera.main.WorldToScreenPoint(new Vector3(4, 0, 2));
        _fingerImage.transform.position = _holeImages[0].transform.position;
        _holeImages[0].gameObject.SetActive(true);
        _holeImages[1].gameObject.SetActive(false);
        _holeImages[2].gameObject.SetActive(false);
    }

    public Tween StartAnimation(Vector3 startPos, Vector3 endPos) {
        var finger = SpotlightsManager.Instance.FingerTransform;
        finger.position = startPos;
        finger.localScale = Vector3.one;
        var color = _fingerImage.color;
        color.a = 0;
        _fingerImage.color = color;
        _currentTween = DOTween.Sequence().Append(_fingerImage.DOFade(1, 0.8f)).Join(finger.DOScale(Vector3.one * 0.75f, 0.8f))
            .Append(finger.DOMove(endPos, 2.5f)).Append(finger.DOScale(Vector3.one, 0.8f)).Join(_fingerImage.DOFade(0, 0.8f))
            .Append(finger.DOMove(startPos, 1)).SetLoops(-1, LoopType.Restart);
        return _currentTween;
    }

    private void ShowFirstStepTutorial() {
        SpotlightsManager.Instance.SpotlightWithText.ShowSpotlight(SpotlightsManager.Instance.CenterScreenAnchor, _step1Config);
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
        TutorialHoleHelper.DestroyHoles();
        GameFieldManager.Instance.OnCellPlaced -= HideFirstStepTutorial;
        GameFieldManager.Instance.ClearAllLockedCells();
        _currentTween.Kill();
        _canSkipTutorial = true;
        _holeImages[0].gameObject.SetActive(false);
        _fingerImage.gameObject.SetActive(false);
        _blackBGImage.gameObject.SetActive(false);
        _tutorialText.gameObject.SetActive(false);
        Invoke(nameof(ShowThirdStepTutorial), 2f);
    }

    public void ShowSecondStepTutorial() {
        TutorialHoleHelper.HighlightObjects(new List<GameObject> { _pieceCellsContainer });
        TutorialHoleHelper.SpawnHoles(_secondStepCells);
        _tutorialStep = 2;
        Time.timeScale = 0;
        _tutorialText.text = _secondTutorialText;
        _holeImages[2].gameObject.SetActive(true);
    }

    public void HideSecondStepTutorial() {
        TutorialHoleHelper.DestroyHoles();
        _holeImages[2].gameObject.SetActive(false);
        Time.timeScale = 1;
        _canSkipTutorial = false;
        _tutorialText.gameObject.SetActive(false);
    }

    public void ShowThirdStepTutorial() {
        _blackBGImage.gameObject.SetActive(true);
        _tutorialText.transform.position =
            new Vector3(GameUI.Instance._tasksContainer.position.x, GameUI.Instance._tasksContainer.position.y - 100f, 0);
        TutorialHoleHelper.SpawnHoles(_thirdStepCells);
        _tutorialStep = 3;
        Time.timeScale = 0;
        _canSkipTutorial = true;
        _tutorialText.text = _thirdTutorialText;
        _tutorialText.gameObject.SetActive(true);
        _holeImages[1].gameObject.SetActive(true);
    }

    public void HideThirdStepTutorial() {
        TutorialHoleHelper.DestroyHoles();
        Time.timeScale = 1;
        _holeImages[1].gameObject.SetActive(false);
        DestroyTutorial();
    }

    public void DestroyTutorial() {
        _currentTween.Kill();
        foreach (var hole in _holeImages) {
            Destroy(hole.gameObject);
        }

        Destroy(_blackBGImage.gameObject);
        Destroy(_fingerImage.gameObject);

        Destroy(gameObject);
    }
}