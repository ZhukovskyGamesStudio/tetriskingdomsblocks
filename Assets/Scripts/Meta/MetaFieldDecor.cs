using System;
using System.Collections.Generic;
using UnityEngine;

public class MetaFieldDecor : MonoBehaviour {
    [SerializeField]
    private List<GameObject> _trees;

    [SerializeField]
    private List<Transform> _treePositions;

    [SerializeField]
    private GameObject _clouds;

    [SerializeField]
    private List<Transform> _cloudsPositions;

    [SerializeField]
    private float _cloudsRotation = 45;

    private void Start() {
        PlaceRandomTrees();
        PlaceClouds();
    }

    private void PlaceRandomTrees() {
        GameObject obj;
        for (int i = 0; i < _treePositions.Count; i++) {
            obj = Instantiate(_trees[UnityEngine.Random.Range(0, _trees.Count)], _treePositions[i].position,
                Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0), transform);
            obj.isStatic = true;
        }
    }

    private void PlaceClouds() {
        GameObject obj;
        for (int i = 0; i < _cloudsPositions.Count; i+=2) {
            obj = Instantiate(_clouds, _cloudsPositions[i].position, Quaternion.Euler(-90, _cloudsRotation, 0), transform);
            
            obj.isStatic = true;
        }
    }
}