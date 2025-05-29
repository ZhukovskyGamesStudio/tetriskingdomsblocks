using System.IO;
using UnityEngine;

public static class StorageManager {
    private const string SaveKey = "tetrisGame";
    public static GameDataForSave gameDataMain = new GameDataForSave();


    public static void CreateNewSaveData() {
        gameDataMain = new GameDataForSave();
        SaveGame();
    }
    public static void SaveGame() {
        string gameData = JsonUtility.ToJson(null);
        PlayerPrefs.SetString(SaveKey, gameData);
    }

    public static void LoadGame() {
        string json = PlayerPrefs.GetString(SaveKey);
        gameDataMain = JsonUtility.FromJson<GameDataForSave>(json);
     //   Debug.Log(gameDataMain.CurMaxLevel);
    }

    public static bool IsNewPlayer() {
        return !PlayerPrefs.HasKey(SaveKey);
    }
}