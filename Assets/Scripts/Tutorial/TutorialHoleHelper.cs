using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialHoleHelper : MonoBehaviour {
    [SerializeField]
    private Image _holeImagePrefab;
    
    private Transform _holesContainer;

    private List<GameObject> _curHoles = new List<GameObject>();

    public void SpawnHoles(List<Vector3Int> cells) {
        if (_holesContainer == null) {
            _holesContainer = GameObject.FindGameObjectWithTag("TutorialHoleContainer").transform;
        }
        
        foreach (var pos in cells) {
            var image = Instantiate(_holeImagePrefab, _holesContainer);
            _curHoles.Add(image.gameObject);
            image.transform.position = Camera.main.WorldToScreenPoint(pos);
        }
    }

    public void DestroyHoles() {
        foreach (GameObject hole in _curHoles) {
            Destroy(hole);
        }

        _curHoles.Clear();
    }
}