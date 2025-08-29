using System;
using System.Collections.Generic;
using System.Linq;
using Abstract;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZhukovskyGamesPlugin;

public class LoadingManager : MonoBehaviour {
    public bool FirstLoad { get; set; }
    public bool IsLoaded { get; private set; }
    public static LoadingManager Instance;

    [SerializeField]
    private float _fakeWaitSeconds = 0.1f;

    [SerializeField]
    private MainMetaConfig _mainMetaConfig;

    private void Awake() {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        LoadingAsync().Forget();
    }

    private async UniTask LoadingAsync() {
        InitManagers();
        await LoadManagers();
        SendFirstLaunchEvent();
        await LoadAndChangeScene();
    }

    private void InitManagers() {
        if (!StorageManager.HasSavedGame()) {
            StorageManager.CreateNewSaveData(_mainMetaConfig);
        } else {
            StorageManager.LoadGame();
            if (!StorageManager.GameDataMain.IsTutorialCompleted) {
                StorageManager.CreateNewSaveData(_mainMetaConfig);
            }
        }

        StorageManager.GameDataMain.IsWonInThisSession = false;
        BackgroundMusicManager.Instance.StopAndPlayEndlessMusic().Forget();
        SettingsManager.Instance.SetSettings();
    }

    private async UniTask LoadManagers() {
        CustomMonoBehaviour[] preloadedManagers = FindObjectsByType<CustomMonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .OrderBy(m => m.InitPriority).ToArray();

        foreach (CustomMonoBehaviour manager in preloadedManagers) {
            if (manager is IPreloadable preloadable) {
                preloadable.Init();
            }
        }

        await UniTask.WaitUntil(() => ZhukovskyAdsManager.Instance.AdsProvider.IsAdsReady());
    }

    private static void SendFirstLaunchEvent() {
        ZhukovskyAnalyticsManager.Instance.SendCustomEvent("technical", new Dictionary<string, object> {
            { "step_name", "01_gameLaunch" },
            { "first_start", StorageManager.GameDataMain.FirstLaunch }
        }, true);
        StorageManager.GameDataMain.FirstLaunch = false;
        StorageManager.SaveGame();
    }

    private async UniTask LoadAndChangeScene() {
        IsLoaded = true;

        await UniTask.Delay(TimeSpan.FromSeconds(_fakeWaitSeconds));

        if (StorageManager.GameDataMain.CurMaxLevel >= 3) {
            FirstLoad = true;
            await SceneManager.LoadSceneAsync("MetaScene");
        } else {
            await SceneManager.LoadSceneAsync("GameScene");
        }
    }
}