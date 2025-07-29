using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialThirdUIElementsView : MonoBehaviour {
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

    void Start() {
        GameFieldManager.Instance.OnCellPlaced += ShowFirstStepTutorial;

        SpawnAllTutorialObjects();
        SetHolesPositions();
    }

    private void Update() {
        if (Input.touchCount > 0 && _tutorialStep == 1) {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
                HideFirstStepTutorial();
        }

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0) && _tutorialStep == 1)
            HideFirstStepTutorial();
#endif
    }

    public void SetHolesPositions() {
        var ultimateContainer = GameUI.Instance._ultimateContainer;
        _holeImages[0].transform.SetParent(ultimateContainer);
        //_holeImages[0].transform.position = posHole;
        _tutorialText.transform.position = new Vector3(ultimateContainer.position.x, ultimateContainer.position.y - 200, 0);
    }

    private void SpawnAllTutorialObjects() {
        _blackBGImage = gameObject.GetComponent<RectTransform>();
    }

    private void ShowFirstStepTutorial(Vector2Int pos, bool[,] cells) {
        _tutorialText.gameObject.SetActive(true);
        _tutorialStep = 1;
        GameFieldManager.Instance.OnCellPlaced -= ShowFirstStepTutorial;
        gameObject.GetComponent<Image>().enabled = true;
    }

    public void HideFirstStepTutorial() {
        GameFieldManager.Instance.ClearAllLockedCells();
        _currentTween.Kill();
        _canSkipTutorial = true;
        _holeImages[0].gameObject.SetActive(false);
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