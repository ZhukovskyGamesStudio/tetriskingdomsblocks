using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public static class TasksUtils {
    public static KeyValuePair<ResourceType, int> GenerateNewResourceTask() {
        var l = Enum.GetNames(typeof(ResourceType));
        ResourceType resource = Enum.Parse<ResourceType>(l[Random.Range(1, l.Length)]);
        int goal = Random.Range(6, 11) * 5;//change this in future
        return new KeyValuePair<ResourceType, int>(resource, goal);
    }
}
