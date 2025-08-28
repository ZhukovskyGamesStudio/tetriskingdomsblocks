using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

public class UltaManager : MonoBehaviour {
    public static UltaManager Instance;

    [SerializeField]
    private FallingStarFx _starPrefab;

    [SerializeField]
    private ParticleSystem _starsParticles;

    
    public int _currentPoints{ get; private set; }

    [SerializeField]
    private float _starDropDuration = 0.25f, _startSpawnXPos = 15;

    [SerializeField]
    private Vector3 _startDropStartPos;

    public bool _ultimateIsActive { get; private set; }

    [SerializeField]
    private AnimationCurve _animationCurveX, _animationCurveY, _animationCurveZ;

    [SerializeField]
    private bool _isRandomPos;

    public Action OnUltimateEndedWorking;
    private MainGameConfig _mainGameConfig;
    private GameData _gameData;

    private void Awake() {
        Instance = this;
    }

    public void Init(MainGameConfig mainGameConfig, GameData gameData) {
        _mainGameConfig = mainGameConfig;
        _gameData = gameData;
        GameUI.Instance.GoalView.UltimateProgressBar.maxValue = _mainGameConfig.NeededUltimatePoints;
        GameUI.Instance.GoalView.UltimateButton.onClick.AddListener(UltimateAction);
    }

    public void AddUltimatePoints(int points) {
        if (_ultimateIsActive || _gameData.IsGameEnded) return;
        _currentPoints += points;

        GameUI.Instance.GoalView.UltimateProgressBar.DOValue(_currentPoints, 0.4f).SetEase(Ease.Linear).OnComplete(() => {
            if (_currentPoints >= GameUI.Instance.GoalView.UltimateProgressBar.maxValue) 
                ActivateButton();
            
        });
    }

    private void ActivateButton() {
        if (_gameData.IsGameEnded) {
            return;
        }

        GameUI.Instance.GoalView.ActivateUltimateButton();
    }

    private async void UltimateAction() {
        if (_gameData.IsGameEnded || FloatingResourcesManager.Instance._currentActiveAnimationsCount != 0 || DragManager.IsDraggingPiece) {
            return;
        }

        GameUI.Instance.GoalView.UltimateButton.enabled = false;
        GameUI.Instance.GoalView.UltimateProgressBar.value = 0;
        _currentPoints = 0;
        _ultimateIsActive = true;
        GameUI.Instance.GoalView.HideUltimateButton();
        _starsParticles.gameObject.SetActive(true);
        _starsParticles.Play();
        GameAudio.Instance.PlayNextSound(GameAudio.Instance.StarsLong);
        int maxStars = _mainGameConfig.MaxUltimateCells;
        List<Vector2Int> pieceCells = GameFieldManager.Instance.CanPlaceAnyPieceForUltimate();
        List<Vector2Int> coordsToSpawn = new List<Vector2Int>();
        if (StorageManager.GameDataMain.CurMaxLevel == 2) {
            coordsToSpawn = FieldUtils.GetRandomEmptyCellsWithoutSomeCells(GameFieldManager.Instance._field, 100,new List<Vector2Int>());
        }
      else if(pieceCells != null)
            coordsToSpawn = FieldUtils.GetRandomEmptyCellsWithoutSomeCells(GameFieldManager.Instance._field, maxStars,pieceCells);
       else 
           coordsToSpawn = FieldUtils.GetCellsFromUltRows(maxStars);
       
       var list = new List<UniTask>();
        foreach (var pos in coordsToSpawn) {
            list.Add(SpawnNewCellFromUltimate(pos));
            await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
        }

        await UniTask.WhenAll(list);
        _starsParticles.Stop();
        _starsParticles.gameObject.SetActive(false);
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
        _ultimateIsActive = false;
        OnUltimateEndedWorking?.Invoke();
    }

    public async void UltimateActionEndRound(Action onUltimateEnded) {
        GameUI.Instance.GoalView.HideUltimateUI();
        _ultimateIsActive = true;
        var needField = GameFieldManager.Instance._field;
        var coordsToSpawn = FieldUtils.GetAllEmptyCells(needField);
        foreach (var pos in coordsToSpawn) {
            SpawnNewCellFromUltimate(pos, true).Forget();
            await UniTask.Delay(TimeSpan.FromSeconds(0.05f));
        }

        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        _ultimateIsActive = false;
        onUltimateEnded?.Invoke();
    }
    

    private async UniTask SpawnNewCellFromUltimate(Vector2Int placedCellPosition, bool isEndRoundUltimate = false) {
        var pieceData = GetRandomCellType();

        var config = PiecesViewTable.Instance.CellsList.CoreCellsConfigs.First(c => c.CellType == pieceData.Type.CellType);
        var cellView =  GameFieldManager.Instance.PlaceOneSizePiece(config, new Vector2Int(placedCellPosition.x, placedCellPosition.y), false);
        var finPos = new Vector3(cellView.transform.position.x, 0.55f, cellView.transform.position.z);
        cellView.transform.position = finPos + _startDropStartPos;
        cellView.transform.localScale = Vector3.zero;
        cellView.gameObject.SetActive(false);
        // cellView.SetSeed(pieceData.CellGuids[0,0]); //doesnt work for something reason
        var star = Instantiate(_starPrefab);
        float multi = (_isRandomPos ? (Random.Range(0, 2) == 0 ? 1 : 0) : 1);
        var pos = cellView.transform.position;
        pos.x = _startSpawnXPos * multi;
        star.transform.position = pos;
        if(!isEndRoundUltimate) {
            GameAudio.Instance.PlayNextSound(GameAudio.Instance.StarsEach);
        }
        await DOTween.Sequence().Append(star.gameObject.transform.DOMoveX(finPos.x, _starDropDuration).SetEase(_animationCurveX))
            .Join(star.gameObject.transform.DOMoveY(finPos.y, _starDropDuration).SetEase(_animationCurveY))
            .Join(star.gameObject.transform.DOMoveZ(finPos.z, _starDropDuration).SetEase(_animationCurveZ)).AsyncWaitForCompletion();
        cellView.transform.position = star.transform.position;
        cellView.gameObject.SetActive(true);
        VibrationsManager.Instance.SpawnVibration(VibrationType.AllRow);
        star.ShowBoom(_starsParticles.transform.parent);
        Destroy(star.gameObject);

       
        if (!isEndRoundUltimate)
            GameFieldManager.Instance.CheckCellTypesBeforePlacePiece(placedCellPosition);
        if (GameFieldManager.Instance != null)
            GameFieldManager.Instance.SetNeededCellTypeOnField(pieceData.Type.CellType, cellView, placedCellPosition, true);
        if (!isEndRoundUltimate) {
            GameFieldManager.Instance.CheckClosestCells(placedCellPosition);
            GameFieldManager.Instance.CollectResourcesOnPlace(pieceData, new []{ cellView});
            GameFieldManager.Instance.ExplodeCellsInRows();
        }
       if(GameFieldManager.Instance._field[placedCellPosition.x, placedCellPosition.y] != CellType.Empty) 
        cellView.gameObject.transform.DOScale(Vector3.one, 0.5f);
       else 
           cellView.gameObject.transform.localScale = Vector3.one;
    }

    private PieceData GetRandomCellType() {
        var cellsToSpawn =  GameFieldManager.Instance._currentCellsToSpawn;
        var chancesToSpawn =  GameFieldManager.Instance.CellsChanceToSpawn;
        CellTypeInfo cellInfo = null;
        float chance = Random.Range(0, chancesToSpawn[chancesToSpawn.Length - 1]);
        for (int j = 0; j < chancesToSpawn.Length; j++) {
            if (chancesToSpawn[j] > chance) {
                cellInfo = PiecesViewTable.Instance.CellsList.CoreCellsConfigs.First(c => c.CellType == cellsToSpawn[j]);
                break;
            }
        }

        bool[,] cells = TetrisPieces.PieceShapesTable["oneBlock"];
        Guid[,] cellGuids = new Guid[cells.GetLength(0), cells.GetLength(1)];
        for (int x = 0; x < cells.GetLength(0); x++) {
            for (int y = 0; y < cells.GetLength(1); y++) {
                cellGuids[x, y] = Guid.NewGuid();
            }
        }

        var data = new PieceData() {
            Type = cellInfo,
            Cells = cells,
            CellGuids = cellGuids
        };
        return data;
    }
}