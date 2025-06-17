using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

public class GameManager : BaseManager, IResetable
{
    public static GameManager Instance;

    public MainGameConfig MainGameConfig;

    private List<PieceData> _nextBlocks = new List<PieceData>();

    [field: SerializeField] public Transform HolesForBGContainer { get; private set; }
    [field: SerializeField] public Transform BlackBGContainer { get; private set; }

    private List<Vector2> _cellsToDestroy = new List<Vector2>();

    [SerializeField] private TaskUIView[] _taskUIViews;

    [SerializeField] private Transform _downUITransform;

    [SerializeField] private FloatingTextView _floatingTextPrefab;

    [SerializeField] private Transform _floatingTextContainer;

    [SerializeField] public RectTransform BgTasksImage;

    [field: SerializeField] public Transform OpenedDoorEndGame;

    [field: SerializeField] public TMP_Text _mainTextUp { get; private set; }

    private LevelConfig _currentLevelConfig;

    [SerializeField] private SpawnedForOneCharTextView _characterInfoTextHelper;

    private List<TaskInfoAndUI> _currentTasks;

    private Dictionary<ResourceType, int> _monoLinesCount;

    private Dictionary<CellType, int> _placedCellsCount;

    private List<CraftingCellInfo> _currentCraftedCells = new List<CraftingCellInfo>();

    public static UnityEvent OnCellIsPlaced = new UnityEvent();
    //  public Vector3 ScreenToWorldPoint => _raycastCamera.ScreenToWorldPoint(Input.mousePosition);


    private int _placedPiecesAmount;

    private ObjectPool<FloatingTextView> _floatingTextsPool;

    public List<CellTypeInfo> CurrentGuaranteedFirstCells;
    public GameData GameData { get; private set; }

    protected override void Awake()
    {
        base.Awake();   
        Instance = this;
        _floatingTextsPool =
            new ObjectPool<FloatingTextView>(() => Instantiate(_floatingTextPrefab, _floatingTextContainer));
        _screenRatio = (float)Screen.width / Screen.height;
    }

    public void Reset()
    {
        throw new NotImplementedException();
    }

    protected override void Start()
    {
        base.Start();
        //  CameraContainer.position = new Vector3(CameraContainer.position.x,
        //     CameraContainer.position.y / (_screenRatio / 0.486f), CameraContainer.position.z);
    }

    private void GenerateField()
    {
    }

    private void GenerateTask()
    {
    }

    private void StartGame()
    {
    }

    public void GenerateNewPieces()
    {
        _nextBlocks = new List<PieceData>()
        {
            PieceUtils.GetNewPiece(),
            PieceUtils.GetNewPiece(),
            PieceUtils.GetNewPiece()
        };
        NextPiecesView.Instance.SetData(_nextBlocks);
    }


    public override void PlacePiece(PieceData pieceData)
    {
        PlacePiece(pieceData, GetPieceClampedPosOnField(), MainGameConfig.FieldSize);
        _nextBlocks.Remove(pieceData);
        _placedPiecesAmount++;

        if (MainGameConfig.resourceOnPlaceCell)
            CollectResourcesOnPlace(pieceData);

        ExplodeCells();

        if (CheckWin())
            return;

        if (_placedPiecesAmount % 3 == 0)
            GenerateNewPieces();

        if (CheckLose())
            Lose();
    }

    protected override void PlacePiece(PieceData pieceData, Vector2Int pos, int fieldSize)
    {
        base.PlacePiece(pieceData, pos, fieldSize);

        CheckPlacedCellsForTask();

        if (StorageManager.GameDataMain.CurMaxLevel == 0)
            OnCellIsPlaced.Invoke();
    }

    protected override void SpawnResourceFx(PieceData pieceData, Vector2Int place, CellView go)
    {
        var cellType = _field[place.x, place.y];
        var resourcesForPlace =
            Instance.MainGameConfig.CellsConfigs.First(c => c.CellType == cellType).ResourcesForPlace;
        var onCanvasPosition = _mainCamera.WorldToScreenPoint(go.transform.position);
        for (int i = 0; i < resourcesForPlace.Length; i++)
        {
            bool isShortAnimation = true;
            for (int j = 0; j < _currentTasks.Count; j++)
            {
                if (isShortAnimation && _currentTasks[j].TaskInfo.taskType == TaskInfo.TaskType.getResource)
                {
                    if (_currentTasks[j].TaskInfo.NeedResource == ResourceType.None ||
                        (_currentTasks[j].TaskInfo.NeedResource == resourcesForPlace[i].ResourceType))
                    {
                        ShowFloatingText((" +" + resourcesForPlace[i].ResourceCount + " <sprite name=" +
                                          resourcesForPlace[i].ResourceType +
                                          ">" + " "), new Vector2(onCanvasPosition.x, onCanvasPosition.y + (i * 15)),
                            20, 1, _currentTasks[j].TaskUIView.CurrentTaskInfo.transform.position);
                        isShortAnimation = false;
                    }
                }
            }

            if (isShortAnimation)
                ShowFloatingText((" +" + resourcesForPlace[i].ResourceCount + " <sprite name=" +
                                  resourcesForPlace[i].ResourceType +
                                  ">" + " "), new Vector2(onCanvasPosition.x, onCanvasPosition.y + (i * 15)), 20, 1,
                    Vector2.zero);
        }

        if (!_placedCellsCount.TryAdd(pieceData.Type.CellType, 1))
            _placedCellsCount[pieceData.Type.CellType]++;
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

                var needResources = placedPiece.Type.ResourcesForPlace;
                for (int i = 0; i < needResources.Length; i++)
                {
                    var resourceType = needResources[i];
                    if (resourceType == null)
                    {
                        continue;
                    }

                    if (!GameData.CollectedResources.TryAdd(resourceType.ResourceType, resourceType.ResourceCount))
                        GameData.CollectedResources[resourceType.ResourceType] += resourceType.ResourceCount;
                }
            }
        }

        CheckResourceCountForTasks();
    }

    private void CheckResourceCountForTasks()
    {
        for (int i = 0; i < _currentTasks.Count; i++)
        {
            if (_currentTasks[i].TaskInfo.taskType == TaskInfo.TaskType.getResource)
            {
                if (_currentTasks[i].TaskInfo.NeedResource == ResourceType.None &&
                    GameData.CollectedResources.Count != 0)
                {
                    ResourceType maxResourceType = ResourceType.None;
                    foreach (var resource in GameData.CollectedResources)
                    {
                        if (maxResourceType == ResourceType.None ||
                            GameData.CollectedResources[maxResourceType] < resource.Value)
                            maxResourceType = resource.Key;
                    }

                    if (_currentTasks[i].TaskInfo.Count <= GameData.CollectedResources[maxResourceType])
                    {
                        _currentTasks[i].TaskUIView.CompleteTask();
                        _currentTasks.RemoveAt(i);
                        break;
                    }
                }
                else if (GameData.CollectedResources.TryGetValue(_currentTasks[i].TaskInfo.NeedResource,
                             out int resourceCount))
                {
                    if (resourceCount >= _currentTasks[i].TaskInfo.Count)
                    {
                        _currentTasks[i].TaskUIView.CompleteTask();
                        _currentTasks.RemoveAt(i);
                    }
                }
            }
        }
    }

    private void CheckPlacedCellsForTask()
    {
        for (int i = 0; i < _currentTasks.Count; i++)
        {
            if (_currentTasks[i].TaskInfo.taskType == TaskInfo.TaskType.placeNeedCell &&
                _placedCellsCount.TryGetValue(_currentTasks[i].TaskInfo.NeedCell.CellType, out int count))
            {
                if (_currentTasks[i].TaskInfo.Count <= count)
                {
                    _currentTasks[i].TaskUIView.CompleteTask();
                    VibrationsManager.Instance.SpawnVibration(VibrationType.Win);
                    _currentTasks.RemoveAt(i);
                }
            }
        }
    }

    private void CheckMonoLinesForTasks()
    {
        for (int i = 0; i < _currentTasks.Count; i++)
        {
            if (_currentTasks[i].TaskInfo.taskType == TaskInfo.TaskType.placeMonoLine &&
                _monoLinesCount.TryGetValue(_currentTasks[i].TaskInfo.NeedResource, out int count))
            {
                // if (_currentTasks[i].taskInfo.needResource == count)
                // {
                //_currentTasks[i].taskUIView.AddTextAnimation();
                //_currentTasks[i].taskUIView.currentTaskValue.text = count + " / " + _currentTasks[i].taskInfo.count;
                if (_currentTasks[i].TaskInfo.Count <= count)
                {
                    _currentTasks[i].TaskUIView.CompleteTask();
                    _currentTasks.RemoveAt(i);
                    i--;
                }
                //}
            }
        }
    }

    private void CheckUnlockedCellForTask(CellTypeInfo needCell)
    {
        for (int i = 0; i < _currentTasks.Count; i++)
        {
            if (_currentTasks[i].TaskInfo.taskType == TaskInfo.TaskType.unlockCell &&
                _currentTasks[i].TaskInfo.NeedCell == needCell)
            {
                //_currentTasks[i].taskUIView.currentTaskValue.text = "1/1";
                // _currentTasks[i].taskUIView.AddTextAnimation(1);
                _currentTasks[i].TaskUIView.CompleteTask();
                _currentTasks.RemoveAt(i);
            }
        }
    }

    private void ExplodeCells()
    {
        int width = _field.GetLength(0);
        int height = _field.GetLength(1);
        string unlockedCellText = "";
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
                DestroyLine(y, width, true, ref unlockedCellText);
            }
        }

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
                DestroyLine(x, height, false, ref unlockedCellText);
            }
        }

        if (unlockedCellText != "")
            ShowFloatingText(unlockedCellText + " is unlocked!", _floatingTextContainer.position, 40, 2.5f,
                Vector2.zero);
        DestroyAllMarkedCells();
    }

    private void DestroyAllMarkedCells()
    {
        for (int i = 0; i < _cellsToDestroy.Count; i++)
        {
            var cell = _cellsToDestroy[i];
            _cellsToDestroy.RemoveAt(i--);
            DestroyCell((int)cell.x, (int)cell.y);
        }
    }

    private void DestroyLine(int mainAxisCurrentValue, int secondAxisLenght, bool isRow,
        ref string unlockedCellText) //cut this to pieces
    {
        bool fullSameResourcesColumn = MainGameConfig.bonusResourcesOnDestroyLine ? true : false;
        int bonusResourcesOnDestroyLine = 0;
        ResourceType currentBonusResourceType = ResourceType.None;
        Dictionary<ResourceType, int> resourcesMultiplayers = new Dictionary<ResourceType, int>();
        Dictionary<CellType, int> cellTypesInLine = new Dictionary<CellType, int>();
        ResourceType currentResourceType = ResourceType.None;
        fullSameResourcesColumn = true;
        for (int secondAxis = 0; secondAxis < secondAxisLenght; secondAxis++)
        {
            Vector2 curPosition = !isRow
                ? new Vector2(mainAxisCurrentValue, secondAxis)
                : new Vector2(secondAxis, mainAxisCurrentValue);

            var cellType = _field[(int)curPosition.x, (int)curPosition.y];
            var config = Instance.MainGameConfig.CellsConfigs.First(c => c.CellType == cellType);
            if (fullSameResourcesColumn)
            {
                if (currentResourceType == ResourceType.None)
                {
                    if (config.ResourcesForDestroy.Length == 0)
                        fullSameResourcesColumn = false;
                    else
                        currentResourceType = config.ResourcesForDestroy[0].ResourceType;
                }
                else if (config.ResourcesForDestroy.Length == 0 ||
                         config.ResourcesForDestroy[0].ResourceType != currentResourceType)
                    fullSameResourcesColumn = false;
            }

            if (config.MultiplayerForSameResourceType != 0 &&
                !resourcesMultiplayers.TryAdd(config.ResourcesForDestroy[0].ResourceType,
                    config.MultiplayerForSameResourceType))
            {
                if (config.MultiplayerForSameResourceType <
                    resourcesMultiplayers[config.ResourcesForDestroy[0].ResourceType])
                    resourcesMultiplayers[config.ResourcesForDestroy[0].ResourceType] =
                        config.MultiplayerForSameResourceType;
            }
        }

        for (int secondAxis = 0; secondAxis < secondAxisLenght; secondAxis++)
        {
            bonusResourcesOnDestroyLine = CheckLineAndDestroyNeededCells(mainAxisCurrentValue, isRow, secondAxis,
                cellTypesInLine, resourcesMultiplayers, fullSameResourcesColumn, bonusResourcesOnDestroyLine,
                ref currentBonusResourceType);
        }

        if (fullSameResourcesColumn && currentBonusResourceType != ResourceType.None)
        {
            if (!_monoLinesCount.TryAdd(currentBonusResourceType, 1))
                _monoLinesCount[currentBonusResourceType]++;
            CheckMonoLinesForTasks();
            GameData.CollectedResources[currentBonusResourceType] += bonusResourcesOnDestroyLine;
            Vector2 curPosition = !isRow ? new Vector2(mainAxisCurrentValue, 5) : new Vector2(5, mainAxisCurrentValue);
            var needPosition =
                _mainCamera.WorldToScreenPoint(_cells[(int)curPosition.x, (int)curPosition.y].transform.position);

            ShowFloatingText("<sprite name=" + currentBonusResourceType + "> " + bonusResourcesOnDestroyLine,
                needPosition, 30,
                1.5f, Vector2.zero);
        }
        else
            Debug.Log("not full same");

        TryCraftNewCells(ref unlockedCellText, cellTypesInLine);

        CheckResourceCountForTasks();
    }

    private int CheckLineAndDestroyNeededCells(int mainAxisCurrentValue, bool isRow, int secondAxis,
        Dictionary<CellType, int> cellTypesInLine, Dictionary<ResourceType, int> resourcesMultiplayers,
        bool fullSameResourcesColumn,
        int bonusResourcesOnDestroyLine, ref ResourceType currentBonusResourceType)
    {
        Vector2 curPosition = !isRow
            ? new Vector2(mainAxisCurrentValue, secondAxis)
            : new Vector2(secondAxis, mainAxisCurrentValue);
        var cellType = _field[(int)curPosition.x, (int)curPosition.y];
        var config = Instance.MainGameConfig.CellsConfigs.First(c => c.CellType == cellType);
        //string floatingText = "+ ";

        if (!cellTypesInLine.TryAdd(cellType, 1))
            cellTypesInLine[cellType]++;

        var canvasPosition =
            _mainCamera.WorldToScreenPoint(_cells[(int)curPosition.x, (int)curPosition.y].transform
                .position);

        for (int i = 0; i < config.ResourcesForDestroy.Length; i++)
        {
            resourcesMultiplayers.TryGetValue(config.ResourcesForDestroy[i].ResourceType,
                out int resourceMultiplayer);
            if (resourceMultiplayer == 0)
                resourceMultiplayer = 1;
            int count = config.ResourcesForDestroy[i].ResourceCount * resourceMultiplayer;
            if (!GameData.CollectedResources.TryAdd(config.ResourcesForDestroy[i].ResourceType,
                    count))
                GameData.CollectedResources[config.ResourcesForDestroy[i].ResourceType] += count;
            // floatingText += " <sprite name=" + config.ResourcesForDestroy[i].ResourceType + "> " + count +
            //                " ";
            if (fullSameResourcesColumn)
            {
                bonusResourcesOnDestroyLine +=
                    config.ResourcesForDestroy[i]
                        .ResourceCount; //fix this if on destroy resources types be more than 1;
                currentBonusResourceType = config.ResourcesForDestroy[i].ResourceType;
            }


            bool isShortAnimation = true;
            for (int j = 0; j < _currentTasks.Count; j++)
            {
                if (isShortAnimation && _currentTasks[j].TaskInfo.taskType == TaskInfo.TaskType.getResource)
                {
                    if (_currentTasks[j].TaskInfo.NeedResource == ResourceType.None ||
                        (_currentTasks[j].TaskInfo.NeedResource == config.ResourcesForDestroy[i].ResourceType))
                    {
                        ShowFloatingText((" +" + count + " <sprite name=" + config.ResourcesForDestroy[i].ResourceType +
                                          ">" + " "), new Vector2(canvasPosition.x, canvasPosition.y + (i * 15)), 20, 1,
                            _currentTasks[j].TaskUIView.CurrentTaskInfo.transform.position);
                        isShortAnimation = false;
                    }
                }
            }

            if (isShortAnimation)
                ShowFloatingText((" +" + count + " <sprite name=" + config.ResourcesForDestroy[i].ResourceType +
                                  ">" + " "), new Vector2(canvasPosition.x, canvasPosition.y + (i * 15)), 20, 1,
                    Vector2.zero);
        }

        _cellsToDestroy.Add(new Vector2(curPosition.x, curPosition.y));
        return bonusResourcesOnDestroyLine;
    }

    private void TryCraftNewCells(ref string unlockedCellText, Dictionary<CellType, int> cellTypesInLine)
    {
        for (int i = 0; i < _currentCraftedCells.Count; i++)
        {
            bool addNewCell = false;
            for (int j = 0; j < _currentCraftedCells[i].CellTypeToCraft.Length; j++)
            {
                if (cellTypesInLine.ContainsKey(_currentCraftedCells[i].CellTypeToCraft[j]))
                {
                    for (int x = 0; x < _currentCraftedCells[i].CellTypeToCraftSecond.Length; x++)
                    {
                        if (cellTypesInLine.ContainsKey(_currentCraftedCells[i].CellTypeToCraftSecond[x]))
                        {
                            _currentCellsToSpawn.Add(_currentCraftedCells[i].CellsToCraft);
                            CheckUnlockedCellForTask(_currentCraftedCells[i].CellsToCraft);
                            unlockedCellText += _currentCraftedCells[i].CellsToCraft.CellName + "\n";

                            _currentCraftedCells.RemoveAt(i);
                            i--;
                            addNewCell = true;
                            CalculateCellSpawnChances();
                            break;
                        }
                    }

                    if (addNewCell) break;
                }
            }
        }
    }

    private void DestroyCell(int x, int y)
    {
        _field[x, y] = CellType.Empty;
        _cells[x, y].DestroyCell();
    }

    private bool CheckWin()
    {
        if (_currentTasks.Count == 0)
        {
            Win();
            return true;
        }

        return false;
    }

    private bool CheckLose()
    {
        foreach (PieceData t in _nextBlocks)
        {
            if (PieceUtils.CanPlacePiece(_field, _nextBlocks[0].Cells))
                return false;
        }

        return true;
    }

    private void Win()
    {
        _mainTextUp.text = "You win!";
        foreach (var taskUI in _taskUIViews) {
            taskUI.gameObject.SetActive(false);
        }

        
        VibrationsManager.Instance.SpawnVibration(VibrationType.Win);
        GoalView.Instance.SetWinState();
    }

    private void Lose()
    {
        _mainTextUp.text = "You lose:(";
        foreach (var taskUI in _taskUIViews) {
            taskUI.gameObject.SetActive(false);
        }
        
        
        VibrationsManager.Instance.SpawnVibration(VibrationType.Lose);
        GoalView.Instance.SetLoseState();
    }

    public void RemoveHealthAfterLose()
    {
        RemoveHealth();
    }
    
    public void Restart()
    {
        if(StorageManager.GameDataMain.HealthCount != 0)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        else
        {
            
            //floating window with "watch ad and get health"
        }
     
    }

    private void CalculateCellSpawnChances()
    {
        float lastChance = 0;
        CellsChanceToSpawn = new float[_currentCellsToSpawn.Count];
        for (int i = 0; i < _currentCellsToSpawn.Count; i++)
        {
            lastChance += _currentCellsToSpawn[i].ChanceToSpawn;
            CellsChanceToSpawn[i] = lastChance;
        }
    }

    protected override void SetupGame()
    {
        CalculateFiguresSpawnChances();
        GenerateField();
        GenerateTask();
        StartGame();
        if (StorageManager.GameDataMain.CurMaxLevel < 20)
            _currentLevelConfig = MainGameConfig.Levels[StorageManager.GameDataMain.CurMaxLevel];
        else
            Debug.Log("meta");

        _placedPiecesAmount = 0;
        _field = new CellType[MainGameConfig.FieldSize, MainGameConfig.FieldSize];
        _cells = new CellView[MainGameConfig.FieldSize, MainGameConfig.FieldSize];

        _currentCraftedCells = new List<CraftingCellInfo>();
        foreach (var craftedCell in MainGameConfig.CellsToCraft)
            _currentCraftedCells.Add(craftedCell);

        if (_currentLevelConfig.TutorialObject != null)
            Instantiate(_currentLevelConfig.TutorialObject);

        var startCells = _currentLevelConfig.CellTypesTableConfig;
        _currentCellsToSpawn = new List<CellTypeInfo>();
        for (int i = 0; i < startCells.CellsToSpawn.Length; i++)
            _currentCellsToSpawn.Add(startCells.CellsToSpawn[i]);
        CalculateCellSpawnChances();

        _placedCellsCount = new Dictionary<CellType, int>();

        _currentTasks = new List<TaskInfoAndUI>();

        SetTaskDescriptions(_currentLevelConfig.Tasks);

        _monoLinesCount = new Dictionary<ResourceType, int>();
        StartCoroutine(_characterInfoTextHelper.StartSpawnText(_currentLevelConfig.GuideForLevelText));
        CurrentGuaranteedFirstCells = new List<CellTypeInfo>();
        foreach (var cellInfo in _currentLevelConfig.CurrentGuaranteedFirstCells)
            CurrentGuaranteedFirstCells.Add(cellInfo);

        GameData = new GameData();

        GenerateNewPieces();
        base.SetupGame();
    }

    private void SetTaskDescriptions(TaskInfo[] tasksArray)
    {
        for (int i = 0; i < _currentLevelConfig.Tasks.Length; i++)
        {
            var task = _currentLevelConfig.Tasks[i];
            var taskUI = _taskUIViews[i];
            taskUI.gameObject.SetActive(true);
            _currentTasks.Add(new TaskInfoAndUI(task, taskUI));
            string needTasktext = "";
            switch (task.taskType)
            {
                case TaskInfo.TaskType.getResource:

                    if (task.NeedResource == ResourceType.None)
                        needTasktext = " Get " + task.Count + " of any resource";
                    else
                        needTasktext = " Get " + task.Count + " <sprite name=" + task.NeedResource + ">";
                    break;

                case TaskInfo.TaskType.placeMonoLine:

                    needTasktext = " Place mono line " + task.Count + " times with " + " <sprite name=" +
                                   task.NeedResource + ">";
                    break;

                case TaskInfo.TaskType.placeNeedCell:

                    needTasktext = " Place " + task.NeedCell.CellName + " " + task.Count + " times";
                    break;

                case TaskInfo.TaskType.unlockCell:

                    needTasktext = " Unlock " + task.NeedCell.CellName;
                    break;
            }

            StartCoroutine(taskUI.TaskInfoTextHelper.StartSpawnText(needTasktext));
        }
    }

    public void ShowFloatingText(string needText, Vector2 newPosition, float textSize, float showTime,
        Vector2 finalposition)
    {
        var floatingText = _floatingTextsPool.Get();
        floatingText.SetText(newPosition, needText, textSize, showTime, finalposition);
    }

    public void ReleaseFloatingText(FloatingTextView needTextObject)
    {
        needTextObject.gameObject.SetActive(false);
        _floatingTextsPool.Release(needTextObject);
    }

    public void GoToMeta()
    {
        SceneManager.LoadScene("MetaScene");
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
    Sawmill,
    Metal,
    Smithy,
    Village,
    MiniCity,
    Mountain,
    Mine,
    FieldOfWheat,
    Farm
}