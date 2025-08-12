using System;
using System.Linq;
using UnityEngine;

public static class TaskUtils {
    public static void CheckResourceCountForTasks(GameData gameData) {
        for (int i = 0; i < gameData.CurrentTasks.Count; i++) {
            if (gameData.CurrentTasks[i].TaskInfo.TaskType != TaskInfo.TaskType.getResource) {
                continue;
            }

            if (gameData.CollectedResources.Count == 0) {
                return;
            }

            if (gameData.CurrentTasks[i].TaskInfo.NeedResource == ResourceType.None) {
                int maxOfOneResource = gameData.CollectedResources.Values.Max();
                TryCompleteTask(gameData, i, maxOfOneResource);
            } else if (gameData.CollectedResources.TryGetValue(gameData.CurrentTasks[i].TaskInfo.NeedResource, out int hasResource)) {
                TryCompleteTask(gameData, i, hasResource);
            }
        }
    }

    private static void TryCompleteTask(GameData gameData, int i, int hasResource) {
        var remainingResourceCount = Math.Max(gameData.CurrentTasks[i].needCount - hasResource, 0);
        gameData.CurrentTasks[i].TaskUIView.TaskInfoTextHelper.SetText(remainingResourceCount.ToString());
        if (hasResource >= gameData.CurrentTasks[i].needCount) {
            CompleteTask(gameData, i);
        }
    }

    private static void CompleteTask(GameData gameData, int i) {
        gameData.ResourceTypesForTasks.Remove(gameData.CurrentTasks[i].TaskInfo.NeedResource);
        gameData.CurrentTasks[i].TaskUIView.CompleteTask();
        gameData.CurrentTasks.RemoveAt(i);
        GameEntryPoint.Instance.CheckWinWithAction();
    }

    public static bool IsResourceNeededForTasks(GameData gameData, ResourceType resourceType) {
        return gameData.CurrentTasks.Any(task => task.TaskInfo.IsResourceNeeded(resourceType));
    }

    public static TaskInfoAndUI GetUIForResourceTask(GameData gameData, ResourceType resource) {
        foreach (TaskInfoAndUI taskInfoAndUI in gameData.CurrentTasks) {
            if (taskInfoAndUI.TaskInfo.TaskType != TaskInfo.TaskType.getResource) {
                continue;
            }

            if (taskInfoAndUI.TaskInfo.NeedResource != ResourceType.None && taskInfoAndUI.TaskInfo.NeedResource != resource) {
                continue;
            }

            return taskInfoAndUI;
        }

        return null;
    }

    public static void CheckMonoLinesForTasks(GameData gameData) {
        for (int i = 0; i < gameData.CurrentTasks.Count; i++) {
            if (gameData.CurrentTasks[i].TaskInfo.TaskType != TaskInfo.TaskType.placeMonoLine ||
                !gameData.MonoLinesCount.TryGetValue(gameData.CurrentTasks[i].TaskInfo.NeedResource, out int count)) {
                continue;
            }

            if (gameData.CurrentTasks[i].TaskInfo.Count <= count) {
                gameData.CurrentTasks[i].TaskUIView.CompleteTask();
                gameData.CurrentTasks.RemoveAt(i);
                i--;
            }
        }
    }

    public static void Add1ToSlimeTask(GameData gameData) {
        foreach (TaskInfoAndUI task in gameData.CurrentTasks.Where(task => task.TaskInfo.NeedResource == ResourceType.Slime)) {
            task.needCount++;
            if (gameData.CollectedResources.TryGetValue(task.TaskInfo.NeedResource, out int resourceCount))
                task.TaskUIView.TaskInfoTextHelper.SetText((task.needCount - resourceCount).ToString());
            else
                task.TaskUIView.TaskInfoTextHelper.SetText(task.needCount.ToString());
        }
    }
}