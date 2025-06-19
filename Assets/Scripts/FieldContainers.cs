using System;
using UnityEngine;

public class FieldContainers : MonoBehaviour {
    public static FieldContainers Instance;

    private void Awake() {
        Instance = this;
    }

    [field: SerializeField]
    public Transform MarkedCellsVerticalAnchor { get;private set; }
}
