using System;
using System.Collections.Generic;

[Serializable]
public class GameData {

    public TaskData TaskData = new TaskData();
    
    //TODO import serializableDictionary
    public Dictionary<ResourceType, int> CollectedResources = new Dictionary<ResourceType, int>();
}
[Serializable]

public class TaskData {
    public Dictionary<ResourceType, int> GoalToCollect = new Dictionary<ResourceType, int>();
}

[Serializable]
public enum ResourceType {
    None = 0,
    Wood,
    Rocks,
    Food,
}
