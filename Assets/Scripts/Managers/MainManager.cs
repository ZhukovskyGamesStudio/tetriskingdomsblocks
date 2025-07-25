using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainManager : MonoBehaviour {
    public static MainManager Instance;

    [field: SerializeField]
    public MainManagerConfig _mainConfig { get; private set; }

    public DateTime _currentGameTime { get; private set; }
    public bool _hasInternetConnection{ get; private set; }

    private const int MAX_HEALTH_COUNT = 3;
    private const float MINUTES_TO_HEALTH_RECOVERY = 5;
    private DateTime _lastHealthRecoveryTime;
 
    private float timerNowTimeSecondCounter;

    public LevelConfig CurrentLevelConfig =>
        _mainConfig.Levels[Math.Min(_mainConfig.Levels.Length - 1, StorageManager.GameDataMain.CurMaxLevel)];

    private NetworkTimeAPI _networkTimeAPI;

    private void Awake() {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    protected virtual void Start() {
        //_currentGameTime = DateTime.Now;
        _networkTimeAPI = new NetworkTimeAPI();
        _networkTimeAPI.GetNetworkTime(dateTime => {
            _currentGameTime = dateTime;
          
            _hasInternetConnection = true;
        }, error => {
            _currentGameTime = DateTime.Now;
            _hasInternetConnection = false;
            //SetupGame();
            // _hasInternetConnection = false;
        });
        
        //  _placeCellEffectsPool = new ObjectPool<ParticleSystem>(() => Instantiate(_placeCellEffect));
        Application.targetFrameRate = 144;
    }

    private void Update() {
        UpdateTimerAndHealth();
    }

    public void SetupGetPieceTimer() {
        Debug.Log(StorageManager.GameDataMain.LastGetPieceTime + "LastGetPieceTimeBefore" + _currentGameTime);
        if (StorageManager.GameDataMain.LastGetPieceTime == DateTime.MinValue.ToString(CultureInfo.InvariantCulture)) {
            StorageManager.GameDataMain.LastGetPieceTime = (_currentGameTime + TimeSpan.FromHours(8)).ToString(CultureInfo.InvariantCulture);
            StorageManager.GameDataMain.LastExitTime = _currentGameTime.ToString(CultureInfo.InvariantCulture);
            StorageManager.SaveGame();
            Debug.Log("First time save");
        }

        Debug.Log(StorageManager.GameDataMain.LastGetPieceTime + "LastGetPieceTime");
    }

    public void SetupHealth() {
        Debug.Log(_currentGameTime);
        if (StorageManager.GameDataMain.HealthCount > MAX_HEALTH_COUNT)
            StorageManager.GameDataMain.HealthCount = MAX_HEALTH_COUNT;

        if (StorageManager.GameDataMain.HealthCount == MAX_HEALTH_COUNT) {
            MetaUI.Instance. HealthView.SetHealthTimerActive(false);
        } else {
            CalculateOfflineHealth();
            if (_hasInternetConnection)
                MetaUI.Instance.HealthView.SetHealthTimerText(StorageManager.GameDataMain.LastHealthRecoveryTime);
            else
                MetaUI.Instance.HealthView.SetHealthTimerText("No internet connection");
            for (int i = 0; i < MAX_HEALTH_COUNT; i++) {
                MetaUI.Instance.HealthView.SetHealthImageActive(i, StorageManager.GameDataMain.HealthCount > i);
            }
        }
    }

    private void CalculateOfflineHealth() {
        if (!_hasInternetConnection) return;
        _lastHealthRecoveryTime = StorageManager.GameDataMain.LastHealthRecoveryTimeDateTime;
        TimeSpan offlineTime = _currentGameTime - _lastHealthRecoveryTime;
        int healthToAdd = (int)(offlineTime.TotalMinutes / MINUTES_TO_HEALTH_RECOVERY);

        if (healthToAdd > 0) {
            StorageManager.GameDataMain.HealthCount = Mathf.Min(StorageManager.GameDataMain.HealthCount + healthToAdd, MAX_HEALTH_COUNT);
        }

        if (StorageManager.GameDataMain.HealthCount != MAX_HEALTH_COUNT)
            _lastHealthRecoveryTime.AddMinutes(healthToAdd * MINUTES_TO_HEALTH_RECOVERY);
    }

   

    private void UpdateTimerAndHealth() {
        if (MetaUI.Instance == null && !_hasInternetConnection) return;
        if (_hasInternetConnection) {
            timerNowTimeSecondCounter += Time.unscaledDeltaTime;
            if (timerNowTimeSecondCounter >= 1) {
                timerNowTimeSecondCounter--;
                AddSecondToTimer();
            }

            if (StorageManager.GameDataMain.HealthCount < MAX_HEALTH_COUNT) {
                TimeSpan timeSinceLastUpdate = _currentGameTime - _lastHealthRecoveryTime;
                int energyToAdd = (int)(timeSinceLastUpdate.TotalMinutes / MINUTES_TO_HEALTH_RECOVERY);

                if (energyToAdd > 0) {
                    StorageManager.GameDataMain.HealthCount =
                        Mathf.Min(StorageManager.GameDataMain.HealthCount + energyToAdd, MAX_HEALTH_COUNT);
                    _lastHealthRecoveryTime = _currentGameTime;
                    StorageManager.GameDataMain.LastHealthRecoveryTime = _currentGameTime.ToString(CultureInfo.InvariantCulture);
                    StorageManager.SaveGame();
                    MetaUI.Instance.HealthView.SetHealthImageActive(StorageManager.GameDataMain.HealthCount - 1, true);
                }

                UpdateHealthTimerUI();
            }
        } else if (MetaUI.Instance.HealthView.HealthTimerText.gameObject.activeSelf) {
            MetaUI.Instance.HealthView.SetHealthTimerActive(false);
        }
    }

    private void AddSecondToTimer() => _currentGameTime = _currentGameTime.AddSeconds(1);

    private void UpdateHealthTimerUI() {
        if (_hasInternetConnection) {
            if (StorageManager.GameDataMain.HealthCount >= MAX_HEALTH_COUNT) {
                MetaUI.Instance.HealthView.SetHealthTimerActive(false);
                return;
            }

            MetaUI.Instance.HealthView.UpdateHealthTimerUI(GetTimeUntilNextHealth());
        } else {
            MetaUI.Instance.HealthView.SetNoConnection();
        }
    }

    private void OnApplicationPause(bool pauseStatus) {
        if (pauseStatus) {
            StorageManager.SaveGame();
        } else {
            StorageManager.LoadGame();
            CalculateOfflineHealth();
        }
    }

    private TimeSpan GetTimeUntilNextHealth() {
        if (StorageManager.GameDataMain.HealthCount >= MAX_HEALTH_COUNT) return TimeSpan.Zero;

        TimeSpan timeSinceLastUpdate = _currentGameTime - _lastHealthRecoveryTime;
        double minutesPassed = timeSinceLastUpdate.TotalMinutes;
        double minutesUntilNext = MINUTES_TO_HEALTH_RECOVERY - (minutesPassed % MINUTES_TO_HEALTH_RECOVERY);

        return TimeSpan.FromMinutes(minutesUntilNext);
    }

    public void RemoveHealthAfterLose() {
        StorageManager.GameDataMain.LastHealthRecoveryTime = _currentGameTime.ToString(CultureInfo.InvariantCulture);
        StorageManager.GameDataMain.HealthCount--;
    }

    public void Restart() {
        if (StorageManager.GameDataMain.HealthCount != 0)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        else {
            //floating window with "watch ad and get health"
        }
    }

    public void GoToMeta() {
        if (StorageManager.GameDataMain.CurMaxLevel > 2)
            SceneManager.LoadScene("MetaScene");
        else if (StorageManager.GameDataMain.HealthCount > 0)
            SceneManager.LoadScene("GameScene");
    }

    public void IncreaseMaxLevel() {
        StorageManager.GameDataMain.CurMaxLevel++;
        if (StorageManager.GameDataMain.CurMaxLevel >= _mainConfig.Levels.Length) {
            StorageManager.GameDataMain.CurMaxLevel = 0;
        }
    }
}