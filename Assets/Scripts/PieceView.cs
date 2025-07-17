using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class PieceView : MonoBehaviour {
    [SerializeField]
    private BoxCollider _collider;

    [field: SerializeField]
    private Transform _markedCellsContainer;

    public Transform _cellsContainer;

    public static Vector2Int CurrentPieceMaxSize;
    
    private Vector3 _initialScale, _initialMarkedScale;
    private Vector2Int _currentCoord;
    public PieceData Data;
    private Vector3 _startingPosition;
    private bool _isDragging;

    private Vector3 _finalPos, _finalScale;
    public CellView[,] _cells;
    private bool _isLerpingDisabled = false;

    private float _colliderSize = 1.65f;

    public void SetData(PieceData data, float initialScale = 1f) {
        Data = data;
        _startingPosition = _cellsContainer.position;
        var width = Data.Cells.GetLength(0);
        var height = Data.Cells.GetLength(1);
        var shift = DragManager.CalculateShift(Data);
        int maxSize = Mathf.Max(width, height);
        initialScale *= 1f / Mathf.Sqrt(maxSize);

        _cells = new CellView[width, height];
        _collider.size = new Vector3(_colliderSize, 0.2f, _colliderSize);
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                bool cell = data.Cells[x, y];
                if (!cell) {
                    continue;
                }

                CreateCellAndMarkedCellView(data, x, y, shift);
            }
        }

        _initialScale = Vector3.one * initialScale;
        _initialMarkedScale = Vector3.one * initialScale;
        _cellsContainer.localScale = _initialScale;
        _finalScale = _initialScale;
        _finalPos = _startingPosition;
        _markedCellsContainer.gameObject.SetActive(false);
    }

    private void CreateCellAndMarkedCellView(PieceData data, int x, int y, Vector3 shift) {
        var prefab = PiecesViewTable.Instance.CellsViewList.GetCellByType(data.Type.CellType);
        CellView go = Instantiate(prefab, _cellsContainer);
        go.SetSeed(data.CellGuids[x, y]);
        var markedCell = PiecesViewTable.Instance.MarkedCell;
        var markCell = Instantiate(markedCell, _markedCellsContainer);
        
        markCell.GetComponent<MeshRenderer>().material.color = new Color(data.Type.MarkCellColor.r, data.Type.MarkCellColor.g,
            data.Type.MarkCellColor.b, 0.75f);
        go.transform.localPosition = (new Vector3(x + 0.5f, 0, y + 0.5f) + shift) * FieldUtils.CellSize;
        markCell.position = new Vector3(go.transform.position.x, _markedCellsContainer.position.y, go.transform.position.z);
        go.transform.localScale *= Mathf.Clamp(FieldUtils.CellSize - 2, 1, 100000);
        _cells[x, y] = go;
    }

    public async UniTask AppearAsync() {
        _isLerpingDisabled = true;
        Vector3 finScale = _cellsContainer.localScale;
        _cellsContainer.localScale = Vector3.zero;
        await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
        await DOTween.Sequence().Append(_cellsContainer.DOScale(finScale * 1.1f, 0.2f)).Append(_cellsContainer.DOScale(finScale, 0.2f))
            .AsyncWaitForCompletion();
        _isLerpingDisabled = false;
    }

    private void Update() {
        if (_isDragging) {
            DragManager.OnDragPiece(ref _currentCoord, ref _finalPos, Data, _markedCellsContainer);
        }

        if (!_isLerpingDisabled)
            DragManager.LerpToFinal(_cellsContainer, _finalPos, _finalScale);
    }

   /* private void LerpToFinal() {
        if (_isLerpingDisabled) {
            return;
        }

        _cellsContainer.position =
            Vector3.Lerp(_cellsContainer.position, _finalPos, Time.deltaTime * ConfigsManager.Instance.DragConfig.LerpSpeed);
        _cellsContainer.localScale = Vector3.Lerp(_cellsContainer.localScale, _finalScale,
            Time.deltaTime * ConfigsManager.Instance.DragConfig.LerpSpeed);
    }*/

  /*  private Vector3 CalculateShift() {
        var width = _data.Cells.GetLength(0);
        var height = _data.Cells.GetLength(1);

        return new Vector3(width / 2f, 0, height / 2f) * -1;
    }*/

    public void OnStartDrag() {
        DragManager.OnStartDrag(ref _isDragging,Data,ref CurrentPieceMaxSize,ref _finalScale,_markedCellsContainer, this);
    }

    private void OnDrag() {
        DragManager.OnDragPiece(ref _currentCoord, ref _finalPos, Data, _markedCellsContainer);
    }

    public void OnDrop() {
        DragManager.OnDrop(ref _isDragging, ref _isLerpingDisabled, Data, ref _markedCellsContainer,ref _finalScale, ref _finalPos,
            _currentCoord,_initialScale, _initialMarkedScale, _startingPosition, this);
    }

    public async UniTask PlacePieceAsync(FieldManager cellManager) {
        var cnfg = ConfigsManager.Instance.DragConfig;
        var distance = (_finalPos - _cellsContainer.position).magnitude;
        var duration = distance / cnfg.MoveBeforeDropAnimationSpeed;
        await DOTween.Sequence().Append(_cellsContainer.DOMove(_finalPos, duration)).SetEase(cnfg.MoveBeforeDropAnimationCurve)
            .AsyncWaitForCompletion();
        cellManager.PlacePiece(Data, _currentCoord, _cells, _cellsContainer);
        Destroy(gameObject);
    }

    private void OnMouseDown() {
        OnStartDrag();
    }

    private void OnMouseUp() {
        OnDrop();
    }
}