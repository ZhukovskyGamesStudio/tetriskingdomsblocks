using System;
using System.Collections.Generic;
using System.Globalization;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[Serializable]
public class GameDataForSave {
    public string CreatedVersion;
    public int CurMaxLevel;
    private SerializedDictionary<ResourceType, float> _resourcesCount;
    public Dictionary<ResourceType, bool> SeenResource;
    public List<int> RemainedLockedZones;
    public bool FieldSaveIsCreated; //change code with this bool
    public int HealthCount;
    public int PlacedInMetaPiecesCount;
    public int ProfileAvatar;
    public bool IsFirstAttemptWin;
    public int FirstAttemptWinLevelsCount;
    
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
        HealthCount = 5;
        _resourcesCount = new SerializedDictionary<ResourceType, float>() {
            { ResourceType.Wood, 0 },
            { ResourceType.Rocks, 0 },
            { ResourceType.Food, 0 },
            { ResourceType.MagicCube, 0 },
            { ResourceType.Coins, 0 },
            { ResourceType.Metal, 0 }
        };
        SeenResource = new Dictionary<ResourceType, bool> {
            { ResourceType.Wood, true },
            { ResourceType.Rocks, false },
            { ResourceType.Food, false },
            { ResourceType.MagicCube, false },
            { ResourceType.Coins, true },
            { ResourceType.Metal, false }
        };
        SettingsData = new SettingsData {
            IsSoundOn = true,
            IsMusicOn = true,
            IsVibrationOn = true
        };
        RemainedLockedZones = new List<int>() {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15
        };
        CreatedVersion = Application.version;
        ProfileAvatar = 0;
    }

    public void AddResource(ResourceType resource, float count) {
        if (!SeenResource[resource]) SeenResource[resource] = true;
        _resourcesCount[resource] += count;
    }

    public void SetResource(ResourceType resource, float count) {
        AddResource(resource, count - _resourcesCount[resource]);
    }
    
    public float GetResource(ResourceType resource) => _resourcesCount[resource];

    public Dictionary<ResourceType, float> GetAllResources() => _resourcesCount;
}

[Serializable]
public struct MetaFieldData {
    public ResourceAndCountData[] RowCells;
}

[Serializable]
public struct ResourceAndCountData {
    public CellType CellType;
    public float ResourceCount;

    public ResourceAndCountData(CellType cellType, float resourceCount) {
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