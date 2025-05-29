using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour {
    public static bool IsLoaded { get; private set; }

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