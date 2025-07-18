using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUIElementsView : MonoBehaviour
{
    [SerializeField] private RectTransform[] _holeImages;
    [SerializeField] private RectTransform _blackBGImage;
    [SerializeField] private RectTransform _fingerImage;
    [SerializeField] private Transform _firstTutorialContainer;
    [SerializeField] private Transform _secondTutorialContainer;
    [SerializeField] private Transform _thirdTutorialContainer;
    [SerializeField] private Tween _currentTween;

    [SerializeField]
    private bool _canSkipTutorial;
    private int _tutorialStep = 1;
    void Start()
    {
       TutorialFieldManager.Instance.OnCellPlaced += HideFirstStepTutorial;
        StartAnimation();
    }

    private void Update() {
        if (Input.touchCount > 0)
        {
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
            if(_tutorialStep == 2)
                HideSecondStepTutorial();
            if(_tutorialStep == 3)
                HideThirdStepTutorial();
        }
    }
    public void SetHolesPositions(Vector3 posHoleFirst, Vector3 posHoleSecond) {
        posHoleFirst = (Vector2)Camera.main.WorldToScreenPoint(posHoleFirst);
        posHoleSecond = (Vector2)Camera.main.WorldToScreenPoint(posHoleSecond);
        // Присваиваем позицию UI-элементу
        _holeImages[0].transform.position= posHoleFirst;
        _fingerImage.transform.position = _holeImages[0].transform.position;
        _holeImages[1].transform.position = posHoleSecond;
    }

    public void StartAnimation()
    {
        _currentTween = DOTween.Sequence()
            .Append(_fingerImage.DOScale(Vector3.one, 0.8f))
            .Append(_fingerImage.DOScale(Vector3.one * 0.75f, 0.8f))
            .Append(_fingerImage.DOScale(Vector3.one, 0.8f))
            .Append(_fingerImage.DOScale(Vector3.one * 0.75f, 0.8f))
            .Append(_fingerImage.DOScale(Vector3.one, 0.8f))
            .Append(_fingerImage.DOScale(Vector3.one * 0.75f, 0.8f))
            .Append(_fingerImage.DOMove(_holeImages[1].transform.position, 2.5f))
            .Append(_fingerImage.DOScale(Vector3.one, 0.8f))
            .Append(_fingerImage.DOMove(_holeImages[0].transform.position, 1))
            .Append(_fingerImage.DOScale(Vector3.one * 0.75f, 0.8f))
            .Append(_fingerImage.DOMove(_holeImages[1].transform.position, 2.5f))
            .Append(_fingerImage.DOScale(Vector3.one, 0.8f))
            .Append(_fingerImage.DOMove(_holeImages[0].transform.position, 1))
            .SetLoops(-1, LoopType.Restart);
    }
    

    public void HideFirstStepTutorial() {  
        TutorialFieldManager.Instance.OnCellPlaced -= HideFirstStepTutorial;
        _firstTutorialContainer.gameObject.SetActive(false);
        ShowSecondStepTutorial();
        _currentTween.Kill();
        _canSkipTutorial = true;
        _holeImages[0].gameObject.SetActive(false);
        _holeImages[1].gameObject.SetActive(false);
    }
    
    public void ShowSecondStepTutorial() {
        _tutorialStep = 2;
        Time.timeScale = 0;
        _secondTutorialContainer.gameObject.SetActive(true);
        _holeImages[2].gameObject.SetActive(true);
    }
    public void HideSecondStepTutorial() {
        _secondTutorialContainer.gameObject.SetActive(false);
        _holeImages[2].gameObject.SetActive(false);
        Time.timeScale = 1;
        _canSkipTutorial = false;
       Invoke("ShowThirdStepTutorial", 0.5f); 
    }
    public void ShowThirdStepTutorial() {
        _tutorialStep = 3;
        Time.timeScale = 0;
        _canSkipTutorial = true;
        _thirdTutorialContainer.gameObject.SetActive(true);
        _holeImages[3].gameObject.SetActive(true);
    }
    public void HideThirdStepTutorial() {
        _thirdTutorialContainer.gameObject.SetActive(false);
        Time.timeScale = 1;
        _holeImages[3].gameObject.SetActive(false);
        DestroyTutorial();
    }

    public void DestroyTutorial()
    {
        _currentTween.Kill();
        foreach (var hole in _holeImages)
        {
            Destroy(hole.gameObject); 
        }

        Destroy(_blackBGImage.gameObject);
        Destroy(_fingerImage.gameObject);
        
        Destroy(gameObject);
     
    }
}
