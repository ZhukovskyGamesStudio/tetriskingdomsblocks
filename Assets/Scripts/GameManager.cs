using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour, IResetable
{
    public static GameManager Instance;

    [SerializeField]
    public int _fieldSize { get; private set; }

    public const int CELL_SIZE = 1;
    private CellType[,] _field;
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
                if (data.Cells[x, y] && _field[pos.x + x, pos.y + y] != CellType.Empty)
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
        CollectResources(pieceData);
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

                var place = new Vector2Int((int)Mathf.Clamp(pos.x + x, 0, _fieldSize), (int)Mathf.Clamp(pos.y + y, 0, _fieldSize));
                var go = Instantiate(PiecesViewTable.Instance.GetCellByType(pieceData.Type), _fieldContainer);
                go.transform.localPosition = new Vector3(place.x, -0.05f, place.y);
                _field[place.x, place.y] = pieceData.Type;
                _cells[place.x, place.y] = go;
            }
        }
    }

    private void CollectResources(PieceData placedPiece)
    {
        for (int x = 0; x < placedPiece.Cells.GetLength(0); x++)
        {
            for (int y = 0; y < placedPiece.Cells.GetLength(1); y++)
            {
                if (!placedPiece.Cells[x, y])
                {
                    continue;
                }

                var resourceType = ResourcesUtils.ResourceByCellType(placedPiece.Type);
                if (resourceType == ResourceType.None)
                {
                    continue;
                }

                if (!GameData.CollectedResources.TryAdd(resourceType, 1))
                {
                    GameData.CollectedResources[resourceType] += 1;
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
                if (_field[x, y] == CellType.Empty)
                {
                    fullRow = false;
                    break;
                }
            }

            if (fullRow)
            {
                DestroyLine(y, width, true);
                /* for (int x = 0; x < width; x++)
                 {
                     DestroyCell(x, y);
                 }*/
            }
        }

        // Проверка столбцов
        for (int x = 0; x < width; x++)
        {
            bool fullColumn = true;

            for (int y = 0; y < height; y++)
            {
                if (_field[x, y] == CellType.Empty)
                {
                    fullColumn = false;
                    break;
                }
            }

            if (fullColumn)
            {
                DestroyLine(x, height, false);
                /*bool fullSameResourcesColumn = true;
                CellType currentCellType = CellType.Empty;

                    for (int y = 0; y < height; y++)
                    { 
                    if (fullSameResourcesColumn)
                    {
                        if (currentCellType == CellType.Empty)
                            currentCellType = _field[x, y];
                        else if (currentCellType != _field[x, y])
                            fullSameResourcesColumn = false;
                    }
                        DestroyCell(x, y);
                    }

                if (fullSameResourcesColumn)
                    Debug.Log("full same");
                else
                    Debug.Log("not full same");*/
            }
        }
    }

    private void DestroyLine(int mainAxisCurrentValue, int secondAxisLeight, bool isRow)
    {
        bool fullSameResourcesColumn = true;
        CellType currentCellType = CellType.Empty;
        for (int secondAxis = 0; secondAxis < secondAxisLeight; secondAxis++)
        {
            Vector2 curPosition = !isRow ? new Vector2(mainAxisCurrentValue, secondAxis) : new Vector2(secondAxis, mainAxisCurrentValue);
            if (fullSameResourcesColumn)
            {
                if (currentCellType == CellType.Empty)
                    currentCellType = _field[(int)curPosition.x, (int)curPosition.y];
                else if (currentCellType != _field[(int)curPosition.x, (int)curPosition.y])
                    fullSameResourcesColumn = false;
            }
            DestroyCell((int)curPosition.x, (int)curPosition.y);
        }

        if (fullSameResourcesColumn && !GameData.CollectedResources.TryAdd(ResourcesUtils.ResourceByCellType(currentCellType), 10))
        {
                GameData.CollectedResources[ResourcesUtils.ResourceByCellType(currentCellType)] += 10;
            Vector2 curPosition = !isRow ? new Vector2(mainAxisCurrentValue, 5) : new Vector2(5, mainAxisCurrentValue);
            var needPosition = _raycastCamera.WorldToScreenPoint(_cells[(int)curPosition.x,(int)curPosition.y].transform.position);

            ShowFloatingText(ResourcesUtils.ResourceByCellType(currentCellType).ToString() + " +10", needPosition,100);

        }
        else
            Debug.Log("not full same");
    }
    private void DestroyCell(int x, int y)
    {
      /*  RectTransformUtility.ScreenPointToLocalPointInRectangle(
          _mainCanvasRectTransform.transform as RectTransform,
          _mainCamera.WorldToScreenPoint(_cells[x, y].transform.position),
          _mainCamera,
          out Vector2 canvasPosition);*/

        var canvasPosition = _raycastCamera.WorldToScreenPoint(_cells[x, y].transform.position);

        ShowFloatingText(ResourcesUtils.ResourceByCellType(_field[x, y]).ToString(), canvasPosition,50);

        _field[x, y] = CellType.Empty;
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
        _fieldSize = 10;
        _field = new CellType[_fieldSize, _fieldSize];
        _cells = new CellView[_fieldSize, _fieldSize];
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
    public CellType Type;
}

[SerializeField]
public enum CellType
{
    Empty = 0,
    Forest,
    Village,
    Mountain
}