using System;
using UnityEngine;

public class CellView : MonoBehaviour {
   // [field: SerializeField]
//    public CellType CellType { get; private set; }

    [SerializeField]
    private GameObject _objectsContainer;

    private void Start() {
        if (_objectsContainer) {
            RandomRotateObjects();
        }
    }

    private void RandomRotateObjects() {
        _objectsContainer.transform.Rotate(Vector3.up, UnityEngine.Random.Range(0f, 360f));
    }
}