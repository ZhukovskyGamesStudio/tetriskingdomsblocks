using System;
using System.Collections.Generic;
using UnityEngine;

public class NextPiecesView : MonoBehaviour , IResetable{
    public static NextPiecesView Instance;

    [SerializeField]
    private Transform _container0, _container1, _container2;

    private void Awake() {
        Instance = this;
    }

    public void SetData(List<PieceData> nextPieces) {
        DestroyChildren();

        var go = Instantiate(PiecesViewTable.Instance.PieceViewPrefab, _container0);
        go.SetData(nextPieces[0]);

        go = Instantiate(PiecesViewTable.Instance.PieceViewPrefab, _container1);
        go.SetData(nextPieces[1]);

        go = Instantiate(PiecesViewTable.Instance.PieceViewPrefab, _container2);
        go.SetData(nextPieces[2]);
    }

    private void DestroyChildren() {
        if (_container0.childCount > 0) {
            Destroy(_container0.GetChild(0).gameObject);
        }

        if (_container1.childCount > 0) {
            Destroy(_container1.GetChild(0).gameObject);
        }

        if (_container2.childCount > 0) {
            Destroy(_container2.GetChild(0).gameObject);
        }
    }

    public void Reset() {
        DestroyChildren();
    }
}