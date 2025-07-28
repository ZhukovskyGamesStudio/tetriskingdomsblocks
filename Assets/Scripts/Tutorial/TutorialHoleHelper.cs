using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialHoleHelper : MonoBehaviour {
    [SerializeField]
    private Image _holeImagePrefab;

    [field: SerializeField]
    public Transform _holesContainer { get; private set; }

    private List<GameObject> _curHoles = new List<GameObject>();

    private TutorialHighlightHelper _highlightHelper = new TutorialHighlightHelper();

    public void SpawnHoles(List<Vector3Int> cells, bool withHiglight = true) {
        if (_holesContainer == null) {
            _holesContainer = GameObject.FindGameObjectWithTag("TutorialHoleContainer").transform;
        }

        Camera mainCamera = Camera.main!;
        foreach (var pos in cells) {
            Image image = Instantiate(_holeImagePrefab, _holesContainer);
            _curHoles.Add(image.gameObject);
            image.transform.position = mainCamera.WorldToScreenPoint(pos);

            Vector2 min = mainCamera.WorldToScreenPoint(pos - new Vector3(0.5f, 0, 0.5f));
            Vector2 max = mainCamera.WorldToScreenPoint(pos + new Vector3(0.5f, 0, 0.5f));
            image.rectTransform.sizeDelta = (max - min);
        }

        if (withHiglight) {
            _highlightHelper.HighlightObjectsInCells(cells);
        }
    }

    public void DestroyHoles() {
        foreach (GameObject hole in _curHoles) {
            Destroy(hole);
        }

        _highlightHelper.ClearHighlights();
        _curHoles.Clear();
    }
}