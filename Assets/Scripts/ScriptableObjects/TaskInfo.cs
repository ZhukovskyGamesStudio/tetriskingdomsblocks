using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "TaskInfo", menuName = "Scriptable Objects/TaskInfo")]
public class TaskInfo : ScriptableObject
{
    public TaskType taskType;
    [FormerlySerializedAs("needResource")] public ResourceType NeedResource;
    [FormerlySerializedAs("count")] public int Count;
    [FormerlySerializedAs("needCell")] public CellTypeInfo NeedCell;
    public enum TaskType
    {
        getResource,
        placeNeedCell,
        unlockCell,
        placeMonoLine
    }
}
