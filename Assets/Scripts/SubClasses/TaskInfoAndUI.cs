using UnityEngine;

public class TaskInfoAndUI
{
    public TaskInfoSubClass TaskInfo;
    public TaskUIView TaskUIView;
    public int needCount;

    public TaskInfoAndUI(TaskInfoSubClass taskInfo, TaskUIView taskUIView, int needCount)
    {
        this.TaskInfo = taskInfo;
        this.TaskUIView = taskUIView;
        this.needCount = needCount;
    }
}

[System.Serializable]
public class TaskInfoSubClass
{
    public TaskInfo.TaskType TaskType;
    public ResourceType NeedResource;
    public int Count;

    public TaskInfoSubClass(TaskInfo.TaskType taskType, ResourceType needResource, int count)
    {
        TaskType = taskType;
        NeedResource = needResource;
        Count = count;
    }
}
