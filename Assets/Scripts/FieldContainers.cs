using System;
using UnityEngine;

public class FieldContainers : MonoBehaviour {
    public static FieldContainers Instance;

    private void Awake() {
        Instance = this;
    }

    [field: SerializeField]
    public Transform MarkedCellsVerticalAnchor { get; private set; }

    [field: SerializeField]
    public Transform PlacedCellsVerticalAnchor { get; private set; }

    [field: SerializeField]
    public Transform FieldStart { get; private set; }

    [field: SerializeField]
    public Transform FieldEnd { get; private set; }

    [field: SerializeField]
    public Transform FieldContainer { get; private set; }
}