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
    
    private void Awake() {
        ChangeToLoading.TryChange();
        Instance = this;
        SetupGame();
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
       // DialogsManager.Instance.ShowDialog(typeof(BuyPieceDialog));

       GenerateNewPieces(); // for test
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

    /*IEnumerator GetAFKTime(Action<DateTime> callback)
    {
        UnityWebRequest request = UnityWebRequest.Get("https://worldtimeapi.org/api/ip");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            ServerTimeData data = JsonUtility.FromJson<ServerTimeData>(json);
            callback(DateTime.Parse(data.datetime));
        }
        else
        {
            Debug.Log("Need internet connction to get rewards");
            callback(DateTime.UtcNow);
        }
    }*/

    private void DestroyChildren()
    {
        if (_cellContainer.childCount > 0)
        {
            Destroy(_cellContainer.GetChild(0).gameObject);
        }
    }
}