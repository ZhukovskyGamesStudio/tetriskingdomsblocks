using ScriptableObjects.Configs;
using UnityEngine;

public class PiecesViewTable : MonoBehaviour {
    public static PiecesViewTable Instance;

    [field: SerializeField]
    public PieceView PieceViewPrefab;

    [field: SerializeField]
    public CellsViewList CellsViewList;

    [field: SerializeField]
    public Transform MarkedCell { get; protected set; }

    [field: SerializeField]
    public CellsList CellsList { get; private set; }
    
    [field: SerializeField]
    public FigureFormConfig[] FigureForms { get; protected set; }

    private void Awake() {
        Instance = this;
    }
}