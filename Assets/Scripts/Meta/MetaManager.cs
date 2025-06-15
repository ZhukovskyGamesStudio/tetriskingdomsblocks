using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MetaManager : BaseManager {
    //protected HeroType[,] _heroesOnField;
    public static MetaManager Instance { get; private set; }
    [SerializeField] private Transform _cellContainer;
    [field:SerializeField]
    public MainMetaConfig MainMetaConfig { get;private set; }
    private PieceData _nextBlock = new PieceData();
    [SerializeField]private TMP_Text[] _resourcesCountText;
    [SerializeField]private TMP_Text _getPieceTimerText;
    private int _minutesToGetPiece = 120;
    
    protected override void Awake() {
        base.Awake();    
        Instance = this;
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
        if(StorageManager.GameDataMain.HealthCount != 0)
        SceneManager.LoadScene("GameScene");
        else
        {
            
            //floating window with "watch ad and get health"
        }
    }

    public void BuyPiece() {
       if (StorageManager.GameDataMain.resourcesCount[0] >= 100 &&
           StorageManager.GameDataMain.resourcesCount[1] >= 100 && StorageManager.GameDataMain.resourcesCount[2] >= 100)
       {
       // DialogsManager.Instance.ShowDialog(typeof(BuyPieceDialog));
       StorageManager.GameDataMain.resourcesCount[0] -= 100;
       StorageManager.GameDataMain.resourcesCount[1] -= 100;
       StorageManager.GameDataMain.resourcesCount[2] -= 100;
       UpdateResourcesCountUIText();
       GenerateNewPieces(); // for test
       
       }
    }

    private void UpdateResourcesCountUIText()
    {
        for (int i = 0; i < _resourcesCountText.Length; i++)
            _resourcesCountText[i].text = StorageManager.GameDataMain.resourcesCount[i].ToString();
    }
    public void GetPiece() {
        if (_hasInternetConnection &&
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
                    _field[i,j] = StorageManager.GameDataMain.FieldRows[i].RowCells[j];
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

        GetResourceCollectMarks();

        UpdateResourcesCountUIText();
        
        if (StorageManager.GameDataMain.LastGetPieceTime.Years == 0)
            StorageManager.GameDataMain.LastGetPieceTime = DateForSaveData.FromDateTime(_currentGameTime - TimeSpan.FromHours(2)); 
        base.SetupGame();
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
            StorageManager.GameDataMain.FieldRows[i].RowCells = new CellType[_field.GetLength(1)];
            for (int j = 0; j < _field.GetLength(1); j++)
                StorageManager.GameDataMain.FieldRows[i].RowCells[j] = _field[i, j];
        }

        StorageManager.SaveGame();
    }

    private void GetResourceCollectMarks()
    {
        var connectedGroups = SameCellsGroupCalculater.FindConnectedCellTypeGroups(_field);


        for (int i = 0; i < connectedGroups.Count; i++)
        {
          //  Debug.Log($"Группа {i + 1} (размер {connectedGroups[i].Count}):");
            Vector3 collectResourceMarkPosition = Vector3.zero;
            
            foreach (var (row, col) in connectedGroups[i])
            {
                var resourceType =MetaManager.Instance.MainMetaConfig.CellsConfigs.First
                    (c => c.CellType == _field[row, col]).ResourcesForDestroy;//make afk collect info in config
                if(resourceType.Length != 0)
                Debug.Log($"  [{row}, {col}] = {_field[row, col]} + {resourceType[0].ResourceType} resource type]");
                collectResourceMarkPosition += _cells[row, col].transform.position;
            }
            collectResourceMarkPosition /= connectedGroups[i].Count;
        Debug.Log($"Найдено {connectedGroups.Count} групп связанных клеток + avg pos {collectResourceMarkPosition}" );
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