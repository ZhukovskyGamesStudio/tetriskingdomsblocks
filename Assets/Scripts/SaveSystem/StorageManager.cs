using System.IO;
using UnityEngine;

public static class StorageManager {
    private const string SaveKey = "tetrisGame";
    public static GameDataForSave GameDataMain = new GameDataForSave();


    public static void CreateNewSaveData(MainMetaConfig mainMetaConfig) {
        GameDataMain = new GameDataForSave();
        MetaFieldManager.CreateLockedMetaField(mainMetaConfig.FieldSize);
        SaveGame();
    }
    public static void SaveGame() {
        string json = JsonUtility.ToJson(GameDataMain);
        PlayerPrefs.SetString(SaveKey, json);
    }

    public static void LoadGame() {
        string json = PlayerPrefs.GetString(SaveKey);
        Debug.Log(json);
      
        GameDataMain = JsonUtility.FromJson<GameDataForSave>(json);
    }

    public static bool HasSavedGame() {
        return PlayerPrefs.HasKey(SaveKey);
    }
}