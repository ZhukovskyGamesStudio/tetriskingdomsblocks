using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class PieceView : MonoBehaviour {
    [SerializeField]
    private BoxCollider _collider;

    [field: SerializeField]
    private Transform _markedCellsContainer;

    [field: SerializeField]
    private Transform _cellsContainer;

    public static Vector2Int PieceMaxSize;
    private Vector3 _initialScale, _initialMarkedScale;
    private Vector2Int _currentCoord;
    private PieceData _data;
    private Vector3 _startingPosition;
    private bool _isDragging;

    private Vector3 _finalPos, _finalScale;
    private CellView[,] _cells;
    private bool _isLerpingDisabled = false;
    
    private void Update() {
        if (_isDragging) {
            OnDrag();
        }

        LerpToFinal();
    }

    public void SetData(PieceData data, float initialScale = 1f) {
      
        _data = data;
        _startingPosition = _cellsContainer.position;
        var width = _data.Cells.GetLength(0);
        var height = _data.Cells.GetLength(1);
        var shift = CalculateShift();
        int maxSize = Mathf.Max(width, height);
        initialScale *= 1f / Mathf.Sqrt(maxSize);

        _cells = new CellView[width, height];
        bool isMetaGame = GameManager.Instance == null;
        _collider.size = new Vector3(width * initialScale, 0.3f, height * initialScale);
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
                    go.transform.localPosition = (new Vector3(x + 0.5f, 0, y + 0.5f) + shift) * GameManager.CELL_SIZE;
                    markCell.position = new Vector3(go.transform.position.x, _markedCellsContainer.position.y, go.transform.position.z);
                    go.transform.localScale *= Mathf.Clamp(GameManager.CELL_SIZE - 2, 1, 100000);
                    _cells[x, y] = go;
                }
            }
        }

        _initialScale = Vector3.one * initialScale;
        _initialMarkedScale = Vector3.one * initialScale;
        _cellsContainer.localScale = _initialScale;
        _finalScale = _initialScale;
        _finalPos = _startingPosition;
        _markedCellsContainer.gameObject.SetActive(false);
    }

    public async UniTask AppearAsync() {
        _isLerpingDisabled = true;
        Vector3 finScale = _cellsContainer.localScale;
        _cellsContainer.localScale = Vector3.zero;
        await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
        await DOTween.Sequence()
            .Append(_cellsContainer.DOScale(finScale*1.1f, 0.2f))
            .Append(_cellsContainer.DOScale(finScale, 0.2f))
            .AsyncWaitForCompletion();
        _isLerpingDisabled = false;
    }
    
    private void LerpToFinal() {
        if (_isLerpingDisabled) {
            return;
        }
        _cellsContainer.position =
            Vector3.Lerp(_cellsContainer.position, _finalPos, Time.deltaTime * ConfigsManager.Instance.DragConfig.LerpSpeed);
        _cellsContainer.localScale =
            Vector3.Lerp(_cellsContainer.localScale, _finalScale, Time.deltaTime * ConfigsManager.Instance.DragConfig.LerpSpeed);
    }

    private Vector3 CalculateShift() {
        var width = _data.Cells.GetLength(0);
        var height = _data.Cells.GetLength(1);

        return new Vector3(width / 2f, 0, height / 2f) * -1;
    }

    public void OnStartDrag() {
        
        if(AdminManager.Instance.AdminToggle.isOn)return;
        
        _isDragging = true;
        _finalScale = Vector3.one;
        _markedCellsContainer.localScale = Vector3.one;
        GameManager.PieceVerticalShift = Mathf.Abs(CalculateShift().z);
        PieceMaxSize = new(_data.Cells.GetLength(0), _data.Cells.GetLength(1));
    }

    public void OnDrag() {
        BaseManager cellManager = GameManager.Instance == null ? MetaManager.Instance : GameManager.Instance;
        var targetMousePos = cellManager.ShiftedDragInputPos();
        targetMousePos.y = _cellsContainer.position.y;

        _currentCoord = cellManager.GetPosInCoord();
        //ebug.Log(_currentCoord);

        bool canPlace = cellManager.CanPlace(_data, _currentCoord);
        if (canPlace) {
            Vector3 targetMarkedPos =
                new Vector3(_currentCoord.x, FieldContainers.Instance.MarkedCellsVerticalAnchor.position.y, _currentCoord.y);
            targetMarkedPos -= GameManager.PieceCenterToCoordShift();
            _markedCellsContainer.position = targetMarkedPos;
        }

        _markedCellsContainer.gameObject.SetActive(canPlace);
        _finalPos = targetMousePos;
    }

    public void OnDrop() {
        if (!_isDragging) {
            return;
        }

        BaseManager cellManager = GameManager.Instance == null ? MetaManager.Instance : GameManager.Instance;
        _markedCellsContainer.gameObject.SetActive(false);
        _isDragging = false;
        if (cellManager.CanPlace(_data, _currentCoord)) {
            _isLerpingDisabled = true;
            _finalPos = _markedCellsContainer.position + ConfigsManager.Instance.DragConfig.HigherFieldShift * Vector3.up;
            PlacePieceAsync(cellManager).Forget();
        } else {
            _finalPos = _startingPosition;
            _finalScale = _initialScale;
            _markedCellsContainer.localScale = _initialMarkedScale;
            _markedCellsContainer.gameObject.SetActive(false);
        }
    }

    private async UniTask PlacePieceAsync( BaseManager cellManager) {
        await DOTween.Sequence().Append(_cellsContainer.DOMove(_finalPos, 0.2f)).AsyncWaitForCompletion();
        cellManager.PlacePiece(_data, _currentCoord,_cells,_cellsContainer);
        Destroy(gameObject);
    }

    private void OnMouseDown() {
        OnStartDrag();
    }

    private void OnMouseUp() {
        OnDrop();
    }
}