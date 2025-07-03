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
        LoadAndChangeScene();
    }

    private async void LoadAndChangeScene() {
        IsLoaded = true;

        await UniTask.Delay(TimeSpan.FromSeconds(_fakeWaitSeconds));
        if (StorageManager.IsNewPlayer()) {
            StorageManager.CreateNewSaveData();
            await SceneManager.LoadSceneAsync("GameScene");
        } else {
            StorageManager.LoadGame();
            await SceneManager.LoadSceneAsync("MetaScene");
        }
    }
}