using System;
using System.Collections.Generic;
using System.Linq;
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
    public CellView[,] _cells{ get; protected set; }

    public CellView GetCellInCoord(Vector3Int coord) {
        return _cells[coord.x, coord.z];
    }

    private static readonly Vector3 HalfCoord = new Vector3(0.5f, 0, 0.5f);

    private ObjectPool<ParticleSystem> _placeCellEffectsPool;

    protected InputRaycaster _inputRaycaster;

    // protected LevelConfig _currentLevelConfig;
    public event Action<Vector2Int, bool[,]> OnCellPlaced;
    public event Action OnCellPlacedTrigger;

    private Tween _currentTween;

    [Header("Audio")]
    [SerializeField]
    protected GameAudio _gameAudio;

    protected bool _isDestroyPieceMode;
    protected bool _placeDynamiteMode;

    [SerializeField]
    protected LayerMask _pieceMask, _groundMask;

    protected virtual void Awake() {
        _inputRaycaster = new InputRaycaster(_mainCamera, _targetMasks, _additionalContainerMask);
       // _mainCamera.cullingMask |= (1 << LayerMask.NameToLayer("Highlighted"));
    }

    protected virtual void Start() {
        //    SetupGame();
    }

    protected virtual void Update() {
        if (Input.GetMouseButtonDown(0)) {
            if (_isDestroyPieceMode) {
                TryDestroyPiece();
                if(BoostersManager.Instance != null) {
                    BoostersManager.Instance.CancelHammer();
                }
                else {
                    MetaFieldManager.Instance.EndDestroyMode();
                }
            } else if (_placeDynamiteMode) {
                TryPlaceDynamite();
                BoostersManager.Instance.CancelDynamite();
            }
        }
    }

    protected virtual bool TryDestroyPiece() {
        return false;
    }

    private async void TryPlaceDynamite() {
        Physics.Raycast(_mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, _groundMask);

        if (hit.collider == null || StorageManager.GameDataMain.DynamiteCount <= 0) {
            return;
        }

        Vector2Int coord = FieldUtils.ClampToCoord(hit.point);

        if (!FieldUtils.IsInsideField(_field, coord)) {
            return;
        }

        //динамит можно применить на любую клетку 
        if (false) {
            //!FieldUtils.CanPlaceOnCell(_field[coord.x, coord.y])) {
            return;
        }

        var pos = new Vector3(coord.x, 1f, coord.y);
        _placeDynamiteMode = false;
        var p = await NextPiecesView.Instance.CreateDynamitePieceView(pos);
        PlacePiece(p.Data, coord, p._cells, p._cellsContainer);
        GameAudio.Instance.PlayNextSoundWithDelay(GameAudio.Instance.UseDynamite, 0.25f, this.GetCancellationTokenOnDestroy());
        Destroy(p.gameObject);
    }

    public virtual void ToggleDestroyPieceMode() {
        _isDestroyPieceMode = !_isDestroyPieceMode;
    }

    public void SetDestroyPieceMode(bool isOn) {
        _isDestroyPieceMode = isOn;
    }

    public virtual void TogglePlaceDynamiteMode() {
        _placeDynamiteMode = !_placeDynamiteMode;
    }

    public void DisablePlaceDynamiteMode() {
        _placeDynamiteMode = false;
    }

    protected async void HummerDestoyPieceAnimation(CellView[] cells) {
        Vector3 hummerNeedPos = Vector3.up;
        foreach (var cell in cells) {
            cell.OffCollider();
            hummerNeedPos += cell.transform.position;
        }

        hummerNeedPos /= cells.Length;
        await HummerManager.Instance.HummerDestroyPieceAnimation(hummerNeedPos);
        DestroyPieceWithHummer(cells);
    }

    public List<Vector3Int> AllHammerableCells() {
        List<Vector3Int> r = AllFieldCells();
        return r.Where(pos => _cells[pos.x, pos.z] != null).Where(pos => FieldUtils.CanHammerOrExplode(_field[pos.x, pos.z])).ToList();
    }
    
    private void DestroyPieceWithHummer(CellView[] cells) {
        foreach (var cell in cells)
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

    private static List<Vector3Int> AllField;

    public static List<Vector3Int> AllFieldCells() {
        if (AllField != null) {
            return AllField;
        }

        AllField = new List<Vector3Int>();
        int size = 8;
        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                AllField.Add(new Vector3Int(i, 0, j));
            }
        }

        return AllField;
    }

    public virtual void PlacePiece(PieceData pieceData, Vector2Int pos, CellView[,] cells, Transform cellsContainer) {
        float cellsAmount = 0;
        OnCellPlaced?.Invoke(pos, pieceData.Cells);
        OnCellPlacedTrigger?.Invoke();
        cellsContainer.transform.SetParent(FieldContainers.Instance.FieldContainer);
        
        //Debug.Log( cellsContainer.transform.localScale);
        cellsContainer.transform.localScale = Vector3.one;
        
        if (pieceData.Type.CellType == CellType.Dynamite) {
            if (!FieldUtils.CanPlaceOnCell(_field[pos.x, pos.y])) {
                cellsContainer.transform.position += Vector3.up;
            }
            BoostersManager.Instance.AnimateDynamite(cellsContainer, pos);
            //destroy dinamyte
            return;
        }

        for (int x = 0; x < pieceData.Cells.GetLength(0); x++) {
            for (int y = 0; y < pieceData.Cells.GetLength(1); y++) {
                if (!pieceData.Cells[x, y]) {
                    continue;
                }

                Vector2Int place = new(pos.x + x, pos.y + y);
                CheckCellTypesBeforePlacePiece(place);

                CellView go = cells[x, y];

                _field[place.x, place.y] = cells[x, y].CellType; //fix if has problem
                _cells[place.x, place.y] = go;

                SpawnResourceFxForCell(place, go);
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

        ShowDropImpact(cellsContainer.transform, cells, pieceData, cellsContainer.gameObject, cellsAmount);
    }

    public virtual void CheckClosestCells(Vector2Int coord) { }
    public virtual void CheckCellTypesBeforePlacePiece(Vector2Int coord) { }

    public void ShowDropImpact(Transform pieceContainer, CellView[,] cells, PieceData pieceData, GameObject tmpContainer, float cellsAmount) {
        DropPeaceTween(pieceContainer, cells, () => {
            SpawnSmokeUnderPiece(tmpContainer.transform);
            float vibrationsAmplitude = cellsAmount / 9;
            if (pieceData.Type.CellType == CellType.Metal || pieceData.Type.CellType == CellType.Mountain ||
                pieceData.Type.CellType == CellType.Mine) {
                vibrationsAmplitude *= 1.5f;
            }

            _gameAudio.PlayNextSound(_gameAudio.PlacePiece);
            switch (pieceData.Type.CellType) {
                case CellType.Wood:
                case CellType.Forest:
                case CellType.ForestLevel2:
                    _gameAudio.PlayNextSound(_gameAudio.WoodPlaced);
                    break;
                case CellType.Stone:
                case CellType.Mountain:
                case CellType.MountainLevel2:
                    _gameAudio.PlayNextSound(_gameAudio.RockPlaced);
                    break;
                case CellType.Wheat:
                case CellType.FieldOfWheat:
                case CellType.FieldOfWheatLevel2:
                    _gameAudio.PlayNextSound(_gameAudio.WheatPlaced);
                    break;
                case CellType.MetalMines:
                case CellType.MetalMinesLevel2:
                case CellType.Metal:
                    _gameAudio.PlayNextSound(_gameAudio.MetalPlaced);
                    break;
                case CellType.Village:
                case CellType.VillageLevel2:
                case CellType.Sawmill:
                    _gameAudio.PlayNextSound(_gameAudio.HousePlaced);
                    break;
            }

            ShakeCamera(vibrationsAmplitude);
            VibrationsManager.Instance.SpawnVibrationEmhpasis(vibrationsAmplitude);
        });
    }

    private void DropPeaceTween(Transform piece, CellView[,] cells, Action dropCallback) {
        var cnfg = ConfigsManager.Instance.DragConfig;
        var animSpeedMultiplayer = cnfg.AfterDropPieceAnimationMultiplayer;
        var finY = FieldContainers.Instance.PlacedCellsVerticalAnchor.position.y - 0.3f;
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

        if (cnfg.IsSquishingOnDrop) {
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

    protected virtual void SpawnResourceFxForCell(Vector2Int place, CellView go) { }

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

    public virtual void SetupGame() {
        _placeCellEffectsPool = new ObjectPool<ParticleSystem>(() => Instantiate(_placeCellEffect));
    }

    public void PlayCollectedSound() {
        _gameAudio.PlayNextSound(_gameAudio.ResourceCollected);
    }
}