using System.Collections.Generic;
using UnityEngine;

public class TutorialHighlightHelper {
    private Dictionary<GameObject, int> _highlitedLayers = new();

    private readonly string _highlightedLayer = "Highlighted";

    public void HighlightObjectsInCells(List<Vector3Int> cells) {
        List<GameObject> list = new List<GameObject>();
        foreach (var pos in cells) {
            if (GameFieldManager.Instance) {
                var cell = GameFieldManager.Instance.GetCellInCoord(pos);
                if (cell != null) {
                    list.Add(cell.gameObject);
                }
            } else {
                var cell = MetaFieldManager.Instance.GetCellInCoord(pos);
                if (cell != null) {
                    list.Add(cell.gameObject);
                }
            }
        }

        HighlightObjects(list);
    }

    public void HighlightObjects(List<GameObject> objects) {
        int newLayer = LayerMask.NameToLayer(_highlightedLayer);
        foreach (var obj in objects) {
            if (obj == null) {
                continue;
            }

            var renders = obj.GetComponentsInChildren<MeshRenderer>();
            foreach (var render in renders) {
                if (_highlitedLayers.ContainsKey(render.gameObject)) {
                    continue;
                }

                _highlitedLayers.Add(render.gameObject, render.gameObject.layer);
                render.gameObject.layer = newLayer;
            }
        }
    }

    public void ClearHighlights() {
        foreach (var kvp in _highlitedLayers) {
            if (kvp.Key == null) {
                continue;
            }

            kvp.Key.layer = kvp.Value;
        }

        _highlitedLayers.Clear();
    }
}