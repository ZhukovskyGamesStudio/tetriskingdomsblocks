using System;
using System.Collections.Generic;
using System.Globalization;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MoreMountains.Feedbacks;
using ScriptableObjects.Configs;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

public class BaseManager : MonoBehaviour  {
    public const int CELL_SIZE = 1;
    protected CellType[,] _field;
    protected CellView[,] _cells;
    private Tween _currentTween;
    private DateTime _lastHealthRecoveryTime;
    public float[] FiguresChanceToSpawn { get; protected set; }
    public float _screenRatio { get; protected set; }

    [SerializeField]
    private AudioQueueMixer _placePieceAudioMixer;
    [SerializeField]
    private AudioQueueMixer _collectedResourceAudioMixer;
    
    [HideInInspector]
    public List<CellTypeInfo> _currentCellsToSpawn { get; protected set; }

    public float[] CellsChanceToSpawn { get; protected set; }

    [HideInInspector]
    public Vector3 ScreenToWorldPoint => Physics.Raycast(_mainCamera.ScreenPointToRay(Input.mousePosition),
        out RaycastHit hit, Mathf.Infinity, _targetMasks)
        ? hit.point
        : Vector3.zero;

    [HideInInspector]
    public Vector3 TouchToWorldPoint => Physics.Raycast(_mainCamera.ScreenPointToRay(Input.GetTouch(0).position),
        out RaycastHit hit, Mathf.Infinity, _targetMasks)
        ? hit.point
        : Vector3.zero;

    [field: SerializeField]
    public Transform _markedCell { get; protected set; }

    [SerializeField]
    protected Camera _mainCamera;

    [SerializeField]
    protected Transform _fieldContainer;

    public Transform CameraContainer;

    [SerializeField]
    protected LayerMask _targetMasks;

    [SerializeField]
    private Transform _fieldStart, _fieldEnd;

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

    protected bool _hasInternetConnection;
    private float timerNowTimeSecondCounter;
    protected DateTime _currentGameTime;
    private ObjectPool<ParticleSystem> _placeCellEffectsPool;

    [SerializeField]
    private MMF_Player _mmfPlayer;

    public const int MAX_HEALTH_COUNT = 3;

    private static readonly Vector3 HalfCoord = new Vector3(0.5f, 0, 0.5f);
    public static float PieceVerticalShift;

    protected virtual void Awake() {
        ChangeToLoading.TryChange();
    }

    protected virtual void Start() {
        networkTimeAPI.GetNetworkTime(dateTime => {
            _currentGameTime = dateTime;
            Debug.Log("has connect" + dateTime);
            _hasInternetConnection = true;
            SetupGame();
        }, error => {
            _currentGameTime = DateTime.Now;
            Debug.Log("not connect");
            _hasInternetConnection = false;
            SetupGame();
            // _hasInternetConnection = false;
        });

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
        Vector2Int coord = ClampToCoord(position);
        return coord;
    }

    public static Vector3 PieceCenterToCoordShift() =>
        -new Vector3(PieceView.PieceMaxSize.x / 2f, 0, PieceView.PieceMaxSize.y / 2f) + HalfCoord;

    public static Vector2Int ClampToCoord(Vector3 coord) => new(Mathf.RoundToInt(coord.x) / CELL_SIZE, Mathf.RoundToInt(coord.z) / CELL_SIZE);

    public Vector3 InputPos() => Input.touchCount == 0 ? ScreenToWorldPoint : TouchToWorldPoint;

    public Vector3 ShiftedDragInputPos() => InputPos() + ConfigsManager.Instance.DragConfig.DragMouseShift + Vector3.forward * PieceVerticalShift;

    public bool CanPlace(PieceData data, Vector2Int pos) {
        if (pos.x < 0 || pos.y < 0)
            return false;

        if (pos.x + data.Cells.GetLength(0) - 1 >= _field.GetLength(0))
            return false;

        if (pos.y + data.Cells.GetLength(1) - 1 >= _field.GetLength(1))
            return false;

        for (int x = 0; x < data.Cells.GetLength(0); x++) {
            for (int y = 0; y < data.Cells.GetLength(1); y++) {
                if (data.Cells[x, y] && _field[pos.x + x, pos.y + y] != CellType.Empty)
                    return false;
            }
        }

        return true;
    }

    protected virtual void PlacePiece(PieceData pieceData, Vector2Int pos, int fieldSize, CellView[,] cells, Transform cellsContainer) {
        float cellsAmount = 0;
        cellsContainer.transform.SetParent(_fieldContainer);
        for (int x = 0; x < pieceData.Cells.GetLength(0); x++) {
            for (int y = 0; y < pieceData.Cells.GetLength(1); y++) {
                if (!pieceData.Cells[x, y]) {
                    continue;
                }

                Vector2Int place = new(Mathf.Clamp(pos.x + x, 0, fieldSize), Mathf.Clamp(pos.y + y, 0, fieldSize));
                CellView go = cells[x, y];
              
                _field[place.x, place.y] = pieceData.Type.CellType;
                _cells[place.x, place.y] = go;

                SpawnResourceFx(pieceData, place, go);
                cellsAmount++;
            }
        }

        ShowDropImpact(cellsContainer.transform, pieceData, cellsContainer.gameObject, cellsAmount);
    }

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
        DOTween.Sequence().Append(piece.DOMoveY(FieldContainers.Instance.MarkedCellsVerticalAnchor.position.y, 0.3f))
            .AppendCallback(() => dropCallback?.Invoke())
            .Append(piece.DOScaleY(piece.localScale.y * 0.6f, 0.25f)).Join(piece.DOScaleX(piece.localScale.x * 1.1f, 0.25f))
            .Join(piece.DOScaleZ(piece.localScale.z * 1.1f, 0.25f)).Append(piece.DOScaleY(piece.localScale.y * 1.2f, 0.2f))
            .Join(piece.DOScaleX(piece.localScale.x * 0.8f, 0.2f)).Join(piece.DOScaleZ(piece.localScale.z * 0.8f, 0.2f))
            .Append(piece.DOScale(new Vector3(1, 1, 1), 0.25f)).OnComplete(() => {
                while (piece.childCount > 0) {
                    piece.GetChild(0).SetParent(_fieldContainer);
                }

                Destroy(piece.gameObject);
            });
    }

    private void SpawnSmokeUnderPiece(Transform piece) {
        SpawnSmokeParticle(piece.transform.position).Forget();
    }

    public virtual void PlacePiece(PieceData pieceData, Vector2Int coord, CellView[,] cells,Transform cellsContainer) { }

    protected virtual void SpawnResourceFx(PieceData pieceData, Vector2Int place, CellView go) { }

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

            TimeSpan timeUntilNext = GetTimeUntilNextHealth();

            if (!_healthTimerText.gameObject.activeSelf)
                _healthTimerText.gameObject.SetActive(true);

            _healthTimerText.text = $"{timeUntilNext.Minutes:D2}:{timeUntilNext.Seconds:D2}";
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
            Debug.Log("maxHP");
        } else {
            Debug.Log("NotmaxHP");
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