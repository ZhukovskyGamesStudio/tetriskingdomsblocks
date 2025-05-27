using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour, IResetable
{
    public static GameManager Instance;

    public MainGameConfig mainGameConfig;

    public const int CELL_SIZE = 1;
    private CellTypeInfo[,] _field;
    private List<PieceData> _nextBlocks = new List<PieceData>();

    [SerializeField]
    private Transform _fieldContainer;

    private CellView[,] _cells;

    [SerializeField]
    private Transform _fieldStart, _fieldEnd;

    [SerializeField]
    private Camera _raycastCamera;

    [SerializeField]
    private FloatingTextView _floatingTextPrefab;

    [SerializeField]
    private Transform _floatingTextContainer;

    [SerializeField]
    public List<CellTypeInfo> currentCellsToSpawn;
    
    [SerializeField]
    public LevelConfig currentLevelConfig;
    
    public List<CraftingCellInfo> currentCraftedCells = new List<CraftingCellInfo>();
    
    
    public Vector3 ScreenToWorldPoint => _raycastCamera.ScreenToWorldPoint(Input.mousePosition);

    private int _placedPiecesAmount;

    private ObjectPool<FloatingTextView> _floatingTextsPool;

    public GameData GameData { get; private set; }

    private void Awake()
    {
        Reset();
        Instance = this;
        _floatingTextsPool = new ObjectPool<FloatingTextView>(() => Instantiate(_floatingTextPrefab, _floatingTextContainer));
    }

    private void Start()
    {
        GenerateField();
        GenerateTask();
        StartGame();
    }

    private void GenerateField() { }

    private void GenerateTask()
    {
        var generatedGoal = TasksUtils.GenerateNewResourceTask();
        GameData.TaskData.GoalToCollect.Add(generatedGoal.Key, generatedGoal.Value);
        GoalView.Instance.InitTask(GameData);
    }

    private void StartGame()
    {
        /*
        var startCells = currentLevelConfig.cellTypesTableConfig;

        for (int i = 0; i < startCells.cellsToSpawn.Length; i++)
            currentCellsToSpawn.Add(startCells.cellsToSpawn[i]);
        */
        GenerateNewPieces();
    }

    public void GenerateNewPieces()
    {
        _nextBlocks = new List<PieceData>() {
            PieceUtils.GetNewPiece(),
            PieceUtils.GetNewPiece(),
            PieceUtils.GetNewPiece()
        };
        NextPiecesView.Instance.SetData(_nextBlocks);
    }

    private void Update()
    {
        //  Debug.Log(GetPosOnField() + "    Dragshift: " + PieceView.DragShift+ "    Piece: " + GetPieceClampedPosOnField());
    }

    public bool CanPlace(PieceData data)
    {
        Vector2Int pos = GetPieceClampedPosOnField();
        return CanPlace(data, pos);
    }

    public Vector2Int GetPosOnField()
    {
        Vector3 coord = GetCoord();
        if (PieceView.PieceMaxSize.x % 2 == 0)
        {
            coord += Vector3.left / 2f;
        }
        if (PieceView.PieceMaxSize.y % 2 == 0)
        {
            coord += Vector3.back / 2f;
        }

        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(coord.x) / CELL_SIZE, Mathf.RoundToInt(coord.z) / CELL_SIZE);
        //pos -= new Vector2Int((int)_fieldStart.position.x, (int)_fieldStart.position.z);
        return pos;
    }

    private static Vector3 GetCoord()
    {
        var coord = Instance.ScreenToWorldPoint;
        //coord += new Vector3(0, 0, 6);
        return coord;
    }

    public Vector2Int GetPieceClampedPosOnField()
    {
        Vector3 coord = GetCoord();
        coord += new Vector3(PieceView.DragShift.x, 0, PieceView.DragShift.z) + Vector3.forward;
        /*
       if (CalculateShift().x % 1 != 0) {
           DragShift-= Vector3.right/2;
       }
       if (CalculateShift().z % 1 != 0) {
           DragShift-= Vector3.forward/2;
       }*/


        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(coord.x) / CELL_SIZE, Mathf.RoundToInt(coord.z) / CELL_SIZE);
        pos -= new Vector2Int((int)_fieldStart.position.x, (int)_fieldStart.position.z);
        return pos;
    }

    public bool CanPlace(PieceData data, Vector2Int pos)
    {
        if (pos.x < 0 || pos.y < 0)
        {
            return false;
        }

        if (pos.x + data.Cells.GetLength(0) - 1 >= _field.GetLength(0))
        {
            return false;
        }

        if (pos.y + data.Cells.GetLength(1) - 1 >= _field.GetLength(1))
        {
            return false;
        }

        for (int x = 0; x < data.Cells.GetLength(0); x++)
        {
            for (int y = 0; y < data.Cells.GetLength(1); y++)
            {
                if (data.Cells[x, y] && _field[pos.x + x, pos.y + y] != null)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void PlacePiece(PieceData pieceData)
    {
        PlacePiece(pieceData, GetPieceClampedPosOnField());
        _nextBlocks.Remove(pieceData);
        _placedPiecesAmount++;
        if(mainGameConfig.resourceOnPlaceCell)
           CollectResourcesOnPlace(pieceData);
        ExplodeCells();
        GoalView.Instance.UpdateTask(GameData);

        if (CheckWin())
        {
            Win();
            return;
        }

        if (_placedPiecesAmount % 3 == 0)
        {
            GenerateNewPieces();
        }

        if (CheckLose())
        {
            Lose();
        }
    }

    private void PlacePiece(PieceData pieceData, Vector2Int pos)
    {
        for (int x = 0; x < pieceData.Cells.GetLength(0); x++)
        {
            for (int y = 0; y < pieceData.Cells.GetLength(1); y++)
            {
                if (!pieceData.Cells[x, y])
                {
                    continue;
                }

                var place = new Vector2Int((int)Mathf.Clamp(pos.x + x, 0, mainGameConfig.fieldSize), (int)Mathf.Clamp(pos.y + y, 0, mainGameConfig.fieldSize));
               // var go = Instantiate(PiecesViewTable.Instance.GetCellByType(pieceData.Type.cellType), _fieldContainer);
               var go = Instantiate(pieceData.Type.cellPrefab, _fieldContainer);
                go.transform.localPosition = new Vector3(place.x, -0.05f, place.y);
                _field[place.x, place.y] = pieceData.Type;
                _cells[place.x, place.y] = go;

                string needText = " +";
                var resourcesForPlace = _field[place.x, place.y].resourcesForPlace;
                for (int i = 0; i < resourcesForPlace.Length; i++)
                    needText += resourcesForPlace[i].resourceCount+ " "+resourcesForPlace[i].resourceType.ToString()+" ";

                if (resourcesForPlace.Length != 0)
                {
                var canvasPosition =
                    _raycastCamera.WorldToScreenPoint(go.transform.position);
                ShowFloatingText(needText, canvasPosition, 50);
                }
            }
        }
    }

    private void CollectResourcesOnPlace(PieceData placedPiece)
    {
        for (int x = 0; x < placedPiece.Cells.GetLength(0); x++)
        {
            for (int y = 0; y < placedPiece.Cells.GetLength(1); y++)
            {
                if (!placedPiece.Cells[x, y])
                {
                    continue;
                }

                var needResources = placedPiece.Type.resourcesForPlace;
                for (int i = 0; i < needResources.Length; i++)
                {
                    var resourceType = needResources[i];
                    if (resourceType == null)
                    {
                        continue;
                    }

                    if (!GameData.CollectedResources.TryAdd(resourceType.resourceType, resourceType.resourceCount))
                    {
                        GameData.CollectedResources[resourceType.resourceType] += resourceType.resourceCount;
                    }
                }
            }
        }
    }

    private void ExplodeCells()
    {
        int width = _field.GetLength(0);
        int height = _field.GetLength(1);

        // Проверка строк
        for (int y = 0; y < height; y++)
        {
            bool fullRow = true;

            for (int x = 0; x < width; x++)
            {
                if (_field[x, y] == null)
                {
                    fullRow = false;
                    break;
                }
            }

            if (fullRow)
            {
                DestroyLine(y, width, true);
            }
        }

        // Проверка столбцов
        for (int x = 0; x < width; x++)
        {
            bool fullColumn = true;

            for (int y = 0; y < height; y++)
            {
                if (_field[x, y] == null)
                {
                    fullColumn = false;
                    break;
                }
            }

            if (fullColumn)
            {
                DestroyLine(x, height, false);
            }
        }
    }

    private void DestroyLine(int mainAxisCurrentValue, int secondAxisLenght, bool isRow)
    {
        bool fullSameResourcesColumn = mainGameConfig.bonusResourcesOnDestroyLine ? true : false;
        CellType currentCellType = CellType.Empty;
        int bonusResourcesOnDestroyLine = 0;
        ResourceType currentBonusResourceType = ResourceType.None;
        
        Dictionary<CellType, int> cellTypesInLine = new Dictionary<CellType, int>();
        
        for (int secondAxis = 0; secondAxis < secondAxisLenght; secondAxis++)
        {
            Vector2 curPosition = !isRow
                ? new Vector2(mainAxisCurrentValue, secondAxis)
                : new Vector2(secondAxis, mainAxisCurrentValue);
            var cellInfo = _field[(int)curPosition.x, (int)curPosition.y];
            if (fullSameResourcesColumn)
            {
                if (currentCellType == CellType.Empty)
                    currentCellType = cellInfo.cellType;
                else if (currentCellType != cellInfo.cellType)
                    fullSameResourcesColumn = false;
            }

            string floatingText = "+ ";

            if (!cellTypesInLine.TryAdd(cellInfo.cellType, 1))
                cellTypesInLine[cellInfo.cellType]++;
            
            for (int i = 0; i < cellInfo.resourcesForDestroy.Length; i++)
            {
                if (fullSameResourcesColumn && !GameData.CollectedResources.TryAdd(
                        cellInfo.resourcesForDestroy[i].resourceType, cellInfo.resourcesForDestroy[i].resourceCount))
                    GameData.CollectedResources[cellInfo.resourcesForDestroy[i].resourceType] +=
                        cellInfo.resourcesForDestroy[i].resourceCount;
                floatingText += cellInfo.resourcesForDestroy[i].resourceType.ToString() + " " + cellInfo
                    .resourcesForDestroy[i]
                    .resourceCount + " ";
                if (fullSameResourcesColumn)
                {
                    bonusResourcesOnDestroyLine +=
                        cellInfo.resourcesForDestroy[i]
                            .resourceCount; //fix this if on destroy resources types be more than 1;
                    currentBonusResourceType = cellInfo.resourcesForDestroy[i].resourceType;
                }
            }

            if (cellInfo.resourcesForDestroy.Length != 0)
            {
            var canvasPosition =
                _raycastCamera.WorldToScreenPoint(_cells[(int)curPosition.x, (int)curPosition.y].transform.position);
            ShowFloatingText(floatingText, canvasPosition, 50);
            }
            // CollectResources( _field[(int)curPosition.x, (int)curPosition.y]);
            DestroyCell((int)curPosition.x, (int)curPosition.y);
        }

        if (fullSameResourcesColumn &&
            !GameData.CollectedResources.TryAdd(currentBonusResourceType, bonusResourcesOnDestroyLine))
        {
            GameData.CollectedResources[currentBonusResourceType] += bonusResourcesOnDestroyLine;
            Vector2 curPosition = !isRow ? new Vector2(mainAxisCurrentValue, 5) : new Vector2(5, mainAxisCurrentValue);
            var needPosition =
                _raycastCamera.WorldToScreenPoint(_cells[(int)curPosition.x, (int)curPosition.y].transform.position);

            ShowFloatingText(currentBonusResourceType.ToString() + " +" + bonusResourcesOnDestroyLine, needPosition,
                100);
        }
        else
            Debug.Log("not full same");
        
        for (int i = 0; i < currentCraftedCells.Count; i++)
        {
            bool addNewCell = true;
            for (int j = 0; j < currentCraftedCells[i].cellTypeToCraft.Length; j++)
            {
                if (!cellTypesInLine.ContainsKey(currentCraftedCells[i].cellTypeToCraft[j]))
                {
                    addNewCell = false;
                    break;
                }
            }

            if (addNewCell)
            {
                currentCellsToSpawn.Add(currentCraftedCells[i].cellsToCraft);
                currentCraftedCells.RemoveAt(i);
            }
            
            Debug.Log(addNewCell + " add new cell");
        }
    }
    private void DestroyCell(int x, int y)
    {
        _field[x, y] = null;
        Destroy(_cells[x, y].gameObject);
    }

    private bool CheckWin()
    {
        var goal = GameData.TaskData.GoalToCollect.First();
        if (GameData.CollectedResources.TryGetValue(goal.Key, out int hasValue))
        {
            if (hasValue >= goal.Value)
            {
                return true;
            }
        }

        return false;
    }
    private bool CheckLose()
    {
        foreach (PieceData t in _nextBlocks)
        {
            if (PieceUtils.CanPlacePiece(_field, _nextBlocks[0].Cells))
            {
                return false;
            }
        }
        return true;
    }

    private void Win()
    {
        GoalView.Instance.SetWinState();
    }

    private void Lose()
    {
        GoalView.Instance.SetLoseState();
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Reset()
    {
        _nextBlocks = new List<PieceData>();
        _placedPiecesAmount = 0;
       // mainGameConfig.fieldSize = 10;
        _field = new CellTypeInfo[mainGameConfig.fieldSize, mainGameConfig.fieldSize];
        _cells = new CellView[mainGameConfig.fieldSize, mainGameConfig.fieldSize];
        
        currentCraftedCells = new List<CraftingCellInfo>();
        foreach (var craftedCell in mainGameConfig.cellsToCraft)
            currentCraftedCells.Add(craftedCell);
        
        var startCells = currentLevelConfig.cellTypesTableConfig;
        currentCellsToSpawn = new List<CellTypeInfo>();
        for (int i = 0; i < startCells.cellsToSpawn.Length; i++)
            currentCellsToSpawn.Add(startCells.cellsToSpawn[i]);
        
        GameData = new GameData();
        
    }
    public void ShowFloatingText(string needText, Vector2 newPosition, float textSize)
    {
        //  var screenPoint = (Input.mousePosition - _mainCanvasRectTransform.position) / _mainCanvasRectTransform.localScale.x;
        var floatingText = _floatingTextsPool.Get();
        floatingText.SetText(newPosition, needText, textSize);
    }

    public void ReleaseFloatingText(FloatingTextView needTextObject)
    {
        needTextObject.gameObject.SetActive(false);
        _floatingTextsPool.Release(needTextObject);
    }
}


[SerializeField]
public class PieceData
{
    public bool[,] Cells;
    public CellTypeInfo Type;
}

[SerializeField]
public enum CellType
{
    Empty = 0,
    Forest,
    Metal,
    Village,
    Mountain,
    MiniCity,
    FoodSource
}