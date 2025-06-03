using System;

[Serializable]
public class GameDataForSave {
    public int CurMaxLevel;

    public int WoodAmount;
    public int RocksAmount;
    public int FoodAmount;
    public int MetalAmount;

    public int CoinsAmount;
    public int GemsAmount;
    public MetaFieldData MetaField = new MetaFieldData();
}

[Serializable]
public class MetaFieldData {
    public CellType[,] Field;
}