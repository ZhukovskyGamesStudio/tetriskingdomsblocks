using UnityEngine;

public class PieceView : MonoBehaviour {
    private PieceData _data;
    private Vector3 _startingPosition;
    private bool _isDragging;
    public static Vector3 DragShift;
    public static Vector2Int PieceMaxSize;

    [SerializeField]
    private BoxCollider _collider;

    [field: SerializeField]
    public Transform _markedCellsContainer { get; private set; }

    [field: SerializeField]
    public Transform _cellsContainer { get; private set; }

    private Vector3 _initialScale, _initialMarkedScale;

    private Vector2Int _currentPosOnField;

    private void Update() {
        if (_isDragging)
            OnDrag();
    }

    public void SetData(PieceData data, float initialScale = 1f) {
        _data = data;
        _startingPosition = _cellsContainer.position;
        var width = _data.Cells.GetLength(0);
        var height = _data.Cells.GetLength(1);
        var shift = CalculateShift();
        bool isMetaGame = GameManager.Instance == null;
        _collider.size = new Vector3(width, 0.3f, height);
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                bool cell = data.Cells[x, y];
                if (cell) {
                    var prefab = data.Type.CellPrefab;
                    CellView go = Instantiate(prefab, _cellsContainer);
                    go.SetSeed(data.CellGuids[x, y]);
                    // int cellSize = isMetaGame ? MetaManager.Instance.Cell:GameManager.Instance._markedCell;
                    var markedCell = isMetaGame ? MetaManager.Instance._markedCell : GameManager.Instance._markedCell;
                    var markCell = Instantiate(markedCell, _markedCellsContainer);
                    markCell.GetComponent<MeshRenderer>().material.color = new Color(data.Type.MarkCellColor.r, data.Type.MarkCellColor.g,
                        data.Type.MarkCellColor.b, 0.75f);
                    go.transform.localPosition = (new Vector3(x +0.5f, 0, y+0.5f) + shift) * GameManager.CELL_SIZE;
                    markCell.position = new Vector3(go.transform.position.x, _markedCellsContainer.position.y, go.transform.position.z);
                    go.transform.localScale *= Mathf.Clamp(GameManager.CELL_SIZE - 2, 1, 100000);
                }
            }
        }

        _initialScale = Vector3.one * initialScale;
        _initialMarkedScale = Vector3.one * initialScale;
        _cellsContainer.localScale = _initialScale;
        _markedCellsContainer.gameObject.SetActive(false);
    }

    private Vector3 CalculateShift() {
        var width = _data.Cells.GetLength(0);
        var height = _data.Cells.GetLength(1);

        return new Vector3(width/2f, 0, height / 2f) * -1;
    }

    public void OnStartDrag() {
        _isDragging = true;
        _cellsContainer.localScale = Vector3.one;
        _markedCellsContainer.localScale = Vector3.one;
        
        BaseManager cellManager = GameManager.Instance == null ? MetaManager.Instance : GameManager.Instance;
        //cellManager.SetCurPieceOffset(transform.position - cellManager.InputCoord());
        
         GameManager.PieceVerticalShift = Mathf.Abs(CalculateShift().z);
        PieceMaxSize = new(_data.Cells.GetLength(0), _data.Cells.GetLength(1));
     
    }

    public void OnDrag() {
        BaseManager cellManager = GameManager.Instance == null ? MetaManager.Instance : GameManager.Instance;
        int fieldSize = GameManager.Instance == null
            ? MetaManager.Instance.MainMetaConfig.FieldSize
            : GameManager.Instance.MainGameConfig.FieldSize;
        var targetMousePos = cellManager.ShiftedDragInputPos();// + cellManager.TotalDragOffset;

        targetMousePos.y = _cellsContainer.position.y;
        var curPos = cellManager.GetPosInCoord();
        _currentPosOnField = cellManager.GetPieceClampedCoordOnField();

        int maxClampFieldPositionZ = fieldSize - _data.Cells.GetLength(1) + 1;
        int maxClampFieldPositionX = fieldSize - _data.Cells.GetLength(0) + 1;

        bool isInsideField = _currentPosOnField.x >= 0 && _currentPosOnField.y >= 0 && _currentPosOnField.x < maxClampFieldPositionX &&
                             _currentPosOnField.y < maxClampFieldPositionZ;
        if (isInsideField) {
            Vector3 clampedPos = new Vector3(curPos.x, _cellsContainer.position.y, curPos.y);

            var clampedShiftedPos = clampedPos;

            if (CalculateShift().x % 1 == 0)
                clampedShiftedPos += Vector3.right / 2;

            if (CalculateShift().z % 1 == 0)
                clampedShiftedPos += Vector3.forward / 2;
            
            //TODO fix it - this is a hack to fix the offset of the piece
            var targetMarkedPos = new Vector3(clampedShiftedPos.x - 0.28f, FieldContainers.Instance.MarkedCellsVerticalAnchor.position.y, clampedShiftedPos.z + 0.05f);
            _markedCellsContainer.position = targetMarkedPos;
            
        }
        
        _markedCellsContainer.gameObject.SetActive(isInsideField);
        _cellsContainer.position = targetMousePos;
    }

    public void OnDrop() {
        if (!_isDragging) {
            return;
        }

        BaseManager cellManager = GameManager.Instance == null ? MetaManager.Instance : GameManager.Instance;
        _markedCellsContainer.gameObject.SetActive(false);
        _isDragging = false;
        if (cellManager.CanPlace(_data)) {
            cellManager.PlacePiece(_data, _currentPosOnField);

            Destroy(gameObject);
        } else {
            _cellsContainer.position = _startingPosition;
            _cellsContainer.localScale = _initialScale;
            _markedCellsContainer.localScale = _initialMarkedScale;
            _markedCellsContainer.gameObject.SetActive(false);
        }
    }

    private void OnMouseDown() {
        OnStartDrag();
    }

    private void OnMouseUp() {
        OnDrop();
    }
}