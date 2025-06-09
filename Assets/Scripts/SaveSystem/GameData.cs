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
   // public CellType[][] Field;
  //  public CellTypesArray[] TestArrayToSave;
   public MetaFieldData[] FieldRows;
}

[System.Serializable]
public struct MetaFieldData {
    public CellType[] RowCells;
}
/*[System.Serializable]
public struct CellTypesArray
{
    public CellType[] CellRow;
}*/