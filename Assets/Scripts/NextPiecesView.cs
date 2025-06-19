using System.Collections.Generic;
using UnityEngine;

public class NextPiecesView : MonoBehaviour, IResetable {
    public static NextPiecesView Instance;

    [SerializeField]
    private List<Transform> _piecesContainers;

    [SerializeField]
    private float _piecesScale = 0.4f;

    private void Awake() {
        Instance = this;
    }

    public void SetData(PieceData nextPiece) {
        DestroyPieces();

        if (_piecesContainers.Count == 0) {
            Debug.LogWarning("NextPiecesView: No containers available for the pieces.");
            return;
        }

        SetData(new List<PieceData>() { nextPiece });
    }
    
    public void SetData(List<PieceData> nextPieces) {
        DestroyPieces();

        for (int i = 0; i < nextPieces.Count; i++) {
            if (i >= _piecesContainers.Count) {
                Debug.LogWarning("NextPiecesView: Not enough containers for the pieces.");
                break;
            }

            Transform container = _piecesContainers[i];
            PieceView go = Instantiate(PiecesViewTable.Instance.PieceViewPrefab, container);
            go.SetData(nextPieces[i], _piecesScale);
        }
    }

    private void DestroyPieces() {
        foreach (Transform container in _piecesContainers) {
            foreach (Transform child in container) {
                Destroy(child.gameObject);
            }
        }
    }

    public void Reset() {
        DestroyPieces();
    }
}