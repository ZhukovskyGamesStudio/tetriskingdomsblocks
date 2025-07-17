using System;
using System.Collections.Generic;

[Serializable]
public class GameData {
    public int MovesLeft;

    //TODO import serializableDictionary
    public Dictionary<ResourceType, int> CollectedResources = new Dictionary<ResourceType, int>();

    public List<TaskInfoAndUI> CurrentTasks = new List<TaskInfoAndUI>();
    public List<ResourceType> ResourceTypesForTasks = new List<ResourceType>();

    //Game stats
    public Dictionary<CellType, int> PlacedCellsCount = new Dictionary<CellType, int>();
    public Dictionary<ResourceType, int> MonoLinesCount = new Dictionary<ResourceType, int>();
}

[Serializable]
public enum ResourceType {
    None = 0,
    Wood,
    Rocks,
    Food,
    Metal,
    TetrisPieces,
    Box,
    Ice,
    Gold,
    Crystal,
    Slime
}