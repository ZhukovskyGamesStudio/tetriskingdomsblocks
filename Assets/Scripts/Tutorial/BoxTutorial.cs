using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class BoxTutorial : TutorialObjectHidedAfterTap {
    [SerializeField]
    private RectTransform rectTransformBoxMark;

    [SerializeField]
    private TMP_Text _tutorialText;

    [SerializeField]
    private TutorialHoleHelper _holeHelper;

    protected override void Start() {
        base.Start();
        GameFieldManager.Instance.OnCellPlaced += CheckIceCells;
        List<Vector3Int> boxPoses = new List<Vector3Int>();
        for (int i = 0; i < GameFieldManager.Instance._field.GetLength(0); i++) {
            for (int j = 0; j < GameFieldManager.Instance._field.GetLength(1); j++) {
                if (GameFieldManager.Instance._field[i, j] == CellType.Box) {
                    boxPoses.Add(new Vector3Int(i, 0, j));
                }
            }
        }

        _holeHelper.SpawnHoles(boxPoses);
    }

    private void CheckIceCells(Vector2Int coord, bool[,] needCells) {
        for (int x = 0; x < needCells.GetLength(0); x++) {
            for (int y = 0; y < needCells.GetLength(1); y++) {
                Vector2Int place = new(coord.x + x, coord.y + y);
                if (needCells[x, y]) {
                    foreach (var checkedCell in FieldUtils.GetCellsAround(GameFieldManager.Instance._field, place)) {
                        if (GameFieldManager.Instance._field[checkedCell.x, checkedCell.y] == CellType.Box) {
                            _holeHelper.DestroyHoles();
                            Destroy(_rectTransform.gameObject);
                            GameFieldManager.Instance.OnCellPlaced -= CheckIceCells;
                            return;
                        }
                    }
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