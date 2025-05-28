using UnityEngine;

public class TaskInfoAndUI
{
    public TaskInfo taskInfo;
    public TaskUIView taskUIView;

    public TaskInfoAndUI(TaskInfo taskInfo, TaskUIView taskUIView)
    {
        this.taskInfo = taskInfo;
        this.taskUIView = taskUIView;
    }
}
