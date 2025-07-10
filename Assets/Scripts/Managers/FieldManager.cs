using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Pool;

public class FieldManager : MonoBehaviour {
    [HideInInspector]
    public List<CellType> _currentCellsToSpawn { get; protected set; }

    public float[] CellsChanceToSpawn { get; protected set; }
    public float[] FiguresChanceToSpawn { get; protected set; }

    [SerializeField]
    protected Camera _mainCamera;

    public Transform CameraContainer;

    [SerializeField]
    protected LayerMask _targetMasks;

    [SerializeField]
    private LayerMask _additionalContainerMask;

    [SerializeField]
    private ParticleSystem _placeCellEffect;

    [SerializeField]
    private NetworkTimeAPI networkTimeAPI;

    [SerializeField]
    private MMF_Player _mmfPlayer;

    public static float PieceVerticalShift;

    protected bool _hasInternetConnection;
    //  protected DateTime _currentGameTime;

    public CellType[,] _field { get; protected set; }
    protected CellView[,] _cells;
    private static readonly Vector3 HalfCoord = new Vector3(0.5f, 0, 0.5f);

    private ObjectPool<ParticleSystem> _placeCellEffectsPool;

    protected InputRaycaster _inputRaycaster;
    // protected LevelConfig _currentLevelConfig;

    private Tween _currentTween;

    [Header("Audio")]
    [SerializeField]
    protected GameAudio _gameAudio;
    
    protected bool _isDestroyPieceMode;
    
    [SerializeField]
    protected LayerMask _pieceMask;

    [Header("Drop Animation Settings")]
    [SerializeField]
    private bool _isSquishingOnDrop = false;
    [SerializeField]
    private float _delayBetweenTileDrop = 0.15f, _delayBetweenDecorDrop=0.1f, _jumpPercent = 0.125f;
    [SerializeField]
    private float _dropLength = 0.3f, _jumpLength = 0.1f;

    protected virtual void Awake() {
        _inputRaycaster = new InputRaycaster(_mainCamera, _targetMasks, _additionalContainerMask);
    }

    protected virtual void Start() {
        SetupGame();
        _placeCellEffectsPool = new ObjectPool<ParticleSystem>(() => Instantiate(_placeCellEffect));
        // Application.targetFrameRate = 144;
    }

    protected virtual void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            if (_isDestroyPieceMode)
                TryDestroyPiece();
        }
    }

    protected virtual void TryDestroyPiece()
    {
        
    }

    public void ToggleDestroyPieceMode() {
        _isDestroyPieceMode = !_isDestroyPieceMode;

       /* if (_isDestroyPieceMode)
            HummerManager.Instance.ShowHummerAnimation();
        else
            HummerManager.Instance.HideHummerAnimation();*/
    }

    protected async void HummerDestoyPieceAnimation(CellView cell) {
        cell.OffCollider();
        await HummerManager.Instance.HummerDestroyPieceAnimation(cell.transform.position);
        DestroyPieceWithHummer(cell);
    }

    private void DestroyPieceWithHummer(CellView cell) {
        cell.DestroyCell();
        VibrationsManager.Instance.SpawnVibration(VibrationType.PlacePiece);
        ShakeCamera(0.6f);
    }


    protected void CalculateFiguresSpawnChances() {
        float lastChance = 0;
        FiguresChanceToSpawn = new float[PiecesViewTable.Instance.FigureForms.Length];
        for (int i = 0; i < PiecesViewTable.Instance.FigureForms.Length; i++) {
            lastChance += PiecesViewTable.Instance.FigureForms[i].Cost;
            FiguresChanceToSpawn[i] = lastChance;
        }
    }

    public Vector2Int GetPosInCoord() {
        Vector3 position = ShiftedDragInputPos();
        position += PieceCenterToCoordShift();
        Vector2Int coord = FieldUtils.ClampToCoord(position);
        return coord;
    }

    public static Vector3 PieceCenterToCoordShift() =>
        -new Vector3(PieceView.CurrentPieceMaxSize.x / 2f, 0, PieceView.CurrentPieceMaxSize.y / 2f) + HalfCoord;

    public Vector3 ShiftedDragInputPos() => _inputRaycaster.InputPos() + ConfigsManager.Instance.DragConfig.DragMouseShift +
                                            Vector3.forward * PieceVerticalShift;

    public bool CanPlace(PieceData data, Vector2Int pos) => FieldUtils.CanPlacePiece(_field, data, pos);

    public virtual void PlacePiece(PieceData pieceData, Vector2Int pos, CellView[,] cells, Transform cellsContainer) {
        float cellsAmount = 0;
        cellsContainer.transform.SetParent(FieldContainers.Instance.FieldContainer);
        for (int x = 0; x < pieceData.Cells.GetLength(0); x++) {
            for (int y = 0; y < pieceData.Cells.GetLength(1); y++) {
                if (!pieceData.Cells[x, y]) {
                    continue;
                }

                if (pieceData.Type.CellType == CellType.Dinamyte)
                {
                    BoostersManager.Instance.AnimateDynamite(pos);
                    //destroy dinamyte
                    return;
                }
                
                Vector2Int place = new(pos.x + x, pos.y + y);
                CheckCellTypesBeforePlacePiece(place);
                
                CellView go = cells[x, y];

                _field[place.x, place.y] = pieceData.Type.CellType;
                _cells[place.x, place.y] = go;

                SpawnResourceFx(place, go);
                cellsAmount++;

                if (UltaManager.Instance != null) {
                    UltaManager.Instance.AddUltimatePoints(1);
                }
            }
        }

        for (int x = 0; x < pieceData.Cells.GetLength(0); x++) {
            for (int y = 0; y < pieceData.Cells.GetLength(1); y++) {
                Vector2Int place = new(pos.x + x, pos.y + y);
                if (!pieceData.Cells[x, y]) continue;

                CheckClosestCells(new Vector2Int(place.x, place.y));
            }
        }

        ShowDropImpact(cellsContainer.transform,cells, pieceData, cellsContainer.gameObject, cellsAmount);
    }

    public virtual void CheckClosestCells(Vector2Int coord) { }
    public virtual void CheckCellTypesBeforePlacePiece(Vector2Int coord) { }

    public void ShowDropImpact(Transform pieceContainer,CellView[,] cells, PieceData pieceData, GameObject tmpContainer, float cellsAmount) {
        DropPeaceTween(pieceContainer,cells, () => {
            SpawnSmokeUnderPiece(tmpContainer.transform);
            float vibrationsAmplitude = cellsAmount / 9;
            if (pieceData.Type.CellType == CellType.Metal || pieceData.Type.CellType == CellType.Mountain ||
                pieceData.Type.CellType == CellType.Mine) {
                vibrationsAmplitude *= 1.5f;
            }

            _gameAudio.PlayNextSound(_gameAudio.PlacePiece);
            switch (pieceData.Type.CellType) {
                case CellType.Forest:
                    _gameAudio.PlayNextSound(_gameAudio.WoodPlaced);
                    break;
                case CellType.Mountain:
                    _gameAudio.PlayNextSound(_gameAudio.RockPlaced);
                    break;
                case CellType.FieldOfWheat:
                    _gameAudio.PlayNextSound(_gameAudio.WheatPlaced);
                    break;
                case CellType.Metal:
                    _gameAudio.PlayNextSound(_gameAudio.MetalPlaced);
                    break;
            }

            ShakeCamera(vibrationsAmplitude);
            VibrationsManager.Instance.SpawnVibrationEmhpasis(vibrationsAmplitude);
        });
    }

    private void DropPeaceTween(Transform piece,CellView[,] cells,  Action dropCallback) {
        var cnfg = ConfigsManager.Instance.DragConfig;
        var animSpeedMultiplayer = cnfg.AfterDropPieceAnimationMultiplayer;
        var finY = FieldContainers.Instance.PlacedCellsVerticalAnchor.position.y-0.3f;
        var seq = DOTween.Sequence();
        int cellsCount = 0;
        foreach (var VARIABLE in cells) {
            if (VARIABLE == null) {
                continue;
            }

            var cellSeq = DOTween.Sequence();
            cellSeq.AppendInterval(cnfg._delayBetweenTileDrop * animSpeedMultiplayer * cellsCount);
            cellSeq.Append(VARIABLE.DropWithDecorSequence(cnfg, finY));
           /* cellSeq.Append(VARIABLE.transform.DOMoveY(finY, _dropLength * animSpeedMultiplayer));
            cellSeq.Append(VARIABLE.transform.DOMoveY(finY + jumpHeight, _jumpLength / 2 * animSpeedMultiplayer));
            cellSeq.Append(VARIABLE.transform.DOMoveY(finY, _jumpLength / 2 * animSpeedMultiplayer));*/
            seq.Join(cellSeq);
            cellsCount++;
        }

        var callbackSeq = DOTween.Sequence();
        callbackSeq.AppendInterval(cnfg._delayBetweenTileDrop * animSpeedMultiplayer * (cellsCount - 1) / 2f +
                                   cnfg._dropLength * animSpeedMultiplayer * cnfg._callbackPercent);
        callbackSeq.AppendCallback(() => dropCallback?.Invoke());
        seq.Join(callbackSeq);

        if (_isSquishingOnDrop) {
            seq.Append(piece.DOScaleY(piece.localScale.y * 0.6f, 0.25f))
                .Join(piece.DOScaleX(piece.localScale.x * 1.1f, 0.25f * animSpeedMultiplayer))
                .Join(piece.DOScaleZ(piece.localScale.z * 1.1f, 0.25f))
                .Append(piece.DOScaleY(piece.localScale.y * 1.2f, 0.2f * animSpeedMultiplayer))
                .Join(piece.DOScaleX(piece.localScale.x * 0.8f, 0.2f))
                .Join(piece.DOScaleZ(piece.localScale.z * 0.8f, 0.2f * animSpeedMultiplayer))
                .Append(piece.DOScale(new Vector3(1, 1, 1), 0.25f * animSpeedMultiplayer));
        }

        seq.OnComplete(() => {
            while (piece.childCount > 0) {
                piece.GetChild(0).SetParent(FieldContainers.Instance.FieldContainer);
            }

            Destroy(piece.gameObject);
        });
        seq.Play();
    }

    private void SpawnSmokeUnderPiece(Transform piece) {
        SpawnSmokeParticle(piece.transform.position).Forget();
    }

    protected virtual void SpawnResourceFx(Vector2Int place, CellView go) { }

    protected void ShakeCamera(float percent) {
        percent = Mathf.LerpUnclamped(0.3f, 1f, percent);
        var screenShake = _mmfPlayer.GetFeedbackOfType<MMF_CameraShake>();
        screenShake.CameraShakeProperties.Amplitude = 1.5f * percent;
        screenShake.CameraShakeProperties.AmplitudeZ = 0.5f * percent;
        screenShake.CameraShakeProperties.Duration = 1f * percent;

        var zoom = _mmfPlayer.GetFeedbackOfType<MMF_CameraZoom>();
        zoom.ZoomFieldOfView = -1 * percent;
        _mmfPlayer.PlayFeedbacks();
    }

    private void OnApplicationQuit() {
        SaveEnergyData();
    }

    public virtual void SaveEnergyData() {
        StorageManager.SaveGame();
    }

    private async UniTask SpawnSmokeParticle(Vector3 pos) {
        var particles = _placeCellEffectsPool.Get();
        particles.gameObject.SetActive(true);
        particles.transform.position = new Vector3(pos.x, pos.y + ConfigsManager.Instance.DragConfig.smokeVerticalShift, pos.z);
        particles.Play();
        await UniTask.Delay(TimeSpan.FromSeconds(2));
        if (particles) {
            ReleaseParticles(particles);
        }
    }

    private void ReleaseParticles(ParticleSystem particles) {
        particles.gameObject.SetActive(false);
        _placeCellEffectsPool.Release(particles);
    }

    public virtual void SetupGame()
    {
        SettingsManager.Instance.SetSettings();
    }

    public void PlayCollectedSound() {
        _gameAudio.PlayNextSound(_gameAudio.ResourceCollected);
    }
}