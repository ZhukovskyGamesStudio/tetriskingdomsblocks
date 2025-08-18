using System;
using System.Collections.Generic;
using System.Globalization;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainManager : MonoBehaviour {
    public static MainManager Instance;

    [field: SerializeField]
    public MainManagerConfig _mainConfig { get; private set; }

    public DateTime _currentGameTime { get; private set; }
    public bool _hasInternetConnection { get; private set; }

    private const int MAX_HEALTH_COUNT = 5;
    private const float MINUTES_TO_HEALTH_RECOVERY = 5;
    private float timerNowTimeSecondCounter;

    private int _currentRewardedCubes;
    private int _currentRewardedCoins;

    public LevelConfig CurrentLevelConfig =>
        _mainConfig.Levels[Math.Min(_mainConfig.Levels.Length - 1, StorageManager.GameDataMain.CurMaxLevel)];

    private NetworkTimeAPI _networkTimeAPI;

    private void Awake() {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SetupTime();

        Application.targetFrameRate = 144;
    }

    private void SetupTime() {
        _currentGameTime = DateTime.Now;
        _hasInternetConnection = true;
        return;
        _networkTimeAPI = new NetworkTimeAPI();
        _networkTimeAPI.GetNetworkTime(dateTime => {
            _currentGameTime = dateTime;
            _hasInternetConnection = true;
        }, error => {
            _currentGameTime = DateTime.Now;
            _hasInternetConnection = false;
        });
    }

    private void Update() {
        UpdateTimerAndHealth();
    }

    public (int cubes, int coins) GetRewardToMeta() {
        int cubes = _currentRewardedCubes;
        int coins = _currentRewardedCoins;
        _currentRewardedCoins = 0;
        _currentRewardedCubes = 0;
        return (cubes, coins);
    }

    public void AddRewardToMeta() {
        _currentRewardedCubes += CurrentLevelConfig.MagicCubesCount;
        _currentRewardedCoins += CurrentLevelConfig.GoldAmount;
    }

    public void SetupGetPieceTimer() {
        if (StorageManager.GameDataMain.LastGetPieceTime == DateTime.MinValue.ToString(CultureInfo.InvariantCulture)) {
            StorageManager.GameDataMain.LastGetPieceTime = _currentGameTime.ToString(CultureInfo.InvariantCulture);
            StorageManager.GameDataMain.LastExitTime = _currentGameTime.ToString(CultureInfo.InvariantCulture);
            StorageManager.SaveGame();
        }
    }

    public void SetupHealth() {
        if (StorageManager.GameDataMain.HealthCount > MAX_HEALTH_COUNT)
            StorageManager.GameDataMain.HealthCount = MAX_HEALTH_COUNT;

        if (StorageManager.GameDataMain.HealthCount == MAX_HEALTH_COUNT) {
            MetaUI.Instance.HealthView.SetHealthTimerActive(false);
        } else {
            CalculateOfflineHealth();
            if (_hasInternetConnection)
                MetaUI.Instance.HealthView.SetHealthTimerText(StorageManager.GameDataMain.LastHealthRecoveryTime);
            else
                MetaUI.Instance.HealthView.SetHealthTimerText("No internet connection");
        }

        MetaUI.Instance.HealthView.SetHealthCountText(StorageManager.GameDataMain.HealthCount);
    }

    private void CalculateOfflineHealth() {
        if (!_hasInternetConnection) return;
        TimeSpan offlineTime = _currentGameTime - StorageManager.GameDataMain.LastHealthRecoveryTimeDateTime;
        int healthToAdd = (int)(offlineTime.TotalMinutes / MINUTES_TO_HEALTH_RECOVERY);

        if (healthToAdd > 0) {
            StorageManager.GameDataMain.HealthCount = Mathf.Min(StorageManager.GameDataMain.HealthCount + healthToAdd, MAX_HEALTH_COUNT);
        }
        

        if (StorageManager.GameDataMain.HealthCount != MAX_HEALTH_COUNT)
            StorageManager.GameDataMain.LastHealthRecoveryTime = StorageManager.GameDataMain.LastHealthRecoveryTimeDateTime
                .AddMinutes(healthToAdd * MINUTES_TO_HEALTH_RECOVERY).ToString(CultureInfo.InvariantCulture);
    }

    public void BuyMetaResource(ResourceOfferData data) {
      
        MetaTabsPanel.Instance.OpenRule();
        ShowOfferRewardDialog(new SerializedDictionary<ResourceType, int>() {
            {data.Resource, data.ResourceCount}
        });
    }

    public void BuyBundleOffer(SpecialOfferData data) {
    
        MetaTabsPanel.Instance.OpenRule();
        ShowOfferRewardDialog(data.Resources);
    }

    public void BuyPiece(int cost) {
        if (StorageManager.GameDataMain.GetResource(ResourceType.Coins) >= cost) {
            StorageManager.GameDataMain.AddResource(ResourceType.Coins, -cost);
            MetaTabsPanel.Instance.OpenRule();
            MetaFieldManager.Instance.GenerateAndOpenLootbox();
        }
    }
    
    private void ShowOfferRewardDialog(SerializedDictionary<ResourceType,int> rewards) {
        Vector2 startPosition = Vector2.zero;
        if(MetaUI.Instance != null)
            startPosition = MetaUI.Instance._mainCanvas.transform.position;
        var dialog = new DialogWithData {
            DialogType = typeof(OfferRewardDialog),
            Data = new OfferRewardDialog.Data {
                ClickDefaultClaim = ()=>ClaimOfferRewards(rewards,startPosition),
                OfflineResources = rewards
            }
        };

        DialogsManager.Instance.ShowDialogWithData(dialog);
    }

    private void ClaimOfferRewards(SerializedDictionary<ResourceType, int> rewards, Vector2 startPosition) {
        foreach (var kvp in rewards) {
            Debug.Log(kvp.Key + " "+ kvp.Value);
            switch (kvp.Key) {
                case ResourceType.Coins:
                    if (MetaFieldManager.Instance != null) {
                        FloatingResourcesManager.Instance.FromPointToPointAnimation(kvp.Value, ResourceType.Coins, startPosition,
                            MetaUI.Instance.CountersPanelView.GetCoinsIconPosition, AddShopItemsInInventory,
                            StorageManager.GameDataMain.GetResource(kvp.Key), false, true, false, false);
                    } else
                        StorageManager.GameDataMain.AddResource(ResourceType.Coins, kvp.Value);

                    break;
                case ResourceType.Health:
                    StorageManager.GameDataMain.HealthCount += kvp.Value;
                    break;
                case ResourceType.ShuffleBooster:
                    AddBoostersToInventory(startPosition, kvp);
                    break;
                case ResourceType.MagicCube:
                    StorageManager.GameDataMain.AddResource(ResourceType.MagicCube, kvp.Value);
                    break;
                case ResourceType.HammerBooster:
                    AddBoostersToInventory(startPosition, kvp);
                    break;
                case ResourceType.BombBooster:
                    AddBoostersToInventory(startPosition, kvp);
                    break;
                case ResourceType.RotateBooster:
                    AddBoostersToInventory(startPosition, kvp);
                    break;
                case ResourceType.Lootbox:
                    for (int i = 0; i < kvp.Value; i++) {
                        MetaFieldManager.Instance.GenerateAndOpenLootbox();
                    }

                    break;
            }
        }
        
        StorageManager.SaveGame();
    }

    private void AddBoostersToInventory(Vector2 startPosition, KeyValuePair<ResourceType, int> kvp) {
        if (MetaFieldManager.Instance != null) {
            FloatingResourcesManager.Instance.FromPointToPointAnimation(kvp.Value, kvp.Key, startPosition,
                MetaUI.Instance._playButton.transform.position, AddShopItemsInInventory,
                StorageManager.GameDataMain.GetResource(kvp.Key), false, true, false, false);
        } else
            StorageManager.GameDataMain.AddResource(kvp.Key, kvp.Value);
    }

    private void AddShopItemsInInventory(ResourceType resourceType, float needCount) {
        StorageManager.GameDataMain.AddResource(resourceType, needCount);
        MetaUI.Instance.CountersPanelView.SetResourceCount(resourceType, StorageManager.GameDataMain.GetResource(resourceType));
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
                TimeSpan timeSinceLastUpdate = _currentGameTime - StorageManager.GameDataMain.LastHealthRecoveryTimeDateTime;
                int energyToAdd = (int)(timeSinceLastUpdate.TotalMinutes / MINUTES_TO_HEALTH_RECOVERY);

                if (energyToAdd > 0) {
                    StorageManager.GameDataMain.HealthCount =
                        Mathf.Min(StorageManager.GameDataMain.HealthCount + energyToAdd, MAX_HEALTH_COUNT);
                    StorageManager.GameDataMain.LastHealthRecoveryTime = _currentGameTime.ToString(CultureInfo.InvariantCulture);
                    StorageManager.SaveGame();
                    if (MetaUI.Instance?.HealthView != null) {
                        MetaUI.Instance.HealthView.SetHealthCountText(StorageManager.GameDataMain.HealthCount);
                    }
                }

                if (MetaUI.Instance?.HealthView != null) {
                    UpdateHealthTimerUI();
                }
            }
        } else if (MetaUI.Instance.HealthView.HealthTimerText.gameObject.activeSelf) {
            if (MetaUI.Instance?.HealthView != null) {
                MetaUI.Instance.HealthView.SetHealthTimerActive(false);
            }
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

        TimeSpan timeSinceLastUpdate = _currentGameTime - StorageManager.GameDataMain.LastHealthRecoveryTimeDateTime;
        double minutesPassed = timeSinceLastUpdate.TotalMinutes;
        double minutesUntilNext = MINUTES_TO_HEALTH_RECOVERY - (minutesPassed % MINUTES_TO_HEALTH_RECOVERY);

        return TimeSpan.FromMinutes(minutesUntilNext);
    }

    public void ChangeHealthCount(int healthCount) {
        StorageManager.GameDataMain.HealthCount = healthCount;
        if (MetaUI.Instance != null) MetaUI.Instance.HealthView.SetHealthCountText(StorageManager.GameDataMain.HealthCount);
    }

    public void RemoveHealthAfterLose() {
        if (StorageManager.GameDataMain.HealthCount == MAX_HEALTH_COUNT)
            StorageManager.GameDataMain.LastHealthRecoveryTime = _currentGameTime.ToString(CultureInfo.InvariantCulture);
        StorageManager.GameDataMain.HealthCount--;
        if (MetaUI.Instance != null) MetaUI.Instance.HealthView.SetHealthCountText(StorageManager.GameDataMain.HealthCount);
        Debug.Log(StorageManager.GameDataMain.HealthCount);
        StorageManager.SaveGame();
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