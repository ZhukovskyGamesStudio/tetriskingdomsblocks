
 using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class TutorialGameEntryPoint : MonoBehaviour
{


    [SerializeField]
    private TutorialFieldManager _gameFieldManager;

    [SerializeField]
    private SpawnRandomNature _spawnRandomNature;

    [SerializeField]
    private GameAudio _gameAudio;

    [SerializeField]
    private MainGameConfig _mainGameConfig;

    [SerializeField]
    private UltaManager _ultaManager;

    private GameData _gameData;
[SerializeField]
private LevelConfig _levelConfig;
    private void Start() {
        LevelConfig levelConfig =_levelConfig;
        _gameData = new GameData {
            MovesLeft = levelConfig.MovesCount
        };

        CreateTasksForLevel(levelConfig);

        _gameFieldManager.Init(_mainGameConfig, _gameData);

        _gameFieldManager.InitFromLevel(levelConfig);
        _gameFieldManager.SetupGame();
        _gameFieldManager.PlaceStartingField(levelConfig);
        _gameFieldManager.OnPieceDestroyedByHammer += CheckGameGoal;
        _ultaManager.Init(_mainGameConfig);
        GameUI.Instance.SetMovesCount(levelConfig.MovesCount);
    //    BoostersManager.Instance.SetAllText();
     //   BoostersManager.Instance.OnBoosterEndedWorking += CheckGameGoal;
        UltaManager.Instance.OnUltimateEndedWorking += CheckGameGoal;
        TutorialFieldManager.Instance.OnMoveEnded += CheckGameGoal;

        if (levelConfig.TutorialObject != null) {
            Instantiate(levelConfig.TutorialObject);
        }

        _spawnRandomNature.Generate();
    }

    private void CreateTasksForLevel(LevelConfig levelConfig) {
        for (int i = 0; i < levelConfig.Tasks.Length; i++) {
            var task = levelConfig.Tasks[i];
            TaskInfoSubClass newTaskInfo = new TaskInfoSubClass(task.taskType, task.NeedResource, task.Count);
            SetTaskUI(i, newTaskInfo, newTaskInfo);
        }

       /* if (levelConfig.StartFieldConfig != null) {
            Dictionary<ResourceType, int> startCellsResourcesCount = new Dictionary<ResourceType, int>();
            for (int i = 0; i < _mainGameConfig.FieldSize; i++) {
                for (int j = 0; j < _mainGameConfig.FieldSize; j++) {
                    var type = levelConfig.StartFieldConfig.GetCell(i, j);
                    if (type != CellType.Empty && type != CellType.LockedMetaCell) {
                        var cellConfig = PiecesViewTable.Instance.CellsList.CoreCellsConfigs.First(c => c.CellType == type);

                        if (!FieldUtils.CantDestroyInRow(cellConfig.CellType) &&
                            !startCellsResourcesCount.TryAdd(cellConfig.ResourcesForDestroy[0].ResourceType, 1))
                            startCellsResourcesCount[cellConfig.ResourcesForDestroy[0].ResourceType]++;
                    }
                }
            }
  SetTaskDescriptionsFromStartField(levelConfig, startCellsResourcesCount);
          
        }*/
    }

    private void SetTaskDescriptionsFromStartField(LevelConfig levelConfig, Dictionary<ResourceType, int> startTasks) {
        int i = levelConfig.Tasks.Length;
        foreach (var (resourceType, count) in startTasks) {
            TaskInfoSubClass newTaskInfo = new TaskInfoSubClass(TaskInfo.TaskType.getResource, resourceType, count);

            SetTaskUI(i, newTaskInfo, newTaskInfo);
            i++;
        }
    }

    private void SetTaskUI(int i, TaskInfoSubClass newTaskInfo, TaskInfoSubClass task) {
        var taskUI = GameUI.Instance.TaskUIViews[i];
        taskUI.SetData(task);

        _gameData.CurrentTasks.Add(new TaskInfoAndUI(newTaskInfo, taskUI, task.Count));
        _gameData.ResourceTypesForTasks.Add(task.NeedResource);

        //GameUI.Instance.StartCharacterInfoTextCoroutine();
    }

    private void CheckGameGoal() {
        TaskUtils.CheckResourceCountForTasks(_gameData);

        if (CheckWin()) {
            TutorialFieldManager.Instance.ClearAllLockedCells();
            UltaManager.Instance.UltimateActionEndRound(Win);
            return;
        }
    }
    
    private bool CheckWin() => _gameData.CurrentTasks.Count == 0;

    private void Win() {
        SaveWinGame();

        TutorialFieldManager.Instance.SetWinState();
        GameUI.Instance.SetMainText("You win!");
        GameUI.Instance.SetTasksActive(false);
        NextPiecesView.Instance.DestroyPieces();
       // NextPiecesView.Instance.DestroyAdditionalPiece();
        VibrationsManager.Instance.SpawnContinuous(0.46f, 0.24f, 0.4f);
        GameUI.Instance.GoalView.SetWinState();

        _gameAudio.PlayNextSound(_gameAudio.Win);
    }

    private void SaveWinGame() {
        StorageManager.SaveGame();
    }
}