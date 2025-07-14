using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameFieldManager : FieldManager, IResetable {
    public static GameFieldManager Instance;

    [field: Header("Game")]
    [SerializeField]
    private SpawnRandomNature _spawnRandomNature;

    [field: SerializeField]
    public MainGameConfig MainGameConfig { get; private set; }

    private List<PieceData> _nextBlocks = new List<PieceData>();
    private List<Vector2Int> _cellsToDestroy = new List<Vector2Int>();
    public List<TaskInfoAndUI> _currentTasks { get; private set; }
    private List<ResourceType> _resourceTypesForTasks = new List<ResourceType>();
    private Dictionary<ResourceType, int> _monoLinesCount;
    private Dictionary<CellType, int> _placedCellsCount;
    private int _currentMovesCount;
    private int _placedPiecesAmount;
    private List<CellTypeInfo> _currentGuaranteedFirstCells;
    private GameData GameData { get; set; }
    private bool _isSlimeExist;

    public Action OnCellPlaced;

    [field: SerializeField]
    public Transform _additionalPieceContainer { get; private set; }

    public PieceView _additionalPiecePrefab { get; private set; }

    public Material _normal, _priorityMaterial;

    protected override void Awake() {
        base.Awake();
        Instance = this;
        _spawnRandomNature.Generate();
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

    protected override void TryDestroyPiece() {
        Physics.Raycast(_mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, _pieceMask);
        if (hit.collider != null && StorageManager.GameDataMain.HummerCount > 0) {
            Vector3 cellPos = new Vector3(Mathf.RoundToInt(hit.collider.transform.localPosition.x),
                Mathf.RoundToInt(hit.collider.transform.localPosition.y), Mathf.RoundToInt(hit.collider.transform.localPosition.z));
            if (FieldUtils.CantDestroyInRow(_field[(int)cellPos.x, (int)cellPos.z])) return;
            BoostersManager.Instance.BreackCellWithHummer();

            if (StorageManager.GameDataMain.HummerCount <= 0)
                _isDestroyPieceMode = false;
            HummerDestoyPieceAnimation(_cells[(int)cellPos.x, (int)cellPos.z]);

            var configSlime = PiecesViewTable.Instance.CellsList.CellsConfigs.First(c => c.CellType == _field[(int)cellPos.x, (int)cellPos.z]);
            for (int j = 0; j < _currentTasks.Count; j++) {
                if (_currentTasks[j].TaskInfo.TaskType == TaskInfo.TaskType.getResource) {
                    CheckNeedResourceInTask(j, configSlime, new Vector2Int((int)cellPos.x, (int)cellPos.z));
                }
            }

            _field[(int)cellPos.x, (int)cellPos.z] = CellType.Empty;

            CheckResourceCountForTasks();
            CheckGameGoal();
        }
    }

    private void CheckGameGoal() {
        if (!CheckWin() && CheckLose())
            Lose();
    }

    public bool AdditionalPieceContainerUnderPiece() =>
        _inputRaycaster.InputPosAdditionalContainer() != Vector3.zero && _additionalPiecePrefab == null;

    public void SetNeededCellTypeOnField(CellType cellType, CellView go, Vector2Int cellPosition, bool needFX) {
        _field[cellPosition.x, cellPosition.y] = cellType;
        _cells[cellPosition.x, cellPosition.y] = go;
        if (needFX)
            SpawnResourceFx(cellPosition, go);
    }

    public void SetPieceInAdditionalContainer(ref Vector3 finalPosition, PieceView piece) {
        piece.transform.SetParent(_additionalPieceContainer);
        finalPosition = _additionalPieceContainer.position;
        Vector3 startPos = piece._cellsContainer.position;
        piece.transform.position = finalPosition;
        piece._cellsContainer.position = startPos;
        _nextBlocks.Remove(piece.Data);
        _additionalPiecePrefab = piece;
        _placedPiecesAmount++;

        if (_placedPiecesAmount % 3 == 0) {
            GenerateNewPieces();
        }
    }

    public override void PlacePiece(PieceData pieceData, Vector2Int coord, CellView[,] cells, Transform cellsContainer) {
        base.PlacePiece(pieceData, coord, cells, cellsContainer);

        //  CheckPlacedCellsForTask();
        if (pieceData.Type.CellType == CellType.Dinamyte) {
            return;
        }

        OnCellPlaced?.Invoke();
        _nextBlocks.Remove(pieceData);
        if (_additionalPiecePrefab != null && _additionalPiecePrefab.Data == pieceData)
            _additionalPiecePrefab = null;
        else {
            _placedPiecesAmount++;
            if (_placedPiecesAmount % 3 == 0)
                GenerateNewPieces();
        }

        _currentMovesCount--;
        GameUI.Instance.SetMovesCount(_currentMovesCount);

        if (MainGameConfig.resourceOnPlaceCell) {
            CollectResourcesOnPlace(pieceData);
        }

        if (_isSlimeExist)
            SlimeMove();

        ExplodeCellsInRows();

        if (CheckWin()) {
            return;
        }

        if (CheckLose()) {
            Lose();
        }
    }

    private void SlimeMove() {
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
                     var config = PiecesViewTable.Instance.CellsList.CellsConfigs.First(c =>
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
        int width = _field.GetLength(0);
        int height = _field.GetLength(1);

        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (_field[i, j] != CellType.Slime) continue;

                var slimePos = new Vector2Int(i, j);
                var emptyCellsAround = new List<Vector2Int>();
                foreach (var cell in FieldUtils.GetCellsAround(_field, slimePos)) {
                    if (_field[cell.x, cell.y] == CellType.Empty)
                        emptyCellsAround.Add(cell);
                }

                if (emptyCellsAround.Count > 0)
                    newSlimeCells.Add((emptyCellsAround[Random.Range(0, emptyCellsAround.Count)], _cells[i, j].transform.position));
            }
        }

        for (int i = newSlimeCells.Count - 1; i > 0; i--) {
            int swapIndex = Random.Range(0, i + 1);
            (newSlimeCells[i], newSlimeCells[swapIndex]) = (newSlimeCells[swapIndex], newSlimeCells[i]);
        }

        int halfSlimeCount = Mathf.CeilToInt((float)newSlimeCells.Count / 2f);
        for (int i = 0; i < halfSlimeCount; i++) {
            var (randomEmptyCell, startPosition) = newSlimeCells[i];
            if (_field[randomEmptyCell.x, randomEmptyCell.y] != CellType.Empty) continue;
            AddSlimeAroundSlime(randomEmptyCell, startPosition);
        }
    }

    private void AddSlimeAroundSlime(Vector2Int randomEmptyCell, Vector3 startPosition) {
        var config = PiecesViewTable.Instance.CellsList.CellsConfigs.First(c => c.CellType == CellType.Slime);
        PlaceOneSizePiece(config, new Vector2Int(randomEmptyCell.x, randomEmptyCell.y), true);
        SpawnNewSlimeAnimation(_cells[randomEmptyCell.x, randomEmptyCell.y].transform, startPosition,
            _cells[randomEmptyCell.x, randomEmptyCell.y].transform.position);
        foreach (var task in _currentTasks) {
            if (task.TaskInfo.NeedResource == ResourceType.Slime) {
                task.needCount++;
                if (GameData.CollectedResources.TryGetValue(task.TaskInfo.NeedResource, out int resourceCount))
                    task.TaskUIView.TaskInfoTextHelper.SetText((task.needCount - resourceCount).ToString());
                else
                    task.TaskUIView.TaskInfoTextHelper.SetText(task.needCount.ToString());
            }
        }
    }

    private void SpawnNewSlimeAnimation(Transform cellContainer, Vector3 startPosition, Vector3 endPosition) {
        cellContainer.localScale = Vector3.zero;
        cellContainer.position = startPosition;
        var _currentTween = DOTween.Sequence().Append(cellContainer.DOScale(Vector3.one, 0.5f)).Join(cellContainer.DOMove(endPosition, 0.5f));
    }

    public override void CheckCellTypesBeforePlacePiece(Vector2Int coord) {
        base.CheckCellTypesBeforePlacePiece(coord);
        var cellType = _field[coord.x, coord.y];

        switch (cellType) {
            case CellType.Ice:
                DestroyCellAfterPlacePiece(coord, cellType);
                _gameAudio.PlayNextSound(_gameAudio.IceBreaks);
                break;
            case CellType.Crystal:

                DestroyCellAfterPlacePiece(coord, cellType);
                break;

            case CellType.Slime:
                DestroyCellAfterPlacePiece(coord, cellType);
                _gameAudio.PlayNextSound(_gameAudio.SlimeBreaks);
                break;
        }
        // CheckClosestCells(coord);
    }

    private void DestroyCellAfterPlacePiece(Vector2Int coord, CellType cellType) {
        var configSlime = PiecesViewTable.Instance.CellsList.CellsConfigs.First(c => c.CellType == cellType);
        for (int j = 0; j < _currentTasks.Count; j++) {
            if (_currentTasks[j].TaskInfo.TaskType == TaskInfo.TaskType.getResource) {
                CheckNeedResourceInTask(j, configSlime, coord);
            }
        }

        DestroyCell(coord);
    }

    protected override void SpawnResourceFx(Vector2Int place, CellView go) {
        var cellType = _field[place.x, place.y];
        var resourcesForPlace = PiecesViewTable.Instance.CellsList.CellsConfigs.First(c => c.CellType == cellType).ResourcesForPlace;
        var onCanvasPosition = _mainCamera.WorldToScreenPoint(go.transform.position);
        for (int i = 0; i < resourcesForPlace.Length; i++) {
            for (int j = 0; j < _currentTasks.Count; j++) {
                if (_currentTasks[j].TaskInfo.TaskType == TaskInfo.TaskType.getResource) {
                    if (_currentTasks[j].TaskInfo.NeedResource == ResourceType.None ||
                        (_currentTasks[j].TaskInfo.NeedResource == resourcesForPlace[i].ResourceType)) {
                        GameUI.Instance.ShowFloatingText(
                            (" +" + resourcesForPlace[i].ResourceCount + " <sprite name=" + resourcesForPlace[i].ResourceType + ">" + " "),
                            new Vector2(onCanvasPosition.x, onCanvasPosition.y + (i * 15)), 20, 1,
                            _currentTasks[j].TaskUIView.CurrentTaskInfo.transform.position);
                        return;
                    }
                }
            }
        }

        if (!_placedCellsCount.TryAdd(cellType, 1))
            _placedCellsCount[cellType]++;
    }

    public void CollectResourcesOnPlace(PieceData placedPiece) {
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

    public void CheckResourceCountForTasks() {
        for (int i = 0; i < _currentTasks.Count; i++) {
            if (_currentTasks[i].TaskInfo.TaskType == TaskInfo.TaskType.getResource) {
                if (_currentTasks[i].TaskInfo.NeedResource == ResourceType.None && GameData.CollectedResources.Count != 0) {
                    ResourceType maxResourceType = ResourceType.None;
                    foreach (var resource in GameData.CollectedResources) {
                        if (maxResourceType == ResourceType.None || GameData.CollectedResources[maxResourceType] < resource.Value)
                            maxResourceType = resource.Key;
                    }

                    var remainingResourceCount = Math.Max(_currentTasks[i].needCount - GameData.CollectedResources[maxResourceType], 0);
                    _currentTasks[i].TaskUIView.TaskInfoTextHelper.SetText(remainingResourceCount.ToString());
                    if (_currentTasks[i].needCount <= GameData.CollectedResources[maxResourceType]) {
                        _resourceTypesForTasks.Remove(_currentTasks[i].TaskInfo.NeedResource);
                        _currentTasks[i].TaskUIView.CompleteTask();
                        _currentTasks.RemoveAt(i);
                        break;
                    }
                } else if (GameData.CollectedResources.TryGetValue(_currentTasks[i].TaskInfo.NeedResource, out int resourceCount)) {
                    var remainingResourceCount = Math.Max(_currentTasks[i].needCount - resourceCount, 0);
                    _currentTasks[i].TaskUIView.TaskInfoTextHelper.SetText(remainingResourceCount.ToString());
                    if (resourceCount >= _currentTasks[i].needCount) {
                        _resourceTypesForTasks.Remove(_currentTasks[i].TaskInfo.NeedResource);
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

    public void ExplodeCellsInRows() {
        int width = _field.GetLength(0);
        int height = _field.GetLength(1);
        string unlockedCellText = "";
        int isDestroyingLinesCount = 0;
        for (int y = 0; y < height; y++) {
            if (Enumerable.Range(0, width).All(x => !FieldUtils.CantBecomeRow(_field[x, y]))) {
                DestroyLine(y, width, true, ref unlockedCellText);
                isDestroyingLinesCount++;
            }
        }

        for (int x = 0; x < width; x++) {
            if (Enumerable.Range(0, height).All(y => !FieldUtils.CantBecomeRow(_field[x, y]))) {
                DestroyLine(x, height, false, ref unlockedCellText);
                isDestroyingLinesCount++;
            }
        }

        if (isDestroyingLinesCount != 0) {
            SpawnDestroyRowVibration();
            _gameAudio.PlayNextSound(_gameAudio.RowCollected);

            if (unlockedCellText != "") {
                GameUI.Instance.ShowFloatingText(unlockedCellText + " is unlocked!", GameUI.Instance.transform.position, 40, 2.5f,
                    Vector2.zero);
            }

            DestroyAllMarkedCells(isDestroyingLinesCount);
        }
    }

    private static void SpawnDestroyRowVibration() {
        if (Random.Range(0, 2) == 0) {
            VibrationsManager.Instance.SpawnVibration(VibrationType.AllRow);
        } else {
            VibrationsManager.Instance.SpawnVibrationEmhpasis(1, 1);
        }
    }

    private void DestroyAllMarkedCells(int linesCount) {
        for (int i = 0; i < _cellsToDestroy.Count; i++) {
            var cell = _cellsToDestroy[i];
            _cellsToDestroy.RemoveAt(i--);
            if (!FieldUtils.CantDestroyInRow(_field[cell.x, cell.y])) {
                DestroyCell(cell);
                UltaManager.Instance.AddUltimatePoints(MainGameConfig.LinesCountMultiplayers[linesCount - 1]);
            }
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
            var config = PiecesViewTable.Instance.CellsList.CellsConfigs.First(c => c.CellType == cellType);
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

            for (int j = 0; j < _currentTasks.Count; j++) {
                if (_currentTasks[j].TaskInfo.TaskType == TaskInfo.TaskType.getResource) {
                    if (_currentTasks[j].TaskInfo.NeedResource == ResourceType.None ||
                        (_currentTasks[j].TaskInfo.NeedResource == currentBonusResourceType)) {
                        GameUI.Instance.ShowFloatingText("<sprite name=" + currentBonusResourceType + "> " + bonusResourcesOnDestroyLine,
                            needPosition, 30, 1.5f, Vector2.zero);
                    }
                }
            }
        } else
            Debug.Log("not full same");

        CheckResourceCountForTasks();
    }

    private int CheckLineAndDestroyNeededCells(int mainAxisCurrentValue, bool isRow, int secondAxis, Dictionary<CellType, int> cellTypesInLine,
        Dictionary<ResourceType, int> resourcesMultiplayers, bool fullSameResourcesColumn, int bonusResourcesOnDestroyLine,
        ref ResourceType currentBonusResourceType) {
        Vector2Int curPosition = !isRow ? new Vector2Int(mainAxisCurrentValue, secondAxis) : new Vector2Int(secondAxis, mainAxisCurrentValue);
        var cellType = _field[(int)curPosition.x, (int)curPosition.y];
        var config = PiecesViewTable.Instance.CellsList.CellsConfigs.First(c => c.CellType == cellType);
        //string floatingText = "+ ";

        if (!cellTypesInLine.TryAdd(cellType, 1))
            cellTypesInLine[cellType]++;

        var canvasPosition = _mainCamera.WorldToScreenPoint(_cells[curPosition.x, curPosition.y].transform.position);

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

            for (int j = 0; j < _currentTasks.Count; j++) {
                if (_currentTasks[j].TaskInfo.TaskType == TaskInfo.TaskType.getResource) {
                    if (_currentTasks[j].TaskInfo.NeedResource == ResourceType.None ||
                        (_currentTasks[j].TaskInfo.NeedResource == config.ResourcesForDestroy[i].ResourceType)) {
                        GameUI.Instance.ShowFloatingText(
                            (" +" + count + " <sprite name=" + config.ResourcesForDestroy[i].ResourceType + ">" + " "),
                            new Vector2(canvasPosition.x, canvasPosition.y + (i * 15)), 20, 1,
                            _currentTasks[j].TaskUIView.CurrentTaskInfo.transform.position);
                    }
                }
            }
        }

        _cellsToDestroy.Add(curPosition);
        return bonusResourcesOnDestroyLine;
    }

    public override void CheckClosestCells(Vector2Int coord) {
        var cellsAround = FieldUtils.GetCellsAround(_field, coord);
        foreach (var coordAround in cellsAround) {
            var cellType = _field[coordAround.x, coordAround.y];
            switch (cellType) {
                case CellType.Box:
                    CellTypeInfo configBox = PiecesViewTable.Instance.CellsList.CellsConfigs.First(c => c.CellType == cellType);
                    for (int j = 0; j < _currentTasks.Count; j++) {
                        if (_currentTasks[j].TaskInfo.TaskType == TaskInfo.TaskType.getResource) {
                            CheckNeedResourceInTask(j, configBox, coordAround);
                        }
                    }

                    _gameAudio.PlayNextSound(_gameAudio.BoxBreaks);
                    DestroyCell(coordAround);

                    break;
                case CellType.GoldMine:
                    CellTypeInfo configGoldMine = PiecesViewTable.Instance.CellsList.CellsConfigs.First(c => c.CellType == cellType);
                    for (int j = 0; j < _currentTasks.Count; j++) {
                        if (_currentTasks[j].TaskInfo.TaskType == TaskInfo.TaskType.getResource) {
                            CheckNeedResourceInTask(j, configGoldMine, coordAround);
                        }
                    }

                    break;

                case CellType.CrystalMine:
                    CellTypeInfo configCrystalMine = PiecesViewTable.Instance.CellsList.CellsConfigs.First(c => c.CellType == cellType);
                    for (int j = 0; j < _currentTasks.Count; j++) {
                        if (_currentTasks[j].TaskInfo.TaskType == TaskInfo.TaskType.getResource) {
                            CheckNeedResourceInTask(j, configCrystalMine, coordAround);
                        }
                    }

                    MineCellAnimation(_cells[coordAround.x, coordAround.y].transform);
                    var randomPos = GetRandomEmptyCell();
                    if (randomPos == new Vector2(-1, -1)) return;
                    var configCrystal = PiecesViewTable.Instance.CellsList.CellsConfigs.First(c => c.CellType == CellType.Crystal);
                    PlaceOneSizePiece(configCrystal, new Vector2Int(randomPos.x, randomPos.y), true);
                    CrystalCellAnimation(randomPos, coordAround);

                    /*var _currentTween = DOTween.Sequence().Append(crystalCellTransform.DOScale(Vector3.one * 1.2f, 0.4f))
                        .Join(crystalCellTransform.DOMoveX(endCrystalPosition.x,0.6f))
                        .Join(crystalCellTransform.DOMoveZ(endCrystalPosition.z,0.6f))
                        .Join(crystalCellTransform.DOMoveY(endCrystalPosition.y + 0.6f,0.4f))
                        .Append(crystalCellTransform.DOScale(Vector3.one, 0.1f))
                        .Join(crystalCellTransform.DOMoveY(endCrystalPosition.y,0.2f));*/
                    //crystal anim
                    break;
            }
        }
    }

    private void CrystalCellAnimation(Vector2Int randomPos, Vector2Int coordAround) {
        var crystalCellTransform = _cells[randomPos.x, randomPos.y].transform;
        Vector3 endCrystalPosition = crystalCellTransform.position;
        Vector3 startPosition = _cells[coordAround.x, coordAround.y].transform.position;
        crystalCellTransform.position = _cells[coordAround.x, coordAround.y].transform.position;
        crystalCellTransform.localScale = Vector3.zero;
        float offsetMultiplayer = Vector3.Distance(startPosition, endCrystalPosition);

        Vector3[] path = new Vector3[3];
        path[0] = crystalCellTransform.position;
        path[1] = (startPosition + endCrystalPosition) * 0.5f + Vector3.up * 0.3f * offsetMultiplayer +
                  Vector3.right * 0.1f * offsetMultiplayer + Vector3.forward * 0.1f * offsetMultiplayer;
        path[2] = endCrystalPosition;
        var _currentTween = DOTween.Sequence().Append(crystalCellTransform.DOScale(Vector3.one * 1.2f, 0.4f))
            .Join(crystalCellTransform.DOPath(path, 0.6f, PathType.CatmullRom, PathMode.Full3D, 10))
            .Append(crystalCellTransform.DOScale(Vector3.one, 0.1f)).Join(crystalCellTransform.DOMoveY(endCrystalPosition.y, 0.2f));
    }

    private void MineCellAnimation(Transform cell) {
        float startY = FieldContainers.Instance.PlacedCellsVerticalAnchor.position.y;
        var _currentTween = DOTween.Sequence().Append(cell.DOScale(Vector3.one * 0.8f, 0.4f)).Join(cell.DOMoveY(startY - 0.2f, 0.4f))
            .Append(cell.DOScale(Vector3.one, 0.15f)).Join(cell.DOMoveY(startY, 0.15f));
    }

    private Vector2Int GetRandomEmptyCell() {
        List<Vector2Int> _emptyCells = new List<Vector2Int>();
        for (int i = 0; i < _field.GetLength(0); i++) {
            for (int j = 0; j < _field.GetLength(1); j++)
                if (_field[i, j] == CellType.Empty)
                    _emptyCells.Add(new Vector2Int(i, j));
        }

        if (_emptyCells.Count == 0) return new Vector2Int(-1, -1);
        return _emptyCells[Random.Range(0, _emptyCells.Count)];
    }

    public void CheckNeedResourceInTask(int j, CellTypeInfo config, Vector2Int coord) {
        if (config.ResourcesForDestroy.Length == 0) return;
        var needResource = config.ResourcesForDestroy[0];
        if (_currentTasks[j].TaskInfo.NeedResource == needResource.ResourceType) {
            if (!GameData.CollectedResources.TryAdd(needResource.ResourceType, needResource.ResourceCount))
                GameData.CollectedResources[needResource.ResourceType] += needResource.ResourceCount;
            var canvasPosition = _mainCamera.WorldToScreenPoint(_cells[coord.x, coord.y].transform.position);
            GameUI.Instance.ShowFloatingText((" + <sprite name=" + needResource.ResourceType + ">" + " "),
                new Vector2(canvasPosition.x, canvasPosition.y + 15), 20, 1, _currentTasks[j].TaskUIView.CurrentTaskInfo.transform.position);
        }
    }

    public void DestroyCell(Vector2Int coord) {
        _field[coord.x, coord.y] = CellType.Empty;
        _cells[coord.x, coord.y].DestroyCell();
    }

    public bool CheckWin() {
        if (_currentTasks.Count == 0) {
            UltaManager.Instance.UltimateActionEndRound();
            return true;
        }

        return false;
    }

    public bool CheckLose() {
        if (_currentMovesCount <= 0)
            return true;
        for (int i = 0; i < _nextBlocks.Count; i++) {
            if (_nextBlocks[i] != null && PieceUtils.CanPlacePiece(_field, _nextBlocks[i]))
                return false;
        }

        if (_additionalPiecePrefab != null && PieceUtils.CanPlacePiece(_field, _additionalPiecePrefab.Data))
            return false;

        return true;
    }

    public void Win() {
        SaveWinGame();

        GameUI.Instance.SetMainText("You win!");
        GameUI.Instance.SetTasksActive(false);
        NextPiecesView.Instance.DestroyPieces();
        NextPiecesView.Instance.DestroyAdditionalPiece();
        VibrationsManager.Instance.SpawnContinuous(0.46f, 0.24f, 0.4f);
        GoalView.Instance.SetWinState();

        _gameAudio.PlayNextSound(_gameAudio.Win);
    }

    public void Lose() {
        MainManager.Instance.RemoveHealthAfterLose();

        GameUI.Instance.SetMainText("You lose:(");
        GameUI.Instance.SetTasksActive(false);
        VibrationsManager.Instance.SpawnContinuous(0.46f, 0.24f, 0.4f);
        GoalView.Instance.SetLoseState();
        UltaManager.Instance.HideUltimateUI();
        Invoke("DestroyCurrentPieces", 2f);
    }

    private void DestroyCurrentPieces() {
        NextPiecesView.Instance.DestroyPieces();
        NextPiecesView.Instance.DestroyAdditionalPiece();
    }
    /*  private void RemoveHealthAfterLose() {
          StorageManager.GameDataMain.LastHealthRecoveryTime = _currentGameTime.ToString(CultureInfo.InvariantCulture);
          StorageManager.GameDataMain.HealthCount--;
      }

     public void Restart() {
          if (StorageManager.GameDataMain.HealthCount != 0)
              SceneManager.LoadScene(SceneManager.GetActiveScene().name);
          else {
              //floating window with "watch ad and get health"
          }
      }*/

    private void CalculateCellSpawnChances() {
        float lastChance = 0;
        CellsChanceToSpawn = new float[_currentCellsToSpawn.Count];
        for (int i = 0; i < _currentCellsToSpawn.Count; i++) {
            lastChance += PiecesViewTable.Instance.CellsList.CellsConfigs.First(c => c.CellType == _currentCellsToSpawn[i]).ChanceToSpawn;
            CellsChanceToSpawn[i] = lastChance;
        }
    }

    public override void SetupGame() {
        CalculateFiguresSpawnChances();
        GenerateField();
        GenerateTask();
        StartGame();

        // MainManager.Instance._currentLevelConfig = MainGameConfig.Levels[StorageManager.GameDataMain.CurMaxLevel];

        _placedPiecesAmount = 0;
        _field = new CellType[MainGameConfig.FieldSize, MainGameConfig.FieldSize];
        _cells = new CellView[MainGameConfig.FieldSize, MainGameConfig.FieldSize];

        if (MainManager.Instance._currentLevelConfig.TutorialObject != null)
            Instantiate(MainManager.Instance._currentLevelConfig.TutorialObject);

        var startCells = MainManager.Instance._currentLevelConfig.CellTypesTableConfig;
        _currentCellsToSpawn = new List<CellType>();
        for (int i = 0; i < startCells.CellsToSpawn.Length; i++)
            _currentCellsToSpawn.Add(startCells.CellsToSpawn[i]);
        CalculateCellSpawnChances();

        _placedCellsCount = new Dictionary<CellType, int>();

        _currentTasks = new List<TaskInfoAndUI>();

        SetTaskDescriptions();

        _monoLinesCount = new Dictionary<ResourceType, int>();
        GameUI.Instance.StartCharacterInfoTextCoroutine(MainManager.Instance._currentLevelConfig.GuideForLevelText);
        _currentGuaranteedFirstCells = new List<CellTypeInfo>();
        foreach (var cellInfo in MainManager.Instance._currentLevelConfig.CurrentGuaranteedFirstCells)
            _currentGuaranteedFirstCells.Add(cellInfo);

        GameData = new GameData();
        if (MainManager.Instance._currentLevelConfig.StartFieldConfig != null) {
            Dictionary<ResourceType, int> startCellsResourcesCount = new Dictionary<ResourceType, int>();
            for (int i = 0; i < _field.GetLength(0); i++) {
                for (int j = 0; j < _field.GetLength(1); j++) {
                    if (MainManager.Instance._currentLevelConfig.StartFieldConfig.GetCell(i, j) != CellType.Empty) {
                        var config = PiecesViewTable.Instance.CellsList.CellsConfigs.First(c =>
                            c.CellType == MainManager.Instance._currentLevelConfig.StartFieldConfig.GetCell(i, j));
                        if (!_isSlimeExist && config.CellType == CellType.Slime)
                            _isSlimeExist = true;
                        PlaceOneSizePiece(config, new Vector2Int(i, j), true);
                        if (!FieldUtils.CantDestroyInRow(config.CellType) &&
                            !startCellsResourcesCount.TryAdd(config.ResourcesForDestroy[0].ResourceType, 1))
                            startCellsResourcesCount[config.ResourcesForDestroy[0].ResourceType]++;
                    }
                }
            }

            SetTaskDescriptionsFromStartField(startCellsResourcesCount);
        }

        _currentMovesCount = MainManager.Instance._currentLevelConfig.MovesCount;
        GameUI.Instance.SetMovesCount(_currentMovesCount);

        GenerateNewPieces();
        BoostersManager.Instance.SetAllText();
        base.SetupGame();
    }

    public CellView PlaceOneSizePiece(CellTypeInfo cellInfo, Vector2Int pos, bool setNewInfo) {
        GameObject tmpContainer = new();
        tmpContainer.transform.SetParent(FieldContainers.Instance.FieldContainer);
        List<Vector3> poses = new List<Vector3>();
        List<GameObject> cells = new List<GameObject>();
        var prefab = PiecesViewTable.Instance.CellsViewList.GetCellByType(cellInfo.CellType);
        CellView go = Instantiate(prefab, FieldContainers.Instance.FieldContainer);
        //go.SetSeed(pieceData.CellGuids[x, y]);

        go.transform.localPosition = new Vector3(pos.x, -0.2f, pos.y);
        poses.Add(new Vector3(pos.x, -0.2f, pos.y));
        if (setNewInfo) {
            _field[pos.x, pos.y] = cellInfo.CellType;
            _cells[pos.x, pos.y] = go;
            SpawnResourceFx(pos, go);
        }

        cells.Add(go.gameObject);

        //go.GetComponent<CellView>().PlaceCellOnField();

        //SpawnSmokeParticle(go.transform.position).Forget();

        // tmpContainer.transform.localPosition = GetAveragePosition(poses);
        foreach (var cell in cells) {
            cell.transform.SetParent(tmpContainer.transform);
        }

        return go;
        // ShowDropImpact(tmpContainer.transform, pieceData, tmpContainer, 1);
    }

    private void SetTaskDescriptions() {
        for (int i = 0; i < MainManager.Instance._currentLevelConfig.Tasks.Length; i++) {
            var task = MainManager.Instance._currentLevelConfig.Tasks[i];
            TaskInfoSubClass newTaskInfo = new TaskInfoSubClass(task.taskType, task.NeedResource, task.Count);
            SetTaskUI(i, newTaskInfo, newTaskInfo);
        }
    }

    private void SetTaskDescriptionsFromStartField(Dictionary<ResourceType, int> startTasks) {
        int i = MainManager.Instance._currentLevelConfig.Tasks.Length;
        foreach (var (resourceType, count) in startTasks) {
            TaskInfoSubClass newTaskInfo = new TaskInfoSubClass(TaskInfo.TaskType.getResource, resourceType, count);

            SetTaskUI(i, newTaskInfo, newTaskInfo);
            i++;
        }
    }

    private void SetTaskUI(int i, TaskInfoSubClass newTaskInfo, TaskInfoSubClass task) {
        var taskUI = GameUI.Instance.TaskUIViews[i];
        taskUI.gameObject.SetActive(true);
        _currentTasks.Add(new TaskInfoAndUI(newTaskInfo, taskUI, task.Count));
        _resourceTypesForTasks.Add(task.NeedResource);
        string needSpiteName = "";
        switch (task.TaskType) {
            case TaskInfo.TaskType.getResource:

                needSpiteName = task.NeedResource.ToString();
                break;

            case TaskInfo.TaskType.placeMonoLine:

                needSpiteName = task.NeedResource.ToString();
                taskUI.TaskSubImage.sprite = ConfigsManager.Instance.SpritesForTasksConfig.LineSprite;
                break;
        }

        taskUI.TaskImage.sprite = ConfigsManager.Instance.SpritesForTasks[needSpiteName];
        taskUI.TaskInfoTextHelper.SetText(task.Count.ToString());
        //GameUI.Instance.StartCharacterInfoTextCoroutine();
    }

    public void ReleaseFloatingText(FloatingTextView needTextObject) {
        GameUI.Instance.ReleaseFloatingText(needTextObject);
    }

    private void SaveWinGame() {
        StorageManager.GameDataMain.GoldAmount += 100 /* + StorageManager.GameDataMain.CurMaxLevel * 5*/;
        // StorageManager.GameDataMain.MagicCubesAmount += 5 + StorageManager.GameDataMain.CurMaxLevel / 2;
        StorageManager.GameDataMain.MagicCubesAmount += MainManager.Instance._currentLevelConfig.MagicCubesCount;
        MainManager.Instance.IncreaseMaxLevel();
        StorageManager.SaveGame();
    }
}