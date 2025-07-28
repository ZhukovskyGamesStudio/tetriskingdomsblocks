using System.IO;
using UnityEngine;

public static class StorageManager {
    private const string SaveKey = "tetrisGame";
    public static GameDataForSave GameDataMain = new GameDataForSave();


    public static void CreateNewSaveData() {
        GameDataMain = new GameDataForSave();
        SaveGame();
    }
    public static void SaveGame() {
        string json = JsonUtility.ToJson(GameDataMain);
        PlayerPrefs.SetString(SaveKey, json);
      
    }

    public static void LoadGame() {
        string json = PlayerPrefs.GetString(SaveKey);
        GameDataMain = JsonUtility.FromJson<GameDataForSave>(json);
        
        
        //Удаляет сохранение при обновлении игры, удалить после тестов
        if (GameDataMain.CreatedVersion != Application.version) {
            CreateNewSaveData();
        }
    }

    public static bool IsNewPlayer() {
        return !PlayerPrefs.HasKey(SaveKey);
    }
    
    public static bool IsTutorialCompleted() {
        return GameDataMain is { CurMaxLevel: >= 1 };
    }
}