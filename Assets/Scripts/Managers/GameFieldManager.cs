using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameFieldManager : FieldManager {
    public static GameFieldManager Instance;

    [field: Header("Game")]
    [field: SerializeField]
    public Transform AdditionalPieceContainer { get; private set; }

    public event Action OnMoveEnded, OnPieceDestroyedByHammer;
    public PieceView AdditionalPiecePrefab { get; private set; }

    private List<PieceData> _nextBlocks = new List<PieceData>();

    private List<Vector2Int> _cellsToDestroy = new List<Vector2Int>();

    private int _placedPiecesAmount;
    private bool _isSlimeExist;
    private GameData _gameData;
    private MainGameConfig _mainGameConfig;

    private List<CellTypeInfo> _currentGuaranteedFirstCells;

    protected override void Awake() {
        base.Awake();
        Instance = this;
    }

    public void GenerateNewPieces() {
        _nextBlocks = new List<PieceData>() {
            PieceUtils.GetNewCorePiece(_currentGuaranteedFirstCells)
        };
        if (_placedPiecesAmount == 0 && MainManager.Instance.CurrentLevelConfig.FirstFiguresCount == 1) {
            _placedPiecesAmount = 2;
            NextPiecesView.Instance.SetData(_nextBlocks);
            return;
        }

        _nextBlocks.Add(PieceUtils.GetNewCorePiece(_currentGuaranteedFirstCells));
        if (_placedPiecesAmount == 0 && MainManager.Instance.CurrentLevelConfig.FirstFiguresCount == 2) {
            _placedPiecesAmount = 1;
            NextPiecesView.Instance.SetData(_nextBlocks);
            return;
        }

        _nextBlocks.Add(PieceUtils.GetNewCorePiece(_currentGuaranteedFirstCells));
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
            HummerDestoyPieceAnimation(new CellView[] { _cells[(int)cellPos.x, (int)cellPos.z] });

            var configSlime =
                PiecesViewTable.Instance.CellsList.CoreCellsConfigs.First(c => c.CellType == _field[(int)cellPos.x, (int)cellPos.z]);
            TryAddResourceForCell(configSlime, new Vector2Int((int)cellPos.x, (int)cellPos.z));

            _field[(int)cellPos.x, (int)cellPos.z] = CellType.Empty;
            OnPieceDestroyedByHammer?.Invoke();
        }
    }

    public bool AdditionalPieceContainerUnderPiece() =>
        _inputRaycaster.InputPosAdditionalContainer() != Vector3.zero && AdditionalPiecePrefab == null;

    public void SetNeededCellTypeOnField(CellType cellType, CellView go, Vector2Int cellPosition, bool needFX) {
        _field[cellPosition.x, cellPosition.y] = cellType;
        _cells[cellPosition.x, cellPosition.y] = go;
        if (needFX) {
            SpawnResourceFxForCell(cellPosition, go);
        }
    }

    public void SetPieceInAdditionalContainer(ref Vector3 finalPosition, PieceView piece) {
        piece.transform.SetParent(AdditionalPieceContainer);
        finalPosition = AdditionalPieceContainer.position;
        Vector3 startPos = piece._cellsContainer.position;
        piece.transform.position = finalPosition;
        piece._cellsContainer.position = startPos;
        _nextBlocks.Remove(piece.Data);
        AdditionalPiecePrefab = piece;
        _placedPiecesAmount++;

        if (_placedPiecesAmount % 3 == 0) {
            GenerateNewPieces();
        }
    }

    public override void PlacePiece(PieceData pieceData, Vector2Int coord, CellView[,] cells, Transform cellsContainer) {
        base.PlacePiece(pieceData, coord, cells, cellsContainer);

        //  CheckPlacedCellsForTask();
        if (pieceData.Type.CellType == CellType.Dynamite) {
            return;
        }

        _nextBlocks.Remove(pieceData);
        if (AdditionalPiecePrefab != null && AdditionalPiecePrefab.Data == pieceData) {
            AdditionalPiecePrefab = null;
        } else {
            _placedPiecesAmount++;
            if (_placedPiecesAmount % 3 == 0)
                GenerateNewPieces();
        }

        if (_mainGameConfig.resourceOnPlaceCell) {
            CollectResourcesOnPlace(pieceData);
        }

        if (_isSlimeExist)
            SlimeMove();

        ExplodeCellsInRows();
        OnMoveEnded?.Invoke();
    }

    private void SlimeMove() {
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
        var config = PiecesViewTable.Instance.CellsList.CoreCellsConfigs.First(c => c.CellType == CellType.Slime);
        PlaceOneSizePiece(config, new Vector2Int(randomEmptyCell.x, randomEmptyCell.y), true);
        SpawnNewSlimeAnimation(_cells[randomEmptyCell.x, randomEmptyCell.y].transform, startPosition,
            _cells[randomEmptyCell.x, randomEmptyCell.y].transform.position);

        TaskUtils.Add1ToSlimeTask(_gameData);
    }

    private void SpawnNewSlimeAnimation(Transform cellContainer, Vector3 startPosition, Vector3 endPosition) {
        cellContainer.localScale = Vector3.zero;
        cellContainer.position = startPosition;
        DOTween.Sequence().Append(cellContainer.DOScale(Vector3.one, 0.5f)).Join(cellContainer.DOMove(endPosition, 0.5f));
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
        var configSlime = PiecesViewTable.Instance.CellsList.CoreCellsConfigs.First(c => c.CellType == cellType);
        TryAddResourceForCell(configSlime, coord);

        DestroyCell(coord);
    }

    public void ClearAllLockedCells() {
        for (int i = 0; i < _field.GetLength(0); i++) {
            for (int j = 0; j < _field.GetLength(1); j++) {
                if (_field[i, j] == CellType.LockedCoreCell)
                    DestroyCell(new Vector2Int(i, j));
            }
        }
    }

    protected override void SpawnResourceFxForCell(Vector2Int place, CellView go) {
        var cellType = _field[place.x, place.y];
        ResourceTypeAndCountSubClass[] resourcesForPlace =
            PiecesViewTable.Instance.CellsList.CoreCellsConfigs.First(c => c.CellType == cellType).ResourcesForPlace;
        var onCanvasPosition = _mainCamera.WorldToScreenPoint(go.transform.position);

        for (int index = 0; index < resourcesForPlace.Length; index++) {
            ResourceTypeAndCountSubClass resource = resourcesForPlace[index];
            var taskUIView = TaskUtils.GetUIForResourceTask(_gameData, resource);
            if (taskUIView == null) {
                continue;
            }

            string text = $" +{resource.ResourceCount} <sprite name={resource.ResourceType}> ";
            Vector2 pos = new(onCanvasPosition.x, onCanvasPosition.y + index * 15);
            GameUI.Instance.ShowFloatingText(text, pos, 20, 1, taskUIView.CurrentTaskInfo.transform.position);
        }
    }

    public void CollectResourcesOnPlace(PieceData placedPiece) {
        for (int x = 0; x < placedPiece.Cells.GetLength(0); x++) {
            for (int y = 0; y < placedPiece.Cells.GetLength(1); y++) {
                if (!placedPiece.Cells[x, y]) {
                    continue;
                }

                var needResources = ((CoreCellTypeInfo)placedPiece.Type).ResourcesForPlace;
                for (int i = 0; i < needResources.Length; i++) {
                    var resourceType = needResources[i];
                    if (resourceType == null) {
                        continue;
                    }

                    if (!_gameData.CollectedResources.TryAdd(resourceType.ResourceType, resourceType.ResourceCount))
                        _gameData.CollectedResources[resourceType.ResourceType] += resourceType.ResourceCount;
                }
            }
        }

        TaskUtils.CheckResourceCountForTasks(_gameData);
    }

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
                UltaManager.Instance.AddUltimatePoints(_mainGameConfig.LinesCountMultiplayers[linesCount - 1]);
            }
        }
    }

    private void DestroyLine(int mainAxisCurrentValue, int secondAxisLenght, bool isRow, ref string unlockedCellText) //cut this to pieces
    {
        bool fullSameResourcesColumn = _mainGameConfig.bonusResourcesOnDestroyLine ? true : false;
        int bonusResourcesOnDestroyLine = 0;
        ResourceType currentBonusResourceType = ResourceType.None;
        Dictionary<ResourceType, int> resourcesMultiplayers = new Dictionary<ResourceType, int>();
        Dictionary<CellType, int> cellTypesInLine = new Dictionary<CellType, int>();
        ResourceType currentResourceType = ResourceType.None;
        fullSameResourcesColumn = true;
        for (int secondAxis = 0; secondAxis < secondAxisLenght; secondAxis++) {
            Vector2 curPosition = !isRow ? new Vector2(mainAxisCurrentValue, secondAxis) : new Vector2(secondAxis, mainAxisCurrentValue);

            var cellType = _field[(int)curPosition.x, (int)curPosition.y];
            var config = PiecesViewTable.Instance.CellsList.CoreCellsConfigs.First(c => c.CellType == cellType);
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
            if (!_gameData.MonoLinesCount.TryAdd(currentBonusResourceType, 1)) {
                _gameData.MonoLinesCount[currentBonusResourceType]++;
            }

            TaskUtils.CheckMonoLinesForTasks(_gameData);
            _gameData.CollectedResources[currentBonusResourceType] += bonusResourcesOnDestroyLine;
            Vector2 curPosition = !isRow ? new Vector2(mainAxisCurrentValue, 5) : new Vector2(5, mainAxisCurrentValue);
            var needPosition = _mainCamera.WorldToScreenPoint(_cells[(int)curPosition.x, (int)curPosition.y].transform.position);
            SpawnResourceFxForLine(currentBonusResourceType, bonusResourcesOnDestroyLine, needPosition);
        }

        TaskUtils.CheckResourceCountForTasks(_gameData);
    }

    private void SpawnResourceFxForLine(ResourceType resourceType, int bonusResourcesOnDestroyLine, Vector2 needPosition) {
        if (!TaskUtils.IsResourceNeededForTasks(_gameData, resourceType)) {
            return;
        }

        string text = $"<sprite name={resourceType}> +{bonusResourcesOnDestroyLine} ";
        GameUI.Instance.ShowFloatingText(text, needPosition, 30, 1.5f, Vector2.zero);
    }

    private int CheckLineAndDestroyNeededCells(int mainAxisCurrentValue, bool isRow, int secondAxis, Dictionary<CellType, int> cellTypesInLine,
        Dictionary<ResourceType, int> resourcesMultiplayers, bool fullSameResourcesColumn, int bonusResourcesOnDestroyLine,
        ref ResourceType currentBonusResourceType) {
        Vector2Int curPosition = !isRow ? new Vector2Int(mainAxisCurrentValue, secondAxis) : new Vector2Int(secondAxis, mainAxisCurrentValue);
        var cellType = _field[(int)curPosition.x, (int)curPosition.y];
        var config = PiecesViewTable.Instance.CellsList.CoreCellsConfigs.First(c => c.CellType == cellType);

        if (!cellTypesInLine.TryAdd(cellType, 1)) {
            cellTypesInLine[cellType]++;
        }

        for (int i = 0; i < config.ResourcesForDestroy.Length; i++) {
            var resourceType = config.ResourcesForDestroy[i].ResourceType;
            resourcesMultiplayers.TryGetValue(resourceType, out int resourceMultiplayer);
            if (resourceMultiplayer == 0) {
                resourceMultiplayer = 1;
            }

            int count = config.ResourcesForDestroy[i].ResourceCount * resourceMultiplayer;
            if (!_gameData.CollectedResources.TryAdd(resourceType, count)) {
                _gameData.CollectedResources[resourceType] += count;
            }

            if (fullSameResourcesColumn) {
                //fix this if on destroy resources types be more than 1;
                bonusResourcesOnDestroyLine += config.ResourcesForDestroy[i].ResourceCount;
                currentBonusResourceType = resourceType;
            }

            if (!TaskUtils.IsResourceNeededForTasks(_gameData, resourceType)) {
                continue;
            }

            SpawnResourceFxForCell(curPosition, _cells[curPosition.x, curPosition.y]);
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
                    CoreCellTypeInfo configBox = PiecesViewTable.Instance.CellsList.CoreCellsConfigs.First(c => c.CellType == cellType);
                    TryAddResourceForCell(configBox, coordAround);

                    _gameAudio.PlayNextSound(_gameAudio.BoxBreaks);
                    DestroyCell(coordAround);

                    break;
                case CellType.GoldMine:
                    CoreCellTypeInfo configGoldMine = PiecesViewTable.Instance.CellsList.CoreCellsConfigs.First(c => c.CellType == cellType);
                    TryAddResourceForCell(configGoldMine, coordAround);

                    break;

                case CellType.CrystalMine:
                    CoreCellTypeInfo configCrystalMine = PiecesViewTable.Instance.CellsList.CoreCellsConfigs.First(c => c.CellType == cellType);
                    TryAddResourceForCell(configCrystalMine, coordAround);

                    MineCellAnimation(_cells[coordAround.x, coordAround.y].transform);
                    var randomPos = GetRandomEmptyCell();
                    if (randomPos == new Vector2(-1, -1)) return;
                    var configCrystal = PiecesViewTable.Instance.CellsList.CoreCellsConfigs.First(c => c.CellType == CellType.Crystal);
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

    public void TryAddResourceForCell(CoreCellTypeInfo config, Vector2Int coord) {
        if (config.ResourcesForDestroy.Length == 0) {
            return;
        }

        ResourceTypeAndCountSubClass gotResource = config.ResourcesForDestroy[0];

        foreach (TaskInfoAndUI infoAndUI in _gameData.CurrentTasks) {
            if (infoAndUI.TaskInfo.TaskType != TaskInfo.TaskType.getResource || infoAndUI.TaskInfo.NeedResource != gotResource.ResourceType) {
                continue;
            }

            if (!_gameData.CollectedResources.TryAdd(gotResource.ResourceType, gotResource.ResourceCount)) {
                _gameData.CollectedResources[gotResource.ResourceType] += gotResource.ResourceCount;
            }

            Vector3 canvasPosition = _mainCamera.WorldToScreenPoint(_cells[coord.x, coord.y].transform.position);
            GameUI.Instance.ShowFloatingText($" + <sprite name={gotResource.ResourceType}> " + " ",
                new Vector2(canvasPosition.x, canvasPosition.y + 15), 20, 1, infoAndUI.TaskUIView.CurrentTaskInfo.transform.position);
        }
    }

    public void DestroyCell(Vector2Int coord) {
        _field[coord.x, coord.y] = CellType.Empty;
        _cells[coord.x, coord.y].DestroyCell();
    }

    public bool CanPlaceAnyPiece() {
        if (AdditionalPiecePrefab != null && PieceUtils.CanPlacePiece(_field, AdditionalPiecePrefab.Data)) {
            return true;
        }

        if (_nextBlocks.Any(t => t != null && PieceUtils.CanPlacePiece(_field, t))) {
            return true;
        }

        return false;
    }

    public void SetWinState() {
        ExplodeCellsInRows();
    }

    public void SetLoseState() {
        Invoke(nameof(DestroyCurrentPieces), 2f);
    }

    private void DestroyCurrentPieces() {
        NextPiecesView.Instance.DestroyPieces();
        NextPiecesView.Instance.DestroyAdditionalPiece();
    }

    private void CalculateCellSpawnChances() {
        float lastChance = 0;
        CellsChanceToSpawn = new float[_currentCellsToSpawn.Count];
        for (int i = 0; i < _currentCellsToSpawn.Count; i++) {
            lastChance += PiecesViewTable.Instance.CellsList.CoreCellsConfigs.First(c => c.CellType == _currentCellsToSpawn[i]).ChanceToSpawn;
            CellsChanceToSpawn[i] = lastChance;
        }
    }

    public void Init(MainGameConfig mainGameConfig, GameData gameData) {
        _mainGameConfig = mainGameConfig;
        _gameData = gameData;
        _placedPiecesAmount = 0;
        _field = new CellType[_mainGameConfig.FieldSize, _mainGameConfig.FieldSize];
        _cells = new CellView[_mainGameConfig.FieldSize, _mainGameConfig.FieldSize];

        CalculateFiguresSpawnChances();
    }

    public override void SetupGame() {
        GenerateNewPieces();

        base.SetupGame();
    }

    public void PlaceStartingField(LevelConfig levelConfig) {
        if (levelConfig.StartFieldConfig == null) {
            return;
        }

        for (int i = 0; i < _field.GetLength(0); i++) {
            for (int j = 0; j < _field.GetLength(1); j++) {
                if (levelConfig.StartFieldConfig.GetCell(i, j) == CellType.Empty) {
                    continue;
                }

                CellTypeInfo config = PiecesViewTable.Instance.CellsList.CoreCellsConfigs.First(c =>
                    c.CellType == levelConfig.StartFieldConfig.GetCell(i, j));
                if (!_isSlimeExist && config.CellType == CellType.Slime) {
                    _isSlimeExist = true;
                }

                PlaceOneSizePiece(config, new Vector2Int(i, j), true, false);
            }
        }
    }

    public void InitFromLevel(LevelConfig config) {
        _currentGuaranteedFirstCells = new List<CellTypeInfo>();
        foreach (var cellInfo in config.CurrentGuaranteedFirstCells) {
            _currentGuaranteedFirstCells.Add(cellInfo);
        }

        var startCells = config.CellTypesTableConfig;
        _currentCellsToSpawn = new List<CellType>();
        for (int i = 0; i < startCells.CellsToSpawn.Length; i++) {
            _currentCellsToSpawn.Add(startCells.CellsToSpawn[i]);
        }

        CalculateCellSpawnChances();
    }

    public CellView PlaceOneSizePiece(CellTypeInfo cellInfo, Vector2Int pos, bool setNewInfo, bool spawnFx = true) {
        GameObject tmpContainer = new();
        tmpContainer.transform.SetParent(FieldContainers.Instance.FieldContainer);
        List<Vector3> poses = new List<Vector3>();
        List<GameObject> cells = new List<GameObject>();
        var prefab = PiecesViewTable.Instance.CellsViewList.GetCellByType(cellInfo.CellType);
        CellView go = Instantiate(prefab, FieldContainers.Instance.FieldContainer);
        go.SetSeed(Guid.NewGuid());
        go.transform.localPosition = new Vector3(pos.x, -0.2f, pos.y);
        poses.Add(new Vector3(pos.x, -0.2f, pos.y));
        if (setNewInfo) {
            _field[pos.x, pos.y] = cellInfo.CellType;
            _cells[pos.x, pos.y] = go;
            if (spawnFx) {
                SpawnResourceFxForCell(pos, go);
            }
        }

        if (!_gameData.PlacedCellsCount.TryAdd(cellInfo.CellType, 1)) {
            _gameData.PlacedCellsCount[cellInfo.CellType]++;
        }

        cells.Add(go.gameObject);

        foreach (var cell in cells) {
            cell.transform.SetParent(tmpContainer.transform);
        }

        return go;
    }

    public void ReleaseFloatingText(FloatingTextView needTextObject) {
        GameUI.Instance.ReleaseFloatingText(needTextObject);
    }

    private void SaveWinGame() {
        StorageManager.GameDataMain.GoldAmount += 100 /* + StorageManager.GameDataMain.CurMaxLevel * 5*/;
        // StorageManager.GameDataMain.MagicCubesAmount += 5 + StorageManager.GameDataMain.CurMaxLevel / 2;
        StorageManager.GameDataMain.MagicCubesAmount += MainManager.Instance.CurrentLevelConfig.MagicCubesCount;
        MainManager.Instance.IncreaseMaxLevel();
        StorageManager.SaveGame();
    }
}