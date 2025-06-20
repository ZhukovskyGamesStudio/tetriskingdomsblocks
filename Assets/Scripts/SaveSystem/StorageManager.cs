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
        Debug.Log(json);
        PlayerPrefs.SetString(SaveKey, json);
    }

    public static void LoadGame() {
        string json = PlayerPrefs.GetString(SaveKey);
        GameDataMain = JsonUtility.FromJson<GameDataForSave>(json);
    }

    public static bool IsNewPlayer() {
        return !PlayerPrefs.HasKey(SaveKey);
    }
}