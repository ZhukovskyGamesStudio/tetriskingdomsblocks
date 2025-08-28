using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class BoxTutorial : TutorialObjectHidedAfterTap {
    [SerializeField]
    private RectTransform rectTransformBoxMark;

    [SerializeField]
    private TutorialHoleHelper _holeHelper;

    protected void Start() {
        GameFieldManager.Instance.OnCellPlaced += CheckIceCells;
        List<Vector3Int> boxPoses = new List<Vector3Int>();
        for (int i = 0; i < GameFieldManager.Instance._field.GetLength(0); i++) {
            for (int j = 0; j < GameFieldManager.Instance._field.GetLength(1); j++) {
                if (GameFieldManager.Instance._field[i, j] == CellType.Box) {
                    boxPoses.Add(new Vector3Int(i, 0, j));
                }
            }
        }

        TutorialHoleHelper.SpawnHoles(boxPoses);
        SendTutorialEventStep();
    }
    
    private void SendTutorialEventStep() {
        ZhukovskyAnalyticsManager.Instance.SendCustomEvent("tutorial", new Dictionary<string, object> {
            { "step_name", "_boxTutorial"  }
        }, true);
    }

    private void CheckIceCells(Vector2Int coord, bool[,] needCells) {
        for (int x = 0; x < needCells.GetLength(0); x++) {
            for (int y = 0; y < needCells.GetLength(1); y++) {
                Vector2Int place = new(coord.x + x, coord.y + y);
                if (needCells[x, y]) {
                    foreach (var checkedCell in FieldUtils.GetCellsAround(GameFieldManager.Instance._field, place)) {
                        if (GameFieldManager.Instance._field[checkedCell.x, checkedCell.y] == CellType.Box) {
                            TutorialHoleHelper.DestroyHoles();
                            //Destroy(_rectTransform.gameObject);
                            GameFieldManager.Instance.OnCellPlaced -= CheckIceCells;
                            return;
                        }
                    }
                }
            }
        }

        PlayAnimationText();
    }

    protected override void HideAndDestroy() {
        TutorialHoleHelper.DestroyHoles();
        base.HideAndDestroy();
    }

    public void PlayAnimationText() {
        //DOTween.Kill(_tutorialText.transform);
        TutorialHoleHelper.DestroyHoles();
        /*_tutorialText.transform.localScale = Vector3.one;

        _tutorialText.transform.DOScale(Vector3.one * 1.2f, 0.4f).SetEase(Ease.OutBack).OnComplete(() => {
            _tutorialText.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.InOutQuad);
        });*/
    }
}