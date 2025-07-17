using UnityEngine;
using Cysharp.Threading.Tasks;

public class DragManager : MonoBehaviour {
    public static void LerpToFinal(Transform _cellsContainer, Vector3 _finalPos, Vector3 _finalScale) {
        _cellsContainer.position =
            Vector3.Lerp(_cellsContainer.position, _finalPos, Time.deltaTime * ConfigsManager.Instance.DragConfig.LerpSpeed);
        _cellsContainer.localScale = Vector3.Lerp(_cellsContainer.localScale, _finalScale,
            Time.deltaTime * ConfigsManager.Instance.DragConfig.LerpSpeed);
    }

    public static Vector3 CalculateShift(PieceData pieceData) {
        var width = pieceData.Cells.GetLength(0);
        var height = pieceData.Cells.GetLength(1);

        return new Vector3(width / 2f, 0, height / 2f) * -1;
    }

    public static void OnDragPiece(ref Vector2Int currentCoord, ref Vector3 finalPos, PieceData data, Transform markedCellsContainer) {
        FieldManager cellManager = GameFieldManager.Instance == null ? MetaFieldManager.Instance : GameFieldManager.Instance;
        var targetMousePos = cellManager.ShiftedDragInputPos();

        targetMousePos.y = ConfigsManager.Instance.DragConfig.HeightUnderField;
        currentCoord = cellManager.GetPosInCoord();

        bool canPlace = cellManager.CanPlace(data, currentCoord);
        if (canPlace) {
            Vector3 targetMarkedPos = new(currentCoord.x, FieldContainers.Instance.MarkedCellsVerticalAnchor.position.y, currentCoord.y);
            targetMarkedPos -= FieldManager.PieceCenterToCoordShift();
            markedCellsContainer.position = targetMarkedPos;
        }

        markedCellsContainer.gameObject.SetActive(canPlace);
        finalPos = targetMousePos;
    }

    public static void ReplaceMaterialInChildren(Transform parent, Material newMaterial) {
        foreach (var meshRenderer in parent.GetComponentsInChildren<MeshRenderer>()) {
            if (meshRenderer.CompareTag("Marked")) {
                continue;
            }

            meshRenderer.material = newMaterial;
        }
    }

    public static void OnStartDrag(ref bool isDragging, PieceData _data, ref Vector2Int CurrentPieceMaxSize, ref Vector3 finalScale,
        Transform markedCellsContainer, PieceView _pieceGameObject) {
        if (AdminManager.Instance.AdminToggle.isOn || (UltaManager.Instance != null && UltaManager.Instance._ultimateIsActive))
            return;

        if (BoostersManager.Instance != null) {
            if (BoostersManager.Instance.RotationState == BoostersManager.RotateBoosterStates.RotatePiece ||
                (BoostersManager.Instance.RotationState == BoostersManager.RotateBoosterStates.SelectPiece &&
                 GameFieldManager.Instance.AdditionalPiecePrefab == _pieceGameObject))
                return;
            else if (BoostersManager.Instance.RotationState == BoostersManager.RotateBoosterStates.SelectPiece) {
                BoostersManager.Instance.SelectPieceToRotate(_pieceGameObject);
                return;
            }
        }

        isDragging = true;
        finalScale = Vector3.one;
        markedCellsContainer.localScale = Vector3.one;

        ReplaceMaterialInChildren(_pieceGameObject.transform, MainManager.Instance._mainConfig._priorityMaterial);

        FieldManager.PieceVerticalShift = Mathf.Abs(DragManager.CalculateShift(_data).z);
        CurrentPieceMaxSize = new Vector2Int(_data.Cells.GetLength(0), _data.Cells.GetLength(1));
    }

    public static void OnDrop(ref bool isDragging, ref bool isLerpingDisabled, PieceData data, ref Transform markedCellsContainer,
        ref Vector3 finalScale, ref Vector3 finalPos, Vector2Int currentCoord, Vector3 initialScale, Vector3 initialMarkedScale,
        Vector3 startingPosition, PieceView _pieceGameObject) {
        if (!isDragging) {
            return;
        }

        FieldManager cellManager = GameFieldManager.Instance == null ? MetaFieldManager.Instance : GameFieldManager.Instance;
        markedCellsContainer.gameObject.SetActive(false);
        isDragging = false;
        if (cellManager.CanPlace(data, currentCoord)) {
            isLerpingDisabled = true;
            finalPos = markedCellsContainer.position + ConfigsManager.Instance.DragConfig.HigherFieldShift * Vector3.up;
            //  Debug.Log("to place " + finalPos);
            _pieceGameObject.PlacePieceAsync(cellManager).Forget();
        } else if (GameFieldManager.Instance != null && GameFieldManager.Instance.AdditionalPieceContainerUnderPiece() &&
                   data.Type.CellType != CellType.Dynamite) {
            finalScale = initialScale;
            markedCellsContainer.localScale = initialMarkedScale;
            markedCellsContainer.gameObject.SetActive(false);
            GameFieldManager.Instance.SetPieceInAdditionalContainer(ref finalPos, _pieceGameObject);
        } else if (GameFieldManager.Instance != null && GameFieldManager.Instance.AdditionalPiecePrefab != null &&
                   GameFieldManager.Instance.AdditionalPiecePrefab == _pieceGameObject && data.Type.CellType != CellType.Dynamite) {
            finalPos = GameFieldManager.Instance.AdditionalPieceContainer.position;
            finalScale = initialScale;
            markedCellsContainer.localScale = initialMarkedScale;
            markedCellsContainer.gameObject.SetActive(false);
        } else {
            if (MetaFieldManager.Instance != null) {
                MetaFieldManager.Instance.SetCurrentPiece();
                // MetaFieldManager.Instance.SaveInventory();
            }

            //Debug.Log("to start");
            finalPos = startingPosition;
            finalScale = initialScale;
            markedCellsContainer.localScale = initialMarkedScale;
            markedCellsContainer.gameObject.SetActive(false);
        }

        ReplaceMaterialInChildren(_pieceGameObject.transform, MainManager.Instance._mainConfig._normal);
    }
}