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
        
        _ultimateProgressBar.DOValue(_currentPoints, 1f).SetEase(Ease.Linear).OnComplete((() =>
        {
        if (_currentPoints >= _ultimateProgressBar.maxValue)
            ActivateButton();
        }));
        
        
        
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
        _ultimateIsActive = false;
        //make animations(maybe scale from 0 to 1)
    }

    private async void UltimateAction()
    {
        _ultimateButton.enabled = false;
        _ultimateProgressBar.value = 0;
        _currentPoints = 0;
        _ultimateIsActive = true;

        for (int i = 0; i < GameFieldManager.Instance.MainGameConfig.MaxUltimateCells; i++)
        {
            var placedCellPosition = FieldUtils.GetRandomEmptyCell(GameFieldManager.Instance._field);
            CellType oldCellType = GameFieldManager.Instance._field[placedCellPosition.x, placedCellPosition.y];
            var pieceData = GetRandomCellType();
            
            var config = PiecesViewTable.Instance.CellsList.CellsConfigs.First(c => c.CellType == pieceData.Type.CellType);
           var cellView = GameFieldManager.Instance.PlaceOneSizePiece(config, new Vector2Int(placedCellPosition.x, placedCellPosition.y));
           
           GameFieldManager.Instance.SetNeededCellTypeOnField(oldCellType,placedCellPosition);
           
           cellView.transform.position = new Vector3(cellView.transform.position.x, 30, cellView.transform.position.z);

           DOTween.Sequence().Append(cellView.gameObject.transform.DOMoveY(0.75f, 0.5f)
                   .SetEase(Ease.OutQuad)).OnComplete(()=>
               {
                   GameFieldManager.Instance.CheckCellTypesBeforePlacePiece(placedCellPosition);
                   GameFieldManager.Instance.SetNeededCellTypeOnField(pieceData.Type.CellType,placedCellPosition);
                   GameFieldManager.Instance.CheckClosestCells(placedCellPosition);
                 //  GameFieldManager.Instance.ShowDropImpact(cellView.transform, pieceData, cellView.gameObject, 1);
                   GameFieldManager.Instance.CollectResourcesOnPlace(pieceData); 
                   GameFieldManager.Instance.ExplodeCellsInRows();
               }); 
           await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
        }
        await UniTask.Delay(TimeSpan.FromSeconds(1.5f));
        if(!GameFieldManager.Instance.CheckWin() && GameFieldManager.Instance.CheckLose())
            GameFieldManager.Instance.Lose(); 
        HideButton();
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