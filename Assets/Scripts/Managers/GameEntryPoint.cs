using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEntryPoint : MonoBehaviour {
    public static GameEntryPoint Instance;
    
    [SerializeField]
    private GameFieldManager _gameFieldManager;

    [SerializeField]
    private SpawnRandomNature _spawnRandomNature;

    [SerializeField]
    private GameAudio _gameAudio;

    [SerializeField]
    private MainGameConfig _mainGameConfig;

    [SerializeField]
    private UltaManager _ultaManager;

    private GameData _gameData;


    private void Start() {
        Instance = this;
        LevelConfig levelConfig = MainManager.Instance.CurrentLevelConfig;
        _gameData = new GameData {
            MovesLeft = levelConfig.MovesCount
        };

        CreateTasksForLevel(levelConfig);

        _gameFieldManager.Init(_mainGameConfig, _gameData);

        _gameFieldManager.InitFromLevel(levelConfig);
        _gameFieldManager.SetupGame();
        _gameFieldManager.PlaceStartingField(levelConfig);
        _gameFieldManager.OnMoveEnded += OnMoveEnded;
        _gameFieldManager.OnPieceDestroyedByHammer += CheckGameGoal;
        _ultaManager.Init(_mainGameConfig, _gameData);
        BoostersManager.Instance.Init(_gameData);
        GameUI.Instance.GoalView.SetMovesCount(levelConfig.MovesCount);
        BoostersManager.Instance.OnBoosterEndedWorking += CheckGameGoal;
        UltaManager.Instance.OnUltimateEndedWorking += CheckGameGoal;
        GameUI.Instance.HideNeededContainers();
        
      

        if (levelConfig.TutorialObject != null && !AdminManager.Instance.IsSkipTutorials) {
            Instantiate(levelConfig.TutorialObject, GameUI.Instance.BlackBgContainer);
        }

        _spawnRandomNature.Generate();
    }

    private void OnMoveEnded() {
        _gameData.MovesLeft--;
        GameUI.Instance.GoalView.SetMovesCount(_gameData.MovesLeft);

        CheckGameGoal();
    }

    private void CreateTasksForLevel(LevelConfig levelConfig) {
        int taskId = 0;
        foreach (var task in levelConfig.Tasks) {
            TaskInfoSubClass newTaskInfo = new TaskInfoSubClass(TaskInfo.TaskType.getResource, task.Key, task.Value);
            SetTaskUI(taskId, newTaskInfo, newTaskInfo);
            taskId++;
        }

        if (levelConfig.StartFieldConfig != null) {
            Dictionary<ResourceType, int> startCellsResourcesCount = new Dictionary<ResourceType, int>();
            for (int i = 0; i < _mainGameConfig.FieldSize; i++) {
                for (int j = 0; j < _mainGameConfig.FieldSize; j++) {
                    var type = levelConfig.StartFieldConfig.GetCell(i, j);
                    if (type != CellType.Empty) {
                        var cellConfig = PiecesViewTable.Instance.CellsList.CoreCellsConfigs.First(c => c.CellType == type);

                        if (!FieldUtils.CantDestroyInRow(cellConfig.CellType) &&
                            !startCellsResourcesCount.TryAdd(cellConfig.ResourcesForDestroy[0].ResourceType, 1))
                            startCellsResourcesCount[cellConfig.ResourcesForDestroy[0].ResourceType]++;
                    }
                }
            }

            SetTaskDescriptionsFromStartField(levelConfig, startCellsResourcesCount);
        }
    }

    private void SetTaskDescriptionsFromStartField(LevelConfig levelConfig, Dictionary<ResourceType, int> startTasks) {
        int i = levelConfig.Tasks.Count;
        foreach (var (resourceType, count) in startTasks) {
            if((int)resourceType > 0 &&  (int)resourceType < 5)continue;
            TaskInfoSubClass newTaskInfo = new TaskInfoSubClass(TaskInfo.TaskType.getResource, resourceType, count);

            SetTaskUI(i, newTaskInfo, newTaskInfo);
            i++;
        }
    }

    private void SetTaskUI(int i, TaskInfoSubClass newTaskInfo, TaskInfoSubClass task) {
        var taskUI = GameUI.Instance.GoalView.TaskUIViews[i];
        taskUI.SetData(task);

        _gameData.CurrentTasks.Add(new TaskInfoAndUI(newTaskInfo, taskUI, task.Count));
        _gameData.ResourceTypesForTasks.Add(task.NeedResource);

        //GameUI.Instance.StartCharacterInfoTextCoroutine();
    }

    private void CheckGameGoal() {
        TaskUtils.CheckResourceCountForTasks(_gameData);

        if(CheckWinWithAction())
            return;

        if (CheckLose() && !_gameData.IsGameEnded) {
            Lose();
            return;
        }

        if (_gameData.MovesLeft <= 0 && !_gameData.RejectedBuyMoves) {
            GameUI.Instance.ShowOutOfMovesDialog();
        }
    }

    public void TryBuyMoves() {
        if (StorageManager.GameDataMain.GetResource(ResourceType.Coins) < 900) {
            GameUI.Instance.ShowShopDialog();
            return;
        }

        StorageManager.GameDataMain.AddResource(ResourceType.Coins, -900);
        AddMoves();
    }

    public bool CheckWinWithAction()
    {
        if (CheckWin() && !_gameData.IsGameEnded) {
            UltaManager.Instance.UltimateActionEndRound(Win);
         
            return true;
        }

        return false;
    }

    private void AddMoves() {
        _gameData.MovesLeft += 5;
    }

    public void RejectMoves() {
        _gameData.RejectedBuyMoves = true;
        Lose();
    }

    public bool CheckWin() => _gameData.CurrentTasks.Count == 0;
    
    private bool CheckLose() {
        if(UltaManager.Instance._currentPoints >= GameUI.Instance.GoalView.UltimateProgressBar.maxValue)
            return false;
        if (_gameData.MovesLeft <= 0 && _gameData.RejectedBuyMoves ) {
            return true;
        }

        return !_gameFieldManager.CanPlaceAnyPiece();
    }

    private void Win() {
        SaveWinGame();
        MainManager.Instance.AddRewardToMeta();
        GameFieldManager.Instance.SetWinState();
        GameUI.Instance.GoalView.SetTasksActive(false);
        NextPiecesView.Instance.DestroyPieces();
        NextPiecesView.Instance.DestroyAdditionalPiece();
        VibrationsManager.Instance.SpawnContinuous(0.46f, 0.24f, 0.4f);
        _gameData.IsGameEnded = true;
        if (GameFieldManager.Instance != null) {
            GameUI.Instance.ShowWinDialog();
        } else {
            SceneManager.LoadScene("GameScene");
        }

        _gameAudio.PlayNextSound(_gameAudio.Win);
    }

    private void SaveWinGame() {
      //  StorageManager.GameDataMain.AddResource(ResourceType.Coins, 100); /* + StorageManager.GameDataMain.CurMaxLevel * 5*/;
        if (StorageManager.GameDataMain.IsFirstAttemptWin)
            StorageManager.GameDataMain.FirstAttemptWinLevelsCount++;
        StorageManager.GameDataMain.IsFirstAttemptWin = true;
        // StorageManager.GameDataMain.ResourcesCount[ResourceType.MagicCube] += 5 + StorageManager.GameDataMain.CurMaxLevel / 2;
      //  StorageManager.GameDataMain.AddResource(ResourceType.MagicCube, MainManager.Instance.CurrentLevelConfig.MagicCubesCount);
        MainManager.Instance.IncreaseMaxLevel();
        StorageManager.SaveGame();
    }

    private void Lose() {
        _gameData.IsGameEnded = true;
        StorageManager.GameDataMain.IsFirstAttemptWin = false;
        MainManager.Instance.RemoveHealthAfterLose();
        GameUI.Instance.GoalView.SetTasksActive(false);
        VibrationsManager.Instance.SpawnContinuous(0.46f, 0.24f, 0.4f);
        GameUI.Instance.ShowLoseDialog();
        GameUI.Instance.GoalView.HideUltimateUI();
        GameFieldManager.Instance.SetLoseState();
    }
}