using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour {
    public bool IsLoaded { get; private set; }
    public static LoadingManager Instance;

    [SerializeField]
    private float _fakeWaitSeconds = 0.1f;

    private void Awake() {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        bool isNewGame = !StorageManager.IsTutorialCompleted();
        InitManagers();
        LoadAndChangeScene(isNewGame);
    }

    private void InitManagers() {
        if (!StorageManager.IsTutorialCompleted()) {
            StorageManager.CreateNewSaveData();
        }
        BackgroundMusicManager.Instance.PlayEndlessMusic().Forget();
        SettingsManager.Instance.SetSettings();
    }

    private async void LoadAndChangeScene(bool isNewGame) {
        IsLoaded = true;

        await UniTask.Delay(TimeSpan.FromSeconds(_fakeWaitSeconds));
       /* if (isNewGame) {
            await SceneManager.LoadSceneAsync("MetaScene");
            return;
        }*/

        StorageManager.LoadGame();
        if (StorageManager.GameDataMain.CurMaxLevel >= 3) {
            await SceneManager.LoadSceneAsync("MetaScene");
        } else {
            await SceneManager.LoadSceneAsync("GameScene");
        }
    }
}