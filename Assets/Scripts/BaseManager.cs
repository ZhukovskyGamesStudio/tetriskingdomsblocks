using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class BaseManager : MonoBehaviour
{
    public const int CELL_SIZE = 1;
    protected CellType[,] _field;
    protected CellView[,] _cells;
    private Tween _currentTween;
    private DateTime _lastHealthRecoveryTime;
    public float[] FiguresChanceToSpawn { get; protected set; }
    public float _screenRatio { get; protected set; }
    [HideInInspector] public Vector3 CusorToCellOffset;

    [HideInInspector] public List<CellTypeInfo> _currentCellsToSpawn { get; protected set; }
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

    [field: SerializeField] public Transform _markedCell { get; protected set; }

    [SerializeField] protected Camera _mainCamera;

    [SerializeField] protected Transform _fieldContainer;
    public Transform CameraContainer;
    [SerializeField] protected LayerMask _targetMasks;
    [SerializeField] private Transform _fieldStart, _fieldEnd;
    [field: SerializeField] public FigureFormConfig[] FigureFormsConfig { get; protected set; }
    [field: SerializeField] private Transform[] _healthImages;
    [SerializeField] private TMP_Text _healthTimerText;
    [SerializeField] private int _minutesToHealthRecovery;
    [SerializeField] private ParticleSystem _placeCellEffect;
    [SerializeField] private NetworkTimeAPI networkTimeAPI;
    protected bool _hasInternetConnection;
    private float timerNowTimeSecondCounter;
    protected DateTime _currentGameTime;
    private ObjectPool<ParticleSystem> _placeCellEffectsPool;
public const int MAX_HEALTH_COUNT = 3;

protected virtual void Awake()
{
    ChangeToLoading.TryChange();
}

protected virtual void Start()
    {
        _screenRatio = (float)Screen.width / Screen.height;
        CameraContainer.position = new Vector3(CameraContainer.position.x,
            CameraContainer.position.y / (_screenRatio / 0.486f), CameraContainer.position.z);

        networkTimeAPI.GetNetworkTime(dateTime =>
            {
                _currentGameTime = dateTime;
                Debug.Log("has connect" + dateTime);
                _hasInternetConnection = true;
                SetupGame();
            },
            error =>
            {
                _currentGameTime = DateTime.Now;
                Debug.Log("not connect");
                _hasInternetConnection = false;
                SetupGame();
               // _hasInternetConnection = false;
            });
        
        _placeCellEffectsPool =
            new ObjectPool<ParticleSystem>(() => Instantiate(_placeCellEffect));
        Application.targetFrameRate = 144;
    }
    
    private void AddSecondToTimer() =>  _currentGameTime = _currentGameTime.AddSeconds(1);

    protected virtual void Update()
    {
        if (_hasInternetConnection)
        {
            timerNowTimeSecondCounter += Time.unscaledDeltaTime;
            if (timerNowTimeSecondCounter >= 1)
            {
                timerNowTimeSecondCounter--;
                AddSecondToTimer();
            }
            if (StorageManager.GameDataMain.HealthCount < MAX_HEALTH_COUNT)
            {
                TimeSpan timeSinceLastUpdate = _currentGameTime - _lastHealthRecoveryTime;
                int energyToAdd = (int)(timeSinceLastUpdate.TotalMinutes / _minutesToHealthRecovery);

                if (energyToAdd > 0)
                {
                    StorageManager.GameDataMain.HealthCount =
                        Mathf.Min(StorageManager.GameDataMain.HealthCount + energyToAdd, MAX_HEALTH_COUNT);
                    _lastHealthRecoveryTime = _currentGameTime;
                    StorageManager.GameDataMain.LastHealthRecoveryTime = DateForSaveData.FromDateTime(_currentGameTime);
                    SaveEnergyData();
                    _healthImages[StorageManager.GameDataMain.HealthCount-1].gameObject.SetActive(true);
                }

                UpdateTimerUI();
            }
        }
        else if (_healthTimerText.gameObject.activeSelf)
        {
            _healthTimerText.gameObject.SetActive(false);
        }
    }

    protected void CalculateFiguresSpawnChances()
    {
        float lastChance = 0;
        FiguresChanceToSpawn = new float[FigureFormsConfig.Length];
        for (int i = 0; i < FigureFormsConfig.Length; i++)
        {
            lastChance += FigureFormsConfig[i].Cost;
            FiguresChanceToSpawn[i] = lastChance;
        }
    }
    public bool CanPlace(PieceData data)
    {
        Vector2Int pos = GetPieceClampedPosOnField();
        return CanPlace(data, pos);
    }

    public Vector2Int GetPosOnField()
    {
        Vector3 coord = GetCoord() + CusorToCellOffset;

        if (PieceView.PieceMaxSize.x % 2 == 0)
            coord += Vector3.left / 2f;

        if (PieceView.PieceMaxSize.y % 2 == 0)
            coord += Vector3.back / 2f;

        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(coord.x) / CELL_SIZE, Mathf.RoundToInt(coord.z) / CELL_SIZE);
        return pos;
    }

    private Vector3 GetCoord()
    {
        return Input.touchCount == 0 ? ScreenToWorldPoint : TouchToWorldPoint;
    }

    public Vector2Int GetPieceClampedPosOnField()
    {
        Vector3 coord = GetCoord() + CusorToCellOffset;
        coord += new Vector3(PieceView.DragShift.x, 0, PieceView.DragShift.z) + Vector3.forward;

        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(coord.x) / CELL_SIZE, Mathf.RoundToInt(coord.z) / CELL_SIZE);
        pos -= new Vector2Int((int)_fieldStart.position.x, (int)_fieldStart.position.z);
        return pos;
    }

    public bool CanPlace(PieceData data, Vector2Int pos)
    {
        if (pos.x < 0 || pos.y < 0)
            return false;

        if (pos.x + data.Cells.GetLength(0) - 1 >= _field.GetLength(0))
            return false;

        if (pos.y + data.Cells.GetLength(1) - 1 >= _field.GetLength(1))
            return false;

        for (int x = 0; x < data.Cells.GetLength(0); x++)
        {
            for (int y = 0; y < data.Cells.GetLength(1); y++)
            {
                if (data.Cells[x, y] && _field[pos.x + x, pos.y + y] != CellType.Empty)
                    return false;
            }
        }

        return true;
    }

    protected virtual void PlacePiece(PieceData pieceData, Vector2Int pos, int fieldSize)
    {
        for (int x = 0; x < pieceData.Cells.GetLength(0); x++)
        {
            for (int y = 0; y < pieceData.Cells.GetLength(1); y++)
            {
                if (!pieceData.Cells[x, y])
                {
                    continue;
                }

                var place = new Vector2Int((int)Mathf.Clamp(pos.x + x, 0, fieldSize),
                    (int)Mathf.Clamp(pos.y + y, 0, fieldSize));
                var go = Instantiate(pieceData.Type.CellPrefab, _fieldContainer);
                go.transform.localPosition = new Vector3(place.x, -0.45f, place.y);
                _field[place.x, place.y] = pieceData.Type.CellType;
                _cells[place.x, place.y] = go;
                go.GetComponent<CellView>().PlaceCellOnField();
                SpawnResourceFx(pieceData, place, go);
                StartCoroutine(SpawnSmokeParticle(go.transform.position)) ;
            }
        }

        ShakeCamera();
    }
    
    public virtual void PlacePiece(PieceData pieceData)
    {
    }

    protected virtual void SpawnResourceFx(PieceData pieceData, Vector2Int place, CellView go)
    {
    }
    
    protected void ShakeCamera()
    {
        Vector3 camPos = CameraContainer.transform.position;
        float xOffset = camPos.x * Random.Range(-0.03f, 0.03f);
        float zOffset = camPos.z * Random.Range(-0.03f, 0.03f);
        _currentTween.Kill();
        _currentTween = DOTween.Sequence()
            .Append(CameraContainer.transform.DOMoveX(camPos.x - xOffset, 0.1f))
            .Join(CameraContainer.transform.DOMoveY(camPos.y * Random.Range(1.01f, 1.03f), 0.2f))
            .Join(CameraContainer.transform.DOMoveZ(camPos.z - zOffset, 0.1f))
            .Append(CameraContainer.transform.DOMoveX(camPos.x + xOffset, 0.1f))
            .Join(CameraContainer.transform.DOMoveZ(camPos.z + zOffset, 0.1f))
            .Append(CameraContainer.transform.DOMove(camPos, 0.1f)); 
    }

    public TimeSpan GetTimeUntilNextHealth()
    {
        if (StorageManager.GameDataMain.HealthCount >= MAX_HEALTH_COUNT) return TimeSpan.Zero;
        
        TimeSpan timeSinceLastUpdate = _currentGameTime - _lastHealthRecoveryTime;
        double minutesPassed = timeSinceLastUpdate.TotalMinutes;
        double minutesUntilNext = _minutesToHealthRecovery - (minutesPassed % _minutesToHealthRecovery);
        
        return TimeSpan.FromMinutes(minutesUntilNext);
    }

    private void UpdateTimerUI()
    {
        if (_hasInternetConnection)
        {
            if (StorageManager.GameDataMain.HealthCount >= MAX_HEALTH_COUNT)
            {
                if (_healthTimerText != null && _healthTimerText.gameObject.activeSelf)
                    _healthTimerText.gameObject.SetActive(false);
                return;
            }

            TimeSpan timeUntilNext = GetTimeUntilNextHealth();

            if (!_healthTimerText.gameObject.activeSelf)
                _healthTimerText.gameObject.SetActive(true);

            _healthTimerText.text = $"{timeUntilNext.Minutes:D2}:{timeUntilNext.Seconds:D2}";
        }
        else
        {
            _healthTimerText.text = "No internet connection";
        }
    }
    
    private void OnApplicationQuit()
    {
        SaveEnergyData();
    }

    private void SaveEnergyData()
    {
        StorageManager.SaveGame();
    }
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveEnergyData();
        }
        else
        {
            StorageManager.LoadGame();
            CalculateOfflineHealth();
        }
    }

    protected void RemoveHealth()
    {
        if (StorageManager.GameDataMain.HealthCount == MAX_HEALTH_COUNT)
        {
            _lastHealthRecoveryTime = _currentGameTime;
            StorageManager.GameDataMain.LastHealthRecoveryTime =
                DateForSaveData.FromDateTime(_lastHealthRecoveryTime);
        }

        _healthImages[StorageManager.GameDataMain.HealthCount-1].gameObject.SetActive(false);
        StorageManager.GameDataMain.HealthCount--;
        SaveEnergyData();
    }

    private void CalculateOfflineHealth()
    {
        if (!_hasInternetConnection)return;
        _lastHealthRecoveryTime = StorageManager.GameDataMain.LastHealthRecoveryTime.ToDateTime();
        TimeSpan offlineTime = _currentGameTime - _lastHealthRecoveryTime;
        int healthToAdd = (int)(offlineTime.TotalMinutes / _minutesToHealthRecovery);


        if (healthToAdd > 0)
        {
            StorageManager.GameDataMain.HealthCount =
                Mathf.Min(StorageManager.GameDataMain.HealthCount + healthToAdd, MAX_HEALTH_COUNT);
        }

        if (StorageManager.GameDataMain.HealthCount != MAX_HEALTH_COUNT)
            _lastHealthRecoveryTime.AddMinutes(healthToAdd * _minutesToHealthRecovery);
    }

    private IEnumerator SpawnSmokeParticle(Vector3 pos)
    {
        var particles = _placeCellEffectsPool.Get();
        particles.gameObject.SetActive(true);
        particles.transform.position = new Vector3(pos.x,pos.y-0.1f,pos.z); 
        particles.Play();
        yield return new WaitForSeconds(2f);
        ReleaseParticles(particles);
    }

    private void ReleaseParticles(ParticleSystem particles)
    {
        particles.gameObject.SetActive(false);
        _placeCellEffectsPool.Release(particles);
    }
    protected virtual void SetupGame()
    {
        if (StorageManager.GameDataMain.HealthCount > MAX_HEALTH_COUNT)
            StorageManager.GameDataMain.HealthCount = MAX_HEALTH_COUNT;
        
        if (StorageManager.GameDataMain.HealthCount == MAX_HEALTH_COUNT)
        {
            _healthTimerText.gameObject.SetActive(false);
            Debug.Log("maxHP");
        }
        else
        {
            Debug.Log("NotmaxHP");
            CalculateOfflineHealth();
            if (_hasInternetConnection)
                _healthTimerText.text = StorageManager.GameDataMain.LastHealthRecoveryTime.ToString();
            else
                _healthTimerText.text = "No internet connection";
            for (int i = 0; i < MAX_HEALTH_COUNT; i++)
            {
                _healthImages[i].gameObject.SetActive(StorageManager.GameDataMain.HealthCount > i);
            }
        }
    }
}