using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameEntryPoint : MonoBehaviour {
    [SerializeField]
    private GameFieldManager _gameFieldManager;

    [SerializeField]
    private SpawnRandomNature _spawnRandomNature;

    [field: SerializeField]
    public MainGameConfig _mainGameConfig;

    private List<TaskInfoAndUI> _currentTasks = new List<TaskInfoAndUI>();
    private List<ResourceType> _resourceTypesForTasks = new List<ResourceType>();

    private void Start() {
        _spawnRandomNature.Generate();
        LevelConfig levelConfig = MainManager.Instance.CurrentLevelConfig;
        CreateTasksForLevel(levelConfig);

        _gameFieldManager.Init();
        _gameFieldManager.InitFromLevel(levelConfig);
        _gameFieldManager.SetupGame();
        _gameFieldManager.PlaceStartingField(levelConfig);
        _gameFieldManager.SetTasks(_currentTasks, _resourceTypesForTasks);

        GameUI.Instance.StartCharacterInfoTextCoroutine(levelConfig.GuideForLevelText);
        GameUI.Instance.SetMovesCount(levelConfig.MovesCount);
        BoostersManager.Instance.SetAllText();

        if (levelConfig.TutorialObject != null) {
            Instantiate(levelConfig.TutorialObject);
        }
    }

    private void CreateTasksForLevel(LevelConfig levelConfig) {
        for (int i = 0; i < levelConfig.Tasks.Length; i++) {
            var task = levelConfig.Tasks[i];
            TaskInfoSubClass newTaskInfo = new TaskInfoSubClass(task.taskType, task.NeedResource, task.Count);
            SetTaskUI(i, newTaskInfo, newTaskInfo);
        }

        if (levelConfig.StartFieldConfig != null) {
            Dictionary<ResourceType, int> startCellsResourcesCount = new Dictionary<ResourceType, int>();
            for (int i = 0; i < _mainGameConfig.FieldSize; i++) {
                for (int j = 0; j < _mainGameConfig.FieldSize; j++) {
                    var type = levelConfig.StartFieldConfig.GetCell(i, j);
                    if (type != CellType.Empty) {
                        var cellConfig = PiecesViewTable.Instance.CellsList.CellsConfigs.First(c => c.CellType == type);

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

        _currentTasks.Add(new TaskInfoAndUI(newTaskInfo, taskUI, task.Count));
        _resourceTypesForTasks.Add(task.NeedResource);

        //GameUI.Instance.StartCharacterInfoTextCoroutine();
    }
}