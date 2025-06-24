using DefaultNamespace;
using UnityEngine;

public class PiecesViewTable : MonoBehaviour {
    [field: SerializeField]
    public PieceView PieceViewPrefab;  
    [field: SerializeField]
    public CellsViewList CellsViewList;

    public static PiecesViewTable Instance;

    private void Awake() {
        Instance = this;
    }
}