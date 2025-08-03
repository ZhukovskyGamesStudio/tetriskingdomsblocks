using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MetaTutorial : MonoBehaviour
{
    [SerializeField]
    private RectTransform _holeImageGetFreeTetramineButton;
    
    [SerializeField]
    private RectTransform _holeImageContinue;
    
    [SerializeField]
    private RectTransform _holeImageBuildButton;
    
    [SerializeField]
    private RectTransform _holeTetraminesToBuild;

    [SerializeField]
    private RectTransform _blackBGImage;

    [SerializeField]
    private TMP_Text _tutorialText;

    [SerializeField]
    private Image _fingerImage;

    [SerializeField]
    private Transform _fingerImageContainer;

    [SerializeField]
    private string _openCloudsText;
    [SerializeField]
    private string _tapButtonOpenCloudsText;


    [SerializeField]
    private string _thirdTutorialText;
    
    [SerializeField]
    private string _fourthTutorialText;
    
    [SerializeField]
    private string _fifethTutorialText;
    [SerializeField]
    private string _sixthTutorialText;

    [SerializeField]
    private Tween _currentTween;
    [SerializeField]
    private Transform _highlightCellsContainer;
    [SerializeField]
    private TutorialHoleHelper _holeHelper;

    [SerializeField]
    private List<Vector3Int> _openedCloudCells, _secondStepCells, _thirdStepCells;

    [SerializeField]
    private bool _canSkipTutorial;

    private int _tutorialStep = 1;

    private Vector3 _cameraPosition = new Vector3(-7.5f, 0, -2);

    private Button _invCell;

    void Start() {
       // GameFieldManager.Instance.OnCellPlaced += HideFirstStepTutorial;
        ShowFirstStepTutorial();
        SetHolesPositions();
      //  StartAnimation();
    }

    private void Update() {
        if (MetaWorldCanvasView.Instance.UnlockFieldCellsView.gameObject.activeInHierarchy && _tutorialStep ==1) {
            ShowSecondStepTutorial();
        }
    }

    public void SetHolesPositions() {
        _holeTetraminesToBuild.gameObject.SetActive(false);
    //    var posHoleSecond = (Vector2)Camera.main.WorldToScreenPoint(NextPiecesView.Instance._piecesContainers[0].transform.position);
    _highlightCellsContainer.position = Vector3.zero;
        _holeImageBuildButton.transform.SetParent(MetaUI.Instance._buildButton);
        _holeImageBuildButton.transform.localPosition = Vector3.zero;
        _holeImageBuildButton.gameObject.SetActive(false);
        _holeImageGetFreeTetramineButton.SetParent(MetaUI.Instance._getPieceButtonView.transform);
        _holeImageGetFreeTetramineButton.transform.localPosition = Vector3.zero;
        _holeImageGetFreeTetramineButton.gameObject.SetActive(false);
        // Присваиваем позицию UI-элементу
        //ar posHoleFirst = GameUI.Instance._tasksContainer.position;
        
   
        
        //_holeImages[1].transform.SetParent(_holeHelper._holesContainer,true);
        //_holeImages[0].transform.SetParent(_holeHelper._holesContainer,true);
        //_holeImages[2].transform.SetParent(_holeHelper._holesContainer,true);
        
        //_holeImages[1].position = posHoleFirst;
      //  _holeImages[0].transform.position = posHoleSecond;
        //_holeImages[2].transform.position = (Vector2)Camera.main.WorldToScreenPoint(new Vector3(4, 0, 3.5f));
        _tutorialText.transform.position = (Vector2)Camera.main.WorldToScreenPoint(new Vector3(3, 0, 6));
        _fingerImage.transform.position = (Vector2)Camera.main.WorldToScreenPoint(new Vector3(4, 0, 2));
      //  _holeImages[0].gameObject.SetActive(true);
      //  _holeImages[1].gameObject.SetActive(false);
       // _holeImages[2].gameObject.SetActive(false);
    }

    public void StartAnimation() {
        _fingerImageContainer.localScale = Vector3.one;
        var color = _fingerImage.color;
        color.a = 0;
        _fingerImage.color = color;
      /*  _currentTween = DOTween.Sequence()
            .Append(_fingerImage.DOFade(1, 0.8f))
            .Join(_fingerImageContainer.DOScale(Vector3.one * 0.75f, 0.8f))
            .Append(_fingerImageContainer.DOMove((Vector2)Camera.main.WorldToScreenPoint(new Vector3(3f, 0, 3.5f)), 2.5f))
            .Append(_fingerImageContainer.DOScale(Vector3.one, 0.8f)).Join(_fingerImage.DOFade(0, 0.8f))
            .Append(_fingerImageContainer.DOMove(_holeImages[0].transform.position, 1)).SetLoops(-1, LoopType.Restart);*/
    }

    private void ShowFirstStepTutorial() {
       Invoke("MoveCameraToNeedPosition", 1.5f);
        TutorialHoleHelper.DestroyHoles();
//        _fingerImage.transform.position =  (Vector2)Camera.main.WorldToScreenPoint(new Vector3(5f, 0, 5f));
        _tutorialText.transform.position = (Vector2)Camera.main.WorldToScreenPoint(new Vector3(5f, 0, 7f));
      //  List<GameObject> highlihtObjects = new List<GameObject>();
       // foreach (var needPos in _openedCloudCells) {
       //     highlihtObjects.Add(MetaFieldManager.Instance._cells[needPos.x, needPos.y].Children.gameObject);    
      //  }
        //TutorialHoleHelper.HighlightObjects(highlihtObjects);
        TutorialHoleHelper.SpawnHoles(_openedCloudCells);
        _tutorialText.text = _openCloudsText;
       // HighlightCurrentPiece();
    }

    private void MoveCameraToNeedPosition() {
         MetaFieldManager.Instance.CanDragCamera = false;
              MetaFieldManager.Instance.CameraContainer.position += _cameraPosition;
    }
    private GameObject _pieceCellsContainer;

    private void HighlightCurrentPiece() {
        PieceView piece = FindAnyObjectByType<PieceView>();
        _pieceCellsContainer = piece._cellsContainer.gameObject;
        TutorialHoleHelper.HighlightObjects(new List<GameObject> { _pieceCellsContainer });
    }

 /*   public void HideFirstStepTutorial() {
        TutorialHoleHelper.DestroyHoles();
        GameFieldManager.Instance.ClearAllLockedCells();
        _currentTween.Kill();
        _canSkipTutorial = true;
      //  _holeImages[0].gameObject.SetActive(false);
        _fingerImage.gameObject.SetActive(false);  
        _blackBGImage.gameObject.SetActive(false);
        _tutorialText.gameObject.SetActive(false);
        ShowSecondStepTutorial();
    }*/

    public void ShowSecondStepTutorial() {
       // TutorialHoleHelper.HighlightObjects(new List<GameObject> { _pieceCellsContainer });
        TutorialHoleHelper.SpawnHoles(_secondStepCells);
        _tutorialStep = 2;
        _tutorialText.text = _tapButtonOpenCloudsText;
        MetaWorldCanvasView.Instance.UnlockFieldCellsView.UnlockButton.onClick.AddListener(HideSecondStepTutorial);
       // _holeImages[2].gameObject.SetActive(true);
    }

    public void HideSecondStepTutorial() {
        TutorialHoleHelper.DestroyHoles();
        MetaWorldCanvasView.Instance.UnlockFieldCellsView.UnlockButton.onClick.RemoveListener(HideSecondStepTutorial);
        _fingerImage.gameObject.SetActive(false);
       Invoke("ShowThirdStepTutorial", 0.8f);
    }
    public void ShowThirdStepTutorial() {
        _tutorialText.text = _tapButtonOpenCloudsText;
        _tutorialStep = 3;
     _holeImageGetFreeTetramineButton.gameObject.SetActive(true);
     MetaUI.Instance._getPieceButtonView.GetPieceButton.onClick.AddListener(HideThirdStepTutorial);
     _tutorialText.text = _thirdTutorialText;
    }

    public void HideThirdStepTutorial() {
        _holeImageGetFreeTetramineButton.gameObject.SetActive(false);
       Invoke("FindContinueButton", 1f);
    }

    private void FindContinueButton() {
         var continueButton = GameObject.Find("OpenState")?.GetComponent<Button>();
                continueButton.onClick.AddListener(FindNextButton);
    }

    private void FindNextButton() {
        var continueButton = GameObject.Find("ContinueState")?.GetComponent<Button>();
        continueButton.onClick.AddListener(ShowFourthStepTutorial);
    }
    private void ShowFourthStepTutorial() {
      //  continueButton.onClick.AddListener(ShowFourthStepTutorial);
      _holeImageBuildButton.gameObject.SetActive(true);
      MetaUI.Instance._buildButton.gameObject.GetComponent<Button>().onClick.AddListener(ShowFifthStepTutorial);
        _tutorialStep = 4;
        _tutorialText.text = _fourthTutorialText;
    }

    private void ShowFifthStepTutorial() {
        _holeImageBuildButton.gameObject.SetActive(true);
        MetaUI.Instance._buildButton.gameObject.GetComponent<Button>().onClick.RemoveListener(ShowFifthStepTutorial);
        _tutorialStep = 5;
        _holeTetraminesToBuild.gameObject.SetActive(true);
        _invCell = GameObject.Find("InventoryCell(Clone)")?.GetComponent<Button>();
        _invCell.onClick.AddListener(ShowSixthStepTutorial);
        _tutorialText.text = _fifethTutorialText;
    }

    private void ShowSixthStepTutorial() {
        _tutorialText.text = _sixthTutorialText;
        _tutorialStep = 5;
        _holeImageBuildButton.gameObject.SetActive(false);
        _invCell.onClick.RemoveListener(ShowSixthStepTutorial);
        MetaFieldManager.Instance.OnCellPlaced += (i, bools) =>  HideSixthStepTutorial();
        //highlight field
    }

    private void HideSixthStepTutorial() {
        MetaFieldManager.Instance.OnCellPlaced -= (i, bools) =>  HideSixthStepTutorial();
        MetaFieldManager.Instance.CanDragCamera = true;
        TutorialHoleHelper.DestroyHoles();
        DestroyTutorial();
    }

    public void DestroyTutorial() {
        _currentTween.Kill();
      //  foreach (var hole in _holeImages) {
    //        Destroy(hole.gameObject);
   //     }

        Destroy(_blackBGImage.gameObject);
        Destroy(_fingerImage.gameObject);

        Destroy(gameObject);
    }
}
