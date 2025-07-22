using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class BoostersManager : MonoBehaviour {
    [SerializeField]
    private TMP_Text _randomFieldCountText;

    [SerializeField]
    private TMP_Text _dinamyteCountText;

    [SerializeField]
    private TMP_Text _hummerCountText;

    [SerializeField]
    private TMP_Text _rotatePieceCountText;

    [SerializeField]
    private Button _randomFieldButton;

    [SerializeField]
    private Button _dinamyteButton;

    [SerializeField]
    private Button _hummerButton;

    [SerializeField]
    private Button _rotatePieceButton;

    [field: SerializeField]
    public Transform _dynamiteContainer { get; private set; }

    [SerializeField]
    private Image _dynamiteImageButton;

    [SerializeField]
    private Image _hummerImageButton;

    [SerializeField]
    private Image _randomImageButton;

    [SerializeField]
    private Image _rotateImageButton;

    [SerializeField]
    private Button _rotatePieceCancelButton;

    [SerializeField]
    private Button _rotatePieceAcceptButton;

    [SerializeField]
    private Transform _rotatePieceSelectContainer;

    [SerializeField]
    private Transform _rotatePieceButtonsContainer;

    [SerializeField]
    private ParticleSystem _dynamiteBoomFx;

    private Vector2 _lastInputPosition;

    public static BoostersManager Instance;
    public Action OnBoosterEndedWorking;

    private bool _dynamiteExploding;
    private bool _dynamiteCancelled;

    public RotateBoosterStates RotationState;
    private int _initialRotationY;
    private int _currentRotationY;
    private Vector3 _rotatingPiecePosition;
    private bool _rotationChanged;
    private PieceView _currentPieceView;

    public enum RotateBoosterStates {
        LockRotate,
        SelectPiece,
        RotatePiece
    }

    public void SetBoosterButtons() {
        var lockSprite = ConfigsManager.Instance.BoostersConfig.LockBoosterSprite;
        int curLevel = StorageManager.GameDataMain.CurMaxLevel;
        if (!AdminManager.Instance.IsInfiniteBoosters) {
            if (ConfigsManager.Instance.BoostersConfig.RotateUnlockLevel > curLevel) {
                _rotateImageButton.sprite = lockSprite;
                _rotatePieceButton.enabled = false;
                _rotatePieceCountText.text = (ConfigsManager.Instance.BoostersConfig.RotateUnlockLevel + 1) + "lvl";
            }

            if (ConfigsManager.Instance.BoostersConfig.DynamiteUnlockLevel > curLevel) {
                _dynamiteImageButton.sprite = lockSprite;
                _dinamyteButton.enabled = false;
                _dinamyteCountText.text = (ConfigsManager.Instance.BoostersConfig.DynamiteUnlockLevel + 1) + "lvl";
            }

            if (ConfigsManager.Instance.BoostersConfig.HummerUnlockLevel > curLevel) {
                _hummerImageButton.sprite = lockSprite;
                _hummerButton.enabled = false;
                _hummerCountText.text = (ConfigsManager.Instance.BoostersConfig.HummerUnlockLevel + 1) + "lvl";
            }

            if (ConfigsManager.Instance.BoostersConfig.RandomUnlockLevel > curLevel) {
                _randomImageButton.sprite = lockSprite;
                _randomFieldButton.enabled = false;
                _randomFieldCountText.text = (ConfigsManager.Instance.BoostersConfig.RandomUnlockLevel + 1) + "lvl";
            }
        }
    }

    private void Awake() {
        Instance = this;
    }

    public void RotatePieceLeft() {
        _currentRotationY -= 90;
        if (_currentRotationY < 0) _currentRotationY += 360;
        _currentPieceView.transform.rotation = Quaternion.Euler(0, _currentRotationY, 0);
    }

    public void RotatePieceRight() {
        _currentRotationY += 90;
        _currentRotationY %= 360;
        _currentPieceView.transform.rotation = Quaternion.Euler(0, _currentRotationY, 0);
    }

    public void CancelRotation() {
        _currentPieceView.transform.rotation = Quaternion.Euler(0, _initialRotationY, 0);
        _currentRotationY = _initialRotationY;
        ApplyRotation();
    }

    public void ApplyRotation() {
        if (_currentRotationY == _initialRotationY)
            return;

        RotateFigure(_currentRotationY - _initialRotationY);

        RotationState = RotateBoosterStates.LockRotate;
        _rotatePieceButtonsContainer.gameObject.SetActive(false);
        _currentPieceView = null;

        StorageManager.GameDataMain.RotatePieceCount--;
        _rotatePieceCountText.text = StorageManager.GameDataMain.RotatePieceCount.ToString();
    }

    private void RotateFigure(int degrees) {
        if (degrees != 90 && degrees != 180 && degrees != 270)
            return;

        int rotations = degrees / 90;
        bool[,] result = _currentPieceView.Data.Cells;

        for (int r = 0; r < rotations; r++) {
            result = Rotate90Clockwise(result);
        }

        _currentPieceView.Data.Cells = result;
    }

    private bool[,] Rotate90Clockwise(bool[,] matrix) {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        bool[,] rotated = new bool[cols, rows];
        CellView[,] oldCells = _currentPieceView._cells;
        CellView[,] newCells = new CellView[cols, rows];
        Guid[,] oldGuids = _currentPieceView.Data.CellGuids;
        Guid[,] newGuids = new Guid[cols, rows];

        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                rotated[j, rows - 1 - i] = matrix[i, j];
                newGuids[j, rows - 1 - i] = oldGuids[i, j];
                newCells[j, rows - 1 - i] = oldCells[i, j];
            }
        }

        _currentPieceView._cells = newCells;
        _currentPieceView.Data.CellGuids = newGuids;
        return rotated;
    }

    public void SelectPieceToRotate(PieceView pieceView) {
        GameUI.Instance.SetUseRotateActive();
        RotationState = RotateBoosterStates.RotatePiece;
        _initialRotationY = (int)Mathf.Round(pieceView.transform.rotation.eulerAngles.y);
        _currentRotationY = _initialRotationY;
        _rotatingPiecePosition = pieceView.transform.position;
        _currentPieceView = pieceView;
        _rotationChanged = false;
        _lastInputPosition = Vector2.zero;
    }

    public void UseRotatePiece() {
        if (!AdminManager.Instance.IsInfiniteBoosters &&
            (ConfigsManager.Instance.BoostersConfig.RotateUnlockLevel > StorageManager.GameDataMain.CurMaxLevel ||
             StorageManager.GameDataMain.RotatePieceCount <= 0 || GameUI.Instance.GoalView._isGameEnded)) return;
        if (RotationState == RotateBoosterStates.LockRotate) {
            RotationState = RotateBoosterStates.SelectPiece;
        } else {
            if (RotationState == RotateBoosterStates.RotatePiece)
                _currentPieceView.transform.rotation = Quaternion.Euler(0, _initialRotationY, 0);
            RotationState = RotateBoosterStates.LockRotate;
            _rotatePieceSelectContainer.gameObject.SetActive(false);
            _rotatePieceButtonsContainer.gameObject.SetActive(false);
        }
    }

    public void UseHummer() {
        if (!AdminManager.Instance.IsInfiniteBoosters &&
            (ConfigsManager.Instance.BoostersConfig.HummerUnlockLevel > StorageManager.GameDataMain.CurMaxLevel ||
             StorageManager.GameDataMain.HummerCount <= 0 || GameUI.Instance.GoalView._isGameEnded)) {
            return;
        }

        GameFieldManager.Instance.ToggleDestroyPieceMode();
    }

    public void BreackCellWithHummer() {
        StorageManager.GameDataMain.HummerCount--;
        _hummerCountText.text = StorageManager.GameDataMain.HummerCount.ToString();
    }

    public void UseRandomField() {
        if (!AdminManager.Instance.IsInfiniteBoosters &&
            (ConfigsManager.Instance.BoostersConfig.RandomUnlockLevel > StorageManager.GameDataMain.CurMaxLevel ||
             StorageManager.GameDataMain.RandomFieldCount <= 0 || GameUI.Instance.GoalView._isGameEnded)) return;

        Dictionary<CellType, int> cellsToPlace = new Dictionary<CellType, int>();
        int cellsCount = 0;
        for (int i = 0; i < GameFieldManager.Instance._field.GetLength(0); i++) {
            for (int j = 0; j < GameFieldManager.Instance._field.GetLength(1); j++) {
                if (FieldUtils.IsResourceCell(GameFieldManager.Instance._field[i, j])) {
                    if (!cellsToPlace.TryAdd(GameFieldManager.Instance._field[i, j], 1))
                        cellsToPlace[GameFieldManager.Instance._field[i, j]]++;
                    GameFieldManager.Instance.DestroyCell(new Vector2Int(i, j));
                    cellsCount++;
                }
            }
        }

        if (cellsToPlace.Count == 0) return;
        StorageManager.GameDataMain.RandomFieldCount--;
        _randomFieldCountText.text = StorageManager.GameDataMain.RandomFieldCount.ToString();
        List<Vector2Int> emptyCells = new List<Vector2Int>();

        for (int i = 0; i < GameFieldManager.Instance._field.GetLength(0); i++) {
            for (int j = 0; j < GameFieldManager.Instance._field.GetLength(1); j++) {
                if (FieldUtils.CanPlaceOnCell(GameFieldManager.Instance._field[i, j]))
                    emptyCells.Add(new Vector2Int(i, j));
            }
        }

        foreach (var cell in cellsToPlace) {
            var curCellType = cell.Key;
            for (int i = 0; i < cell.Value; i++) {
                int needIndex = Random.Range(0, emptyCells.Count);
                var needPosition = emptyCells[needIndex];
                emptyCells.RemoveAt(needIndex);
                var config = PiecesViewTable.Instance.CellsList.CoreCellsConfigs.First(c => c.CellType == curCellType);
                var cellView = GameFieldManager.Instance.PlaceOneSizePiece(config, new Vector2Int(needPosition.x, needPosition.y), false);

                GameFieldManager.Instance.CheckCellTypesBeforePlacePiece(needPosition);
                GameFieldManager.Instance.SetNeededCellTypeOnField(curCellType, cellView, needPosition, false);
                GameFieldManager.Instance.CheckClosestCells(needPosition);
            }
        }

        GameFieldManager.Instance.ExplodeCellsInRows();

        OnBoosterEndedWorking?.Invoke();
    }

    public void UseDynamite() {
        if (!AdminManager.Instance.IsInfiniteBoosters &&
            (ConfigsManager.Instance.BoostersConfig.DynamiteUnlockLevel > StorageManager.GameDataMain.CurMaxLevel ||
             StorageManager.GameDataMain.DynamiteCount <= 0 || GameUI.Instance.GoalView._isGameEnded)) {
            return;
        }

        GameFieldManager.Instance.TogglePlaceDynamiteMode();
        _dynamiteExploding = false;
        _dynamiteCancelled = false;
    }

    public void CancelDynamite() {
        _dynamiteCancelled = true;
        if (_dynamiteExploding) return;

        _dinamyteButton.enabled = true;
        _dinamyteButton.gameObject.SetActive(true);
    }

    private void ExplodeDynamite(Transform dynamite, Vector2Int position) {
        int dinamyteRadius = ConfigsManager.Instance.BoostersConfig.DynamiteRadius;

        for (int i = 0; i < dinamyteRadius * 2 + 1; i++) {
            for (int j = 0; j < dinamyteRadius * 2 + 1; j++) {
                var newPosition = new Vector2Int(position.x - dinamyteRadius + i, position.y - dinamyteRadius + j);
                if (!FieldUtils.IsInsideField(GameFieldManager.Instance._field, newPosition) ||
                    FieldUtils.CantDestroyInRow(GameFieldManager.Instance._field[newPosition.x, newPosition.y])) {
                    continue;
                }

                var cellType = GameFieldManager.Instance._field[newPosition.x, newPosition.y];
                var cellConfig = PiecesViewTable.Instance.CellsList.CoreCellsConfigs.First(c => c.CellType == cellType);
                GameFieldManager.Instance.TryAddResourceForCell(cellConfig, newPosition);

                GameFieldManager.Instance.DestroyCell(newPosition);
            }
        }

        VibrationsManager.Instance.SpawnVibration(VibrationType.AllRow);
        Instantiate(_dynamiteBoomFx, dynamite.transform.position, Quaternion.identity);
        Destroy(dynamite.gameObject);

        StorageManager.GameDataMain.DynamiteCount--;
        _dinamyteCountText.text = StorageManager.GameDataMain.DynamiteCount.ToString();
        _dinamyteButton.enabled = true;
        _dinamyteButton.gameObject.SetActive(true);
        OnBoosterEndedWorking?.Invoke();
    }

    public void AnimateDynamite(Transform dynamite, Vector2Int position) {
        _dynamiteExploding = true;
        GameUI.Instance.SetBoosterActive(BoosterType.Bomb, false);
        // Запоминаем исходный масштаб
        Vector3 originalScale = dynamite.transform.localScale;

        // Создаем последовательность анимаций
        DOTween.Sequence().Append(
            dynamite.transform.DOScale(originalScale * 1.3f, 0.4f).SetEase(Ease.OutBack) // Добавляем "пружинность" для эффекта раздувания
        ).Append(dynamite.transform.DOScale(Vector3.zero, 0.6f).SetEase(Ease.InBack) // Эффект "втягивания"
                .OnComplete(() => { ExplodeDynamite(dynamite, position); }) // Удаляем объект после анимации
        );
    }

    public void SetAllText() {
        _randomFieldCountText.text = StorageManager.GameDataMain.RandomFieldCount.ToString();
        _dinamyteCountText.text = StorageManager.GameDataMain.DynamiteCount.ToString();
        _hummerCountText.text = StorageManager.GameDataMain.HummerCount.ToString();
        _rotatePieceCountText.text = StorageManager.GameDataMain.RotatePieceCount.ToString();
    }
}

public enum BoosterType {
    Shuffle,
    Bomb,
    Hammer,
    Rotate
}