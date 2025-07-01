using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public class DragManager : MonoBehaviour
{
    /*public static DragManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }*/

    public static void LerpToFinal(Transform _cellsContainer, Vector3 _finalPos, Vector3 _finalScale)
    {
        _cellsContainer.position =
            Vector3.Lerp(_cellsContainer.position, _finalPos,
                Time.deltaTime * ConfigsManager.Instance.DragConfig.LerpSpeed);
        _cellsContainer.localScale = Vector3.Lerp(_cellsContainer.localScale, _finalScale,
            Time.deltaTime * ConfigsManager.Instance.DragConfig.LerpSpeed);
    }

    public static Vector3 CalculateShift(PieceData pieceData)
    {
        var width = pieceData.Cells.GetLength(0);
        var height = pieceData.Cells.GetLength(1);

        return new Vector3(width / 2f, 0, height / 2f) * -1;
    }

    public static void OnDragPiece(ref Vector2Int currentCoord, ref Vector3 finalPos, PieceData data,
        Transform markedCellsContainer)
    {
        FieldManager cellManager = GameFieldManager.Instance == null ? MetaFieldManager.Instance : GameFieldManager.Instance;
        var targetMousePos = cellManager.ShiftedDragInputPos();

        targetMousePos.y = ConfigsManager.Instance.DragConfig.HeightUnderField;
        currentCoord = cellManager.GetPosInCoord();

        bool canPlace = cellManager.CanPlace(data, currentCoord);
        if (canPlace)
        {
            Vector3 targetMarkedPos = new(currentCoord.x, FieldContainers.Instance.MarkedCellsVerticalAnchor.position.y,
                currentCoord.y);
            targetMarkedPos -= FieldManager.PieceCenterToCoordShift();
            markedCellsContainer.position = targetMarkedPos;
        }

        markedCellsContainer.gameObject.SetActive(canPlace);
        finalPos = targetMousePos;
    }

    public static void OnStartDrag(ref bool isDragging, PieceData _data,
        ref Vector2Int CurrentPieceMaxSize, ref Vector3 finalScale, Transform markedCellsContainer)
    {
        if (AdminManager.Instance.AdminToggle.isOn)
            return;

        isDragging = true;
        finalScale = Vector3.one;
        markedCellsContainer.localScale = Vector3.one;
        FieldManager.PieceVerticalShift = Mathf.Abs(DragManager.CalculateShift(_data).z);
        CurrentPieceMaxSize = new Vector2Int(_data.Cells.GetLength(0), _data.Cells.GetLength(1));
    }

    public static void OnDrop(ref bool isDragging, ref bool isLerpingDisabled, PieceData data,
        ref Transform markedCellsContainer, ref Vector3 finalScale,
        ref Vector3 finalPos, Vector2Int currentCoord, Vector3 initialScale, Vector3 initialMarkedScale,
        Vector3 startingPosition, PieceView _pieceGameObject)
    {
        if (!isDragging)
        {
            return;
        }

        FieldManager cellManager =
            GameFieldManager.Instance == null ? MetaFieldManager.Instance : GameFieldManager.Instance;
        markedCellsContainer.gameObject.SetActive(false);
        isDragging = false;
        if (cellManager.CanPlace(data, currentCoord))
        {
            isLerpingDisabled = true;
            finalPos = markedCellsContainer.position + ConfigsManager.Instance.DragConfig.HigherFieldShift * Vector3.up;
          //  Debug.Log("to place " + finalPos);
            _pieceGameObject.PlacePieceAsync(cellManager).Forget();
        }
        else if (GameFieldManager.Instance != null && GameFieldManager.Instance.AdditionalPieceContainerUnderPiece())
        {
            finalScale = initialScale;
            markedCellsContainer.localScale = initialMarkedScale;
            markedCellsContainer.gameObject.SetActive(false);
            GameFieldManager.Instance.SetPieceInAdditionalContainer(ref finalPos, _pieceGameObject);
        }
        else if (GameFieldManager.Instance != null && GameFieldManager.Instance._additionalPiecePrefab != null &&
                 GameFieldManager.Instance._additionalPiecePrefab == _pieceGameObject)
        {
            finalPos = GameFieldManager.Instance._additionalPieceContainer.position;
            finalScale = initialScale;
            markedCellsContainer.localScale = initialMarkedScale;
            markedCellsContainer.gameObject.SetActive(false);
        }
        else
        {
            //Debug.Log("to start");
            finalPos = startingPosition;
            finalScale = initialScale;
            markedCellsContainer.localScale = initialMarkedScale;
            markedCellsContainer.gameObject.SetActive(false);
        }
    }
}
