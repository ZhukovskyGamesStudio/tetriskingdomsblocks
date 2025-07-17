using System;

[Serializable]
public enum CellType {
    Empty = 0,

    Wood = 1,
    Stone = 2,
    Wheat = 3,
    Metal = 4,

    Village = 100,
    Forest = 101,
    Mountain = 102,
    FieldOfWheat = 103,
    MetalMines = 104,
    LockedMetaCell = 105,
    
    
    Sawmill,
    Smithy,
    MiniCity,
    Mine,
    Farm,

    Box = 200,
    Ice = 201,
    GoldMine = 202,
    CrystalMine = 203,
    Crystal = 204,
    Slime = 205,

    Dynamite,
   
    VillagePart,
    FieldOfWheatLevel2,
    MountainLevel2,
    ForestLevel2
}