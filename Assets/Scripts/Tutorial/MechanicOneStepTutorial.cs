using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ScriptableObjects;
using UnityEngine;

public class MechanicOneStepTutorial : MonoBehaviour {
    [SerializeField]
    private RectTransform rectTransformIceMark;

    [SerializeField]
    private bool isHighlightUnplacedCells = true;

    [SerializeField]
    private SpotlightAnimConfig _animConfig;

    private void Start() {
        //  base.Start();
        GameFieldManager.Instance.OnCellPlaced += HideAndDestroy;
        List<Vector3Int> icePoses = new();

        for (int i = 0; i < GameFieldManager.Instance._field.GetLength(0); i++) {
            for (int j = 0; j < GameFieldManager.Instance._field.GetLength(1); j++) {
                if (isHighlightUnplacedCells || FieldUtils.CanPlaceOnCell(GameFieldManager.Instance._field[i, j]))
                    icePoses.Add(new Vector3Int(i, 0, j));
            }
        }

        PieceView[] pieces = FindObjectsByType<PieceView>(FindObjectsSortMode.None);
        List<GameObject> _pieceCellsContainer = new List<GameObject>();
        foreach (var piece in pieces) {
            _pieceCellsContainer.Add(piece._cellsContainer.gameObject);
        }
        GameUI.Instance.GoalView.Witch.gameObject.SetActive(false);
        GameUI.Instance.GoalView.gameObject.SetActive(false);
        SpotlightsManager.Instance.SpotlightWithText.ShowSpotlight(SpotlightsManager.Instance.CenterScreenAnchor, _animConfig);
        TutorialHoleHelper.HighlightObjects(_pieceCellsContainer);
        TutorialHoleHelper.SpawnHoles(icePoses);
    }

    private void HideAndDestroy(Vector2Int coord, bool[,] needCells) {
        GameUI.Instance.GoalView.gameObject.SetActive(true);
        SpotlightsManager.Instance.SpotlightWithText.HideSpotlight().Forget();
        GameUI.Instance.GoalView.ShowWitchWithAnimation();
        GameFieldManager.Instance.OnCellPlaced -= HideAndDestroy;
        TutorialHoleHelper.DestroyHoles();
        Destroy(gameObject);
    }
}