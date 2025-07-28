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

    Box = 200,
    Ice = 201,
    GoldMine = 202,
    CrystalMine = 203,
    Crystal = 204,
    Slime = 205,

    Dynamite = 250,

    VillagePart = 300,
    MountainLevel2 = 301,
    ForestLevel2 = 302,
    FieldOfWheatLevel2 = 303,
    MetalMinesLevel2 = 304,
    VillageLevel2 = 305,

    Sawmill = 1000,
    Smithy = 1001,
    MiniCity = 1002,
    Mine = 1003,
    Farm = 1004,
}