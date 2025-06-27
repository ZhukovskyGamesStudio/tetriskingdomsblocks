using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : BaseManager, IResetable {
    public static GameManager Instance;

    [field: Header("Game")]
    public MainGameConfig MainGameConfig;

    private List<PieceData> _nextBlocks = new List<PieceData>();

    [field: SerializeField]
    public Transform HolesForBGContainer { get; private set; }

    [field: SerializeField]
    public Transform BlackBGContainer { get; private set; }

    private List<Vector2Int> _cellsToDestroy = new List<Vector2Int>();

    [SerializeField]
    private TaskUIView[] _taskUIViews;

    [SerializeField]
    private Transform _downUITransform;

    [SerializeField]
    private FloatingTextView _floatingTextPrefab;

    [SerializeField]
    private Transform _floatingTextContainer;

    [SerializeField]
    public RectTransform BgTasksImage;

    [field: SerializeField]
    public Transform OpenedDoorEndGame;

    [field: SerializeField]
    public TMP_Text _mainTextUp { get; private set; }

    [SerializeField]
    private TMP_Text _currentMovesCountText;

    [SerializeField]
    private SpawnedForOneCharTextView _characterInfoTextHelper;

    public Action OnCellPlaced;

    private LevelConfig _currentLevelConfig;
    private List<TaskInfoAndUI> _currentTasks;

    private Dictionary<ResourceType, int> _monoLinesCount;

    private Dictionary<CellType, int> _placedCellsCount;

    private List<CraftingCellInfo> _currentCraftedCells = new List<CraftingCellInfo>();

    private int _currentMovesCount;

    private int _placedPiecesAmount;

    private ObjectPool<FloatingTextView> _floatingTextsPool;

    private List<CellTypeInfo> _currentGuaranteedFirstCells;
    private GameData GameData { get; set; }
    private bool slimeIsExist;

    protected override void Awake() {
        base.Awake();
        Instance = this;
        _floatingTextsPool = new ObjectPool<FloatingTextView>(() => Instantiate(_floatingTextPrefab, _floatingTextContainer));
    }

    public void Reset() { }

    private void GenerateField() { }

    private void GenerateTask() { }

    private void StartGame() { }

    public void GenerateNewPieces() {
        _nextBlocks = new List<PieceData>() {
            PieceUtils.GetNewPiece(_currentGuaranteedFirstCells),
            PieceUtils.GetNewPiece(_currentGuaranteedFirstCells),
            PieceUtils.GetNewPiece(_currentGuaranteedFirstCells)
        };
        NextPiecesView.Instance.SetData(_nextBlocks);
    }

    public override void PlacePiece(PieceData pieceData, Vector2Int coord, CellView[,] cells, Transform cellsContainer) {
        base.PlacePiece(pieceData, coord, cells, cellsContainer);

      //  CheckPlacedCellsForTask();

        OnCellPlaced?.Invoke();
        _nextBlocks.Remove(pieceData);
        _placedPiecesAmount++;

        _currentMovesCount--;
        _currentMovesCountText.text = _currentMovesCount.ToString();

        if (MainGameConfig.resourceOnPlaceCell) {
            CollectResourcesOnPlace(pieceData);
        }
        if (slimeIsExist)
            SlimeMove();

        ExplodeCellsInRows();

        if (CheckWin()) {
            return;
        }

        if (_placedPiecesAmount % 3 == 0) {
            GenerateNewPieces();
        }

        if (CheckLose()) {
            Lose();
        }

    }

    private void SlimeMove()
    {
       /* List<List<(int row, int col)>> connectedGroupsPieces = null;
        connectedGroupsPieces = SameCellsGroupCalculater.FindConnectedCellTypeGroupsWithoutCellIndexes(_field, CellType.Slime);
        foreach (var groupPieces in connectedGroupsPieces)
        {
            if (_field[groupPieces[0].row, groupPieces[0].col] == CellType.Slime)
            {
                List<Vector2Int> emptyCellsAround = new List<Vector2Int>();
                foreach (var piece in groupPieces)
                {
                    foreach (var cell in FieldUtils.GetCellsAround(_field, new Vector2Int(piece.row, piece.col)))
                    {
                        if (_field[cell.x, cell.y] == CellType.Empty && !emptyCellsAround.Contains(cell))
                            emptyCellsAround.Add(cell);
                    }
                }

                if (emptyCellsAround.Count > 0)
                {
                    var randomEmptyCell = emptyCellsAround[Random.Range(0, emptyCellsAround.Count)];
                    var config = Instance.MainGameConfig.CellsConfigs.First(c =>
                        c.CellType ==CellType.Slime);
                    PlaceOneSizePiece(config, new Vector2Int(randomEmptyCell.x, randomEmptyCell.y));
                    //add in task
                    foreach (var task in _currentTasks)
                    {
                        if (task.TaskInfo.NeedResource == ResourceType.Slime)
                        {
                            task.needCount++;
                            if (GameData.CollectedResources.TryGetValue(task.TaskInfo.NeedResource,
                                    out int resourceCount))
                                task.TaskUIView.TaskInfoTextHelper.SetText((task.needCount - resourceCount).ToString());
                            else
                                task.TaskUIView.TaskInfoTextHelper.SetText(task.needCount.ToString());
                        }
                        
                    }
                }
            }
        }*/
       List<(Vector2Int, Vector3)> newSlimeCells = new List<(Vector2Int, Vector3)>();
       for (int i = 0; i < _field.GetLength(0); i++)
       {
           for (int j = 0; j < _field.GetLength(1); j++)
           {
               if (_field[i, j] == CellType.Slime)
               {
                   List<Vector2Int> emptyCellsAround = new List<Vector2Int>();
                   foreach (var cell in FieldUtils.GetCellsAround(_field, new Vector2Int(i, j)))
                   {
                       if (_field[cell.x, cell.y] == CellType.Empty && !emptyCellsAround.Contains(cell))
                           emptyCellsAround.Add(cell);
                   }


                   if (emptyCellsAround.Count > 0)
                   {
                       var randomEmptyCell = emptyCellsAround[Random.Range(0, emptyCellsAround.Count)];
                       newSlimeCells.Add((randomEmptyCell, _cells[i, j].transform.position));
                   }
               }
           }
       }

       foreach (var (randomEmptyCell, startPosition) in newSlimeCells)
       {
           var config = Instance.MainGameConfig.CellsConfigs.First(c =>
               c.CellType == CellType.Slime);
           PlaceOneSizePiece(config, new Vector2Int(randomEmptyCell.x, randomEmptyCell.y));
           SpawnNewSlimeAnimation(_cells[randomEmptyCell.x, randomEmptyCell.y].transform, startPosition, _cells[randomEmptyCell.x, randomEmptyCell.y].transform.position);
           foreach (var task in _currentTasks)
           {
               if (task.TaskInfo.NeedResource == ResourceType.Slime)
               {
                   task.needCount++;
                   if (GameData.CollectedResources.TryGetValue(task.TaskInfo.NeedResource,
                           out int resourceCount))
                       task.TaskUIView.TaskInfoTextHelper.SetText((task.needCount - resourceCount).ToString());
                   else
                       task.TaskUIView.TaskInfoTextHelper.SetText(task.needCount.ToString());
               }
           }
       }
    }

    private void SpawnNewSlimeAnimation(Transform cellContainer, Vector3 startPosition, Vector3 endPosition)
    {
        cellContainer.localScale = Vector3.zero;
        cellContainer.position = startPosition;
        var _currentTween = DOTween.Sequence().Append(cellContainer.DOScale(Vector3.one, 0.5f))
            .Join(cellContainer.DOMove(endPosition, 0.5f));
    }
    protected override void CheckCellTypesBeforePlacePiece(Vector2Int coord) {
        base.CheckCellTypesBeforePlacePiece(coord);
        var cellType = _field[coord.x, coord.y];

        switch (cellType) {
            case CellType.Ice:
                DestroyCellAfterPlacePiece(coord, cellType);
                break;
            case CellType.Crystal:
                
                DestroyCellAfterPlacePiece(coord, cellType);
                break;
            
            case CellType.Slime:
                
                DestroyCellAfterPlacePiece(coord, cellType);
                break;
        }
       // CheckClosestCells(coord);
    }

    private void DestroyCellAfterPlacePiece(Vector2Int coord, CellType cellType)
    {
        var configSlime = Instance.MainGameConfig.CellsConfigs.First(c => c.CellType == cellType);
        for (int j = 0; j < _currentTasks.Count; j++) {
            if (_currentTasks[j].TaskInfo.TaskType == TaskInfo.TaskType.getResource) {
                CheckNeedResourceInTask(j, configSlime, coord);
            }
        }

        DestroyCell(coord);
    }

    protected override void SpawnResourceFx(Vector2Int place, CellView go) {
        var cellType = _field[place.x, place.y];
        var resourcesForPlace = Instance.MainGameConfig.CellsConfigs.First(c => c.CellType == cellType).ResourcesForPlace;
        var onCanvasPosition = _mainCamera.WorldToScreenPoint(go.transform.position);
        for (int i = 0; i < resourcesForPlace.Length; i++) {
            bool isShortAnimation = true;
            for (int j = 0; j < _currentTasks.Count; j++) {
                if (isShortAnimation && _currentTasks[j].TaskInfo.TaskType == TaskInfo.TaskType.getResource) {
                    if (_currentTasks[j].TaskInfo.NeedResource == ResourceType.None ||
                        (_currentTasks[j].TaskInfo.NeedResource == resourcesForPlace[i].ResourceType)) {
                        ShowFloatingText(
                            (" +" + resourcesForPlace[i].ResourceCount + " <sprite name=" + resourcesForPlace[i].ResourceType + ">" + " "),
                            new Vector2(onCanvasPosition.x, onCanvasPosition.y + (i * 15)), 20, 1,
                            _currentTasks[j].TaskUIView.CurrentTaskInfo.transform.position);
                        isShortAnimation = false;
                    }
                }
            }

            if (isShortAnimation)
                ShowFloatingText((" +" + resourcesForPlace[i].ResourceCount + " <sprite name=" + resourcesForPlace[i].ResourceType + ">" + " "),
                    new Vector2(onCanvasPosition.x, onCanvasPosition.y + (i * 15)), 20, 1, Vector2.zero);
        }

        if (!_placedCellsCount.TryAdd(cellType, 1))
            _placedCellsCount[cellType]++;
    }

    private void CollectResourcesOnPlace(PieceData placedPiece) {
        for (int x = 0; x < placedPiece.Cells.GetLength(0); x++) {
            for (int y = 0; y < placedPiece.Cells.GetLength(1); y++) {
                if (!placedPiece.Cells[x, y]) {
                    continue;
                }

                var needResources = placedPiece.Type.ResourcesForPlace;
                for (int i = 0; i < needResources.Length; i++) {
                    var resourceType = needResources[i];
                    if (resourceType == null) {
                        continue;
                    }

                    if (!GameData.CollectedResources.TryAdd(resourceType.ResourceType, resourceType.ResourceCount))
                        GameData.CollectedResources[resourceType.ResourceType] += resourceType.ResourceCount;
                }
            }
        }

        CheckResourceCountForTasks();
    }

    private void CheckResourceCountForTasks() {
        for (int i = 0; i < _currentTasks.Count; i++) {
            if (_currentTasks[i].TaskInfo.TaskType == TaskInfo.TaskType.getResource) {
                if (_currentTasks[i].TaskInfo.NeedResource == ResourceType.None && GameData.CollectedResources.Count != 0) {
                    ResourceType maxResourceType = ResourceType.None;
                    foreach (var resource in GameData.CollectedResources) {
                        if (maxResourceType == ResourceType.None || GameData.CollectedResources[maxResourceType] < resource.Value)
                            maxResourceType = resource.Key;
                    }
                    _currentTasks[i].TaskUIView.TaskInfoTextHelper.SetText((_currentTasks[i].needCount - GameData.CollectedResources[maxResourceType]).ToString());
                    if (_currentTasks[i].needCount <= GameData.CollectedResources[maxResourceType]) {
                        _currentTasks[i].TaskUIView.CompleteTask();
                        _currentTasks.RemoveAt(i);
                        break;
                    }
                } else if (GameData.CollectedResources.TryGetValue(_currentTasks[i].TaskInfo.NeedResource, out int resourceCount)) {
                    _currentTasks[i].TaskUIView.TaskInfoTextHelper.SetText((_currentTasks[i].needCount - resourceCount).ToString());
                    if (resourceCount >= _currentTasks[i].needCount) {
                        _currentTasks[i].TaskUIView.CompleteTask();
                        _currentTasks.RemoveAt(i);
                    }
                }
            }
        }
    }

   /* private void CheckPlacedCellsForTask() {
        for (int i = 0; i < _currentTasks.Count; i++) {
            if (_currentTasks[i].TaskInfo.taskType == TaskInfo.TaskType.placeNeedCell &&
                _placedCellsCount.TryGetValue(_currentTasks[i].TaskInfo.NeedCell.CellType, out int count)) {
                if (_currentTasks[i].TaskInfo.Count <= count) {
                    _currentTasks[i].TaskUIView.CompleteTask();
                    VibrationsManager.Instance.SpawnContinuous(0.46f, 0.24f, 0.2f);
                    _currentTasks.RemoveAt(i);
                }
            }
        }
    }*/

    private void CheckMonoLinesForTasks() {
        for (int i = 0; i < _currentTasks.Count; i++) {
            if (_currentTasks[i].TaskInfo.TaskType == TaskInfo.TaskType.placeMonoLine &&
                _monoLinesCount.TryGetValue(_currentTasks[i].TaskInfo.NeedResource, out int count)) {
                if (_currentTasks[i].TaskInfo.Count <= count) {
                    _currentTasks[i].TaskUIView.CompleteTask();
                    _currentTasks.RemoveAt(i);
                    i--;
                }
            }
        }
    }

  /*  private void CheckUnlockedCellForTask(CellTypeInfo needCell) {
        for (int i = 0; i < _currentTasks.Count; i++) {
            if (_currentTasks[i].TaskInfo.taskType == TaskInfo.TaskType.unlockCell && _currentTasks[i].TaskInfo.NeedCell == needCell) {
                _currentTasks[i].TaskUIView.CompleteTask();
                _currentTasks.RemoveAt(i);
            }
        }
    }*/

    private void ExplodeCellsInRows() {
        int width = _field.GetLength(0);
        int height = _field.GetLength(1);
        string unlockedCellText = "";
        bool isDestroying = false;
        for (int y = 0; y < height; y++) {
            if (Enumerable.Range(0, width).All(x => !FieldUtils.CantBecomeRow(_field[x, y]))) {
                DestroyLine(y, width, true, ref unlockedCellText);
                isDestroying = true;
            }
        }

        for (int x = 0; x < width; x++) {
            if (Enumerable.Range(0, height).All(y => !FieldUtils.CantBecomeRow(_field[x, y]))) {
                DestroyLine(x, height, false, ref unlockedCellText);
                isDestroying = true;
            }
        }

        if (isDestroying) {
            SpawnDestroyRowVibration();
        }

        if (unlockedCellText != "") {
            ShowFloatingText(unlockedCellText + " is unlocked!", _floatingTextContainer.position, 40, 2.5f, Vector2.zero);
        }

        DestroyAllMarkedCells();
    }

    private static void SpawnDestroyRowVibration() {
        if (Random.Range(0, 2) == 0) {
            VibrationsManager.Instance.SpawnVibration(VibrationType.AllRow);
        } else {
            VibrationsManager.Instance.SpawnVibrationEmhpasis(1, 1);
        }
    }

    private void DestroyAllMarkedCells() {
        for (int i = 0; i < _cellsToDestroy.Count; i++) {
            var cell = _cellsToDestroy[i];
            _cellsToDestroy.RemoveAt(i--);
            if (!FieldUtils.CantDestroyInRow(_field[cell.x, cell.y]))
                DestroyCell(cell);
        }
    }

    private void DestroyLine(int mainAxisCurrentValue, int secondAxisLenght, bool isRow, ref string unlockedCellText) //cut this to pieces
    {
        bool fullSameResourcesColumn = MainGameConfig.bonusResourcesOnDestroyLine ? true : false;
        int bonusResourcesOnDestroyLine = 0;
        ResourceType currentBonusResourceType = ResourceType.None;
        Dictionary<ResourceType, int> resourcesMultiplayers = new Dictionary<ResourceType, int>();
        Dictionary<CellType, int> cellTypesInLine = new Dictionary<CellType, int>();
        ResourceType currentResourceType = ResourceType.None;
        fullSameResourcesColumn = true;
        for (int secondAxis = 0; secondAxis < secondAxisLenght; secondAxis++) {
            Vector2 curPosition = !isRow ? new Vector2(mainAxisCurrentValue, secondAxis) : new Vector2(secondAxis, mainAxisCurrentValue);

            var cellType = _field[(int)curPosition.x, (int)curPosition.y];
            var config = Instance.MainGameConfig.CellsConfigs.First(c => c.CellType == cellType);
            if (fullSameResourcesColumn) {
                if (currentResourceType == ResourceType.None) {
                    if (config.ResourcesForDestroy.Length == 0)
                        fullSameResourcesColumn = false;
                    else
                        currentResourceType = config.ResourcesForDestroy[0].ResourceType;
                } else if (config.ResourcesForDestroy.Length == 0 || config.ResourcesForDestroy[0].ResourceType != currentResourceType)
                    fullSameResourcesColumn = false;
            }

            if (config.MultiplayerForSameResourceType != 0 &&
                !resourcesMultiplayers.TryAdd(config.ResourcesForDestroy[0].ResourceType, config.MultiplayerForSameResourceType)) {
                if (config.MultiplayerForSameResourceType < resourcesMultiplayers[config.ResourcesForDestroy[0].ResourceType])
                    resourcesMultiplayers[config.ResourcesForDestroy[0].ResourceType] = config.MultiplayerForSameResourceType;
            }
        }

        for (int secondAxis = 0; secondAxis < secondAxisLenght; secondAxis++) {
            bonusResourcesOnDestroyLine = CheckLineAndDestroyNeededCells(mainAxisCurrentValue, isRow, secondAxis, cellTypesInLine,
                resourcesMultiplayers, fullSameResourcesColumn, bonusResourcesOnDestroyLine, ref currentBonusResourceType);
        }

        if (fullSameResourcesColumn && currentBonusResourceType != ResourceType.None) {
            if (!_monoLinesCount.TryAdd(currentBonusResourceType, 1))
                _monoLinesCount[currentBonusResourceType]++;
            CheckMonoLinesForTasks();
            GameData.CollectedResources[currentBonusResourceType] += bonusResourcesOnDestroyLine;
            Vector2 curPosition = !isRow ? new Vector2(mainAxisCurrentValue, 5) : new Vector2(5, mainAxisCurrentValue);
            var needPosition = _mainCamera.WorldToScreenPoint(_cells[(int)curPosition.x, (int)curPosition.y].transform.position);

            ShowFloatingText("<sprite name=" + currentBonusResourceType + "> " + bonusResourcesOnDestroyLine, needPosition, 30, 1.5f,
                Vector2.zero);
        } else
            Debug.Log("not full same");

        TryCraftNewCells(ref unlockedCellText, cellTypesInLine);

        CheckResourceCountForTasks();
    }

    private int CheckLineAndDestroyNeededCells(int mainAxisCurrentValue, bool isRow, int secondAxis, Dictionary<CellType, int> cellTypesInLine,
        Dictionary<ResourceType, int> resourcesMultiplayers, bool fullSameResourcesColumn, int bonusResourcesOnDestroyLine,
        ref ResourceType currentBonusResourceType) {
        Vector2Int curPosition = !isRow ? new Vector2Int(mainAxisCurrentValue, secondAxis) : new Vector2Int(secondAxis, mainAxisCurrentValue);
        var cellType = _field[(int)curPosition.x, (int)curPosition.y];
        var config = Instance.MainGameConfig.CellsConfigs.First(c => c.CellType == cellType);
        //string floatingText = "+ ";

        if (!cellTypesInLine.TryAdd(cellType, 1))
            cellTypesInLine[cellType]++;

        var canvasPosition = _mainCamera.WorldToScreenPoint(_cells[(int)curPosition.x, (int)curPosition.y].transform.position);

        for (int i = 0; i < config.ResourcesForDestroy.Length; i++) {
            resourcesMultiplayers.TryGetValue(config.ResourcesForDestroy[i].ResourceType, out int resourceMultiplayer);
            if (resourceMultiplayer == 0)
                resourceMultiplayer = 1;
            int count = config.ResourcesForDestroy[i].ResourceCount * resourceMultiplayer;
            if (!GameData.CollectedResources.TryAdd(config.ResourcesForDestroy[i].ResourceType, count))
                GameData.CollectedResources[config.ResourcesForDestroy[i].ResourceType] += count;
            // floatingText += " <sprite name=" + config.ResourcesForDestroy[i].ResourceType + "> " + count +
            //                " ";
            if (fullSameResourcesColumn) {
                bonusResourcesOnDestroyLine +=
                    config.ResourcesForDestroy[i].ResourceCount; //fix this if on destroy resources types be more than 1;
                currentBonusResourceType = config.ResourcesForDestroy[i].ResourceType;
            }

            bool isShortAnimation = true;
            for (int j = 0; j < _currentTasks.Count; j++) {
                if (isShortAnimation && _currentTasks[j].TaskInfo.TaskType == TaskInfo.TaskType.getResource) {
                    if (_currentTasks[j].TaskInfo.NeedResource == ResourceType.None ||
                        (_currentTasks[j].TaskInfo.NeedResource == config.ResourcesForDestroy[i].ResourceType)) {
                        ShowFloatingText((" +" + count + " <sprite name=" + config.ResourcesForDestroy[i].ResourceType + ">" + " "),
                            new Vector2(canvasPosition.x, canvasPosition.y + (i * 15)), 20, 1,
                            _currentTasks[j].TaskUIView.CurrentTaskInfo.transform.position);
                        isShortAnimation = false;
                    }
                }
            }

            if (isShortAnimation)
                ShowFloatingText((" +" + count + " <sprite name=" + config.ResourcesForDestroy[i].ResourceType + ">" + " "),
                    new Vector2(canvasPosition.x, canvasPosition.y + (i * 15)), 20, 1, Vector2.zero);
        }

        _cellsToDestroy.Add(curPosition);
        return bonusResourcesOnDestroyLine;
    }

    private void TryCraftNewCells(ref string unlockedCellText, Dictionary<CellType, int> cellTypesInLine) {
        for (int i = 0; i < _currentCraftedCells.Count; i++) {
            bool addNewCell = false;
            for (int j = 0; j < _currentCraftedCells[i].CellTypeToCraft.Length; j++) {
                if (cellTypesInLine.ContainsKey(_currentCraftedCells[i].CellTypeToCraft[j])) {
                    for (int x = 0; x < _currentCraftedCells[i].CellTypeToCraftSecond.Length; x++) {
                        if (cellTypesInLine.ContainsKey(_currentCraftedCells[i].CellTypeToCraftSecond[x])) {
                            _currentCellsToSpawn.Add(_currentCraftedCells[i].CellsToCraft);
                       //     CheckUnlockedCellForTask(_currentCraftedCells[i].CellsToCraft);
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

    protected override void CheckClosestCells(Vector2Int coord) {
        var cellsAround = FieldUtils.GetCellsAround(_field, coord);
        foreach (var coordAround in cellsAround) {
            var cellType = _field[coordAround.x, coordAround.y];
            switch (cellType) {
                case CellType.Box:
                    CellTypeInfo configBox = Instance.MainGameConfig.CellsConfigs.First(c => c.CellType == cellType);
                    for (int j = 0; j < _currentTasks.Count; j++) {
                        if (_currentTasks[j].TaskInfo.TaskType == TaskInfo.TaskType.getResource) {
                            CheckNeedResourceInTask(j, configBox, coordAround);
                        }
                    }

                    DestroyCell(coordAround);

                    break;
                case CellType.GoldMine:
                    CellTypeInfo configGoldMine = Instance.MainGameConfig.CellsConfigs.First(c => c.CellType == cellType);
                    for (int j = 0; j < _currentTasks.Count; j++) {
                        if (_currentTasks[j].TaskInfo.TaskType == TaskInfo.TaskType.getResource) {
                            CheckNeedResourceInTask(j, configGoldMine, coordAround);
                        }
                    }

                    break;
                
                case CellType.CrystalMine:
                    CellTypeInfo configCrystalMine = Instance.MainGameConfig.CellsConfigs.First(c => c.CellType == cellType);
                    for (int j = 0; j < _currentTasks.Count; j++) {
                        if (_currentTasks[j].TaskInfo.TaskType == TaskInfo.TaskType.getResource) {
                            CheckNeedResourceInTask(j, configCrystalMine, coordAround);
                        }
                    }

                    var randomPos = GetRandomEmptyCell();
                    if(randomPos == new Vector2(-1,-1))return;
                    //spawn crystal on empty cell
                    var configCrystal = Instance.MainGameConfig.CellsConfigs.First(c =>
                        c.CellType == CellType.Crystal);
                    PlaceOneSizePiece(configCrystal, new Vector2Int(randomPos.x, randomPos.y));
                    break;
            }
        }
    }

    private Vector2Int GetRandomEmptyCell()
    {
        List<Vector2Int> _emptyCells = new List<Vector2Int>();
        for (int i = 0; i < _field.GetLength(0); i++)
        {
            for (int j = 0; j < _field.GetLength(1); j++)
                if(_field[i,j] == CellType.Empty)
                _emptyCells.Add(new Vector2Int(i, j));
        }

        if (_emptyCells.Count == 0) return new Vector2Int(-1,-1);
        return _emptyCells[Random.Range(0, _emptyCells.Count)];
            
    }
    private void CheckNeedResourceInTask(int j, CellTypeInfo config, Vector2Int coord) {
        if(config.ResourcesForDestroy.Length == 0) return;
        var needResource = config.ResourcesForDestroy[0];
        if (_currentTasks[j].TaskInfo.NeedResource == needResource.ResourceType) {
            if (!GameData.CollectedResources.TryAdd(needResource.ResourceType, needResource.ResourceCount))
                GameData.CollectedResources[needResource.ResourceType] += needResource.ResourceCount;
            var canvasPosition = _mainCamera.WorldToScreenPoint(_cells[coord.x, coord.y].transform.position);
            ShowFloatingText((" + <sprite name=" + needResource.ResourceType + ">" + " "), new Vector2(canvasPosition.x, canvasPosition.y + 15),
                20, 1, _currentTasks[j].TaskUIView.CurrentTaskInfo.transform.position);
        }
    }

    private void DestroyCell(Vector2Int coord) {
        _field[coord.x, coord.y] = CellType.Empty;
        _cells[coord.x, coord.y].DestroyCell();
    }

    private bool CheckWin() {
        if (_currentTasks.Count == 0) {
            Win();
            return true;
        }

        return false;
    }

    private bool CheckLose() {
        if (_currentMovesCount <= 0)
            return true;
        for (int i = 0; i < _nextBlocks.Count; i++) {
            if (_nextBlocks[i] != null && PieceUtils.CanPlacePiece(_field, _nextBlocks[i]))
                return false;
        }

        return true;
    }

    private void Win() {
        _mainTextUp.text = "You win!";
        foreach (var taskUI in _taskUIViews) {
            taskUI.gameObject.SetActive(false);
        }

        NextPiecesView.Instance.DestroyPieces();

        VibrationsManager.Instance.SpawnContinuous(0.46f, 0.24f, 0.4f);
        GoalView.Instance.SetWinState();
    }

    private void Lose() {
        _mainTextUp.text = "You lose:(";
        foreach (var taskUI in _taskUIViews) {
            taskUI.gameObject.SetActive(false);
        }

        NextPiecesView.Instance.DestroyPieces();

        VibrationsManager.Instance.SpawnContinuous(0.46f, 0.24f, 0.4f);
        GoalView.Instance.SetLoseState();
    }

    public void RemoveHealthAfterLose() {
        RemoveHealth();
    }

    public void Restart() {
        if (StorageManager.GameDataMain.HealthCount != 0)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        else {
            //floating window with "watch ad and get health"
        }
    }

    private void CalculateCellSpawnChances() {
        float lastChance = 0;
        CellsChanceToSpawn = new float[_currentCellsToSpawn.Count];
        for (int i = 0; i < _currentCellsToSpawn.Count; i++) {
            lastChance += _currentCellsToSpawn[i].ChanceToSpawn;
            CellsChanceToSpawn[i] = lastChance;
        }
    }

    protected override void SetupGame() {
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

        SetTaskDescriptions();

        _monoLinesCount = new Dictionary<ResourceType, int>();
        StartCoroutine(_characterInfoTextHelper.StartSpawnText(_currentLevelConfig.GuideForLevelText));
        _currentGuaranteedFirstCells = new List<CellTypeInfo>();
        foreach (var cellInfo in _currentLevelConfig.CurrentGuaranteedFirstCells)
            _currentGuaranteedFirstCells.Add(cellInfo);

        GameData = new GameData();
        if (_currentLevelConfig.StartFieldConfig != null)
        {
            Dictionary<ResourceType, int> startCellsResourcesCount = new Dictionary<ResourceType, int>();
            for (int i = 0; i < _field.GetLength(0); i++)
            {
                for (int j = 0; j < _field.GetLength(1); j++)
                {
                    if (_currentLevelConfig.StartFieldConfig.GetCell(i, j) != CellType.Empty)
                    {
                        var config = Instance.MainGameConfig.CellsConfigs.First(c =>
                            c.CellType == _currentLevelConfig.StartFieldConfig.GetCell(i, j));
                        if (!slimeIsExist && config.CellType == CellType.Slime)
                            slimeIsExist = true;
                        PlaceOneSizePiece(config, new Vector2Int(i, j));
                        if (!FieldUtils.CantDestroyInRow(config.CellType) && !startCellsResourcesCount.TryAdd(config.ResourcesForDestroy[0].ResourceType, 1))
                            startCellsResourcesCount[config.ResourcesForDestroy[0].ResourceType]++;
                    }
                }
            }

            
            SetTaskDescriptionsFromStartField(startCellsResourcesCount);
        }
            Debug.Log((100 +StorageManager.GameDataMain.CurMaxLevel*5) + " gold" + (5 + StorageManager.GameDataMain.CurMaxLevel/2) + " magicCubes");

        _currentMovesCount = _currentLevelConfig.MovesCount;
        _currentMovesCountText.text = _currentMovesCount.ToString();
        
        GenerateNewPieces();
        base.SetupGame();
    }


    private void PlaceOneSizePiece(CellTypeInfo cellInfo, Vector2Int pos) {
        GameObject tmpContainer = new();
        tmpContainer.transform.SetParent(FieldContainers.Instance.FieldContainer);
        List<Vector3> poses = new List<Vector3>();
        List<GameObject> cells = new List<GameObject>();
        var prefab = PiecesViewTable.Instance.CellsViewList.GetCellByType(cellInfo.CellType);
        CellView go = Instantiate(prefab, FieldContainers.Instance.FieldContainer);
        //go.SetSeed(pieceData.CellGuids[x, y]);

        go.transform.localPosition = new Vector3(pos.x, -0.45f, pos.y);
        poses.Add(new Vector3(pos.x, -0.45f, pos.y));
        _field[pos.x, pos.y] = cellInfo.CellType;
        _cells[pos.x, pos.y] = go;
        cells.Add(go.gameObject);

        //go.GetComponent<CellView>().PlaceCellOnField();
        SpawnResourceFx(pos, go);
        //SpawnSmokeParticle(go.transform.position).Forget();

        // tmpContainer.transform.localPosition = GetAveragePosition(poses);
        foreach (var cell in cells) {
            cell.transform.SetParent(tmpContainer.transform);
        }

        // ShowDropImpact(tmpContainer.transform, pieceData, tmpContainer, 1);
    }

    private void SetTaskDescriptions() {
        for (int i = 0; i < _currentLevelConfig.Tasks.Length; i++) {
            var task = _currentLevelConfig.Tasks[i];
            TaskInfoSubClass newTaskInfo = new TaskInfoSubClass(task.taskType, task.NeedResource, task.Count);
            SetTaskUI(i, newTaskInfo, newTaskInfo);
        }
    }

    private void SetTaskDescriptionsFromStartField(Dictionary<ResourceType, int> startTasks)
    {
        int i = _currentLevelConfig.Tasks.Length;
        foreach (var (resourceType, count) in startTasks)
        {
            TaskInfoSubClass newTaskInfo = new TaskInfoSubClass(TaskInfo.TaskType.getResource, resourceType, count);
            
            SetTaskUI(i, newTaskInfo, newTaskInfo);
            i++;
        }
    }

    private void SetTaskUI(int i, TaskInfoSubClass newTaskInfo, TaskInfoSubClass task)
    {
        var taskUI = _taskUIViews[i];
        taskUI.gameObject.SetActive(true);
        Debug.Log(task.NeedResource.ToString());
        _currentTasks.Add(new TaskInfoAndUI(newTaskInfo, taskUI, task.Count));
        string needSpiteName = "";
        switch (task.TaskType)
        {
            case TaskInfo.TaskType.getResource:

                needSpiteName = task.NeedResource.ToString();
                break;

            case TaskInfo.TaskType.placeMonoLine:

                needSpiteName = task.NeedResource.ToString();
                taskUI.TaskSubImage.sprite = ConfigsManager.Instance.SpritesForTasksConfig.LineSprite;
                break;

        }
        taskUI.TaskImage.sprite = ConfigsManager.Instance.SpritesForTasks[needSpiteName];
        StartCoroutine(taskUI.TaskInfoTextHelper.StartSpawnText(task.Count.ToString()));
    }

    public void ShowFloatingText(string needText, Vector2 newPosition, float textSize, float showTime, Vector2 finalposition) {
        var floatingText = _floatingTextsPool.Get();
        floatingText.SetText(newPosition, needText, textSize, showTime, finalposition);
    }

    public void ReleaseFloatingText(FloatingTextView needTextObject) {
        needTextObject.gameObject.SetActive(false);
        _floatingTextsPool.Release(needTextObject);
    }

    public void GoToMeta() {
        SceneManager.LoadScene("MetaScene");
    }
}