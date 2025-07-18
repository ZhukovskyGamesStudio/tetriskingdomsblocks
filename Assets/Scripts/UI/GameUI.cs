using System;
using UnityEngine;
using TMPro;
using UnityEngine.Pool;
using System.Collections;

public class GameUI : MonoBehaviour {
    public static GameUI Instance;

    [Header("Game UI")]
    [SerializeField]
    private Transform _holesForBgContainer;

    [SerializeField]
    private Transform _blackBgContainer;

    [SerializeField]
    private RectTransform _bgTasksImage;

    [SerializeField]
    private Transform _openedDoorEndGame;

    [SerializeField]
    private TMP_Text _mainTextUp;

    [SerializeField]
    private TMP_Text _currentMovesCountText;

    [SerializeField]
    private TaskUIView[] _taskUIViews;

    [SerializeField]
    private Transform _downUITransform;

    [SerializeField]
    private FloatingTextView _floatingTextPrefab;

    [SerializeField]
    private Transform _floatingTextContainer;

    [SerializeField]
    private SpawnedForOneCharTextView _characterInfoTextHelper;

    [SerializeField]
    private GameObject _shuffleWindow, _bombWindow, _hammerWindow, _rotateWindow;

    [field: SerializeField]
    public GoalView GoalView { get; private set; }

    private ObjectPool<FloatingTextView> _floatingTextsPool;

    private void Awake() {
        Instance = this;
        _floatingTextsPool = new ObjectPool<FloatingTextView>(() => Instantiate(_floatingTextPrefab, _floatingTextContainer));
    }

    public void Init(GameData gameData) {
        _gameData = gameData;
    }

    private GameData _gameData;

    public Transform HolesForBgContainer => _holesForBgContainer;
    public Transform BlackBgContainer => _blackBgContainer;
    public RectTransform BgTasksImage => _bgTasksImage;
    public Transform OpenedDoorEndGame => _openedDoorEndGame;
    public TaskUIView[] TaskUIViews => _taskUIViews;

    public void SetMovesCount(int count) {
        _currentMovesCountText.text = count.ToString();
    }

    public void SetMainText(string text) {
        _mainTextUp.text = text;
    }

    public void ShowFloatingText(string needText, Vector2 newPosition, float textSize, float showTime, Vector2 finalposition) {
        var floatingText = _floatingTextsPool.Get();
        floatingText.SetText(newPosition, needText, textSize, showTime, finalposition);
    }

    public void ReleaseFloatingText(FloatingTextView needTextObject) {
        needTextObject.gameObject.SetActive(false);
        _floatingTextsPool.Release(needTextObject);
    }

    public void SetTasksActive(bool active) {
        foreach (var taskUI in _taskUIViews) {
            taskUI.gameObject.SetActive(active);
        }
    }

    public Coroutine StartCharacterInfoTextCoroutine(string text) {
        return StartCoroutine(_characterInfoTextHelper.StartSpawnText(text));
    }

    public void SetTaskUI(int i, TaskInfoSubClass newTaskInfo, TaskInfoSubClass task) {
        var taskUI = _taskUIViews[i];
        taskUI.gameObject.SetActive(true);
        string needSpiteName = "";
        switch (task.TaskType) {
            case TaskInfo.TaskType.getResource:
                needSpiteName = task.NeedResource.ToString();
                break;
            case TaskInfo.TaskType.placeMonoLine:
                needSpiteName = task.NeedResource.ToString();
                taskUI.TaskSubImage.sprite = ConfigsManager.Instance.SpritesForTasksConfig.LineSprite;
                break;
        }

        taskUI.TaskImage.sprite = ConfigsManager.Instance.SpritesForTasks[needSpiteName];
        StartCoroutine(taskUI.TaskInfoTextHelper.StartSpawnText(task.Count.ToString()));
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
                Balance = StorageManager.GameDataMain.GoldAmount,
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

    public void SetShuffleActive(bool isActive) {
        _shuffleWindow.SetActive(isActive);
        
        _bombWindow.SetActive(false);
        _hammerWindow.SetActive(false);
        _rotateWindow.SetActive(false);
    }
    
    public void SetBombActive(bool isActive) {
        _bombWindow.SetActive(isActive);
        
        _shuffleWindow.SetActive(false);
        _hammerWindow.SetActive(false);
        _rotateWindow.SetActive(false);
    }
    
    public void SetHammerActive(bool isActive) {
        _hammerWindow.SetActive(isActive);
        
        _bombWindow.SetActive(false);
        _shuffleWindow.SetActive(false);
        _rotateWindow.SetActive(false);
    }
    
    public void SetRotateActive(bool isActive) {
        _rotateWindow.SetActive(isActive);
        
        _bombWindow.SetActive(false);
        _hammerWindow.SetActive(false);
        _shuffleWindow.SetActive(false);
    }

    // Методы для работы с TaskUI, GoalView, NextPiecesView и т.д. можно добавить по мере необходимости
}