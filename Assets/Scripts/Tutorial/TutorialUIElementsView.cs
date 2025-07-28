using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUIElementsView : MonoBehaviour {
    [SerializeField]
    private RectTransform[] _holeImages;
    
    private RectTransform _blackBGImage;
    private TMP_Text _tutorialText;
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
    private bool _canSkipTutorial;

    private int _tutorialStep = 1;

    void Start() {
        GameFieldManager.Instance.OnCellPlaced += HideFirstStepTutorial;
       
        SpawnAllTutorialObjects(); 
        ShowFirstStepTutorial();
        SetHolesPositions();
        StartAnimation();
        
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

    private void SpawnAllTutorialObjects() {
        
        _blackBGImage = gameObject.GetComponent<RectTransform>();
        _tutorialText = gameObject.transform.GetChild(0).GetComponent<TMP_Text>();
        _fingerImage = gameObject.transform.GetChild(1).GetComponent<Image>();
    }
    
    private void TrySkipStep() {
        if (_canSkipTutorial) {
            if (_tutorialStep == 2)
                HideSecondStepTutorial();
            if (_tutorialStep == 3)
                HideThirdStepTutorial();
        }
    }

    public void SetHolesPositions() {
        var posHoleSecond = (Vector2)Camera.main.WorldToScreenPoint(NextPiecesView.Instance._piecesContainers[0].transform.position);
        // Присваиваем позицию UI-элементу
        var posHoleFirst = GameUI.Instance._tasksContainer.position;
        _holeImages[1].transform.parent = _holeHelper._holesContainer;
        _holeImages[0].transform.parent = _holeHelper._holesContainer;
        _holeImages[2].transform.parent = _holeHelper._holesContainer;
        _holeImages[1].position = posHoleFirst;
        _holeImages[0].transform.position = posHoleSecond; 
        _holeImages[2].transform.position = (Vector2)Camera.main.WorldToScreenPoint(new Vector3(4,0,3.5f)); 
        _tutorialText.transform.position = (Vector2)Camera.main.WorldToScreenPoint(new Vector3(4,0,2)); 
        _fingerImage.transform.position = _holeImages[0].transform.position;
        _holeImages[0].gameObject.SetActive(true);
        _holeImages[1].gameObject.SetActive(false);
        _holeImages[2].gameObject.SetActive(false);
    }

    public void StartAnimation() {
        _fingerImage.rectTransform.localScale = Vector3.one;
        var color = _fingerImage.color;
        color.a = 0;
        _fingerImage.color = color;
        _currentTween = DOTween.Sequence().Append(_fingerImage.DOFade(1, 0.8f))
            .Join(_fingerImage.rectTransform.DOScale(Vector3.one * 0.75f, 0.8f))
            .Append(_fingerImage.rectTransform.DOMove((Vector2)Camera.main.WorldToScreenPoint(new Vector3(4,0,3)), 2.5f))
            .Append(_fingerImage.rectTransform.DOScale(Vector3.one, 0.8f)).Join(_fingerImage.DOFade(0, 0.8f))
            .Append(_fingerImage.rectTransform.DOMove(_holeImages[0].transform.position, 1)).SetLoops(-1, LoopType.Restart);
    }

    private void ShowFirstStepTutorial() {
        _holeHelper.DestroyHoles();
        _holeHelper.SpawnHoles(_firstStepCells);
        _tutorialText.text = _firstTutorialText;
    }
    

    public void HideFirstStepTutorial(Vector2Int pos,bool[,] cells ) {
        _holeHelper.DestroyHoles();
        GameFieldManager.Instance.OnCellPlaced -= HideFirstStepTutorial;
        GameFieldManager.Instance.ClearAllLockedCells();
        ShowSecondStepTutorial();
        _currentTween.Kill();
        _canSkipTutorial = true;
        _holeImages[0].gameObject.SetActive(false);
        _fingerImage.gameObject.SetActive(false);
    }

    public void ShowSecondStepTutorial() {
        _holeHelper.SpawnHoles(_secondStepCells);
        _tutorialStep = 2;
        Time.timeScale = 0;
        _tutorialText.text = _secondTutorialText;
        _holeImages[2].gameObject.SetActive(true);
    }

    public void HideSecondStepTutorial() {
        _holeHelper.DestroyHoles();
        _holeImages[2].gameObject.SetActive(false);
        Time.timeScale = 1;
        _canSkipTutorial = false;
        _tutorialText.gameObject.SetActive(false);
        _tutorialText.transform.position = new Vector3(GameUI.Instance._tasksContainer.position.x,GameUI.Instance._tasksContainer.position.y-100f,0); 
        Invoke(nameof(ShowThirdStepTutorial), 0.5f);
    }

    public void ShowThirdStepTutorial() {
        _holeHelper.SpawnHoles(_thirdStepCells);
        _tutorialStep = 3;
        Time.timeScale = 0;
        _canSkipTutorial = true;
        _tutorialText.text = _thirdTutorialText;
        _tutorialText.gameObject.SetActive(true);
        _holeImages[1].gameObject.SetActive(true);
    }

    public void HideThirdStepTutorial() {
        _holeHelper.DestroyHoles();
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