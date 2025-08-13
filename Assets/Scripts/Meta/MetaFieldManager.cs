using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AYellowpaper.SerializedCollections;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using Vector3 = UnityEngine.Vector3;

public class MetaFieldManager : FieldManager {
    public static MetaFieldManager Instance { get; private set; }

    [field: SerializeField]
    private VillageStuffConfig _villageStuffConfig;

    [field: Header("Meta")]
    [field: SerializeField]
    public MainMetaConfig MainMetaConfig { get; private set; }

    private List<ResourceMarkAndPieces> _connectedGroups = new List<ResourceMarkAndPieces>();

    [SerializeField]
    private InventoryCellView _inventoryCellPrefab;

    [SerializeField]
    private Transform _inventoryCellsContainer;

    private InventoryCellView _currentDraggedPieceButton = null;
    private PieceView _currentDraggedPiece = null;
    private List<InventoryCellView> _currentPiecesInInventory = new List<InventoryCellView>();
    private int[,] _groupCellIndex;

    private int[,] _formGroupCellIndex;
    private Dictionary<int, List<Vector2Int>> _formGroupCellPositions = new Dictionary<int, List<Vector2Int>>();

    private Vector3 _dragStartPosition;
    private Vector3 _dragStartPositionForUICheck;
    private bool _nowCellUnlockUIWasClose;
    private float _instantiatedCellsGlobalY = -0.25f;
    public Dictionary<int, List<Vector2Int>> LockedCellGroups { get; private set; }

    private Vector2Int _currentMarkedFieldCell;

    [SerializeField]
    private Transform _cameraMin;

    [SerializeField]
    private Transform _cameraMax;

    [SerializedDictionary]
    public SerializedDictionary<CellType, ResourceType> _cellsResources;

    [SerializeField]
    private GameObject _markedCellPrefab;

    [SerializeField]
    private Transform _markedLockedCellsContainer;

    private List<GameObject> _markedLockedCells = new();
    
    public bool CanDragCamera = true;

    protected override void Awake() {
        base.Awake();
        Instance = this;
    }

    public void FilterInventoryPieces(int filterResourceId) {
        ResourceType filterResource = (ResourceType)filterResourceId;
        foreach (Transform child in _inventoryCellsContainer) {
            InventoryCellView cell = child.GetComponent<InventoryCellView>();
            CellType cellType = cell.Data.Type.CellType;
            ResourceType cellResource = _cellsResources.ContainsKey(cellType) ? _cellsResources[cellType] : ResourceType.None;
            child.gameObject.SetActive(filterResource == ResourceType.None || cellResource == filterResource);
        }
    }

    public void SetCurrentPiece(PieceView pieceView = null, InventoryCellView inventoryCellView = null) {
        if (pieceView == null)
            Destroy(_currentDraggedPiece.gameObject);
        _currentDraggedPieceButton = inventoryCellView;
        _currentDraggedPiece = pieceView;
    }

    protected override void Update() {
        base.Update();

        if (MainManager.Instance._hasInternetConnection) {
            var span = StorageManager.GameDataMain.LastGetPieceTimeDateTime - MainManager.Instance._currentGameTime;
            MetaUI.Instance.UpdateGetPieceTimer(span);
        }

        CheckDragCamera();
    }

    public override void ToggleDestroyPieceMode() {
        base.ToggleDestroyPieceMode();
        CloseCellUI();
    }

    private bool _isDragging;

    private void CheckDragCamera() {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) {
            _dragStartPosition = Input.mousePosition;
            _dragStartPositionForUICheck = Input.mousePosition;
            _isDragging = true;
        }

        if (Input.GetMouseButtonUp(0)) {
            _isDragging = false;
            if (_currentDraggedPieceButton != null) {
                _currentDraggedPiece.OnDrop();
                CloseCellUI();
            } else if (!_nowCellUnlockUIWasClose && !_isDestroyPieceMode && _dragStartPosition == _dragStartPositionForUICheck) {
                if (!EventSystem.current.IsPointerOverGameObject()) {
                    TryCastLockCell();
                }
            } else if (Vector3.Distance(_dragStartPosition, _dragStartPositionForUICheck) > 5f && _currentMarkedFieldCell != -Vector2Int.one)
                CloseCellUI();

            _nowCellUnlockUIWasClose = false;
        }

        if (Input.GetMouseButton(0) && CanDragCamera && _isDragging && _currentDraggedPieceButton == null) {
            DragCamera();
        }
    }

    private void DragCamera() {
        Ray prevRay = _mainCamera.ScreenPointToRay(_dragStartPosition);
        Ray currRay = _mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(prevRay, out RaycastHit prevHit, Mathf.Infinity, _groundMask) &&
            Physics.Raycast(currRay, out RaycastHit currHit, Mathf.Infinity, _groundMask)) {
            Vector3 delta = prevHit.point - currHit.point;
            Vector3 needPosition = CameraContainer.position + delta;
            needPosition.x = Mathf.Clamp(needPosition.x, _cameraMin.position.x, _cameraMax.position.x);
            needPosition.z = Mathf.Clamp(needPosition.z, _cameraMin.position.z, _cameraMax.position.z);

            CameraContainer.position = new Vector3(needPosition.x, needPosition.y, needPosition.z);

            _dragStartPosition = Input.mousePosition;
        }
    }

    public bool HasPieceInInventory(CellType cellType) {
        foreach (InventoryCellView cell in _currentPiecesInInventory) {
            if (cell.Data.Type.CellType == cellType) return true;
        }
        
        return false;
    }

    public void RemovePieceFromInventory(CellType cellType) {
        foreach (InventoryCellView cell in _currentPiecesInInventory) {
            if (cell.Data.Type.CellType != cellType) continue;
            
            Destroy(cell.gameObject);
            _currentPiecesInInventory.Remove(cell);
            return;
        }
    }

    protected override bool TryDestroyPiece() {
        Physics.Raycast(_mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, _pieceMask);
        if (hit.collider != null && StorageManager.GameDataMain.MetaHummerCount > 0) {
            Vector3 cellPos = new Vector3(Mathf.RoundToInt(hit.collider.transform.localPosition.x),
                Mathf.RoundToInt(hit.collider.transform.localPosition.y), Mathf.RoundToInt(hit.collider.transform.localPosition.z));
            var cellType = _field[(int)cellPos.x, (int)cellPos.z];
            if (cellType == CellType.LockedMetaCell || cellType == CellType.VillagePart || FieldUtils.IsVillageCell(cellType)) {
                return false;
            }

            StorageManager.GameDataMain.MetaHummerCount--;

            int groupIndex = _groupCellIndex[(int)cellPos.x, (int)cellPos.z];

            int figureIndex = _formGroupCellIndex[(int)cellPos.x, (int)cellPos.z];

            CollectResourcesFromMark(groupIndex - 1, 1);
            _connectedGroups[groupIndex - 1].ResourceMarkView.CollectAnimation();

            var destroyedCellsPositions = _formGroupCellPositions[figureIndex];

            var destroyedCells = new CellView[destroyedCellsPositions.Count];
            int index = 0;
            foreach (var cellPosition in destroyedCellsPositions) {
                _groupCellIndex[cellPosition.x, cellPosition.y] = 0;
                destroyedCells[index] = _cells[cellPosition.x, cellPosition.y];
                index++;
                _field[cellPosition.x, cellPosition.y] = CellType.Empty;
                StorageManager.GameDataMain.FieldRows[cellPosition.x].RowCells[cellPosition.y] =
                    new ResourceAndCountData(_field[cellPosition.x, cellPosition.y], 0);
            }

            DeleteFigureFormFromList(figureIndex);
            HummerDestoyPieceAnimation(destroyedCells);
            RecalculateCellGroupAfterDeletePiece(groupIndex);
            return true;
        }

        return false;
    }

    private void TryCastLockCell() {
        Physics.Raycast(_mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, _pieceMask);
        if (hit.collider != null) {
            Vector2Int cellPos = new Vector2Int(Mathf.RoundToInt(hit.collider.transform.localPosition.x),
                Mathf.RoundToInt(hit.collider.transform.localPosition.z));

            if (!FieldUtils.IsInsideField(_field, cellPos)) {
                return;
            }

            if (_field[cellPos.x, cellPos.y] == CellType.LockedMetaCell)
                CastLockedCell(cellPos);
            else
                CastResourceCell(cellPos);

            //check neighbour closed cell
        }
    }

    private void CastResourceCell(Vector2Int cellPos) {
        if (_currentMarkedFieldCell != -Vector2Int.one) CloseCellUI();
        int groupIndex = _groupCellIndex[cellPos.x, cellPos.y] - 1;

        _currentMarkedFieldCell = new Vector2Int(cellPos.x, cellPos.y);

        var cellConfig =
            PiecesViewTable.Instance.CellsList.MetaCellsConfigs.First(c =>
                c.CellType == _field[_currentMarkedFieldCell.x, _currentMarkedFieldCell.y]);

        float resourceMultiplier = MainMetaConfig.ResourceMultipliers[_connectedGroups[groupIndex].Pieces.Count];

        ShowUpgradeTileDialog(cellPos, cellConfig, resourceMultiplier);
    }

    private void ShowUpgradeTileDialog(Vector2Int cellPos, MetaCellTypeInfo cell, float multiplier) {
        float production = cell.AfkProduceCountPerSecond * multiplier *
                           _formGroupCellPositions[_formGroupCellIndex[cellPos.x, cellPos.y]].Count;
        float capacity = cell.MaxAfkCapacity * multiplier * _formGroupCellPositions[_formGroupCellIndex[cellPos.x, cellPos.y]].Count;

        var dialogData = new DialogWithData {
            DialogType = typeof(UpgradeTileDialog),
            Data = new UpgradeTileDialog.Data {
                ClickUpgrade = UpgradeResourceCell,
                ClickClose = CloseCellUI,
                Resource = cell.AfkResourceType,
                IncomeBefore = (int)production,
                IncomeAfter = (int)(production * 2), // TODO: брать из конфига
                CapacityBefore = (int)capacity,
                CapacityAfter = (int)(capacity * 2), // TODO: брать из конфига
                TileName = cell.CellName,
                CurrentLevel = 1, // TODO: убрать заглушку уровня
                IsMaxLevel = cell.UpgradeCellType == CellType.Empty,
                UpgradeCost = cell.UpgradeCost
            }
        };
        DialogsManager.Instance.ShowDialogWithData(dialogData);
    }

    public void UpgradeResourceCell() {
        //   int groupIndex = _groupCellIndex[_currentMarkedFieldCell.x, _currentMarkedFieldCell.y] - 1;
        var cellsToUpgrade = _formGroupCellPositions[_formGroupCellIndex[_currentMarkedFieldCell.x, _currentMarkedFieldCell.y]];
        var cellConfig =
            PiecesViewTable.Instance.CellsList.MetaCellsConfigs.First(c =>
                c.CellType == _field[_currentMarkedFieldCell.x, _currentMarkedFieldCell.y]);
        //  Vector3 uiPos = Vector3.zero;
   Vector3 finalUiNeedPos = Vector3.zero;
            foreach (var cellPos in cellsToUpgrade)
                finalUiNeedPos += new Vector3(cellPos.x, 0, cellPos.y);

            finalUiNeedPos /= cellsToUpgrade.Count;
        if (cellConfig.AfkResourceType != ResourceType.Coins) {
            if (StorageManager.GameDataMain.GetResource(cellConfig.AfkResourceType) < cellConfig.UpgradeCost) {
                return;
            }

           FloatingResourcesManager.Instance.FromPointToPointAnimation(cellConfig.UpgradeCost, cellConfig.AfkResourceType,
                MetaUI.Instance._openResourceTabButtonTransform.position, _mainCamera.WorldToScreenPoint(finalUiNeedPos),
                ChangeResorceText,StorageManager.GameDataMain.GetResource(cellConfig.AfkResourceType),true,false);   
       //     StorageManager.GameDataMain.AddResource(cellConfig.AfkResourceType, -cellConfig.UpgradeCost);
        } else {
            if (StorageManager.GameDataMain.GetResource(ResourceType.Coins) < cellConfig.UpgradeCost) 
                return;
            
            FloatingResourcesManager.Instance.FromPointToPointAnimation(cellConfig.UpgradeCost, ResourceType.Coins,
                MetaUI.Instance.CountersPanelView.GetCoinsIconPosition, _mainCamera.WorldToScreenPoint(finalUiNeedPos),
                ChangeResorceText,StorageManager.GameDataMain.GetResource(cellConfig.AfkResourceType), true, false);
          //   StorageManager.GameDataMain.AddResource(ResourceType.Coins, -cellConfig.UpgradeCost);
        }

        CollectResourcesFromMark(_groupCellIndex[_currentMarkedFieldCell.x, _currentMarkedFieldCell.y] - 1, 1);
        var dragConfig = ConfigsManager.Instance.DragConfig;
        var finY = FieldContainers.Instance.PlacedCellsVerticalAnchor.position.y - 0.3f;
        var upgradedPrefab = PiecesViewTable.Instance.CellsViewList.GetCellByType(cellConfig.UpgradeCellType);
        foreach (var cell in cellsToUpgrade) {
            if (_field[cell.x, cell.y] == CellType.VillagePart) continue;
            _field[cell.x, cell.y] = cellConfig.UpgradeCellType;
            _cells[cell.x, cell.y].UpgradeStart();
            var seed = _cells[cell.x, cell.y].Seed;

            var newCell = Instantiate(upgradedPrefab, FieldContainers.Instance.FieldContainer);
            var pos = _cells[cell.x, cell.y].transform.localPosition;
            pos.y = _instantiatedCellsGlobalY;
            newCell.transform.localPosition = pos;
            _cells[cell.x, cell.y] = newCell;
            StorageManager.GameDataMain.FieldRows[cell.x].RowCells[cell.y].CellType = cellConfig.UpgradeCellType;
            newCell.SetSeed(seed);
            newCell.UpgradeEnd(dragConfig, finY);
        }

        CloseCellUI();
    }

    public void ChangeResorceText(ResourceType resourceType ,float needCount)
    {
        StorageManager.GameDataMain.AddResource(resourceType, needCount);
        MetaUI.Instance.CountersPanelView.SetResourceCount(resourceType, StorageManager.GameDataMain.GetResource(resourceType));
    }
    
    private void CastLockedCell(Vector2Int cellPos) {
        if (_currentMarkedFieldCell != -Vector2Int.one) CloseCellUI();

        int groupIndex = _groupCellIndex[cellPos.x, cellPos.y] - 1000;

        if (groupIndex != 1) {
            var lockedCells = LockedCellGroups[groupIndex];
            bool hasEmptyCellAround = false;
            foreach (var lockedCell in lockedCells) {
                var cellsAround = FieldUtils.GetCellsAround(_field, lockedCell);
                foreach (var checkedCell in cellsAround) {
                    if (_field[checkedCell.x, checkedCell.y] == CellType.Empty) {
                        hasEmptyCellAround = true;
                        break;
                    }
                }

                if (hasEmptyCellAround) break;
            }

            if (!hasEmptyCellAround) {
                int groupParent = MainMetaConfig.LockedCellsFieldConfig.GroupsParents[groupIndex - 1];
                CastLockedCell(LockedCellGroups[groupParent][0]);
                return;
            }
            //check for empty cells
        }

        MarkLockedGroup(groupIndex);
        var lockedCellGroup = LockedCellGroups[groupIndex];
        Vector3 uiPos = Vector3.zero;

        foreach (var lockCellPos in lockedCellGroup) {
            uiPos += _cells[lockCellPos.x, lockCellPos.y].transform.position;
        }

        uiPos /= lockedCellGroup.Count;
        _currentMarkedFieldCell = new Vector2Int(cellPos.x, cellPos.y);

        MetaWorldCanvasView.Instance.UnlockFieldCellsView.SetData(uiPos, LockedCellGroups[groupIndex].Count, GetGroupSize(groupIndex));
        MetaWorldCanvasView.Instance.UnlockFieldCellsView.SetActiveUnlockUI(true);
    }

    private void MarkLockedGroup(int groupIndex) {
        UnmarkLockedGroup();

        foreach (Vector2Int cell in LockedCellGroups[groupIndex]) {
            GameObject newCell = Instantiate(_markedCellPrefab, _markedLockedCellsContainer);
            newCell.transform.localPosition = _cells[cell.x, cell.y].transform.position;
            _markedLockedCells.Add(newCell);
        }
    }

    public void Craft(MetaCraftInfo craftInfo) {
        foreach (var resource in craftInfo.NeededResources) {
            StorageManager.GameDataMain.AddResource(resource.Key, -resource.Value);
        }
        
        RemovePieceFromInventory(craftInfo.NeededCell);
        var pieceData = PieceUtils.GetNewMetaPiece(craftInfo.ResultCellTypeInfo);
        AddPieceToInventory(pieceData);
    }

    private Vector2Int GetGroupSize(int groupIndex) {
        Vector2Int min = LockedCellGroups[groupIndex][0], max = min;
        
        foreach (Vector2Int cell in LockedCellGroups[groupIndex]) {
            min = new Vector2Int(Mathf.Min(cell.x, min.x), Mathf.Min(cell.y, min.y));
            max = new Vector2Int(Mathf.Max(cell.x, max.x), Mathf.Max(cell.y, max.y));
        }

        return max - min + Vector2Int.one;
    }

    private void UnmarkLockedGroup() {
        foreach (GameObject markedCell in _markedLockedCells) {
            Destroy(markedCell);
        }
        _markedLockedCells = new List<GameObject>();
    }

    public void UnlockCellForButton()
    {
        UnlockCell();
    }
    

    public async UniTask UnlockCell() {
        int groupIndex = _groupCellIndex[_currentMarkedFieldCell.x, _currentMarkedFieldCell.y] - 1000;
        var lockedCellGroup = LockedCellGroups[groupIndex];
        Vector3 uiPos = Vector3.zero;

        if (StorageManager.GameDataMain.GetResource(ResourceType.MagicCube) <= lockedCellGroup.Count - 1) return;

        UnmarkLockedGroup();
        List<Vector2> endPositions= new List<Vector2>();
        foreach (var lockCell in lockedCellGroup)
            endPositions .Add(_mainCamera.WorldToScreenPoint(_cells[lockCell.x, lockCell.y].transform.position)  );

        MetaUI.Instance.CountersPanelView.SetMagicCubes((int)StorageManager.GameDataMain.GetResource(ResourceType.MagicCube)); 
        FloatingResourcesManager.Instance.FromPointToSomePointsAnimation( ResourceType.MagicCube, MetaUI.Instance.CountersPanelView.GetMagicCubesIconPosition,
            endPositions, ChangeResorceText, StorageManager.GameDataMain.GetResource(ResourceType.MagicCube), true, false );
           
        StorageManager.GameDataMain.RemainedLockedZones.Remove(groupIndex);
       CloseCellUI();
       await UniTask.Delay(TimeSpan.FromSeconds(0.7f));
        foreach (var lockCellPos in lockedCellGroup) {
            _cells[lockCellPos.x, lockCellPos.y].DestroyCell();
            _cells[lockCellPos.x, lockCellPos.y] = null;
            _field[lockCellPos.x, lockCellPos.y] = CellType.Empty;
            _groupCellIndex[lockCellPos.x, lockCellPos.y] = 0;
            StorageManager.GameDataMain.FieldRows[lockCellPos.x].RowCells[lockCellPos.y] =
                new ResourceAndCountData(_field[lockCellPos.x, lockCellPos.y], 0);
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
        }
        
      
    }

    public void CloseCellUI() {
        if (_currentMarkedFieldCell == -Vector2Int.one) return;
        
        UnmarkLockedGroup();
        MetaWorldCanvasView.Instance.UnlockFieldCellsView.SetActiveUnlockUI(false);
        DialogsManager.Instance.CloseDialog(typeof(UpgradeTileDialog));
        _currentMarkedFieldCell = -Vector2Int.one;
        _nowCellUnlockUIWasClose = true;
    }

    public void RecalculateCellGroupAfterDeletePiece(int groupIndex) {
        if (_connectedGroups[groupIndex - 1].Pieces.Count == 1) {
            MetaWorldCanvasView.Instance.ReleaseResourceMark(_connectedGroups[groupIndex - 1].ResourceMarkView);
            _connectedGroups[groupIndex - 1] = new ResourceMarkAndPieces(null, new List<(int, int)>());
            return;
        }

        var checkedCells = new int[_field.GetLength(0), _field.GetLength(1)];
        int curMaxIndex = 1;
        foreach (var (row, col) in _connectedGroups[groupIndex - 1].Pieces) {
            if (checkedCells[row, col] != 0 || _field[row, col] == CellType.Empty) continue;

            int curGroupIndex = 0;
            var checkedCellType = _field[row, col];
            foreach (var dir in FieldUtils.Directions) {
                Vector2Int combined = dir + new Vector2Int(col, row);
                if (combined.y >= _field.GetLength(0) || combined.x >= _field.GetLength(1) || combined.y < 0 || combined.x < 0) continue;

                if (checkedCellType == _field[combined.y, combined.x]) {
                    if (checkedCells[combined.y, combined.x] != 0) {
                        if (curGroupIndex == 0)
                            curGroupIndex = checkedCells[combined.y, combined.x];
                        else {
                            int newIndex = checkedCells[combined.y, combined.x];
                            foreach (var (cellRow, cellCol) in _connectedGroups[groupIndex - 1].Pieces) {
                                if (checkedCells[cellRow, cellCol] == newIndex)
                                    checkedCells[cellRow, cellCol] = curGroupIndex;
                            }
                        }
                    } else
                        checkedCells[combined.y, combined.x] = curGroupIndex;
                }
            }

            if (curGroupIndex == 0)
                curGroupIndex = curMaxIndex++;

            checkedCells[row, col] = curGroupIndex;
        }

        Dictionary<int, List<(int row, int col)>> cellsGroupIndex = new Dictionary<int, List<(int row, int col)>>();
        for (int i = 0; i < checkedCells.GetLength(0); i++) {
            for (int j = 0; j < checkedCells.GetLength(1); j++) {
                if (checkedCells[i, j] == 0) continue;
                if (cellsGroupIndex.ContainsKey(checkedCells[i, j]))
                    cellsGroupIndex[checkedCells[i, j]].Add((i, j));
                else
                    cellsGroupIndex.Add(checkedCells[i, j], new List<(int row, int col)> { (i, j) });
            }
        }

        MetaWorldCanvasView.Instance.ReleaseResourceMark(_connectedGroups[groupIndex - 1].ResourceMarkView);

        _connectedGroups[groupIndex - 1] = new ResourceMarkAndPieces();
        List<int> emptyIndexes = new List<int>();
        for (int i = 0; i < _connectedGroups.Count; i++) {
            if (_connectedGroups[i].ResourceMarkView == null)
                emptyIndexes.Add(i);
        }

        Color needColor = Color.clear;
        foreach (var checkedCell in cellsGroupIndex) {
            Vector3 collectResourceMarkPosition = new Vector3();
            foreach (var (row, col) in checkedCell.Value) {
                collectResourceMarkPosition += _cells[row, col].transform.position;

                if (needColor == Color.clear) {
                    needColor = PiecesViewTable.Instance.CellsList.MetaCellsConfigs.First(c => c.CellType == _field[row, col]).MarkCellColor;
                    needColor.a = 1;
                }
            }

            var cellMarkView = SpawnResourceMark(collectResourceMarkPosition / checkedCell.Value.Count, 0, 0, ResourceType.None, needColor);
            cellMarkView.gameObject.SetActive(false);
            var resourceMarkAndPieces = new ResourceMarkAndPieces(cellMarkView, checkedCell.Value);

            int needIndex = 0;
            if (emptyIndexes.Count > 0) {
                _connectedGroups[emptyIndexes[0]] = resourceMarkAndPieces;
                needIndex = emptyIndexes[0] + 1;
                emptyIndexes.RemoveAt(0);
            } else {
                _connectedGroups.Add(resourceMarkAndPieces);
                needIndex = _connectedGroups.Count;
            }

            foreach (var (row, col) in checkedCell.Value)
                _groupCellIndex[row, col] = needIndex;
        }
    }

    private ResourceMarkView SpawnResourceMark(Vector3 pos, int maxResource, float currentResource, ResourceType resourceType,
        Color resourceColor) {
        return MetaWorldCanvasView.Instance.SpawnResourceMark(pos, maxResource, currentResource, resourceType, resourceColor,
            _connectedGroups.Count);
    }

    public TimeSpan GetTimeUntilNextPiece() {
        // if (StorageManager.GameDataMain.HealthCount >= MAX_HEALTH_COUNT) return TimeSpan.Zero;

        TimeSpan timeSinceLastUpdate = MainManager.Instance._currentGameTime - StorageManager.GameDataMain.LastGetPieceTimeDateTime;
        double minutesPassed = timeSinceLastUpdate.TotalMinutes;
        double minutesUntilNext = 480 - (minutesPassed % 480);
        return TimeSpan.FromMinutes(minutesUntilNext);
    }

    public void Play() {
        if (StorageManager.GameDataMain.HealthCount > 0) {
            StorageManager.GameDataMain.LastExitTime = MainManager.Instance._currentGameTime.ToString(CultureInfo.InvariantCulture);
            StorageManager.SaveGame();
            SceneManager.LoadScene("GameScene");
        } else {
            //floating window with "watch ad and get health"
        }
    }

    public void BuyPiece() {
        if (StorageManager.GameDataMain.GetResource(ResourceType.Wood) >= 100 && StorageManager.GameDataMain.GetResource(ResourceType.Rocks) >= 100 &&
            StorageManager.GameDataMain.GetResource(ResourceType.Food) >= 100) {
            // DialogsManager.Instance.ShowDialog(typeof(BuyPieceDialog));
            StorageManager.GameDataMain.AddResource(ResourceType.Wood, -100);
            StorageManager.GameDataMain.AddResource(ResourceType.Rocks, -100);
            StorageManager.GameDataMain.AddResource(ResourceType.Food, -100);
            UpdateResourcesCountUIText();
            GenerateNewPiece();
        }
    }

    public void UpdateResourcesCountUIText() {
        foreach (var kvp in StorageManager.GameDataMain.GetAllResources()) {
            MetaUI.Instance.CountersPanelView.SetResourceCount(kvp.Key, kvp.Value);
        }
    }

    public void GetPiece() {
        if (!MainManager.Instance._hasInternetConnection) {
            return;
        }

        if (MainManager.Instance._currentGameTime >= StorageManager.GameDataMain.LastGetPieceTimeDateTime) {
            StorageManager.GameDataMain.LastGetPieceTime =
                (MainManager.Instance._currentGameTime + TimeSpan.FromHours(8)).ToString(CultureInfo.InvariantCulture);
            var pieceData = GenerateNewPiece();
            MetaUI.Instance.OpenLootboxDialog(pieceData);
        }
    }

    public void CollectAll() {
        DialogsManager.Instance.ShowDialog(typeof(CollectAllDialog));
    }

    public PieceData GenerateNewPiece() {
        CellTypeInfo cellTypeInfo = StorageManager.GameDataMain.PlacedInMetaPiecesCount == 0 ? _villageStuffConfig.VillageCellTypeInfo : null;
        var pieceData = PieceUtils.GetNewMetaPiece(cellTypeInfo);
        AddPieceToInventory(pieceData);
        StorageManager.GameDataMain.PlacedInMetaPiecesCount++;
        return pieceData;
    }

    public void AddPieceToInventory(PieceData pieceView) {
        InventoryCellView inventoryCell = Instantiate(_inventoryCellPrefab, _inventoryCellsContainer);
        inventoryCell.SetPieceInfo(pieceView);
        MetaBuildManager.Instance.SetInventoryCellIcon(inventoryCell);
        _currentPiecesInInventory.Add(inventoryCell);
        SaveInventory();
    }

    public override void SetupGame() {
        _field = new CellType[MainMetaConfig.FieldSize, MainMetaConfig.FieldSize];
        _cells = new CellView[MainMetaConfig.FieldSize, MainMetaConfig.FieldSize];
        CalculateFiguresSpawnChances();
        _currentCellsToSpawn = new List<CellType>();
        foreach (var cellType in MainMetaConfig.CellsToSpawn) {
            _currentCellsToSpawn.Add(cellType);
        }

        CalculateCellSpawnChances();
        if (!StorageManager.GameDataMain.FieldSaveIsCreated) {
            StorageManager.GameDataMain.FieldSaveIsCreated = true;
            StorageManager.GameDataMain.FieldRows = new MetaFieldData[_field.GetLength(0)];
            for (int i = 0; i < _field.GetLength(0); i++) {
                StorageManager.GameDataMain.FieldRows[i].RowCells = new ResourceAndCountData[_field.GetLength(1)];
                for (int j = 0; j < _field.GetLength(1); j++) {
                    _field[i, j] = CellType.LockedMetaCell;
                    var prefab = PiecesViewTable.Instance.CellsViewList.GetCellByType(CellType.LockedMetaCell);
                    var go = Instantiate(prefab, FieldContainers.Instance.FieldContainer);
                    go.transform.localPosition = new Vector3(i, _instantiatedCellsGlobalY, j);
                    _cells[i, j] = go;
                    // go.SetSeed(Guid.NewGuid());

                    StorageManager.GameDataMain.FieldRows[i].RowCells[j] = new ResourceAndCountData(_field[i, j], 0);
                }
            }
        } else if (StorageManager.GameDataMain.FieldRows != null && StorageManager.GameDataMain.FieldRows.Length > 1) {
            _field = new CellType[StorageManager.GameDataMain.FieldRows.Length, StorageManager.GameDataMain.FieldRows[0].RowCells.Length];
            Vector2Int villagePosition = -Vector2Int.one;
            for (int i = 0; i < _field.GetLength(0); i++) {
                for (int j = 0; j < _field.GetLength(1); j++) {
                    _field[i, j] = StorageManager.GameDataMain.FieldRows[i].RowCells[j].CellType;
                    var cellType = _field[i, j];
                    if (cellType != CellType.Empty) {
                        if (cellType != CellType.VillagePart) {
                            CellView prefab = PiecesViewTable.Instance.CellsViewList.GetCellByType(cellType);
                            var go = Instantiate(prefab, FieldContainers.Instance.FieldContainer);
                            go.transform.localPosition = new Vector3(i, _instantiatedCellsGlobalY, j);
                            _cells[i, j] = go;

                            go.SetSeed(Guid.NewGuid());
                            if (FieldUtils.IsVillageCell(cellType))
                                villagePosition = new Vector2Int(i, j);
                        } else {
                            CellView prefab = _cells[villagePosition.x, villagePosition.y];
                            _cells[i, j] = prefab;
                        }
                    }
                }
            }
        }

        SetFigureFormsInfoFromData();
        UpdateResourcesCountUIText();

        GetResourceCollectMarks();

        InvokeRepeating(nameof(UpdateResourceMarks), MainMetaConfig.resourceMarksUpdateCouldown, MainMetaConfig.resourceMarksUpdateCouldown);
        GetInventoryFromSave();
        MetaUI.Instance.CountersPanelView.SetMagicCubes((int)StorageManager.GameDataMain.GetResource(ResourceType.MagicCube));
        MetaUI.Instance.CountersPanelView.SetGold((int)StorageManager.GameDataMain.GetResource(ResourceType.Coins));
        MetaUI.Instance.SetPlayText("Lv. " + (StorageManager.GameDataMain.CurMaxLevel + 1));

        if (MainManager.Instance._hasInternetConnection) {
            MainManager.Instance.SetupGetPieceTimer();
            MainManager.Instance.SetupHealth();
        }

        if (LoadingManager.Instance.FirstLoad && MainManager.Instance._hasInternetConnection) {
            TryShowRetentionDialog();
        }
        base.SetupGame();
    }
    
    private void TryShowRetentionDialog() {
        LoadingManager.Instance.FirstLoad = false;
        Dictionary<ResourceType, float> afkResources = GetAllAfkResourceInfoForDialog();

        if (afkResources.All(kvp => kvp.Value == 0)) {
            return;
        }

        List<RetentionDialog.RetentionResource> resourcesToDialog = new List<RetentionDialog.RetentionResource>();
        foreach (var resource in afkResources) {
            resourcesToDialog.Add(new() { Count = (int)resource.Value, Resource = resource.Key });
        }

        var dialog = new DialogWithData {
            DialogType = typeof(RetentionDialog),
            Data = new RetentionDialog.Data {
                ClickDoubleClaim = CollectDoubleResourcesFromAllMarks,
                ClickDefaultClaim = CollectDefaultResourcesFromAllMarks,
                OfflineResources = resourcesToDialog
            }
        };

        DialogsManager.Instance.ShowDialogWithData(dialog);
    }

    private Dictionary<ResourceType, float> GetAllAfkResourceInfoForDialog() {
        Dictionary<ResourceType, float> infoForDialog = new Dictionary<ResourceType, float>();

        for (int i = 0; i < StorageManager.GameDataMain.FieldRows.Length; i++) {
            for (int j = 0; j < StorageManager.GameDataMain.FieldRows[i].RowCells.Length; j++) {
                var cellType = StorageManager.GameDataMain.FieldRows[i].RowCells[j].CellType;
                if (cellType == CellType.Empty || cellType == CellType.LockedMetaCell) continue;
                float resourceCount = StorageManager.GameDataMain.FieldRows[i].RowCells[j].ResourceCount;
                ResourceType resourceType =
                    PiecesViewTable.Instance.CellsList.MetaCellsConfigs.First(c => c.CellType == cellType).AfkResourceType;
                if (resourceType == ResourceType.None) {
                    continue;
                }
                if (!infoForDialog.TryAdd(resourceType, resourceCount))
                    infoForDialog[resourceType] += resourceCount;
            }
        }

        return infoForDialog.Where(kvp => kvp.Value >= 1).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public Dictionary<ResourceType, float> GetAllResourceInfoForDialog() {
        Dictionary<ResourceType, float> infoForDialog = new();
        for (int i = 0; i < _connectedGroups.Count; i++) {
            if (_connectedGroups[i].ResourceMarkView == null) continue;
            ResourceType curResource = ResourceType.None;
            foreach (var (row, col) in _connectedGroups[i].Pieces) {
                if (_field[row, col] == CellType.Empty) continue;
                var cellConfig = PiecesViewTable.Instance.CellsList.MetaCellsConfigs.First(c => c.CellType == _field[row, col]);
                if (curResource == ResourceType.None)
                    curResource = cellConfig.AfkResourceType;
                if (cellConfig.AfkResourceType != ResourceType.None) {
                    float resourceMultiplayer = MainMetaConfig.ResourceMultipliers[_connectedGroups[i].Pieces.Count];
                    if (!infoForDialog.TryAdd(curResource, cellConfig.AfkProduceCountPerSecond * resourceMultiplayer)) ;
                    infoForDialog[curResource] += cellConfig.AfkProduceCountPerSecond * resourceMultiplayer;
                }
            }
        }

        return infoForDialog;
    }

    private void SetFigureFormsInfoFromData() {
        _formGroupCellIndex = new int[MainMetaConfig.FieldSize, MainMetaConfig.FieldSize];
        if (StorageManager.GameDataMain.FigureFormsData.Count == 0) return;
        int currentIndex = 1;
        foreach (var formCells in StorageManager.GameDataMain.FigureFormsData) {
            List<Vector2Int> cells = new List<Vector2Int>();
            foreach (var cell in formCells.FormCoordinates) {
                _formGroupCellIndex[cell.x, cell.y] = currentIndex;
                cells.Add(cell);
            }

            _formGroupCellPositions.Add(currentIndex, cells);
            currentIndex++;
        }
    }

    private void SetFigureFormsInfoToData() {
        List<FormPositionsData> forms = _formGroupCellPositions.Select(cells => cells.Value.ToArray())
            .Select(cellArray => new FormPositionsData(cellArray)).ToList();
        StorageManager.GameDataMain.FigureFormsData = forms;
    }

    public void CollectResourcesFromMark(int index, float multiplayerResources) {
        float collectedResouces = 0;
        Vector3 startPosition = new Vector3();
        ResourceType curResource = ResourceType.None;
        foreach (var (row, col) in _connectedGroups[index].Pieces) {
            var cellConfig = PiecesViewTable.Instance.CellsList.MetaCellsConfigs.First(c => c.CellType == _field[row, col]);
            startPosition += _cells[row, col].transform.position;
            if (curResource == ResourceType.None)
                curResource = cellConfig.AfkResourceType;
            if (cellConfig.AfkResourceType != ResourceType.None) {
                collectedResouces += StorageManager.GameDataMain.FieldRows[row].RowCells[col].ResourceCount;
                StorageManager.GameDataMain.FieldRows[row].RowCells[col].ResourceCount -=
                    Mathf.FloorToInt(StorageManager.GameDataMain.FieldRows[row].RowCells[col].ResourceCount);
            }
        }

        startPosition /= _connectedGroups[index].Pieces.Count;
        startPosition = _mainCamera.WorldToScreenPoint(startPosition);
        StorageManager.GameDataMain.LastExitTime = MainManager.Instance._currentGameTime.ToString(CultureInfo.InvariantCulture);
        var finalResourceCount = collectedResouces * multiplayerResources;
        if (curResource != ResourceType.Coins) {
         
            FloatingResourcesManager.Instance.FromPointToPointAnimation((int)(collectedResouces * multiplayerResources), curResource, startPosition,
                MetaUI.Instance._openResourceTabButtonTransform.position,
            ChangeResorceText, StorageManager.GameDataMain.GetResource(curResource), false,true); 
           // StorageManager.GameDataMain.AddResource(curResource, finalResourceCount);
            UpdateResourcesCountUIText();
        } else {
           
          //  MetaUI.Instance.CountersPanelView.SetGold(StorageManager.GameDataMain.GetResource(ResourceType.Coins));
          FloatingResourcesManager.Instance.FromPointToPointAnimation((int)(collectedResouces * multiplayerResources), ResourceType.Coins, startPosition,
                MetaUI.Instance.CountersPanelView.GetCoinsIconPosition,
            ChangeResorceText, StorageManager.GameDataMain.GetResource(curResource), false, true); 
         //   StorageManager.GameDataMain.AddResource(ResourceType.Coins, finalResourceCount);
        }
    }

    public override void SaveEnergyData() {
        Debug.Log("save");
        StorageManager.GameDataMain.LastExitTime = MainManager.Instance._currentGameTime.ToString(CultureInfo.InvariantCulture);
        SetFigureFormsInfoToData();
        base.SaveEnergyData();
    }

    public void ShowCollectAllDialog() {
        DialogsManager.Instance.ShowDialog(typeof(CollectAllDialog));
    }

    public void CollectResourcesFromAllMarks(float multiplayer) {
        foreach (var resourceMarkGroup in _connectedGroups) {
            if (resourceMarkGroup.ResourceMarkView != null)
                resourceMarkGroup.ResourceMarkView.CollectAnimation();
            CollectResourcesFromMark(resourceMarkGroup.ResourceMarkView.markIndex, multiplayer);
        }
    }

    public void CollectDoubleResourcesFromAllMarks() {
        CollectResourcesFromAllMarks(2);
    }

    public void CollectDefaultResourcesFromAllMarks() {
        CollectResourcesFromAllMarks(1);
    }

    private void CalculateCellSpawnChances() {
        float lastChance = 0;
        CellsChanceToSpawn = new float[_currentCellsToSpawn.Count];
        for (int i = 0; i < _currentCellsToSpawn.Count; i++) {
            lastChance += PiecesViewTable.Instance.CellsList.MetaCellsConfigs.First(c => c.CellType == _currentCellsToSpawn[i]).ChanceToSpawn;
            CellsChanceToSpawn[i] = lastChance;
        }
    }

    public override void PlacePiece(PieceData pieceData, Vector2Int coord, CellView[,] cells, Transform cellsContainer) {
        base.PlacePiece(pieceData, coord, cells, cellsContainer);
        List<(int, int)> placedCells = GetPlacedCells(pieceData, coord);

        ResourceType cellResource = _cellsResources.GetValueOrDefault(pieceData.Type.CellType, ResourceType.None);
        if (cellResource != ResourceType.None) {
            StorageManager.GameDataMain.SeenResource[cellResource] = true;
        }

        UpdateResourceMarksAfterPlacePiece(placedCells);
        AddFigureFormToList(placedCells);
        _currentPiecesInInventory.Remove(_currentDraggedPieceButton);
        Destroy(_currentDraggedPieceButton.gameObject);
        Destroy(_currentDraggedPiece.gameObject);
        SetCurrentPiece();
        SaveInventory();
        
        SetFigureFormsInfoToData();
    }

    private void AddFigureFormToList(List<(int, int)> placedCells) {
        int currentIndex = 1;
        if (_formGroupCellPositions.Count != 0) {
            foreach (var index in _formGroupCellPositions.Keys)
                if (currentIndex <= index)
                    currentIndex = index + 1;
        }

        List<Vector2Int> cells = new List<Vector2Int>();
        foreach (var placedCell in placedCells) {
            cells.Add(new Vector2Int(placedCell.Item1, placedCell.Item2));
            _formGroupCellIndex[placedCell.Item1, placedCell.Item2] = currentIndex;
        }

        _formGroupCellPositions.Add(currentIndex, cells);
    }

    private void DeleteFigureFormFromList(int destroyedForm) {
        foreach (var cellPos in _formGroupCellPositions[destroyedForm])
            _formGroupCellIndex[cellPos.x, cellPos.y] = 0;

        _formGroupCellPositions.Remove(destroyedForm);
    }

    private static List<(int, int)> GetPlacedCells(PieceData pieceData, Vector2Int pos) {
        List<(int, int)> placedCells = new();
        for (int x = 0; x < pieceData.Cells.GetLength(0); x++) {
            for (int y = 0; y < pieceData.Cells.GetLength(1); y++) {
                if (!pieceData.Cells[x, y]) {
                    continue;
                }

                Vector2Int place = new(pos.x + x, pos.y + y);
                placedCells.Add((place.x, place.y));
            }
        }

        return placedCells;
    }

    private void UpdateResourceMarksAfterPlacePiece(List<(int, int)> placedCells) {
        List<int> connectedCellGroups = new List<int>();
        Color needColor = Color.clear;
        foreach (var (row, col) in placedCells) {
            if (needColor == Color.clear) {
                needColor = PiecesViewTable.Instance.CellsList.MetaCellsConfigs.First(c => c.CellType == _field[row, col]).MarkCellColor;
                needColor.a = 1;
            }

            var cellResource = PiecesViewTable.Instance.CellsList.MetaCellsConfigs.First(c => c.CellType == _field[row, col]).AfkResourceType;

            foreach (var pos in FieldUtils.Directions) {
                var newRow = row + pos.y;
                var newCol = col + pos.x;
                if (newRow >= _field.GetLength(0) || newCol >= _field.GetLength(1) || newRow < 0 || newCol < 0 ||
                    _field[newRow, newCol] == CellType.Empty || _field[newRow, newCol] == CellType.LockedMetaCell || PiecesViewTable.Instance
                        .CellsList.MetaCellsConfigs.First(c => c.CellType == _field[newRow, newCol]).AfkResourceType != cellResource) continue;

                if (_groupCellIndex[newRow, newCol] != 0) {
                    //fix bug if piece has holes 
                    if (!connectedCellGroups.Contains(_groupCellIndex[newRow, newCol])) {
                        {
                            connectedCellGroups.Add(_groupCellIndex[newRow, newCol]);
                        }
                    }
                }
            }
        }

        foreach (var curIndex in connectedCellGroups) {
            CollectResourcesFromMark(curIndex - 1, 1);
            _connectedGroups[curIndex - 1].ResourceMarkView.CollectAnimation();
        }

        List<(int, int)> cellsInNewGroup = new ();
        Vector3 newResourceMarkPosition = new ();
        int curGroupIndex = 0;
        if (connectedCellGroups.Count == 0)
            curGroupIndex = _connectedGroups.Count + 1;
        else {
            curGroupIndex = connectedCellGroups[0];
            foreach (var pieces in _connectedGroups[curGroupIndex - 1].Pieces) {
                cellsInNewGroup.Add((pieces.row, pieces.col));
                newResourceMarkPosition += _cells[pieces.row, pieces.col].transform.position;
            }

            if (connectedCellGroups.Count > 1) {
                for (int i = 1; i < connectedCellGroups.Count; i++) {
                    var connectedGroup = _connectedGroups[connectedCellGroups[i] - 1];
                    MetaWorldCanvasView.Instance.ReleaseResourceMark(connectedGroup.ResourceMarkView);
                    foreach (var pieces in connectedGroup.Pieces) {
                        _groupCellIndex[pieces.row, pieces.col] = curGroupIndex;
                        cellsInNewGroup.Add((pieces.row, pieces.col));
                        newResourceMarkPosition += _cells[pieces.row, pieces.col].transform.position;
                    }

                    _connectedGroups[connectedCellGroups[i] - 1] = new ResourceMarkAndPieces(null, new List<(int, int)>());
                }
            }
        }

        foreach (var (row, col) in placedCells) {
            _groupCellIndex[row, col] = curGroupIndex;
            newResourceMarkPosition += _cells[row, col].transform.position;
            cellsInNewGroup.Add((row, col));
        }

        newResourceMarkPosition /= cellsInNewGroup.Count;
        newResourceMarkPosition += new Vector3(0, 1, 0);
        if (connectedCellGroups.Count == 0) {
            var resourceMarkView = SpawnResourceMark(newResourceMarkPosition, 0, 0, ResourceType.None, needColor);
            resourceMarkView.gameObject.SetActive(false);
            _connectedGroups.Add(new ResourceMarkAndPieces(resourceMarkView, cellsInNewGroup));
        } else {
            _connectedGroups[curGroupIndex - 1].ResourceMarkView.gameObject.transform.position = newResourceMarkPosition;
            var resourceMarkView = _connectedGroups[curGroupIndex - 1].ResourceMarkView;
            //  resourceMarkView.gameObject.SetActive(false);
            _connectedGroups[curGroupIndex - 1] = new ResourceMarkAndPieces(resourceMarkView, cellsInNewGroup);
        }

        foreach (var (row, col) in cellsInNewGroup) {
            StorageManager.GameDataMain.FieldRows[row].RowCells[col] = new ResourceAndCountData(_field[row, col], 0);
        }
    }

    private void UpdateResourceMarks() {
        for (int i = 0; i < _connectedGroups.Count; i++) {
            if (_connectedGroups[i].ResourceMarkView == null) continue;
            float collectedResouces = 0;
            int maxCollectedResouces = 0;
            ResourceType curResource = ResourceType.None;
            foreach (var (row, col) in _connectedGroups[i].Pieces) {
                if (_field[row, col] == CellType.Empty) continue;
                var cellConfig = PiecesViewTable.Instance.CellsList.MetaCellsConfigs.First(c => c.CellType == _field[row, col]);
                if (curResource == ResourceType.None)
                    curResource = cellConfig.AfkResourceType;
                if (cellConfig.AfkResourceType != ResourceType.None) {
                    float resourceMultiplayer = MainMetaConfig.ResourceMultipliers[_connectedGroups[i].Pieces.Count];

                    maxCollectedResouces += (int)(cellConfig.MaxAfkCapacity * resourceMultiplayer);
                    var currentCellCollectedResources = StorageManager.GameDataMain.FieldRows[row].RowCells[col].ResourceCount +
                                                        (cellConfig.AfkProduceCountPerSecond * resourceMultiplayer *
                                                         MainMetaConfig.resourceMarksUpdateCouldown);
                    currentCellCollectedResources = Mathf.Min(currentCellCollectedResources,
                        (int)(cellConfig.MaxAfkCapacity * resourceMultiplayer));
                    collectedResouces += currentCellCollectedResources;
                    StorageManager.GameDataMain.FieldRows[row].RowCells[col].ResourceCount = currentCellCollectedResources;
                }
            }

            if (curResource != ResourceType.None)
                _connectedGroups[i].ResourceMarkView.SetResourceMarkInfo(maxCollectedResouces, collectedResouces, curResource, i);
        }
    }

    private void GetResourceCollectMarks() {
        List<List<(int row, int col)>> connectedGroupsPieces = null;
        (_groupCellIndex, connectedGroupsPieces) = SameCellsGroupCalculater.FindConnectedCellTypeGroups(_field);
        int afkTimeInSeconds = (int)(MainManager.Instance._currentGameTime - StorageManager.GameDataMain.LastExitTimeDateTime).TotalSeconds;
//fix afk calculate
        for (int i = 0; i < connectedGroupsPieces.Count; i++) {
            Vector3 collectResourceMarkPosition = Vector3.zero;
            float collectedResouces = 0;
            int maxCollectedResouces = 0;
            ResourceType curResource = ResourceType.None;
            Color resourceColor = Color.clear;
            foreach (var (row, col) in connectedGroupsPieces[i]) {
                var cellConfig = PiecesViewTable.Instance.CellsList.MetaCellsConfigs.First(c => c.CellType == _field[row, col]);

                if (curResource == ResourceType.None) {
                    curResource = cellConfig.AfkResourceType;
                    resourceColor = cellConfig.MarkCellColor;
                    resourceColor.a = 1;
                }
                if (cellConfig.AfkResourceType != ResourceType.None) {
                    float resourceMultiplayer = MainMetaConfig.ResourceMultipliers[connectedGroupsPieces[i].Count];
                    if (StorageManager.GameDataMain.FieldRows[row].RowCells[col].ResourceCount < 0)
                        StorageManager.GameDataMain.FieldRows[row].RowCells[col].ResourceCount = 0;
                    maxCollectedResouces += (int)(cellConfig.MaxAfkCapacity * resourceMultiplayer);
                    float afkCollectedResources = StorageManager.GameDataMain.FieldRows[row].RowCells[col].ResourceCount +
                                                  afkTimeInSeconds * cellConfig.AfkProduceCountPerSecond * resourceMultiplayer;
                    afkCollectedResources = Mathf.Min(afkCollectedResources, (int)(cellConfig.MaxAfkCapacity * resourceMultiplayer));
                    StorageManager.GameDataMain.FieldRows[row].RowCells[col].ResourceCount = afkCollectedResources;
                    collectedResouces += afkCollectedResources;
                    collectResourceMarkPosition += _cells[row, col].transform.position;
                }
            }

            collectedResouces = Mathf.Min(collectedResouces, maxCollectedResouces);
            collectResourceMarkPosition /= connectedGroupsPieces[i].Count;
            var resourceMark = SpawnResourceMark(collectResourceMarkPosition, maxCollectedResouces, collectedResouces, curResource,
                resourceColor);
         
            resourceMark.gameObject.SetActive(collectedResouces / maxCollectedResouces > 0.1f);
            _connectedGroups.Add(new ResourceMarkAndPieces(resourceMark, connectedGroupsPieces[i]));  
            Debug.Log($"{collectedResouces} / {maxCollectedResouces} type {curResource} index {_connectedGroups.Count}");
        }

        LockedCellGroups = new Dictionary<int, List<Vector2Int>>();
        foreach (var needCell in MainMetaConfig.LockedCellsFieldConfig.LockedCellsGroups) {
            LockedCellGroups.TryAdd(needCell.index, new List<Vector2Int>());
            LockedCellGroups[needCell.index].Add(needCell.position);
        }

        foreach (var zoneIndex in StorageManager.GameDataMain.RemainedLockedZones) {
            var lockedCells = LockedCellGroups[zoneIndex];
            foreach (var cellPos in lockedCells) {
                _groupCellIndex[cellPos.x, cellPos.y] = zoneIndex + 1000;
            }
        }
    }

    public void SpawnPieceFromInventory(PieceView piece, InventoryCellView inventoryCell) {
        SetCurrentPiece(piece, inventoryCell);
        piece.transform.position = _inputRaycaster.InputPos();
        piece.OnStartDrag();
    }

    /*  private void OnApplicationPause(bool pauseStatus) {
          if (pauseStatus) {
              SaveEnergyData();
          } else {
              StorageManager.LoadGame();
              CalculateOfflineHealth();
          }
      }

      private TimeSpan GetTimeUntilNextHealth() {
          if (StorageManager.GameDataMain.HealthCount >= MAX_HEALTH_COUNT) return TimeSpan.Zero;

          TimeSpan timeSinceLastUpdate = _currentGameTime - _lastHealthRecoveryTime;
          double minutesPassed = timeSinceLastUpdate.TotalMinutes;
          double minutesUntilNext = MainMetaConfig.MinutesToHealthRecovery - (minutesPassed % MainMetaConfig.MinutesToHealthRecovery);

          return TimeSpan.FromMinutes(minutesUntilNext);
      }*/
    public void GetInventoryFromSave() {
        if (StorageManager.GameDataMain.InventoryFigures == null) return;
        var inventoryFigures = StorageManager.GameDataMain.InventoryFigures;
        foreach (var figure in inventoryFigures) {
            bool[,] cells = TetrisPieces.PieceShapesTable[figure.FormName];

            Guid[,] cellGuids = new Guid[cells.GetLength(0), cells.GetLength(1)];
            for (int x = 0; x < cells.GetLength(0); x++) {
                for (int y = 0; y < cells.GetLength(1); y++) {
                    if (cells[x, y]) {
                        cellGuids[x, y] = Guid.NewGuid();
                    } else {
                        cellGuids[x, y] = Guid.Empty;
                    }
                }
            }

            var cellInfo = PiecesViewTable.Instance.CellsList.MetaCellsConfigs.First(c => c.CellType == figure.FormCellType);
            var data = new PieceData() { Type = cellInfo, Cells = cells, CellGuids = cellGuids, FormName = figure.FormName };

            AddPieceToInventory(data);

            /* var inventoryCellView = Instantiate(_inventoryCellPrefab, _inventoryCellsContainer);
             inventoryCellView.SetPieceInfo(data);
             _currentPiecesInInventory.Add(inventoryCellView);*/
        }
    }

    public void SaveInventory() {
        StorageManager.GameDataMain.InventoryFigures = new List<FormAndCellTypeData>();

        for (int i = 0; i < _currentPiecesInInventory.Count; i++) {
            var pieceData = _currentPiecesInInventory[i].Data;
            StorageManager.GameDataMain.InventoryFigures.Add(new FormAndCellTypeData(pieceData.FormName, pieceData.Type.CellType));
        }
        StorageManager.SaveGame();
        //
    }
}

public struct ResourceMarkAndPieces {
    public ResourceMarkView ResourceMarkView;
    public List<(int row, int col)> Pieces;

    public ResourceMarkAndPieces(ResourceMarkView resourceMarkView, List<(int row, int col)> pieces) {
        ResourceMarkView = resourceMarkView;
        Pieces = pieces;
    }
}