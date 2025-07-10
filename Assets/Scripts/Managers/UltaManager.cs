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
    [SerializeField] private FallingStarFx _starPrefab;

    [SerializeField]
    private ParticleSystem _starsParticles;
    
    
    private int _currentPoints;

    [SerializeField]
    private float _starDropDuration = 0.25f, _startSpawnXPos = 15;

    [SerializeField]
    private Vector3 _startDropStartPos;
    
    public bool _ultimateIsActive { get; private set; }

    [SerializeField]
    private AnimationCurve _animationCurveX,_animationCurveY,_animationCurveZ;

    [SerializeField]
    private bool _isRandomPos;
    
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
        if(GoalView.Instance._isGameEnded) {
            return;
        }
        
        
        _ultimateButton.enabled = false;
        _ultimateProgressBar.value = 0;
        _currentPoints = 0;
        _ultimateIsActive = true;
        HideButton();
        _starsParticles.gameObject.SetActive(true);
        _starsParticles.Play();
        
        var coordsToSpawn = FieldUtils.GetRandomEmptyCells(GameFieldManager.Instance._field,
            GameFieldManager.Instance.MainGameConfig.MaxUltimateCells);
        var list = new List<UniTask>();
        foreach (var pos in coordsToSpawn) {
            list.Add(SpawnNewCellFromUltimate(pos));
            await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
        }

        await UniTask.WhenAll(list);
        _starsParticles.Stop();
        _starsParticles.gameObject.SetActive(false);
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
       
        if (!GameFieldManager.Instance.CheckWin() && GameFieldManager.Instance.CheckLose()) {
            GameFieldManager.Instance.Lose();
        }
      
        _ultimateIsActive = false;
    }
    
    public async void UltimateActionEndRound()
    {
        _ultimateIsActive = true;
        
        var coordsToSpawn = FieldUtils.GetRandomEmptyCells(GameFieldManager.Instance._field,0);
        foreach (var pos in coordsToSpawn) {
            SpawnNewCellFromUltimate(pos, true).Forget();
            await UniTask.Delay(TimeSpan.FromSeconds(0.05f));
        }

        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        
        GameFieldManager.Instance.ExplodeCellsInRows();
        
        GameFieldManager.Instance.Win();
        _ultimateIsActive = false;
    }

    private async UniTask SpawnNewCellFromUltimate(Vector2Int placedCellPosition, bool isEndRoundUltimate = false)
    {
        var pieceData = GetRandomCellType();

        var config =
            PiecesViewTable.Instance.CellsList.CellsConfigs.First(c => c.CellType == pieceData.Type.CellType);
        var cellView = GameFieldManager.Instance.PlaceOneSizePiece(config,
            new Vector2Int(placedCellPosition.x, placedCellPosition.y), false);
        var finPos = new Vector3(cellView.transform.position.x, 0.75f, cellView.transform.position.z);
        cellView.transform.position = finPos + _startDropStartPos;
        cellView.transform.localScale = Vector3.zero;
        cellView.gameObject.SetActive(false);
        var star = Instantiate(_starPrefab);
        float multi = (_isRandomPos ? (Random.Range(0, 2) == 0 ? 1 : 0) : 1);
        var pos = cellView.transform.position;
        pos.x = _startSpawnXPos * multi;
        star.transform.position = pos;
       
        await DOTween.Sequence().Append(star.gameObject.transform.DOMoveX(finPos.x, _starDropDuration).SetEase(_animationCurveX))
            .Join(star.gameObject.transform.DOMoveY(finPos.y, _starDropDuration).SetEase(_animationCurveY))
            .Join(star.gameObject.transform.DOMoveZ(finPos.z, _starDropDuration).SetEase(_animationCurveZ)).AsyncWaitForCompletion();
        cellView.transform.position = star.transform.position;
        cellView.gameObject.SetActive(true);
        VibrationsManager.Instance.SpawnVibration(VibrationType.AllRow);
        star.ShowBoom(_starsParticles.transform.parent);
        Destroy(star.gameObject);

        cellView.gameObject.transform.DOScale(Vector3.one, 0.5f);
        if (!isEndRoundUltimate)
            GameFieldManager.Instance.CheckCellTypesBeforePlacePiece(placedCellPosition);
        
        GameFieldManager.Instance.SetNeededCellTypeOnField(pieceData.Type.CellType, cellView, placedCellPosition, true);
        if (!isEndRoundUltimate)
        {
            GameFieldManager.Instance.CheckClosestCells(placedCellPosition);
            GameFieldManager.Instance.CollectResourcesOnPlace(pieceData);
            GameFieldManager.Instance.ExplodeCellsInRows();
        }
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