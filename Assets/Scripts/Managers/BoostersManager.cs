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
    private bool _dynamiteExploding;
    private bool _dynamiteCancelled;
    
    public RotateBoosterStates RotationState ; 
    private float _initialRotationY; 
    private bool _rotationChanged; 
    private PieceView _currentPieceView;
    
    [SerializeField] private Button _rotatePieceCancelButton;
    [SerializeField] private Button _rotatePieceAcceptButton;
    [SerializeField] private Transform _rotatePieceSelectContainer;
    [SerializeField] private Transform _rotatePieceButtonsContainer;

    [SerializeField]
    private ParticleSystem _dynamiteBoomFx;
    private Vector2 _lastInputPosition;
    
    public static BoostersManager Instance;
    public Action OnBoosterEndedWorking;

    public enum RotateBoosterStates
    {
        LockRotate,
        SelectPiece,
        RotatePiece
    }
    private void Awake()
    {
        Instance = this;
        
        _rotatePieceAcceptButton.onClick.AddListener(()=> ApplyRotation());
        _rotatePieceButton.onClick.AddListener(()=> UseRotatePiece());
        _rotatePieceCancelButton.onClick.AddListener(()=> UseRotatePiece());
    }
    
    void Update()
    {
        if (RotationState != RotateBoosterStates.RotatePiece)
            return;

        bool isInputActive = false;
        float rotationDelta = 0f;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                rotationDelta = touch.deltaPosition.x * 0.5f; // Чувствительность
                isInputActive = true;
            }
        }
        else if (Input.GetMouseButton(0))
        {
            Vector2 currentMousePos = Input.mousePosition;
            if (_lastInputPosition != Vector2.zero)
            {
                rotationDelta = (currentMousePos.x - _lastInputPosition.x) * 0.3f; // Чувствительность
                isInputActive = true;
            }

            _lastInputPosition = currentMousePos;
        }
        else
        {
            _lastInputPosition = Vector2.zero;
        }

        if (isInputActive)
        {
            _currentPieceView.transform.Rotate(0, -rotationDelta, 0); // Вращаем по оси Y
            _rotationChanged = true;
        }
    }

    public void ApplyRotation()
    {
        if (!_rotationChanged) 
            return;

        float currentRotationY = _currentPieceView.transform.rotation.eulerAngles.y % 360f;
        if (currentRotationY < 0) currentRotationY += 360f;

        float closestAngle = Mathf.Round(currentRotationY / 90f) * 90f;
        closestAngle = closestAngle % 360f; 

        _currentPieceView.transform.rotation = Quaternion.Euler(0, closestAngle, 0);

        if (Mathf.Abs(closestAngle - _initialRotationY) > 89f)
        {
            int degrees = Mathf.RoundToInt(closestAngle - _initialRotationY);
            if (degrees < 0) degrees += 360;
            RotateFigure(degrees);
        }
        else
        {
            UseRotatePiece();
            return;
        }

        RotationState = RotateBoosterStates.LockRotate;
        _rotatePieceButtonsContainer.gameObject.SetActive(false);
        _currentPieceView = null;

        StorageManager.GameDataMain.RotatePieceCount--;
        _rotatePieceCountText.text =  StorageManager.GameDataMain.RotatePieceCount.ToString();
    }
    
    private void RotateFigure(int degrees)
    {
        if (degrees != 90 && degrees != 180 && degrees != 270)
            return;

        int rotations = degrees / 90;
        bool[,] result = _currentPieceView.Data.Cells;

        for (int r = 0; r < rotations; r++)
        {
            result = Rotate90Clockwise(result);
        }

        _currentPieceView.Data.Cells = result;
       
    }

    private bool[,] Rotate90Clockwise(bool[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        bool[,] rotated = new bool[cols, rows];
        CellView[,] oldCells = _currentPieceView._cells;
        CellView[,] newCells  = new CellView[ cols,rows];
        Guid[,] oldGuids =  _currentPieceView.Data.CellGuids;
        Guid[,] newGuids = new Guid[ cols,rows];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                rotated[j, rows - 1 - i] = matrix[i, j];
                newGuids[j, rows - 1 - i] = oldGuids[i, j];
                newCells[j, rows - 1 - i] = oldCells[i, j];
            }
        }
        _currentPieceView._cells = newCells;
        _currentPieceView.Data.CellGuids = newGuids;
        return rotated;
    }

    public void SelectPieceToRotate(PieceView pieceView)
    {
        _rotatePieceSelectContainer.gameObject.SetActive(false);
        _rotatePieceButtonsContainer.gameObject.SetActive(true);
        RotationState = RotateBoosterStates.RotatePiece;
        _initialRotationY = pieceView.transform.rotation.eulerAngles.y; 
        _currentPieceView = pieceView;
        _rotatePieceButtonsContainer.transform.position = pieceView.transform.position + new Vector3(1.5f,1,0);
        _rotationChanged = false;
        _lastInputPosition = Vector2.zero;
    }

    public void UseRotatePiece()
    {
       if(StorageManager.GameDataMain.RotatePieceCount <= 0|| GameUI.Instance.GoalView._isGameEnded) return;
        if (RotationState == RotateBoosterStates.LockRotate)
        {
         
            RotationState = RotateBoosterStates.SelectPiece;
            _rotatePieceSelectContainer.gameObject.SetActive(true);
        }
        else
        {
           
            if (RotationState == RotateBoosterStates.RotatePiece)
                _currentPieceView.transform.rotation = Quaternion.Euler(0, _initialRotationY, 0);
            RotationState = RotateBoosterStates.LockRotate;
            _rotatePieceSelectContainer.gameObject.SetActive(false);
            _rotatePieceButtonsContainer.gameObject.SetActive(false);
        }
    }

    public void UseHummer()
    {
        if (StorageManager.GameDataMain.HummerCount <= 0) {
            return;
        }
        GameFieldManager.Instance.ToggleDestroyPieceMode();
    }

    public void BreackCellWithHummer()
    {
        StorageManager.GameDataMain.HummerCount --;
        _hummerCountText.text =  StorageManager.GameDataMain.HummerCount.ToString();
    }
    public void UseRandomField()
    {
        if(StorageManager.GameDataMain.RandomFieldCount <= 0|| GameUI.Instance.GoalView._isGameEnded) return;

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
                    PiecesViewTable.Instance.CellsList.CoreCellsConfigs.First(c => c.CellType == curCellType);
                var cellView = GameFieldManager.Instance.PlaceOneSizePiece(config,
                    new Vector2Int(needPosition.x, needPosition.y), false);
                
                GameFieldManager.Instance.CheckCellTypesBeforePlacePiece(needPosition);
                GameFieldManager.Instance.SetNeededCellTypeOnField(curCellType, cellView, needPosition, false);
                GameFieldManager.Instance.CheckClosestCells(needPosition);
            }
        }

        GameFieldManager.Instance.ExplodeCellsInRows();

        OnBoosterEndedWorking?.Invoke();
    }

    public void UseDynamite()
    {
        if(_currentDynamite != null || StorageManager.GameDataMain.DynamiteCount <= 0 || GameUI.Instance.GoalView._isGameEnded) return;
        _dynamiteExploding = false;
        _dynamiteCancelled = false;
        _dinamyteButton.enabled = false;
        _dinamyteButton.gameObject.SetActive(false);
        PieceData dynamiteCellInfo = PieceUtils.GetExactPiece(ConfigsManager.Instance.BoostersConfig.DinamyteCellInfo);
        NextPiecesView.Instance.CreateDynamitePieceView(dynamiteCellInfo);
    }

    public void SetCurrentDynamite(Transform pieceView)
    {
        _currentDynamite = pieceView;
        if(_dynamiteCancelled) CancelDynamite();
    }

    public void CancelDynamite() {
        _dynamiteCancelled = true;
        if (_dynamiteExploding || _currentDynamite is null) return;
        
        Destroy(_currentDynamite.parent.gameObject);
        _currentDynamite = null;
        _dinamyteButton.enabled = true;
        _dinamyteButton.gameObject.SetActive(true);
    }

    private void ExplodeDynamite(Vector2Int position)
    {
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
                GameFieldManager.Instance.TryAddResourceForCell( cellConfig, newPosition);

                GameFieldManager.Instance.DestroyCell(newPosition);
            }
        }
        VibrationsManager.Instance.SpawnVibration(VibrationType.AllRow);
        Instantiate(_dynamiteBoomFx, _currentDynamite.transform.position, Quaternion.identity);
        Destroy(_currentDynamite.gameObject);
        _currentDynamite = null;
        StorageManager.GameDataMain.DynamiteCount--;
        _dinamyteCountText.text =  StorageManager.GameDataMain.DynamiteCount.ToString();
        _dinamyteButton.enabled = true;
        _dinamyteButton.gameObject.SetActive(true);
        OnBoosterEndedWorking?.Invoke();
    }

    public void AnimateDynamite(Vector2Int position) {
        _dynamiteExploding = true;
        GameUI.Instance.SetBoosterActive(BoosterType.Bomb, false);
        // Запоминаем исходный масштаб
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
    

    public void SetAllText()
    {
        _randomFieldCountText.text =  StorageManager.GameDataMain.RandomFieldCount.ToString();
        _dinamyteCountText.text =  StorageManager.GameDataMain.DynamiteCount.ToString();
        _hummerCountText.text =  StorageManager.GameDataMain.HummerCount.ToString();
        _rotatePieceCountText.text =  StorageManager.GameDataMain.RotatePieceCount.ToString();
    }

    private void SnapRotatedPieceToClosestStraightAngle()
    {
        
    }
}

public enum BoosterType {
    Shuffle,
    Bomb,
    Hammer,
    Rotate
}
