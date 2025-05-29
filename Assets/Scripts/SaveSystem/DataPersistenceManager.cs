using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DataPersistenceManager : MonoBehaviour
{
    private GameDataForSave gameData;
    private string fileName = "tetrisGame";
    private bool useEncryption = true;
    private FileDataHandler dataHandler;

    public static DataPersistenceManager Instance;
    public void Start()
    {
        Instance = this;
       
        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, useEncryption);
        
        LoadGame();
    }

    public void StartNewGame()
    {
            this.gameData = null;
            dataHandler.Save(this.gameData);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NewGame()
    {
        this.gameData = new GameDataForSave();
    }
    public void LoadGame()
    {
        this.gameData = dataHandler.Load();
       // this.gameData = null;
        if (this.gameData == null)
        {
            NewGame();
        }

        GameManager.Instance.currentLevel = gameData.curMaxLevel;
        GameManager.Instance.Reset();
    }
    
    public void SaveGame()
    {
        Debug.Log("Game saved");

        gameData.curMaxLevel = GameManager.Instance.currentLevel;


        dataHandler.Save(this.gameData);
    }


}
