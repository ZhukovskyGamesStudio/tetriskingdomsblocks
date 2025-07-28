using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class IceTutorial : TutorialObjectHidedAfterTap {
    [SerializeField]
    private RectTransform rectTransformIceMark;

    [SerializeField]
    private TMP_Text _tutorialText;

    [SerializeField]
    private TutorialHoleHelper _holeHelper;

    protected void Start() {
      //  base.Start();
        GameFieldManager.Instance.OnCellPlaced += CheckIceCells;
        var mainCamera = Camera.main;
        List<Vector3Int> icePoses = new List<Vector3Int>();

        for (int i = 0; i < GameFieldManager.Instance._field.GetLength(0); i++) {
            for (int j = 0; j < GameFieldManager.Instance._field.GetLength(1); j++) {
                if (GameFieldManager.Instance._field[i, j] == CellType.Ice) {
                    var iceHoleUI = Instantiate(rectTransformIceMark, GameUI.Instance.HolesForBgContainer);
                    iceHoleUI.transform.position = (Vector2)mainCamera.WorldToScreenPoint(new Vector3(i, 0, j));
                }
            }
        }

        _holeHelper.SpawnHoles(icePoses);
    }

    private void CheckIceCells(Vector2Int coord, bool[,] needCells) {
        for (int x = 0; x < needCells.GetLength(0); x++) {
            for (int y = 0; y < needCells.GetLength(1); y++) {
                Vector2Int place = new(coord.x + x, coord.y + y);
                if (needCells[x, y] && GameFieldManager.Instance._field[place.x, place.y] == CellType.Ice) {
                    _holeHelper.DestroyHoles();
                   // Destroy(_rectTransform.gameObject);
                    GameFieldManager.Instance.OnCellPlaced -= CheckIceCells;
                    return;
                }
            }
        }

        PlayAnimationText();
    }

    public void PlayAnimationText() {
        DOTween.Kill(_tutorialText.transform);

        _tutorialText.transform.localScale = Vector3.one;

        _tutorialText.transform.DOScale(Vector3.one * 1.2f, 0.4f).SetEase(Ease.OutBack).OnComplete(() => {
            _tutorialText.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.InOutQuad);
        });
    }
}