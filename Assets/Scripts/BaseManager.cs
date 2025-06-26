using System;
using System.Collections.Generic;
using System.Globalization;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

public class BaseManager : MonoBehaviour {
    [SerializeField]
    private AudioQueueMixer _placePieceAudioMixer;

    [SerializeField]
    private AudioQueueMixer _collectedResourceAudioMixer;

    [HideInInspector]
    public List<CellTypeInfo> _currentCellsToSpawn { get; protected set; }

    public float[] CellsChanceToSpawn { get; protected set; }
    public float[] FiguresChanceToSpawn { get; protected set; }

    [SerializeField]
    protected Camera _mainCamera;

    public Transform CameraContainer;

    [SerializeField]
    protected LayerMask _targetMasks;

    [field: SerializeField]
    public FigureFormConfig[] FigureFormsConfig { get; protected set; }

    [field: SerializeField]
    private Transform[] _healthImages;

    [SerializeField]
    private TMP_Text _healthTimerText;

    [SerializeField]
    private int _minutesToHealthRecovery;

    [SerializeField]
    private ParticleSystem _placeCellEffect;

    [SerializeField]
    private NetworkTimeAPI networkTimeAPI;

    [SerializeField]
    private MMF_Player _mmfPlayer;

    public static float PieceVerticalShift;

    protected bool _hasInternetConnection;
    protected DateTime _currentGameTime;

    protected CellType[,] _field;
    protected CellView[,] _cells;
    private static readonly Vector3 HalfCoord = new Vector3(0.5f, 0, 0.5f);

    private ObjectPool<ParticleSystem> _placeCellEffectsPool;
    private float timerNowTimeSecondCounter;

    private InputRaycaster _inputRaycaster;

    private Tween _currentTween;
    private DateTime _lastHealthRecoveryTime;

    private const int MAX_HEALTH_COUNT = 3;

    protected virtual void Awake() {
        ChangeToLoading.TryChange();
        _inputRaycaster = new InputRaycaster(_mainCamera, _targetMasks);
    }

    protected virtual void Start() {
        _currentGameTime = DateTime.Now;
        networkTimeAPI.GetNetworkTime(dateTime => {
            _currentGameTime = dateTime;
            Debug.Log("has connect" + dateTime);
            _hasInternetConnection = true;
            //SetupGame();
        }, error => {
            _currentGameTime = DateTime.Now;
            Debug.Log("not connect");
            _hasInternetConnection = false;
            //SetupGame();
            // _hasInternetConnection = false;
        });

        SetupGame();
        _placeCellEffectsPool = new ObjectPool<ParticleSystem>(() => Instantiate(_placeCellEffect));
        Application.targetFrameRate = 144;
    }

    private void AddSecondToTimer() => _currentGameTime = _currentGameTime.AddSeconds(1);

    protected virtual void Update() {
        if (_hasInternetConnection) {
            timerNowTimeSecondCounter += Time.unscaledDeltaTime;
            if (timerNowTimeSecondCounter >= 1) {
                timerNowTimeSecondCounter--;
                AddSecondToTimer();
            }

            if (StorageManager.GameDataMain.HealthCount < MAX_HEALTH_COUNT) {
                TimeSpan timeSinceLastUpdate = _currentGameTime - _lastHealthRecoveryTime;
                int energyToAdd = (int)(timeSinceLastUpdate.TotalMinutes / _minutesToHealthRecovery);

                if (energyToAdd > 0) {
                    StorageManager.GameDataMain.HealthCount =
                        Mathf.Min(StorageManager.GameDataMain.HealthCount + energyToAdd, MAX_HEALTH_COUNT);
                    _lastHealthRecoveryTime = _currentGameTime;
                    StorageManager.GameDataMain.LastHealthRecoveryTime = _currentGameTime.ToString(CultureInfo.InvariantCulture);
                    SaveEnergyData();
                    _healthImages[StorageManager.GameDataMain.HealthCount - 1].gameObject.SetActive(true);
                }

                UpdateTimerUI();
            }
        } else if (_healthTimerText.gameObject.activeSelf) {
            _healthTimerText.gameObject.SetActive(false);
        }
    }

    protected void CalculateFiguresSpawnChances() {
        float lastChance = 0;
        FiguresChanceToSpawn = new float[FigureFormsConfig.Length];
        for (int i = 0; i < FigureFormsConfig.Length; i++) {
            lastChance += FigureFormsConfig[i].Cost;
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

                Vector2Int place = new(pos.x + x,pos.y + y);
                CheckCellTypesBeforePlacePiece(place);
                CellView go = cells[x, y];

                _field[place.x, place.y] = pieceData.Type.CellType;
                _cells[place.x, place.y] = go;

                SpawnResourceFx(place, go);
                cellsAmount++;
            }
        }

        for (int x = 0; x < pieceData.Cells.GetLength(0); x++)
        {
            for (int y = 0; y < pieceData.Cells.GetLength(1); y++)
            {
                Vector2Int place = new(pos.x + x,pos.y + y);
                if (!pieceData.Cells[x, y]) continue;

                CheckClosestCells(new Vector2Int(place.x, place.y));
            }
        }

        ShowDropImpact(cellsContainer.transform, pieceData, cellsContainer.gameObject, cellsAmount);
    }
protected virtual void CheckClosestCells(Vector2Int coord){}
    protected virtual void CheckCellTypesBeforePlacePiece(Vector2Int coord) { }

    private void ShowDropImpact(Transform pieceContainer, PieceData pieceData, GameObject tmpContainer, float cellsAmount) {
        DropPeaceTween(pieceContainer, () => {
            _placePieceAudioMixer.PlayNext();
            SpawnSmokeUnderPiece(tmpContainer.transform);
            float vibrationsAmplitude = cellsAmount / 9;
            if (pieceData.Type.CellType == CellType.Metal || pieceData.Type.CellType == CellType.Mountain ||
                pieceData.Type.CellType == CellType.Mine) {
                vibrationsAmplitude *= 1.5f;
            }

            ShakeCamera(vibrationsAmplitude);
            VibrationsManager.Instance.SpawnVibrationEmhpasis(vibrationsAmplitude);
        });
    }

    private void DropPeaceTween(Transform piece, Action dropCallback) {
        var animSpeedMultiplayer = ConfigsManager.Instance.DragConfig.AfterDropPieceAnimationMultiplayer;
        DOTween.Sequence().Append(piece.DOMoveY(FieldContainers.Instance.PlacedCellsVerticalAnchor.position.y, 0.3f * animSpeedMultiplayer))
            .AppendCallback(() => dropCallback?.Invoke()).Append(piece.DOScaleY(piece.localScale.y * 0.6f, 0.25f))
            .Join(piece.DOScaleX(piece.localScale.x * 1.1f, 0.25f * animSpeedMultiplayer))
            .Join(piece.DOScaleZ(piece.localScale.z * 1.1f, 0.25f))
            .Append(piece.DOScaleY(piece.localScale.y * 1.2f, 0.2f * animSpeedMultiplayer))
            .Join(piece.DOScaleX(piece.localScale.x * 0.8f, 0.2f)).Join(piece.DOScaleZ(piece.localScale.z * 0.8f, 0.2f * animSpeedMultiplayer))
            .Append(piece.DOScale(new Vector3(1, 1, 1), 0.25f * animSpeedMultiplayer)).OnComplete(() => {
                while (piece.childCount > 0) {
                    piece.GetChild(0).SetParent(FieldContainers.Instance.FieldContainer);
                }

                Destroy(piece.gameObject);
            });
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

    public TimeSpan GetTimeUntilNextHealth() {
        if (StorageManager.GameDataMain.HealthCount >= MAX_HEALTH_COUNT) return TimeSpan.Zero;

        TimeSpan timeSinceLastUpdate = _currentGameTime - _lastHealthRecoveryTime;
        double minutesPassed = timeSinceLastUpdate.TotalMinutes;
        double minutesUntilNext = _minutesToHealthRecovery - (minutesPassed % _minutesToHealthRecovery);

        return TimeSpan.FromMinutes(minutesUntilNext);
    }

    private void UpdateTimerUI() {
        if (_hasInternetConnection) {
            if (StorageManager.GameDataMain.HealthCount >= MAX_HEALTH_COUNT) {
                if (_healthTimerText != null && _healthTimerText.gameObject.activeSelf)
                    _healthTimerText.gameObject.SetActive(false);
                return;
            }

            if (!_healthTimerText.gameObject.activeSelf)
                _healthTimerText.gameObject.SetActive(true);

            _healthTimerText.text = TimeConverter.ConvertToTimeString(GetTimeUntilNextHealth());
        } else {
            _healthTimerText.text = "No internet connection";
        }
    }

    private void OnApplicationQuit() {
        SaveEnergyData();
    }

    protected virtual void SaveEnergyData() {
        StorageManager.SaveGame();
    }

    private void OnApplicationPause(bool pauseStatus) {
        if (pauseStatus) {
            SaveEnergyData();
        } else {
            StorageManager.LoadGame();
            CalculateOfflineHealth();
        }
    }

    protected void RemoveHealth() {
        if (StorageManager.GameDataMain.HealthCount == MAX_HEALTH_COUNT) {
            _lastHealthRecoveryTime = _currentGameTime;
            StorageManager.GameDataMain.LastHealthRecoveryTime = _lastHealthRecoveryTime.ToString(CultureInfo.InvariantCulture);
        }

        _healthImages[StorageManager.GameDataMain.HealthCount - 1].gameObject.SetActive(false);
        StorageManager.GameDataMain.HealthCount--;
        SaveEnergyData();
    }

    private void CalculateOfflineHealth() {
        if (!_hasInternetConnection) return;
        _lastHealthRecoveryTime = StorageManager.GameDataMain.LastHealthRecoveryTimeDateTime;
        TimeSpan offlineTime = _currentGameTime - _lastHealthRecoveryTime;
        int healthToAdd = (int)(offlineTime.TotalMinutes / _minutesToHealthRecovery);

        if (healthToAdd > 0) {
            StorageManager.GameDataMain.HealthCount = Mathf.Min(StorageManager.GameDataMain.HealthCount + healthToAdd, MAX_HEALTH_COUNT);
        }

        if (StorageManager.GameDataMain.HealthCount != MAX_HEALTH_COUNT)
            _lastHealthRecoveryTime.AddMinutes(healthToAdd * _minutesToHealthRecovery);
    }

    private async UniTask SpawnSmokeParticle(Vector3 pos) {
        var particles = _placeCellEffectsPool.Get();
        particles.gameObject.SetActive(true);
        particles.transform.position = new Vector3(pos.x, pos.y - 0.2f, pos.z);
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

    protected virtual void SetupGame() {
        if (StorageManager.GameDataMain.HealthCount > MAX_HEALTH_COUNT)
            StorageManager.GameDataMain.HealthCount = MAX_HEALTH_COUNT;

        if (StorageManager.GameDataMain.HealthCount == MAX_HEALTH_COUNT) {
            _healthTimerText.gameObject.SetActive(false);
        } else {
            CalculateOfflineHealth();
            if (_hasInternetConnection)
                _healthTimerText.text = StorageManager.GameDataMain.LastHealthRecoveryTime.ToString();
            else
                _healthTimerText.text = "No internet connection";
            for (int i = 0; i < MAX_HEALTH_COUNT; i++) {
                _healthImages[i].gameObject.SetActive(StorageManager.GameDataMain.HealthCount > i);
            }
        }
    }

    public void PlayCollectedSound() {
        _collectedResourceAudioMixer.PlayNext();
    }
}