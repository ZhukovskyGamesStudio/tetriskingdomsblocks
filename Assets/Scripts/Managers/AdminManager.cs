using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AdminManager : MonoBehaviour {
    public static AdminManager Instance { get; private set; }
    public Button LevelButton;
    public Transform LevelButtonsContainer;
    public int LevelsCount;
    public Transform AdminPanelContainer;
    public Toggle AdminToggle;

    [SerializeField]
    private Toggle _infiniteHpToggle;

    public static bool IsInfiniteHealth = true;

    [SerializeField]
    private Toggle _infiniteBoostersToggle;

    public bool IsInfiniteBoosters = true;  

    private void Awake() {
        Instance = this;
        _infiniteHpToggle.SetIsOnWithoutNotify(IsInfiniteHealth);
        _infiniteBoostersToggle.SetIsOnWithoutNotify(IsInfiniteBoosters);
        DontDestroyOnLoad(this);
        SetupLevelButtons();
    }

    public void ResetFreeCellTime() {
        StorageManager.GameDataMain.LastGetPieceTime = DateTime.MinValue.ToString();
    }

    public void ChangeAdminPanelState(bool isOn) {
        AdminPanelContainer.gameObject.SetActive(isOn);
    }

    public void AddResources() {
        
        for (int i = 0; i <  StorageManager.GameDataMain.ResourcesCount.Length; i++) {
            StorageManager.GameDataMain.ResourcesCount[i] += 100;
        }

        StorageManager.GameDataMain.MagicCubesAmount += 20;
        StorageManager.GameDataMain.GoldAmount += 100;
        if(MetaUI.Instance != null)
            MetaUI.Instance.CountersPanelView.SetMagicCubes( StorageManager.GameDataMain.MagicCubesAmount );
        
        MetaFieldManager.Instance.UpdateResourcesCountUIText();
    }

    public void RestoreAllHPForAdminButton() {
        StorageManager.GameDataMain.HealthCount = 5;
        if(MetaUI.Instance == null )return;
        MetaUI.Instance.HealthView.SetHealthCountText(5);
    }

    public void RemoveOneHealthAdminButton() {  
     
        if(StorageManager.GameDataMain.HealthCount <= 0)return;
         MainManager.Instance.RemoveHealthAfterLose();
    }

    public void GenerateNewPiecesForButton() {
        if (GameFieldManager.Instance != null)
            GameFieldManager.Instance.GenerateNewPieces();
    }

    public void AddBoosters()
    {
        StorageManager.GameDataMain.RandomFieldCount+=5;
        StorageManager.GameDataMain.HummerCount+=5;
        StorageManager.GameDataMain.RotatePieceCount+=5;
        StorageManager.GameDataMain.DynamiteCount+=5;
        
        StorageManager.GameDataMain.MetaHummerCount+=5;
        
        if(BoostersManager.Instance != null) {
            GameUI.Instance.GameBoostersButtons.UpdateCounters(StorageManager.GameDataMain);
        }
    }

    public void RestartGame() {
        if (GameFieldManager.Instance != null)
            MainManager.Instance.Restart();
    }

    private void SetupLevelButtons() {
        for (int i = 0; i < LevelsCount; i++) {
            int needLevel = i;
            var levelButton = Instantiate(LevelButton, LevelButtonsContainer);
            levelButton.onClick.AddListener(() => ChangeLevelToNeeded(needLevel));
            levelButton.GetComponentInChildren<TMP_Text>().text = (i + 1).ToString();
        }
    }

    public void ChangeLevelToNeeded(int needLevelNumber) {
        StorageManager.GameDataMain.CurMaxLevel = needLevelNumber;
        StorageManager.SaveGame();
        SceneManager.LoadScene("GameScene");
    }

    public void SetInfinite(bool isInfinite) {
        IsInfiniteHealth = isInfinite;
    }
    
    public void SetInfiniteBoosters(bool isInfiniteBoosters) {
        IsInfiniteBoosters = isInfiniteBoosters;
    }
}