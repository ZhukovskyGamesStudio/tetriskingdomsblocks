using System;
using System.Collections.Generic;
using System.Globalization;

[Serializable]
public class GameDataForSave {
    public int CurMaxLevel;

    /*public int WoodAmount;
    public int RocksAmount;
    public int FoodAmount;
    public int MetalAmount;*/
    public int[] resourcesCount;
    public List<int> RemainedLockedZones;
    public int GoldAmount;
    public int MagicCubesAmount;
    public bool FieldSaveIsCreated; //change code with this bool

    public string LastHealthRecoveryTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
    public DateTime LastHealthRecoveryTimeDateTime => DateTime.Parse(LastHealthRecoveryTime, CultureInfo.InvariantCulture);

    public int HealthCount;

    public string LastExitTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
    public DateTime LastExitTimeDateTime => DateTime.Parse(LastExitTime, CultureInfo.InvariantCulture);

    public string LastGetPieceTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
    public DateTime LastGetPieceTimeDateTime => DateTime.Parse(LastGetPieceTime, CultureInfo.InvariantCulture);

    public MetaFieldData[] FieldRows;

    public int DynamyteCount;
    public int RandomFieldCount;
    public int RotatePieceCount;
    public int HummerCount;
    
    public bool IsSoundOn;
    public bool IsMusicOn;
    public bool IsVibrationOn;
    public GameDataForSave() {
        HealthCount = 3;
        resourcesCount = new int[4];
        resourcesCount[0] = 3000;
        resourcesCount[1] = 3000;
        resourcesCount[2] = 3000;
        IsSoundOn = true;
        IsMusicOn = true;
        IsVibrationOn = true;
        RemainedLockedZones = new List<int>()
        {
            1,2,3,4,5,6,7,8,9,10,11,12,13,14,15
        };
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