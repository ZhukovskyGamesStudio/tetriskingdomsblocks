using UnityEngine;

public class TaskInfoAndUI
{
    public TaskInfo TaskInfo;
    public TaskUIView TaskUIView;

    public TaskInfoAndUI(TaskInfo taskInfo, TaskUIView taskUIView)
    {
        this.TaskInfo = taskInfo;
        this.TaskUIView = taskUIView;
    }
}
