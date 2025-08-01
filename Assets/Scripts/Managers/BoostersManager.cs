using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class BoostersManager : MonoBehaviour {
    [SerializeField]
    private ParticleSystem _dynamiteBoomFx;

    [SerializeField]
    private GameBoostersPanels _gameBoostersPanels;
    
    public static BoostersManager Instance;
    public Action OnBoosterEndedWorking;
    private GameData _gameData;
    private bool _dynamiteExploding;

    public RotateBoosterStates RotationState;
    private int _initialRotationY;
    private int _currentRotationY;
    private PieceView _currentPieceView;

    public enum RotateBoosterStates {
        LockRotate,
        SelectPiece,
        RotatePiece
    }

    private void Awake() {
        Instance = this;
    }

    public void Init(GameData gameData) {
        _gameData = gameData;
        GameUI.Instance.GameBoostersButtons.SetBoosterButtons(ConfigsManager.Instance.BoostersConfig, StorageManager.GameDataMain.CurMaxLevel);
        GameUI.Instance.GameBoostersButtons.UpdateCounters(StorageManager.GameDataMain);
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

    public void ApplyRotation() {
        if (_currentRotationY == _initialRotationY)
            return;

        RotateFigure(_currentRotationY - _initialRotationY);

        RotationState = RotateBoosterStates.LockRotate;
        _currentPieceView = null;

        StorageManager.GameDataMain.RotatePieceCount--;
        GameUI.Instance.GameBoostersButtons.UpdateCounters(StorageManager.GameDataMain);
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
        GameUI.Instance.BoostersPanels.SetUseRotateActive();
        RotationState = RotateBoosterStates.RotatePiece;
        _initialRotationY = (int)Mathf.Round(pieceView.transform.rotation.eulerAngles.y);
        _currentRotationY = _initialRotationY;
        _currentPieceView = pieceView;
    }

    public void UseRotatePiece() {
        if (!CanRotate()) {
            return;
        }

        if (RotationState == RotateBoosterStates.LockRotate) {
            RotationState = RotateBoosterStates.SelectPiece;
        } else {
            if (RotationState == RotateBoosterStates.RotatePiece)
                _currentPieceView.transform.rotation = Quaternion.Euler(0, _initialRotationY, 0);
            RotationState = RotateBoosterStates.LockRotate;
        }
    }

    public void UseHammer() {
        if (!CanHammer()) {
            return;
        }

        GameFieldManager.Instance.SetDestroyPieceMode(true);
    }

    public void BreakCellWithHammer() {
        StorageManager.GameDataMain.HummerCount--;
        GameUI.Instance.GameBoostersButtons.UpdateCounters(StorageManager.GameDataMain);
        GameFieldManager.Instance.SetDestroyPieceMode(false);
        _gameBoostersPanels.CancelHammer();
    }

    public async UniTask UseRandomField() {
        if (!CanShuffle()) {
            return;
        }

        Dictionary<CellType, int> cellsToPlace = new Dictionary<CellType, int>();
        List<UniTask> _destroyTasks = new List<UniTask>();
        int cellsCount = 0;
        for (int i = 0; i < GameFieldManager.Instance._field.GetLength(0); i++) {
            for (int j = 0; j < GameFieldManager.Instance._field.GetLength(1); j++) {
                if (FieldUtils.IsResourceCell(GameFieldManager.Instance._field[i, j])) {
                    if (!cellsToPlace.TryAdd(GameFieldManager.Instance._field[i, j], 1))
                        cellsToPlace[GameFieldManager.Instance._field[i, j]]++;
                    _destroyTasks.Add(GameFieldManager.Instance.DestroyCell(new Vector2Int(i, j)));
                    cellsCount++;
                }
            }
        }

        await UniTask.WhenAll(_destroyTasks);
        
        if (cellsToPlace.Count == 0) {
            return;
        }
        StorageManager.GameDataMain.RandomFieldCount--;
        GameUI.Instance.GameBoostersButtons.UpdateCounters(StorageManager.GameDataMain);
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
        if (!CanDynamite()) {
            return;
        }

        GameFieldManager.Instance.TogglePlaceDynamiteMode();
        _dynamiteExploding = false;
    }

    public void CancelDynamite() {
        if (_dynamiteExploding) {
            return;
        }

        GameFieldManager.Instance.DisablePlaceDynamiteMode();
        _gameBoostersPanels.CancelBomb();
    }

    public void CancelHammer() {
        GameFieldManager.Instance.SetDestroyPieceMode(false);
        _gameBoostersPanels.CancelHammer();
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
        GameUI.Instance.GameBoostersButtons.UpdateCounters(StorageManager.GameDataMain);

        OnBoosterEndedWorking?.Invoke();
    }

    public void AnimateDynamite(Transform dynamite, Vector2Int position) {
        _dynamiteExploding = true;
        GameUI.Instance.BoostersPanels.SetBoosterActive(BoosterType.Bomb, false);
        // Запоминаем исходный масштаб
        Vector3 originalScale = dynamite.transform.localScale;
        Vector3 finPos = dynamite.position;
        dynamite.position += Vector3.up;
        dynamite.localScale = Vector3.zero;
        // Создаем последовательность анимаций
        DOTween.Sequence()
            // Добавляем "пружинность" для эффекта раздувания
            .Append(dynamite.transform.DOScale(originalScale * 1.3f, 0.4f).SetEase(Ease.OutBack))
            .Join(dynamite.transform.DOMove(finPos,0.4f))
            // Эффект "втягивания"
            .Append(dynamite.transform.DOScale(Vector3.zero, 0.6f).SetEase(Ease.InBack)
            // Удаляем объект после анимации
            .OnComplete(() => { ExplodeDynamite(dynamite, position); }) 
        );
    }

    public bool CanShuffle() {
        return CanUseBooster(ConfigsManager.Instance.BoostersConfig.RandomUnlockLevel, StorageManager.GameDataMain.RandomFieldCount);
    }

    public bool CanDynamite() {
        return CanUseBooster(ConfigsManager.Instance.BoostersConfig.DynamiteUnlockLevel, StorageManager.GameDataMain.DynamiteCount);
    }

    public bool CanHammer() {
        return CanUseBooster(ConfigsManager.Instance.BoostersConfig.HummerUnlockLevel, StorageManager.GameDataMain.HummerCount);
    }

    public bool CanRotate() {
        return CanUseBooster(ConfigsManager.Instance.BoostersConfig.RotateUnlockLevel, StorageManager.GameDataMain.RotatePieceCount);
    }

    private bool CanUseBooster(int unlockLvl, int hasAmount) {
        if (_gameData.IsGameEnded) {
            return false;
        }

        if (StorageManager.GameDataMain.CurMaxLevel < unlockLvl) {
            return false;
        }

        return hasAmount > 0;
    }
}

public enum BoosterType {
    Shuffle,
    Bomb,
    Hammer,
    Rotate
}