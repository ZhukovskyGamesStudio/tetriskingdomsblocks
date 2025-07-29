using System;
using System.Collections.Generic;
using UnityEngine;

public class HighlightObjectsHelper : MonoBehaviour {
    public static HighlightObjectsHelper Instance;

    [SerializeField]
    private GameObject _cellPrefab, _cellsContainer;

    private List<GameObject> _cells = new List<GameObject>();

    private void Awake() {
        Instance = this;
    }

    public void SpawnCells(List<Vector3Int> coords) {
        for (int i = 0; i < coords.Count; i++) {
            var cell = Instantiate(_cellPrefab, _cellsContainer.transform);
            cell.transform.localPosition = new Vector3(coords[i].x, 0, coords[i].z);
            _cells.Add(cell);
        }
    }

    public void ClearCells() {
        foreach (var cell in _cells) {
            Destroy(cell);
        }

        _cells.Clear();
    }
}