using System;
using System.Globalization;

[Serializable]
public class GameDataForSave {
    public int CurMaxLevel;

    /*public int WoodAmount;
    public int RocksAmount;
    public int FoodAmount;
    public int MetalAmount;*/
    public int[] resourcesCount;

    public int CoinsAmount;
    public int GemsAmount;
    public bool FieldSaveIsCreated; //change code with this bool

    public string LastHealthRecoveryTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
    public DateTime LastHealthRecoveryTimeDateTime => DateTime.Parse(LastHealthRecoveryTime, CultureInfo.InvariantCulture);

    public int HealthCount;

    public string LastExitTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
    public DateTime LastExitTimeDateTime => DateTime.Parse(LastExitTime, CultureInfo.InvariantCulture);

    public string LastGetPieceTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
    public DateTime LastGetPieceTimeDateTime => DateTime.Parse(LastGetPieceTime, CultureInfo.InvariantCulture);

    public MetaFieldData[] FieldRows;

    public GameDataForSave() {
        HealthCount = 3;
        resourcesCount = new int[4];
        resourcesCount[0] = 3000;
        resourcesCount[1] = 3000;
        resourcesCount[2] = 3000;
    }
}

[Serializable]
public struct MetaFieldData {
    public ResourceAndCountData[] RowCells;
}

[Serializable]
public struct ResourceAndCountData {
    public CellType CellType;
    public int ResourceCount;

    public ResourceAndCountData(CellType cellType, int resourceCount) {
        CellType = cellType;
        ResourceCount = resourceCount;
    }
}