using UnityEngine;

[CreateAssetMenu(fileName = "TaskInfo", menuName = "Scriptable Objects/TaskInfo")]
public class TaskInfo : ScriptableObject
{
    public TaskType taskType;
    public ResourceType needResource;
    public int count;
    public CellTypeInfo needCell;
    public enum TaskType
    {
        getResource,
        placeNeedCell,
        unlockCell,
        placeMonoLine
    }
}
