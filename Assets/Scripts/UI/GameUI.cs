using System;
using UnityEngine;
using UnityEngine.Pool;

public class GameUI : MonoBehaviour {
    public static GameUI Instance;

    [Header("Game UI")]
    [SerializeField]
    private Transform _holesForBgContainer;

    [SerializeField]
    private Transform _blackBgContainer;
    
    [SerializeField]
    private FloatingTextView _floatingTextPrefab;

    [SerializeField]
    private Transform _floatingTextContainer;

    [field: SerializeField]
    public GameBoostersButtons GameBoostersButtons { get; private set; }

    [field:SerializeField]
    public GameBoostersPanels BoostersPanels { get; private set; }

    [field: SerializeField]
    public GoalView GoalView { get; private set; }

    private ObjectPool<FloatingTextView> _floatingTextsPool;

    private void Awake() {
        Instance = this;
        _floatingTextsPool = new ObjectPool<FloatingTextView>(() => Instantiate(_floatingTextPrefab, _floatingTextContainer));
    }

    public Transform HolesForBgContainer => _holesForBgContainer;
    public Transform BlackBgContainer => _blackBgContainer;

    public void ShowFloatingText(string needText, Vector2 newPosition, float textSize, float showTime, Vector2 finalposition) {
        var floatingText = _floatingTextsPool.Get();
        floatingText.SetText(newPosition, needText, textSize, showTime, finalposition);
    }

    public void ReleaseFloatingText(FloatingTextView needTextObject) {
        needTextObject.gameObject.SetActive(false);
        _floatingTextsPool.Release(needTextObject);
    }

    public void ShowSettings() {
        SettingsManager.Instance.ShowGameSettingsDialog();
    }

    public void ShowOutOfMovesDialog(Action tryBuyMoves, Action rejectMoves) {
        var outOfMovesData = new DialogWithData {
            DialogType = typeof(OutOfMovesDialog),
            Data = new OutOfMovesDialog.Data {
                ClickAdd = tryBuyMoves,
                ClickClose = rejectMoves,
                Balance = Mathf.FloorToInt(StorageManager.GameDataMain.GoldAmount) ,
                Cost = 900
            }
        };
        DialogsManager.Instance.ShowDialogWithData(outOfMovesData);
    }

    public void ShowLoseDialog() {
        var loseData = new DialogWithData {
            DialogType = typeof(LoseDialog),
            Data = new LoseDialog.Data {
                ClickContinue = MainManager.Instance.GoToMeta,
                Hp = StorageManager.GameDataMain.HealthCount
            }
        };

        DialogsManager.Instance.ShowDialogWithData(loseData);
    }

    public void ShowWinDialog() {
        var winData = new DialogWithData() {
            DialogType = typeof(WinDialog),
            Data = new WinDialog.Data() {
                ClickClaim = MainManager.Instance.GoToMeta,
                Coins = MainManager.Instance.CurrentLevelConfig.GoldAmount,
                Cubes = MainManager.Instance.CurrentLevelConfig.MagicCubesCount
            }
        };
        DialogsManager.Instance.ShowDialogWithData(winData);
    }

    public void SwitchShuffleWindowActive() {
      
    }

    public void SwitchBombWindowActive() {
      
    }

    public void SwitchHammerWindowActive() {
     
    }

    public void SwitchRotateWindowActive() {
      
    }
}