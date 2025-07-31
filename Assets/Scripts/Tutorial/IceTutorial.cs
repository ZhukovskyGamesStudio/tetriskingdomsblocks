using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class IceTutorial : MonoBehaviour {
    [SerializeField]
    private RectTransform rectTransformIceMark;


    private void Start() {
        //  base.Start();
        GameFieldManager.Instance.OnCellPlaced += HideAndDestroy;
        List<Vector3Int> icePoses = new();

        for (int i = 0; i < GameFieldManager.Instance._field.GetLength(0); i++) {
            for (int j = 0; j < GameFieldManager.Instance._field.GetLength(1); j++) {
                icePoses.Add(new Vector3Int(i, 0, j));
            }
        }
        PieceView[] pieces = FindObjectsByType<PieceView>(FindObjectsSortMode.None); 
        List<GameObject> _pieceCellsContainer = new List<GameObject>();
        foreach (var piece in pieces) {
            _pieceCellsContainer.Add(piece._cellsContainer.gameObject);
        }
       
        TutorialHoleHelper.HighlightObjects(_pieceCellsContainer);
        TutorialHoleHelper.SpawnHoles(icePoses);
    }

    private void HideAndDestroy(Vector2Int coord, bool[,] needCells) {
        GameFieldManager.Instance.OnCellPlaced -= HideAndDestroy;
        TutorialHoleHelper.DestroyHoles();
        Destroy(gameObject);
    }
}