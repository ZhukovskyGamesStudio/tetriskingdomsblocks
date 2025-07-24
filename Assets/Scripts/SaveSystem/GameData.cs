using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

[Serializable]
public class GameDataForSave {
    public string CreatedVersion;
    public int CurMaxLevel;
    public int[] ResourcesCount;
    public List<int> RemainedLockedZones;
    public int GoldAmount;
    public int MagicCubesAmount;
    public bool FieldSaveIsCreated; //change code with this bool
    public int HealthCount;
    
    public List<FormPositionsData> FigureFormsData = new List<FormPositionsData>();
    public List<FormAndCellTypeData> InventoryFigures = new List<FormAndCellTypeData>{};
    public MetaFieldData[] FieldRows;
    
    
    public string LastHealthRecoveryTime = DateTime.MinValue.ToString(CultureInfo.InvariantCulture);
    public DateTime LastHealthRecoveryTimeDateTime => DateTime.Parse(LastHealthRecoveryTime, CultureInfo.InvariantCulture);

    public string LastExitTime = DateTime.MinValue.ToString(CultureInfo.InvariantCulture);
    public DateTime LastExitTimeDateTime => DateTime.Parse(LastExitTime, CultureInfo.InvariantCulture);

    public string LastGetPieceTime = DateTime.MinValue.ToString(CultureInfo.InvariantCulture);
    public DateTime LastGetPieceTimeDateTime => DateTime.Parse(LastGetPieceTime, CultureInfo.InvariantCulture);
    

    public int DynamiteCount;
    public int RandomFieldCount;
    public int RotatePieceCount;
    public int HummerCount;
    public int MetaHummerCount;
    
    public SettingsData SettingsData;

    public GameDataForSave() {
        ResourcesCount = new int[4];
        ResourcesCount[0] = 3000;
        ResourcesCount[1] = 3000;
        ResourcesCount[2] = 3000;
        SettingsData = new SettingsData {
            IsSoundOn = true,
            IsMusicOn = true,
            IsVibrationOn = true
        };
        RemainedLockedZones = new List<int>() {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15
        };
        CreatedVersion = Application.version;
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

[Serializable]
public struct FormPositionsData {
    public Vector2Int[] FormCoordinates;

    public FormPositionsData(Vector2Int[] formCoordinates) {
        FormCoordinates = formCoordinates;
    }
}
[Serializable]
public struct FormAndCellTypeData {
    public CellType FormCellType;
    public string FormName;

    public FormAndCellTypeData(string formName,CellType formCellType) {
        FormName = formName;
        FormCellType = formCellType;
    }
    
    
}