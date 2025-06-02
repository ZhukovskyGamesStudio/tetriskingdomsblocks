using System;
using System.Collections.Generic;
//using Lofelt.NiceVibrations;
using UnityEngine;


public class PieceView : MonoBehaviour {
    private PieceData _data;
    private Vector3 _startingPosition;
    private bool _isDragging;
    public static Vector3 DragShift;
    public static Vector2Int PieceMaxSize;
    [field:SerializeField]
    public Transform _markedCellsContainer { get; private set; }
    [field:SerializeField]
    public Transform _cellsContainer { get; private set; }
    private List<CellView> _cells = new List<CellView>();

    private void Update()
    {
        if(_isDragging)
            OnDrag();
    }
    public void SetData(PieceData data) {
        _data = data;
        _startingPosition = _cellsContainer.position;
        var width = _data.Cells.GetLength(0);
        var height = _data.Cells.GetLength(1);
        var shift = CalculateShift();
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                bool cell = data.Cells[x, y];
                if (cell) {
                  //  var prefab = PiecesViewTable.Instance.GetCellByType(data.Type.cellType);
                  var prefab = data.Type.cellPrefab;
                    var go = Instantiate(prefab, _cellsContainer);
                    var markCell = Instantiate(GameManager.Instance._markedCell, _markedCellsContainer);
                    markCell.GetComponent<MeshRenderer>().material.color = new Color(data.Type.MarkCellColor.r, data.Type.MarkCellColor.g, data.Type.MarkCellColor.b,0.5f);
                    go.transform.localPosition = (new Vector3(x, 0, y) + shift + Vector3.one / 2f) * GameManager.CELL_SIZE;
                    markCell.position = new Vector3(go.transform.position.x, go.transform.position.y-0.5f, go.transform.position.z);
                    go.transform.localScale *= Mathf.Clamp(GameManager.CELL_SIZE - 2, 1, 100000);
                    _cells.Add(go);
                }
            }
        }
    }

    private Vector3 CalculateShift() {
        var width = _data.Cells.GetLength(0);
        var height = _data.Cells.GetLength(1);
       
        return new Vector3(width / 2f, 0, height / 2f) * -1;
    }

    public void OnStartDrag() {
        _isDragging = true;
        DragShift = CalculateShift() + Vector3.one/2;
        PieceMaxSize = new(_data.Cells.GetLength(0), _data.Cells.GetLength(1));
    }

    public void OnDrag() {
        var targetMousePos = GameManager.Instance.ScreenToWorldPoint;
        if (Input.touchCount > 0)
        {
            targetMousePos = GameManager.Instance.TouchToWorldPoint;
        }
        targetMousePos.y = _cellsContainer.position.y;
        var posOnField = GameManager.Instance.GetPosOnField();
        var clampedPosOnField = GameManager.Instance.GetPieceClampedPosOnField();
        var targetPos = targetMousePos;

        int maxClampFieldPositionZ = GameManager.Instance.MainGameConfig.fieldSize - _data.Cells.GetLength(1)+1;
        int maxClampFieldPositionX = GameManager.Instance.MainGameConfig.fieldSize - _data.Cells.GetLength(0)+1;

        if (clampedPosOnField.x >= 0 && clampedPosOnField.y >= 0 && clampedPosOnField.x < maxClampFieldPositionX && clampedPosOnField.y  < maxClampFieldPositionZ)
        {
            Vector3 clampedPos = new Vector3(posOnField.x, _cellsContainer.position.y, posOnField.y);
            
            var clampedShiftedPos = clampedPos;
            if (CalculateShift().x % 1 == 0) {
                clampedShiftedPos+= Vector3.right/2;
            }
            if (CalculateShift().z % 1 == 0) {
                clampedShiftedPos+= Vector3.forward/2;
            }
          
           // targetPos = Vector3.Lerp(targetMousePos, clampedShiftedPos, 0.95f);
           targetPos = clampedShiftedPos;
            targetPos = new Vector3(targetPos.x,_cellsContainer.position.y-1.05f,targetPos.z+0.05f) ;
            _markedCellsContainer.gameObject.SetActive(true);
            _markedCellsContainer.position = targetPos;
        }
        else
        {
            _markedCellsContainer.gameObject.SetActive(false);
        }
        
        _cellsContainer.position = targetMousePos;
      //  transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);
    }

    public void OnDrop() {
        if (!_isDragging) {
            return;
        }
        _markedCellsContainer.gameObject.SetActive(false);
        _isDragging = false;
        if (GameManager.Instance.CanPlace(_data)) {
            GameManager.Instance.PlacePiece(_data);
            
           // HapticPatterns.PlayEmphasis(1.0f, 0.0f);
            Destroy(gameObject);
        } else {
            _cellsContainer.position = _startingPosition;
        }
    }

  /*  private void OnMouseDrag() {
        OnDrag();
    }*/
    

    private void OnMouseDown() {
        OnStartDrag();
    }

    private void OnMouseUp() {
        OnDrop();
    }
}