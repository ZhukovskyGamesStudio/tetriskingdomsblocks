using System;
using System.Collections.Generic;

public class GameData {
    public int MovesLeft;
    public bool RejectedBuyMoves;
    public bool IsGameEnded;
    
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
    Wood = 1,
    Rocks = 2,
    Food = 3,
    Metal = 4,
    TetrisPieces = 5,
    Box = 6,
    Ice = 7,
    Gold = 8,
    Crystal = 9,
    Slime = 10,
    MetaPiece = 11,
    Health = 12,
    MetaGold = 13,
    MagicCube = 14
}