using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CellsManager : MonoBehaviour
{
    public const int CELL_SIZE = 1;
    protected CellType[,] _field;
    protected CellView[,] _cells;
    private Tween _currentTween;
    public float[] FiguresChanceToSpawn { get; protected set; }
    public float _screenRatio { get; protected set; }
    [HideInInspector] public Vector3 CusorToCellOffset;

    [HideInInspector] public List<CellTypeInfo> _currentCellsToSpawn { get; protected set; }
    public float[] CellsChanceToSpawn { get; protected set; }
    
    
    [HideInInspector]
    public Vector3 ScreenToWorldPoint => Physics.Raycast(_mainCamera.ScreenPointToRay(Input.mousePosition),
        out RaycastHit hit, Mathf.Infinity, _targetMasks)
        ? hit.point
        : Vector3.zero;

    [HideInInspector]
    public Vector3 TouchToWorldPoint => Physics.Raycast(_mainCamera.ScreenPointToRay(Input.GetTouch(0).position),
        out RaycastHit hit, Mathf.Infinity, _targetMasks)
        ? hit.point
        : Vector3.zero;

    [field: SerializeField] public Transform _markedCell { get; protected set; }

    [SerializeField] protected Camera _mainCamera;

    [SerializeField] protected Transform _fieldContainer;
    public Transform CameraContainer;
    [SerializeField] protected LayerMask _targetMasks;
    [SerializeField] private Transform _fieldStart, _fieldEnd;
    [field: SerializeField] public FigureFormConfig[] FigureFormsConfig { get; protected set; }

    protected virtual void Start()
    {
        _screenRatio = (float)Screen.width / Screen.height;
        CameraContainer.position = new Vector3(CameraContainer.position.x,
            CameraContainer.position.y / (_screenRatio / 0.486f), CameraContainer.position.z);

        Application.targetFrameRate = 144;
    }
protected void CalculateFiguresSpawnChances()
    {
        float lastChance = 0;
        FiguresChanceToSpawn = new float[FigureFormsConfig.Length];
        for (int i = 0; i < FigureFormsConfig.Length; i++)
        {
            lastChance += FigureFormsConfig[i].Cost;
            FiguresChanceToSpawn[i] = lastChance;
        }
    }
    public bool CanPlace(PieceData data)
    {
        Vector2Int pos = GetPieceClampedPosOnField();
        return CanPlace(data, pos);
    }

    public Vector2Int GetPosOnField()
    {
        Vector3 coord = GetCoord() + CusorToCellOffset;

        if (PieceView.PieceMaxSize.x % 2 == 0)
            coord += Vector3.left / 2f;

        if (PieceView.PieceMaxSize.y % 2 == 0)
            coord += Vector3.back / 2f;

        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(coord.x) / CELL_SIZE, Mathf.RoundToInt(coord.z) / CELL_SIZE);
        return pos;
    }

    private Vector3 GetCoord()
    {
        return Input.touchCount == 0 ? ScreenToWorldPoint : TouchToWorldPoint;
    }

    public Vector2Int GetPieceClampedPosOnField()
    {
        Vector3 coord = GetCoord() + CusorToCellOffset;
        coord += new Vector3(PieceView.DragShift.x, 0, PieceView.DragShift.z) + Vector3.forward;

        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(coord.x) / CELL_SIZE, Mathf.RoundToInt(coord.z) / CELL_SIZE);
        pos -= new Vector2Int((int)_fieldStart.position.x, (int)_fieldStart.position.z);
        return pos;
    }

    public bool CanPlace(PieceData data, Vector2Int pos)
    {
        if (pos.x < 0 || pos.y < 0)
            return false;

        if (pos.x + data.Cells.GetLength(0) - 1 >= _field.GetLength(0))
            return false;

        if (pos.y + data.Cells.GetLength(1) - 1 >= _field.GetLength(1))
            return false;

        for (int x = 0; x < data.Cells.GetLength(0); x++)
        {
            for (int y = 0; y < data.Cells.GetLength(1); y++)
            {
                if (data.Cells[x, y] && _field[pos.x + x, pos.y + y] != CellType.Empty)
                    return false;
            }
        }

        return true;
    }

    protected virtual void PlacePiece(PieceData pieceData, Vector2Int pos, int fieldSize)
    {
        for (int x = 0; x < pieceData.Cells.GetLength(0); x++)
        {
            for (int y = 0; y < pieceData.Cells.GetLength(1); y++)
            {
                if (!pieceData.Cells[x, y])
                {
                    continue;
                }

                var place = new Vector2Int((int)Mathf.Clamp(pos.x + x, 0, fieldSize),
                    (int)Mathf.Clamp(pos.y + y, 0, fieldSize));
                var go = Instantiate(pieceData.Type.CellPrefab, _fieldContainer);
                go.transform.localPosition = new Vector3(place.x, -0.45f, place.y);
                _field[place.x, place.y] = pieceData.Type.CellType;
                _cells[place.x, place.y] = go;
                go.GetComponent<CellView>().PlaceCellOnField();
                SpawnResourceFx(pieceData, place, go);
            }
        }

        ShakeCamera();
    }
    
    public virtual void PlacePiece(PieceData pieceData)
    {
    }

    protected virtual void SpawnResourceFx(PieceData pieceData, Vector2Int place, CellView go)
    {
    }
    
    protected void ShakeCamera()
    {
        _currentTween.Kill();
        _currentTween = DOTween.Sequence()
            .Append(CameraContainer.transform.DOMoveY(CameraContainer.transform.position.y * 1.02f, 0.12f))
            .Append(CameraContainer.transform.DOMoveY(10f / (_screenRatio / 0.486f), 0.08f));
    }

    public void PlaceCell()
    {
    }
}