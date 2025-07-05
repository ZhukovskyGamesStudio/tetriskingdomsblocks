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
    public PieceData _data { get; private set; }
    private Vector3 _startingPosition;
    private bool _isDragging;

    private Vector3 _finalPos, _finalScale;
    private CellView[,] _cells;
    private bool _isLerpingDisabled = false;

    public void SetData(PieceData data, float initialScale = 1f) {
        _data = data;
        _startingPosition = _cellsContainer.position;
        var width = _data.Cells.GetLength(0);
        var height = _data.Cells.GetLength(1);
        var shift = DragManager.CalculateShift(_data);
        int maxSize = Mathf.Max(width, height);
        initialScale *= 1f / Mathf.Sqrt(maxSize);

        _cells = new CellView[width, height];
        _collider.size = new Vector3(width * initialScale, 0.3f, height * initialScale);
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
            DragManager.OnDragPiece(ref _currentCoord, ref _finalPos, _data, _markedCellsContainer);
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
        DragManager.OnStartDrag(ref _isDragging,_data,ref CurrentPieceMaxSize,ref _finalScale,_markedCellsContainer);
       /* if (AdminManager.Instance.AdminToggle.isOn) {
            return;
        }

        _isDragging = true;
        _finalScale = Vector3.one;
        _markedCellsContainer.localScale = Vector3.one;
        BaseManager.PieceVerticalShift = Mathf.Abs(DragManager.CalculateShift(_data).z);
        CurrentPieceMaxSize = new Vector2Int(_data.Cells.GetLength(0), _data.Cells.GetLength(1));*/
    }

    private void OnDrag() {
        /*BaseManager cellManager = GameManager.Instance == null ? MetaManager.Instance : GameManager.Instance;
        var targetMousePos = cellManager.ShiftedDragInputPos();

        targetMousePos.y = ConfigsManager.Instance.DragConfig.HeightUnderField;
        _currentCoord = cellManager.GetPosInCoord();

        bool canPlace = cellManager.CanPlace(_data, _currentCoord);
        if (canPlace) {
            Vector3 targetMarkedPos = new(_currentCoord.x, FieldContainers.Instance.MarkedCellsVerticalAnchor.position.y, _currentCoord.y);
            targetMarkedPos -= BaseManager.PieceCenterToCoordShift();
            _markedCellsContainer.position = targetMarkedPos;
        }

        _markedCellsContainer.gameObject.SetActive(canPlace);
        _finalPos = targetMousePos;*/
        
        DragManager.OnDragPiece(ref _currentCoord, ref _finalPos, _data, _markedCellsContainer);
    }

    private void OnDrop() {
        DragManager.OnDrop(ref _isDragging, ref _isLerpingDisabled, _data, ref _markedCellsContainer,ref _finalScale, ref _finalPos,
            _currentCoord,_initialScale, _initialMarkedScale, _startingPosition, this);
       
       /* if (!_isDragging) {
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
        } */
    }

    public async UniTask PlacePieceAsync(FieldManager cellManager) {
        await DOTween.Sequence().Append(_cellsContainer.DOMove(_finalPos, ConfigsManager.Instance.DragConfig.DropPieceAnimationDuration))
            .AsyncWaitForCompletion();
        cellManager.PlacePiece(_data, _currentCoord, _cells, _cellsContainer);
        Destroy(gameObject);
    }

    private void OnMouseDown() {
        OnStartDrag();
    }

    private void OnMouseUp() {
        OnDrop();
    }
}