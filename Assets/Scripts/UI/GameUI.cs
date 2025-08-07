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
    
    [field:SerializeField]
    public Transform _tasksContainer { get; private set; }
    [SerializeField]
    private Transform _boostersContainer;
    [field:SerializeField]
    public Transform _movesContainer{ get; private set; }
    [field:SerializeField]
    public Transform _ultimateContainer{ get; private set; }
    [field: SerializeField]
    public GoalView GoalView { get; private set; }

    private ObjectPool<FloatingTextView> _floatingTextsPool;

    private void Awake() {
        Instance = this;
        _floatingTextsPool = new ObjectPool<FloatingTextView>(() => Instantiate(_floatingTextPrefab, _floatingTextContainer));
    }

    public Transform HolesForBgContainer => _holesForBgContainer;
    public Transform BlackBgContainer => _blackBgContainer;

    public void HideNeededContainers() {
        var curLevel = StorageManager.GameDataMain.CurMaxLevel;
        if (curLevel < 3) {
            _boostersContainer.gameObject.SetActive(false);
            if (curLevel < 2) {
                 _ultimateContainer.gameObject.SetActive(false);
                 if (curLevel == 0)  
                     _movesContainer.gameObject.SetActive(false);
            }
        }
    } 
    public void ShowFloatingText(Sprite needSprite, Vector2 newPosition, float textSize, float showTime, Vector2 finalposition) {
        var floatingText = _floatingTextsPool.Get();
        floatingText.SetText(newPosition, needSprite, textSize, showTime, finalposition);
    }

    public void ReleaseFloatingText(FloatingTextView needTextObject) {
        needTextObject.gameObject.SetActive(false);
        _floatingTextsPool.Release(needTextObject);
    }

    public void ShowSettings() {
        SettingsManager.Instance.ShowGameSettingsDialog();
    }

    public void ShowShopDialog() {
        var dialogData = new DialogWithData {
            DialogType = typeof(RealShopDialog),
            Data = new RealShopDialog.Data {
                Balance = Mathf.FloorToInt(StorageManager.GameDataMain.ResourcesCount[ResourceType.Coins]),
                ClickClose = ShowOutOfMovesDialog,
                BuyResource = MainManager.Instance.BuyMetaResource,
                IsCore = true
            }
        };
        
        DialogsManager.Instance.ShowDialogWithData(dialogData);
    }

    public void ShowOutOfMovesDialog() {
        var outOfMovesData = new DialogWithData {
            DialogType = typeof(OutOfMovesDialog),
            Data = new OutOfMovesDialog.Data {
                BuyMoves = GameEntryPoint.Instance.TryBuyMoves,
                ClickClose = GameEntryPoint.Instance.RejectMoves,
                Cost = 900
            }
        };
        DialogsManager.Instance.ShowDialogWithData(outOfMovesData);
    }

    public void ShowLoseDialog() {
        var loseData = new DialogWithData {
            DialogType = typeof(LoseDialog),
            Data = new LoseDialog.Data {
                ClickExit = MainManager.Instance.GoToMeta,
                ClickRetry = MainManager.Instance.Restart,
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