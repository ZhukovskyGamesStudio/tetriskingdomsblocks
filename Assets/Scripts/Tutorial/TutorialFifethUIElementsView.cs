using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialFifthUIElementsView : MonoBehaviour {
    [SerializeField]
    private RectTransform[] _holeImages;

    private RectTransform _blackBGImage;

    [SerializeField]
    private TMP_Text _tutorialText;

    [SerializeField]
    private Tween _currentTween;
    
    
    [SerializeField]
    private bool _canSkipTutorial;

    private int _tutorialStep = 0;

    private List<Vector3Int> _firstStepCells;

    void Start() {
        GameFieldManager.Instance.OnPieceDestroyedByHammer += HideFirstStepTutorial;

        SpawnAllTutorialObjects();
        SetHolesPositions();

        ShowFirstStepTutorial();
        
        StorageManager.GameDataMain.HummerCount=5;
        
        if(BoostersManager.Instance != null) {
            GameUI.Instance.GameBoostersButtons.UpdateCounters(StorageManager.GameDataMain);
        }
    }

   /* private void Update() {
        if (Input.touchCount > 0 && _tutorialStep == 1) {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
                HideFirstStepTutorial();
        }

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0) && _tutorialStep == 1)
            HideFirstStepTutorial();
#endif
    }*/
  // private GameObject _pieceCellsContainer;
    public void SetHolesPositions() {
        var boosterContainer = GameUI.Instance.GameBoostersButtons._hummerButton.transform;
        _holeImages[0].transform.SetParent(boosterContainer);
        //_holeImages[0].transform.localPosition = Vector3.zero;
        //_holeImages[0].transform.position = posHole;
        _firstStepCells = new List<Vector3Int>();
        
        PieceView piece = FindAnyObjectByType<PieceView>();
       // _pieceCellsContainer = piece._cellsContainer.gameObject;
      //  TutorialHoleHelper.HighlightObjects(new List<GameObject> { _pieceCellsContainer });
        
        for (int i = 0; i < 8; i++) {
            for (int j = 0; j < 8; j++) {
                   _firstStepCells.Add(new Vector3Int(i, 0, j));
            }
        }
     
        _tutorialText.transform.position = new Vector3(boosterContainer.position.x+200, boosterContainer.position.y + 300, 0);
        _holeImages[0].transform.position = boosterContainer.position;
    }

    private void SpawnAllTutorialObjects() {
        _blackBGImage = gameObject.GetComponent<RectTransform>();
    }

    private void ShowFirstStepTutorial() {
     //   TutorialHoleHelper.HighlightObjects(new List<GameObject> { _pieceCellsContainer });
        _tutorialText.gameObject.SetActive(true);
        _tutorialStep = 1;
        TutorialHoleHelper.SpawnHoles(_firstStepCells);
        gameObject.GetComponent<Image>().enabled = true;
    }

    public void HideFirstStepTutorial() { 
        GameFieldManager.Instance.OnPieceDestroyedByHammer -= HideFirstStepTutorial;
        GameFieldManager.Instance.ClearAllLockedCells();
        _currentTween.Kill();
        _canSkipTutorial = true;
        _holeImages[0].gameObject.SetActive(false);
        TutorialHoleHelper.DestroyHoles();
        DestroyTutorial();
    }

    public void DestroyTutorial() {
        _currentTween.Kill();
        foreach (var hole in _holeImages) {
            Destroy(hole.gameObject);
        }

        Destroy(_blackBGImage.gameObject);
        Destroy(gameObject);
    }
}