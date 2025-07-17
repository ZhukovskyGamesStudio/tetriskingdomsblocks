using System;
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

    private void Awake() {
        Instance = this;
        _infiniteHpToggle.SetIsOnWithoutNotify(IsInfiniteHealth);
        DontDestroyOnLoad(this);
        SetupLevelButtons();
    }

    public void ChangeAdminPanelState(bool isOn) {
        AdminPanelContainer.gameObject.SetActive(isOn);
    }

    public void AddResources() {
        for (int i = 0; i < 3; i++) {
            StorageManager.GameDataMain.ResourcesCount[i] += 1000;
        }

        StorageManager.GameDataMain.MagicCubesAmount += 20;
        if(MetaUI.Instance != null)
            MetaUI.Instance.CountersPanelView.SetMagicCubes( StorageManager.GameDataMain.MagicCubesAmount );
        
        MetaFieldManager.Instance.UpdateResourcesCountUIText();
    }

    public void RestoreAllHPForAdminButton() => StorageManager.GameDataMain.HealthCount = 3;

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
        
        if(BoostersManager.Instance != null)
        BoostersManager.Instance.SetAllText();
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
}