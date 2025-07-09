using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnRandomNature : MonoBehaviour {
    [SerializeField]
    private List<GameObject> _natureObjects;

    [SerializeField]
    private List<Transform> _natureAnchors;

    public void Generate() {
        foreach (var VARIABLE in _natureAnchors) {
            foreach (Transform child in VARIABLE) {
                Destroy(child.gameObject);
            }
        }

        var shuffled = _natureObjects.OrderBy(_ => Random.Range(0, 1f)).ToList();
        for (int index = 0; index < _natureAnchors.Count; index++) {
            Transform anchor = _natureAnchors[index];
            var obj =Instantiate(shuffled[index], anchor);
            obj.SetActive(true);
        }
    }
}