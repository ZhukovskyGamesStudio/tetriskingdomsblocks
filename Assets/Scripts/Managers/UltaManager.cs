using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

public class UltaManager : MonoBehaviour
{
    public static UltaManager Instance;
    [SerializeField] private Slider _ultimateProgressBar;
    [SerializeField] private Button _ultimateButton;
    [SerializeField] private Transform _starPrefab;
    private int _currentPoints;
    public bool _ultimateIsActive { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _ultimateProgressBar.maxValue = GameFieldManager.Instance.MainGameConfig.NeededUltimatePoints;
        _ultimateButton.onClick.AddListener(() => UltimateAction());
    }

    public void AddUltimatePoints(int points)
    {
        if(_ultimateIsActive )return;   
        _currentPoints += points;
        //_ultimateProgressBar.value += points;
        
        _ultimateProgressBar.DOValue(_currentPoints, 0.4f).SetEase(Ease.Linear).OnComplete(() =>
        {
        if (_currentPoints >= _ultimateProgressBar.maxValue)
            ActivateButton();
        });
        
    }

    private void ActivateButton()
    {
        _ultimateButton.enabled = true;
        _ultimateButton.gameObject.SetActive(true);
        _ultimateProgressBar.gameObject.SetActive(false);
        //make animations(maybe scale from 0 to 1)
    }
    
    private void HideButton()
    {
        _ultimateButton.gameObject.SetActive(false);
        _ultimateProgressBar.gameObject.SetActive(true);
        //make animations(maybe scale from 0 to 1)
    }

    private async void UltimateAction()
    {
        _ultimateButton.enabled = false;
        _ultimateProgressBar.value = 0;
        _currentPoints = 0;
        _ultimateIsActive = true;
        HideButton();
        
        
        var coordsToSpawn = FieldUtils.GetRandomEmptyCells(GameFieldManager.Instance._field,
            GameFieldManager.Instance.MainGameConfig.MaxUltimateCells);
        foreach (var pos in coordsToSpawn) {
            SpawnNewCellFromUltimate(pos).Forget();
            await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
        }

        await UniTask.Delay(TimeSpan.FromSeconds(1.5f));
        if (!GameFieldManager.Instance.CheckWin() && GameFieldManager.Instance.CheckLose()) {
            GameFieldManager.Instance.Lose();
        }
      
        _ultimateIsActive = false;
    }

    private async UniTask SpawnNewCellFromUltimate(Vector2Int placedCellPosition)
    {
        var pieceData = GetRandomCellType();

        var config =
            PiecesViewTable.Instance.CellsList.CellsConfigs.First(c => c.CellType == pieceData.Type.CellType);
        var cellView = GameFieldManager.Instance.PlaceOneSizePiece(config,
            new Vector2Int(placedCellPosition.x, placedCellPosition.y), false);

        cellView.transform.position = new Vector3(cellView.transform.position.x, 30, cellView.transform.position.z);
        cellView.transform.localScale = Vector3.zero;
        cellView.gameObject.SetActive(false);
        var star = Instantiate(_starPrefab);
        star.position = cellView.transform.position;
        await DOTween.Sequence().Append(star.gameObject.transform.DOMoveY(0.75f, 0.5f).SetEase(Ease.OutQuad)).AsyncWaitForCompletion();
        cellView.transform.position = star.position;
        cellView.gameObject.SetActive(true);
        Destroy(star.gameObject);

        cellView.gameObject.transform.DOScale(Vector3.one, 0.5f);
        GameFieldManager.Instance.CheckCellTypesBeforePlacePiece(placedCellPosition);
        //  GameFieldManager.Instance.ShowDropImpact(cellView.transform, pieceData, cellView.gameObject, 1);
        GameFieldManager.Instance.SetNeededCellTypeOnField(pieceData.Type.CellType, cellView, placedCellPosition);
        GameFieldManager.Instance.CheckClosestCells(placedCellPosition);
        GameFieldManager.Instance.CollectResourcesOnPlace(pieceData);
        GameFieldManager.Instance.ExplodeCellsInRows();
                
        
    }

    private PieceData GetRandomCellType()
    {
        var cellsToSpawn = GameFieldManager.Instance._currentCellsToSpawn;
        var chancesToSpawn =  GameFieldManager.Instance.CellsChanceToSpawn;
        CellTypeInfo cellInfo = null;

        float chance = Random.Range(0, chancesToSpawn[chancesToSpawn.Length - 1]);
        for (int j = 0; j < chancesToSpawn.Length; j++)
        {
            if (chancesToSpawn[j] > chance)
            {
                cellInfo = PiecesViewTable.Instance.CellsList.CellsConfigs.First(c =>
                    c.CellType == cellsToSpawn[j]);
                break;
            }
        }

        bool[,] cells = TetrisPieces.PieceShapesTable["oneBlock"];
        Guid[,] cellGuids = new Guid[cells.GetLength(0), cells.GetLength(1)];
        for (int x = 0; x < cells.GetLength(0); x++)
        {
            for (int y = 0; y < cells.GetLength(1); y++)
            {
               cellGuids[x, y] = Guid.NewGuid();
            }
        }

        var data = new PieceData()
        {
            Type = cellInfo,
            Cells = cells,
            CellGuids = cellGuids
        };
        return data;
    }
}