using System;
using System.Collections.Generic;
using UnityEngine;

public class PiecesViewTable : MonoBehaviour {
    [SerializeField]
    private List<CellView> _cellViews;

    [field: SerializeField]
    public PieceView PieceViewPrefab; 

    public static PiecesViewTable Instance;

    private void Awake() {
        Instance = this;
    }
}