using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialHoleHelper : MonoBehaviour {
    [SerializeField]
    private Image _holeImagePrefab;

    [field: SerializeField]
    public Transform _holesContainer { get; private set; }

    private static List<GameObject> _curHoles = new List<GameObject>();

    private static TutorialHighlightHelper _highlightHelper = new TutorialHighlightHelper();

    public static void SpawnHoles(List<Vector3Int> cells, bool withHighlight = true) {
        HighlightObjectsHelper.Instance.SpawnCells(cells);
        
        if (withHighlight) {
            _highlightHelper.HighlightObjectsInCells(cells);
        }
    }

    public static void HighlightCells(List<Vector3Int> cells) {
        _highlightHelper.HighlightObjectsInCells(cells);
    }
    
    public static void HighlightObjects(List<GameObject> objs) {
        _highlightHelper.HighlightObjects(objs);
    }

    public static void DestroyHoles() {
        foreach (GameObject hole in _curHoles) {
            Destroy(hole);
        }
        HighlightObjectsHelper.Instance.ClearCells();
        _highlightHelper.ClearHighlights();
        _curHoles.Clear();
    }
}