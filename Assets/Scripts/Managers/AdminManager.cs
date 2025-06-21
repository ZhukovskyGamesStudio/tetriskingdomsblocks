using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AdminManager : MonoBehaviour
{
    public static AdminManager Instance { get; private set; }
    public Button LevelButton;
    public Transform LevelButtonsContainer;
    public int LevelsCount;
    public Transform AdminPanelContainer;
    public Toggle AdminToggle;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
        SetupLevelButtons();
    }
    public void ChangeAdminPanelState(bool isOn)
    {
        AdminPanelContainer.gameObject.SetActive(isOn);
    }
    public void AddResources() {
        for (int i = 0; i < 3; i++) {
            StorageManager.GameDataMain.resourcesCount[i] += 1000;
        }
        MetaManager.Instance.UpdateResourcesCountUIText();
    }
    
    public void RestoreAllHPForAdminButton() => StorageManager.GameDataMain.HealthCount = 3;
    public void GenerateNewPiecesForButton()
    {
        if(GameManager.Instance != null)
        GameManager.Instance.GenerateNewPieces();
    } 
    
    public void RestartGame()
    {
        if(GameManager.Instance != null)
            GameManager.Instance.Restart();
    } 

    private void SetupLevelButtons()
    {
        for (int i = 0; i < LevelsCount; i++)
        {
            int needLevel = i;
        var levelButton = Instantiate(LevelButton, LevelButtonsContainer);
            levelButton.onClick.AddListener(() => ChangeLevelToNeeded(needLevel));
            levelButton.GetComponentInChildren<TMP_Text>().text = i.ToString();
        }
    }
    public void ChangeLevelToNeeded(int needLevelNumber)
    {
        Debug.Log(needLevelNumber);
        StorageManager.GameDataMain.CurMaxLevel = needLevelNumber;
        StorageManager.SaveGame();
        SceneManager.LoadScene("GameScene");
    }
}