using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

public class MetaManager : BaseManager {
    //protected HeroType[,] _heroesOnField;
    public static MetaManager Instance { get; private set; }
    private List<ResourceMarkAndPieces> connectedGroups = new List<ResourceMarkAndPieces>();
    [SerializeField] private Transform _cellContainer;
    [field:SerializeField]
    public MainMetaConfig MainMetaConfig { get;private set; }
    private PieceData _nextBlock = null;
    [SerializeField]private TMP_Text[] _resourcesCountText;
    [SerializeField]private TMP_Text _getPieceTimerText;
    [SerializeField]private TMP_Text _destroyPieceText;
    private int _minutesToGetPiece = 120;
    private bool _isDestroyPieceMode;
    [SerializeField] private Transform _resourcesMarksContainer;
    [SerializeField] private LayerMask _pieceMask;
    [SerializeField] private ResourceMarkView resourceMarkViewPrefab;
    [HideInInspector]
    public Vector3 ScreenToWorldPointOnPiece => Physics.Raycast(_mainCamera.ScreenPointToRay(Input.mousePosition),
        out RaycastHit hit, Mathf.Infinity, _pieceMask)
        ? hit.point
        : Vector3.zero;

    [HideInInspector]
    public Vector3 TouchToWorldPointOnPiece => Physics.Raycast(_mainCamera.ScreenPointToRay(Input.GetTouch(0).position),
        out RaycastHit hit, Mathf.Infinity, _pieceMask)
        ? hit.point
        : Vector3.zero;
    
    private ObjectPool<ResourceMarkView> _resourcesMarksPool;
    
    protected override void Awake() {
        base.Awake();    
        Instance = this;
        _resourcesMarksPool =
            new ObjectPool<ResourceMarkView>(() => Instantiate(resourceMarkViewPrefab,_resourcesMarksContainer));
    }

    protected override void Start()
    {
       base.Start();
    }

    protected override void Update()
    {
        base.Update();
        if (_hasInternetConnection &&
            (_currentGameTime - StorageManager.GameDataMain.LastGetPieceTime.ToDateTime()).TotalHours < 2)
        {
            TimeSpan timeUntilNext = GetTimeUntilNextPiece();
            _getPieceTimerText.text = $"{timeUntilNext.Hours:D1}:{timeUntilNext.Minutes:D2}:{timeUntilNext.Seconds:D2} to new piece";
        }

        if (Input.GetMouseButtonDown(0) && _isDestroyPieceMode)
        {
            Physics.Raycast(_mainCamera.ScreenPointToRay(Input.mousePosition),
                out RaycastHit hit, Mathf.Infinity, _pieceMask);
            if (hit.collider != null)
            {
                Vector3 cellPos = hit.collider.transform.localPosition;
                _cells[(int)cellPos.x, (int)cellPos.z].DestroyCell();
                _cells[(int)cellPos.x, (int)cellPos.z] = null;
                _field[(int)cellPos.x, (int)cellPos.z] = CellType.Empty;
                //reset resource marks
            }
        }
    }

    private ResourceMarkView SpawnResourceMark(Vector3 pos,int maxResource, int currentResource, ResourceType resourceType)
    {
        var mark = _resourcesMarksPool.Get();
        mark.gameObject.SetActive(true);
        pos = _mainCamera.WorldToScreenPoint(pos);
        mark.transform.position = new Vector2(pos.x,pos.y); 
        mark.SetResourceMarkInfo(maxResource,currentResource,resourceType,connectedGroups.Count);
        return mark;
    }

    private void ReleaseResourceMark(ResourceMarkView mark)
    {
        mark.gameObject.SetActive(false);
        _resourcesMarksPool.Release(mark);
    }
    public TimeSpan GetTimeUntilNextPiece()
    {
       // if (StorageManager.GameDataMain.HealthCount >= MAX_HEALTH_COUNT) return TimeSpan.Zero;
        
        TimeSpan timeSinceLastUpdate = _currentGameTime - StorageManager.GameDataMain.LastGetPieceTime.ToDateTime();
        double minutesPassed = timeSinceLastUpdate.TotalMinutes;
        double minutesUntilNext = _minutesToGetPiece - (minutesPassed % _minutesToGetPiece);
        
        return TimeSpan.FromMinutes(minutesUntilNext);
    }
    public void Play() {
        if (StorageManager.GameDataMain.HealthCount != 0)
        {
            StorageManager.GameDataMain.LastExitTime = DateForSaveData.FromDateTime(_currentGameTime);
                StorageManager.SaveGame();
        SceneManager.LoadScene("GameScene");
        }
        else
        {
            
            //floating window with "watch ad and get health"
        }
    }

    public void BuyPiece() {
       if (StorageManager.GameDataMain.resourcesCount[0] >= 100 &&
           StorageManager.GameDataMain.resourcesCount[1] >= 100 && StorageManager.GameDataMain.resourcesCount[2] >= 100
           && _nextBlock == null)
       {
       // DialogsManager.Instance.ShowDialog(typeof(BuyPieceDialog));
       StorageManager.GameDataMain.resourcesCount[0] -= 100;
       StorageManager.GameDataMain.resourcesCount[1] -= 100;
       StorageManager.GameDataMain.resourcesCount[2] -= 100;
       UpdateResourcesCountUIText();
       GenerateNewPieces(); // for test
       
       }
    }

    public void ToggleDestroyPieceMode()
    {
        _isDestroyPieceMode = !_isDestroyPieceMode;
        if (_isDestroyPieceMode)
        {
            _destroyPieceText.text = "Cancel";
        }
        else
        {
            _destroyPieceText.text = "Destroy pieces mode";
        }
    }

    private void UpdateResourcesCountUIText()
    {
        for (int i = 0; i < _resourcesCountText.Length; i++)
            _resourcesCountText[i].text = StorageManager.GameDataMain.resourcesCount[i].ToString();
    }
    public void GetPiece() {
        Debug.Log((_nextBlock == null) + " " + _hasInternetConnection);
        if (_hasInternetConnection && _nextBlock == null&&
            (_currentGameTime - StorageManager.GameDataMain.LastGetPieceTime.ToDateTime()).TotalHours >= 2)
        {
        // DialogsManager.Instance.ShowDialog(typeof(BuyPieceDialog));
        StorageManager.GameDataMain.LastGetPieceTime = DateForSaveData.FromDateTime(_currentGameTime);
        GenerateNewPieces(); // for test
        }
    }

    public void CollectAll() {
        DialogsManager.Instance.ShowDialog(typeof(CollectAllDialog));
    }
    
    public void GenerateNewPieces()
    {
        _nextBlock = PieceUtils.GetNewPiece();
        SetData(_nextBlock);
    }
    public void SetData(PieceData nextPiece) {
        DestroyChildren();

        var go = Instantiate(PiecesViewTable.Instance.PieceViewPrefab, _cellContainer);
        go.SetData(nextPiece);
    }

    protected override void SetupGame()
    {
        //_placedPiecesAmount = 0;
        _field = new CellType[MainMetaConfig.FieldSize, MainMetaConfig.FieldSize];
        _cells = new CellView[MainMetaConfig.FieldSize, MainMetaConfig.FieldSize];
        CalculateFiguresSpawnChances();
        var startCells = MainMetaConfig.CellsConfigs;
        _currentCellsToSpawn = new List<CellTypeInfo>();
        for (int i = 0; i < startCells.Count; i++)
            _currentCellsToSpawn.Add(startCells[i]);
        CalculateCellSpawnChances();
        if (StorageManager.GameDataMain.FieldRows != null && StorageManager.GameDataMain.FieldRows.Length != 0)
        {
            _field = new CellType[StorageManager.GameDataMain.FieldRows.Length,StorageManager.GameDataMain.FieldRows[0].RowCells.Length];

            for (int i = 0; i < _field.GetLength(0); i++)
            {
                for (int j = 0; j < _field.GetLength(1); j++)
                {
                    _field[i,j] = StorageManager.GameDataMain.FieldRows[i].RowCells[j].CellType;
                    var cellType = _field[i, j];
                    if (cellType != CellType.Empty)
                    {
                        var config = Instance.MainMetaConfig.CellsConfigs.First(c => c.CellType == cellType);
                        var go = Instantiate(config.CellPrefab, _fieldContainer);
                        go.transform.localPosition = new Vector3(i, -0.45f, j);
                        _cells[i, j] = go;
                    }
                }
            }
        }
        UpdateResourcesCountUIText();
        
       if (StorageManager.GameDataMain.LastGetPieceTime.Years == 0)
       {
            StorageManager.GameDataMain.LastGetPieceTime = DateForSaveData.FromDateTime(_currentGameTime - TimeSpan.FromHours(2)); 
           StorageManager.GameDataMain.LastExitTime = DateForSaveData.FromDateTime(_currentGameTime);
       }
       
        GetResourceCollectMarks();
        
       InvokeRepeating("UpdateResourceMarks",MainMetaConfig.resourceMarksUpdateCouldown,MainMetaConfig.resourceMarksUpdateCouldown);
       
        base.SetupGame();
    }

    public void CollectResourcesFromMark(int index)
    {
        int collectedResouces = 0;
        ResourceType curResource = ResourceType.None;
        foreach (var (row, col) in connectedGroups[index].Pieces)
        {
            var cellConfig = MetaManager.Instance.MainMetaConfig.CellsConfigs.First
                (c => c.CellType == _field[row, col]);
            if (curResource == ResourceType.None)
                curResource = cellConfig.AfkResourceType;
            if (cellConfig.AfkResourceType != ResourceType.None)
            {
                collectedResouces += StorageManager.GameDataMain.FieldRows[row].RowCells[col].ResourceCount;
                StorageManager.GameDataMain.FieldRows[row].RowCells[col].ResourceCount =0;
            }
        }

        StorageManager.GameDataMain.resourcesCount[(int)curResource - 1] += collectedResouces;
        UpdateResourcesCountUIText();
        StorageManager.SaveGame();
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
    
    public override void PlacePiece(PieceData pieceData)
    {
        PlacePiece(pieceData, GetPieceClampedPosOnField(), MainMetaConfig.FieldSize);
        _nextBlock = null;

        StorageManager.GameDataMain.FieldRows = new MetaFieldData[_field.GetLength(0)];

        for (int i = 0; i < _field.GetLength(0); i++)
        {
            StorageManager.GameDataMain.FieldRows[i].RowCells = new ResourceAndCountData[_field.GetLength(1)];
            for (int j = 0; j < _field.GetLength(1); j++)
                StorageManager.GameDataMain.FieldRows[i].RowCells[j] = new ResourceAndCountData(_field[i, j],0);
        }

        StorageManager.SaveGame();
    }

    private void UpdateResourceMarks()
    {
        for (int i = 0; i < connectedGroups.Count; i++)
        {
            //  Debug.Log($"Группа {i + 1} (размер {connectedGroups[i].Count}):");
            // Vector3 collectResourceMarkPosition = Vector3.zero;
            int collectedResouces = 0;
            int maxCollectedResouces = 0;
            ResourceType curResource = ResourceType.None;
            foreach (var (row, col) in connectedGroups[i].Pieces)
            {
                var cellConfig = MetaManager.Instance.MainMetaConfig.CellsConfigs.First
                    (c => c.CellType == _field[row, col]);
                if (curResource == ResourceType.None)
                    curResource = cellConfig.AfkResourceType;
                if (cellConfig.AfkResourceType != ResourceType.None)
                {
                    float resourceMultiplayer = MainMetaConfig.ResourceMultipliers[connectedGroups[i].Pieces.Count];

                    maxCollectedResouces += (int)(cellConfig.MaxAfkCapacity * resourceMultiplayer);
                    var currentCellCollectedResources =
                        StorageManager.GameDataMain.FieldRows[row].RowCells[col].ResourceCount
                        + (int)(cellConfig.AfkProduceCountPerSecond * resourceMultiplayer *
                                MainMetaConfig.resourceMarksUpdateCouldown);
                    currentCellCollectedResources = Mathf.Min(currentCellCollectedResources,
                        (int)(cellConfig.MaxAfkCapacity * resourceMultiplayer));
                    collectedResouces += currentCellCollectedResources;
                    StorageManager.GameDataMain.FieldRows[row].RowCells[col].ResourceCount =
                        currentCellCollectedResources;
                }

            }
                if (curResource != ResourceType.None)
                    connectedGroups[i].ResourceMarkView
                        .SetResourceMarkInfo(maxCollectedResouces, collectedResouces, curResource, i);
        }
    }

    private void GetResourceCollectMarks()
    {
        var connectedGroupsPieces = SameCellsGroupCalculater.FindConnectedCellTypeGroups(_field);
        var afkTimeInSeconds = (_currentGameTime - StorageManager.GameDataMain.LastExitTime.ToDateTime()).TotalSeconds;
        for (int i = 0; i < connectedGroupsPieces.Count; i++)
        {
            //  Debug.Log($"Группа {i + 1} (размер {connectedGroups[i].Count}):");
            Vector3 collectResourceMarkPosition = Vector3.zero;
            int collectedResouces = 0;
            int maxCollectedResouces = 0;
            ResourceType curResource = ResourceType.None;
            foreach (var (row, col) in connectedGroupsPieces[i])
            {
                var cellConfig = MetaManager.Instance.MainMetaConfig.CellsConfigs.First
                    (c => c.CellType == _field[row, col]); //make afk collect info in config
                // Debug.Log($"  [{row}, {col}] = {_field[row, col]} + {cellConfig.AfkResourceType} resource type]");
                if (curResource == ResourceType.None)
                    curResource = cellConfig.AfkResourceType;
                if (cellConfig.AfkResourceType != ResourceType.None)
                {
                    float resourceMultiplayer = MainMetaConfig.ResourceMultipliers[connectedGroupsPieces[i].Count];

                    maxCollectedResouces += (int)(cellConfig.MaxAfkCapacity * resourceMultiplayer);
                    int afkCollectedResources = StorageManager.GameDataMain.FieldRows[row].RowCells[col].ResourceCount +
                                                (int)(afkTimeInSeconds * cellConfig.AfkProduceCountPerSecond *
                                                      resourceMultiplayer);
                    afkCollectedResources = Mathf.Min(afkCollectedResources,
                        (int)(cellConfig.MaxAfkCapacity * resourceMultiplayer));
                    StorageManager.GameDataMain.FieldRows[row].RowCells[col].ResourceCount = afkCollectedResources;
                    collectedResouces += afkCollectedResources;
                    collectResourceMarkPosition += _cells[row, col].transform.position;
                }
            }

                    collectedResouces = Mathf.Min(collectedResouces, maxCollectedResouces);
            collectResourceMarkPosition /= connectedGroupsPieces[i].Count;

            connectedGroups.Add(new ResourceMarkAndPieces(
                SpawnResourceMark(collectResourceMarkPosition, maxCollectedResouces, collectedResouces, curResource),
                connectedGroupsPieces[i]));
            Debug.Log(
                $"Найдено {connectedGroups.Count} групп связанных клеток + avg pos {collectResourceMarkPosition}");
        }
    }

    private void DestroyChildren()
    {
        if (_cellContainer.childCount > 0)
        {
            Destroy(_cellContainer.GetChild(0).gameObject);
        }
    }
}

public struct ResourceMarkAndPieces
{
    public ResourceMarkView ResourceMarkView;
    public List<(int row, int col)> Pieces;

    public ResourceMarkAndPieces(ResourceMarkView resourceMarkView, List<(int row, int col)> pieces)
    {
        ResourceMarkView = resourceMarkView;
        Pieces = pieces;
    }
}