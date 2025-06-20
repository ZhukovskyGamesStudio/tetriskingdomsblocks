using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour {
    public bool IsLoaded { get; private set; }
    public static LoadingManager Instance;

    private void Awake() {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        LoadAndChangeScene();
    }

    private async void LoadAndChangeScene() {
        IsLoaded = true;

        await UniTask.Delay(TimeSpan.FromSeconds(1));
        if (StorageManager.IsNewPlayer()) {
            StorageManager.CreateNewSaveData();
            await SceneManager.LoadSceneAsync("GameScene");
        } else {
            StorageManager.LoadGame();
            await SceneManager.LoadSceneAsync("MetaScene");
        }
    }
}