using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class BoostersManager : MonoBehaviour
{
    [SerializeField] private TMP_Text _randomFieldCountText;
    [SerializeField] private TMP_Text _dinamyteCountText;
    [SerializeField] private TMP_Text _hummerCountText;
    [SerializeField] private TMP_Text _rotatePieceCountText;
    
    [SerializeField] private Button _randomFieldButton;
    [SerializeField] private Button _dinamyteButton;
    [SerializeField] private Button _hummerButton;
    [SerializeField] private Button _rotatePieceButton;
    [field:SerializeField] 
    public Transform _dynamiteContainer { get; private set; }
    private Transform _currentDynamite;
    
    public RotateBoosterStates rotationState ; 
    private float initialRotationY; 
    private bool rotationChanged = false; 
    private PieceView pieceView;
    
    public static BoostersManager Instance;

    public enum RotateBoosterStates
    {
        SelectPiece,
        RotatePiece,
        LockRotate
    }
    private void Awake()
    {
        Instance = this;
    }
    
    void Update()
    {
        if (rotationState == RotateBoosterStates.RotatePiece && Input.touchCount > 0)
        {
            float rotationSpeed = 2f;//move to config
            float rotationDelta = Input.GetTouch(0).deltaPosition.x * rotationSpeed;
            pieceView.transform.Rotate(0, rotationDelta, 0); 
            rotationChanged = true;
        }
    }

    public void UnlockRotation()
    {
        initialRotationY = transform.rotation.eulerAngles.y; // Запоминаем начальный угол Y
        rotationChanged = false;
    }
    
    public void ApplyRotation()
    {
        if (!rotationChanged) 
            return;

        float currentRotationY = transform.rotation.eulerAngles.y % 360f;
        if (currentRotationY < 0) currentRotationY += 360f;

        float closestAngle = Mathf.Round(currentRotationY / 90f) * 90f;
        closestAngle = closestAngle % 360f; 

        transform.rotation = Quaternion.Euler(0, closestAngle, 0);

        if (Mathf.Abs(closestAngle - initialRotationY) > 0.1f)
        {
            int degrees = Mathf.RoundToInt(closestAngle - initialRotationY);
            if (degrees < 0) degrees += 360;
          //  Cells = RotateFigure(Cells, degrees);
        }

        rotationState = RotateBoosterStates.LockRotate;
    }
    
    private bool[,] RotateFigure(bool[,] cells, int degrees)
    {
        if (degrees != 90 && degrees != 180 && degrees != 270)
            return cells;

        int rotations = degrees / 90;
        bool[,] result = cells;

        for (int r = 0; r < rotations; r++)
        {
            result = Rotate90Clockwise(result);
        }

        return result;
    }

    private bool[,] Rotate90Clockwise(bool[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        bool[,] rotated = new bool[cols, rows];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                rotated[j, rows - 1 - i] = matrix[i, j];
            }
        }

        return rotated;
    }
    
    public void UseRandomField()
    {
        if(StorageManager.GameDataMain.RandomFieldCount <= 0|| GoalView.Instance._isGameEnded) return;

        Dictionary<CellType, int> cellsToPlace = new Dictionary<CellType, int>();
        int cellsCount = 0;
        for (int i = 0; i < GameFieldManager.Instance._field.GetLength(0); i++)
        {
            for (int j = 0; j < GameFieldManager.Instance._field.GetLength(1); j++)
            {
                if (FieldUtils.IsResourceCell(GameFieldManager.Instance._field[i, j]))
                {
                    if (!cellsToPlace.TryAdd(GameFieldManager.Instance._field[i, j], 1))
                        cellsToPlace[GameFieldManager.Instance._field[i, j]]++;
                    GameFieldManager.Instance.DestroyCell(new Vector2Int(i, j));
                    cellsCount++;
                }
            }
        }
        if(cellsToPlace.Count == 0)return;
        StorageManager.GameDataMain.RandomFieldCount--;
        _randomFieldCountText.text =  StorageManager.GameDataMain.RandomFieldCount.ToString();
        List<Vector2Int> emptyCells = new List<Vector2Int>();
        
        for (int i = 0; i < GameFieldManager.Instance._field.GetLength(0); i++)
        {
            for (int j = 0; j < GameFieldManager.Instance._field.GetLength(1); j++)
            {
                if (FieldUtils.CanPlaceOnCell(GameFieldManager.Instance._field[i, j]))
                    emptyCells.Add(new Vector2Int(i, j));
            }
        }

        foreach (var cell in cellsToPlace)
        {
            var curCellType = cell.Key;
            for (int i = 0; i < cell.Value; i++)
            {
                int needIndex = Random.Range(0, emptyCells.Count);
                var needPosition = emptyCells[needIndex];
                emptyCells.RemoveAt(needIndex);
                var config =
                    PiecesViewTable.Instance.CellsList.CellsConfigs.First(c => c.CellType == curCellType);
                var cellView = GameFieldManager.Instance.PlaceOneSizePiece(config,
                    new Vector2Int(needPosition.x, needPosition.y), false);
                
                GameFieldManager.Instance.CheckCellTypesBeforePlacePiece(needPosition);
                GameFieldManager.Instance.SetNeededCellTypeOnField(curCellType, cellView, needPosition, false);
                GameFieldManager.Instance.CheckClosestCells(needPosition);
            }
        }

        GameFieldManager.Instance.ExplodeCellsInRows();

        CheckGameGoal();

        GameFieldManager.Instance.CheckResourceCountForTasks();
    }

    public void UseDynamite()
    {
        if(_currentDynamite != null || StorageManager.GameDataMain.DynamyteCount <= 0 || GoalView.Instance._isGameEnded) return;
        _dinamyteButton.enabled = false;
        _dinamyteButton.gameObject.SetActive(false);
        var dinamiteCellInfo = PieceUtils.GetNewPiece(ConfigsManager.Instance.BoostersConfig.DinamyteCellInfo);
        NextPiecesView.Instance.CreateDynamitePieceView(dinamiteCellInfo);
    }

    public void SetCurrentDynamite(Transform pieceView)
    {
        _currentDynamite = pieceView;
      //  _currentDynamite.OnStartDrag();
    }

    private void ExplodeDynamite(Vector2Int position)
    {
        int dinamyteRadius = ConfigsManager.Instance.BoostersConfig.DynamiteRadius;

        for (int i = 0; i < dinamyteRadius * 2 + 1; i++)
        {
            for (int j = 0; j < dinamyteRadius * 2 + 1; j++)
            {
                var newPosition = new Vector2Int(position.x - dinamyteRadius + i, position.y - dinamyteRadius + j);
                if (FieldUtils.IsInsideField(GameFieldManager.Instance._field, newPosition)
                    && !FieldUtils.CantDestroyInRow(GameFieldManager.Instance._field[newPosition.x, newPosition.y]))
                {
                    var configSlime = PiecesViewTable.Instance.CellsList.CellsConfigs.First(c =>
                        c.CellType == GameFieldManager.Instance._field[newPosition.x, newPosition.y]);
                    for (int k = 0; k < GameFieldManager.Instance._currentTasks.Count; k++)
                    {
                        if (GameFieldManager.Instance._currentTasks[k].TaskInfo.TaskType ==
                            TaskInfo.TaskType.getResource)
                        {
                            GameFieldManager.Instance.CheckNeedResourceInTask(k, configSlime, newPosition);
                        }
                    }

                    GameFieldManager.Instance.DestroyCell(newPosition);
                }
            }
        }

        Destroy(_currentDynamite.gameObject);
        _currentDynamite = null;
        CheckGameGoal();
        GameFieldManager.Instance.CheckResourceCountForTasks();
        StorageManager.GameDataMain.DynamyteCount--;
        _dinamyteCountText.text =  StorageManager.GameDataMain.DynamyteCount.ToString();
        _dinamyteButton.enabled = true;
        _dinamyteButton.gameObject.SetActive(true);
    }

    public void AnimateDynamite(Vector2Int position)
    {
        // Запоминаем исходный масштаб
        Debug.Log(_currentDynamite);
        Vector3 originalScale = _currentDynamite.transform.localScale;

        // Создаем последовательность анимаций
        DOTween.Sequence().Append(
            _currentDynamite.transform.DOScale(originalScale * 1.3f, 0.4f)
                .SetEase(Ease.OutBack) // Добавляем "пружинность" для эффекта раздувания
        ).Append(
            _currentDynamite.transform.DOScale(Vector3.zero, 0.6f)
                .SetEase(Ease.InBack) // Эффект "втягивания"
                .OnComplete(() => { ExplodeDynamite(position); }) // Удаляем объект после анимации
        );

    }

    private void CheckGameGoal()
    {
        if (!GameFieldManager.Instance.CheckWin() && GameFieldManager.Instance.CheckLose())
            GameFieldManager.Instance.Lose();
    }

    public void SetAllText()
    {
        _randomFieldCountText.text =  StorageManager.GameDataMain.RandomFieldCount.ToString();
        _dinamyteCountText.text =  StorageManager.GameDataMain.DynamyteCount.ToString();
        _hummerCountText.text =  StorageManager.GameDataMain.HummerCount.ToString();
        _rotatePieceCountText.text =  StorageManager.GameDataMain.RotatePieceCount.ToString();
    }

    private void SnapRotatedPieceToClosestStraightAngle()
    {
        
    }
}
