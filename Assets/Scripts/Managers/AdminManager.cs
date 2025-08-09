using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AdminManager : MonoBehaviour {
    public static AdminManager Instance { get; private set; }
    public Button LevelButton;
    public Transform LevelButtonsContainer;
    public Transform AdminPanelContainer;
    public Toggle AdminToggle;

    [SerializeField]
    private Toggle _infiniteHpToggle;
    
    [SerializeField]
    private MainManagerConfig _mainManagerConfig;

    public static bool IsInfiniteHealth = true;

    [SerializeField]
    private Toggle _infiniteBoostersToggle;

    public bool IsInfiniteBoosters = true;
    
    [SerializeField]
    private Toggle _skipTutorialsToggle;

    public bool IsSkipTutorials = false;

    private void Awake() {
        Instance = this;
        _infiniteHpToggle.SetIsOnWithoutNotify(IsInfiniteHealth);
        _infiniteBoostersToggle.SetIsOnWithoutNotify(IsInfiniteBoosters);
        //_skipTutorialsToggle.SetIsOnWithoutNotify(IsSkipTutorials);
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
        var keys = StorageManager.GameDataMain.GetAllResources().Keys.ToList();

        foreach (var key in keys) {
            StorageManager.GameDataMain.AddResource(key, 100);
        }

        StorageManager.GameDataMain.AddResource(ResourceType.MagicCube, 20);
        StorageManager.GameDataMain.AddResource(ResourceType.Coins, 100);
        if (MetaUI.Instance != null) {
            MetaUI.Instance.CountersPanelView.SetMagicCubes((int)StorageManager.GameDataMain.GetResource(ResourceType.MagicCube));
        }

        MetaFieldManager.Instance.UpdateResourcesCountUIText();
    }

    public void RestoreAllHPForAdminButton() {
        StorageManager.GameDataMain.HealthCount = 5;
        if (MetaUI.Instance == null) return;
        MetaUI.Instance.HealthView.SetHealthCountText(5);
    }

    public void GoToMetaAdminButton() {
        SceneManager.LoadScene("MetaScene");
    }
    public void RemoveOneHealthAdminButton() {
        if (StorageManager.GameDataMain.HealthCount <= 0) return;
        MainManager.Instance.RemoveHealthAfterLose();
    }

    public void GenerateNewPiecesForButton() {
        if (GameFieldManager.Instance != null)
            GameFieldManager.Instance.GenerateNewPieces();
    }

    public void AddBoosters() {
        StorageManager.GameDataMain.RandomFieldCount += 5;
        StorageManager.GameDataMain.HummerCount += 5;
        StorageManager.GameDataMain.RotatePieceCount += 5;
        StorageManager.GameDataMain.DynamiteCount += 5;

        StorageManager.GameDataMain.MetaHummerCount += 5;

        if (BoostersManager.Instance != null) {
            GameUI.Instance.GameBoostersButtons.UpdateCounters(StorageManager.GameDataMain);
        }
    }

    public void RestartGame() {
        if (GameFieldManager.Instance != null)
            MainManager.Instance.Restart();
    }

    private void SetupLevelButtons() {
        for (int i = 0; i < _mainManagerConfig.Levels.Length; i++) {
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
    
    public void SetTutorialSkip(bool isSkipTutorialsBoosters) {
        IsSkipTutorials = !IsSkipTutorials;
    }
}