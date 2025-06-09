using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MetaManager : CellsManager {
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
        SceneManager.LoadScene("GameScene");
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

    private void SetupGame()
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
        if (StorageManager.GameDataMain.MetaField.Field != null)
        {
            _field = StorageManager.GameDataMain.MetaField.Field;

            for (int i = 0; i < _field.GetLength(0); i++)
            {
                for (int j = 0; j < _field.GetLength(1); j++)
                {
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
        if (StorageManager.GameDataMain.MetaField == null)
            StorageManager.GameDataMain.MetaField = new MetaFieldData();

        StorageManager.GameDataMain.MetaField.Field = _field;
    }
    private void DestroyChildren()
    {
        if (_cellContainer.childCount > 0)
        {
            Destroy(_cellContainer.GetChild(0).gameObject);
        }
    }
}