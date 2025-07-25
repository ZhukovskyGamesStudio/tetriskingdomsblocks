using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUIElementsView : MonoBehaviour {
    [SerializeField]
    private RectTransform[] _holeImages;

    [SerializeField]
    private RectTransform _blackBGImage;

    [SerializeField]
    private Image _fingerImage;

    [SerializeField]
    private Transform _firstTutorialContainer;

    [SerializeField]
    private Transform _secondTutorialContainer;

    [SerializeField]
    private Transform _thirdTutorialContainer;

    [SerializeField]
    private Tween _currentTween;

    [SerializeField]
    private TutorialHoleHelper _holeHelper;
    
    [SerializeField]
    private List<Vector3Int> _firstStepCells, _secondStepCells, _thirdStepCells;
    
    [SerializeField]
    private bool _canSkipTutorial;

    private int _tutorialStep = 1;

    void Start() {
        TutorialFieldManager.Instance.OnCellPlaced += HideFirstStepTutorial;
        StartAnimation();
        ShowFirstStepTutorial();
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
            if (_tutorialStep == 2)
                HideSecondStepTutorial();
            if (_tutorialStep == 3)
                HideThirdStepTutorial();
        }
    }

    public void SetHolesPositions(Vector3 posHoleFirst, Vector3 posHoleSecond) {
        posHoleFirst = (Vector2)Camera.main.WorldToScreenPoint(posHoleFirst);
        posHoleSecond = (Vector2)Camera.main.WorldToScreenPoint(posHoleSecond);
        // Присваиваем позицию UI-элементу
        _holeImages[0].transform.position = posHoleFirst;
        _fingerImage.transform.position = _holeImages[0].transform.position;
        _holeImages[1].transform.position = posHoleSecond;
    }

    public void StartAnimation() {
        _fingerImage.rectTransform.localScale = Vector3.one;
        var color = _fingerImage.color;
        color.a = 0;
        _fingerImage.color = color;
        _currentTween = DOTween.Sequence().Append(_fingerImage.DOFade(1, 0.8f))
            .Join(_fingerImage.rectTransform.DOScale(Vector3.one * 0.75f, 0.8f))
            .Append(_fingerImage.rectTransform.DOMove(_holeImages[1].transform.position, 2.5f))
            .Append(_fingerImage.rectTransform.DOScale(Vector3.one, 0.8f)).Join(_fingerImage.DOFade(0, 0.8f))
            .Append(_fingerImage.rectTransform.DOMove(_holeImages[0].transform.position, 1)).SetLoops(-1, LoopType.Restart);
    }

    private void ShowFirstStepTutorial() {
        _holeHelper.DestroyHoles();
        _holeHelper.SpawnHoles(_firstStepCells);
    }
    

    public void HideFirstStepTutorial() {
        _holeHelper.DestroyHoles();
        TutorialFieldManager.Instance.OnCellPlaced -= HideFirstStepTutorial;
        _firstTutorialContainer.gameObject.SetActive(false);
        ShowSecondStepTutorial();
        _currentTween.Kill();
        _canSkipTutorial = true;
        _holeImages[0].gameObject.SetActive(false);
        _holeImages[1].gameObject.SetActive(false);
    }

    public void ShowSecondStepTutorial() {
        _holeHelper.SpawnHoles(_secondStepCells);
        _tutorialStep = 2;
        Time.timeScale = 0;
        _secondTutorialContainer.gameObject.SetActive(true);
        _holeImages[2].gameObject.SetActive(true);
    }

    public void HideSecondStepTutorial() {
        _holeHelper.DestroyHoles();
        _secondTutorialContainer.gameObject.SetActive(false);
        _holeImages[2].gameObject.SetActive(false);
        Time.timeScale = 1;
        _canSkipTutorial = false;
        Invoke(nameof(ShowThirdStepTutorial), 0.5f);
    }

    public void ShowThirdStepTutorial() {
        _holeHelper.SpawnHoles(_thirdStepCells);
        _tutorialStep = 3;
        Time.timeScale = 0;
        _canSkipTutorial = true;
        _thirdTutorialContainer.gameObject.SetActive(true);
        _holeImages[3].gameObject.SetActive(true);
    }

    public void HideThirdStepTutorial() {
        _holeHelper.DestroyHoles();
        _thirdTutorialContainer.gameObject.SetActive(false);
        Time.timeScale = 1;
        _holeImages[3].gameObject.SetActive(false);
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