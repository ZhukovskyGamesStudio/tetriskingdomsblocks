using System;
using System.Collections.Generic;
using System.Globalization;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[Serializable]
public class GameDataForSave {
    public bool IsTutorialCompleted;
    public int CurMaxLevel;
    public int GamesCount;
    public SerializedDictionary<ResourceType, float> ResourcesCount;
    public SerializedDictionary<ResourceType, bool> SeenResource;

    public List<int> RemainedLockedZones;
   
    public string PlayerName = "PlayerName12345";
    public int HealthCount;
    public int PlacedInMetaPiecesCount;
    public int GotPiecesCount;
    public int ProfileAvatar;
    public bool IsFirstAttemptWin;
    public int FirstAttemptWinLevelsCount;

    public bool FirstLaunch = true;
    public string LastLaunchGameVersion;
    
    public bool IsWonInThisSession = false;
    public bool IsSpecialOfferBought = false;
    public bool WasRated = false;
    public bool HasNoAds = false;
    public bool IsIntersUnlocked = false;
    public int UncollectedLootboxes = 0;
    public bool IsFirstPaymentLogged = false;

    public float SessionSeconds;
    public int SessionMinutes;
    
    public string LastTimeRateUsShowed = DateTime.MinValue.ToString(CultureInfo.InvariantCulture);
    public DateTime LastTimeRateUsShowedDateTime => DateTime.Parse(LastTimeRateUsShowed, CultureInfo.InvariantCulture);

    public List<FormPositionsData> FigureFormsData = new List<FormPositionsData>();
    public List<FormAndCellTypeData> InventoryFigures = new List<FormAndCellTypeData> { };
    public MetaFieldData[] FieldRows;

    public string LastHealthRecoveryTime = DateTime.MinValue.ToString(CultureInfo.InvariantCulture);
    public DateTime LastHealthRecoveryTimeDateTime => DateTime.Parse(LastHealthRecoveryTime, CultureInfo.InvariantCulture);

    public string LastExitTime = DateTime.MinValue.ToString(CultureInfo.InvariantCulture);
    public DateTime LastExitTimeDateTime => DateTime.Parse(LastExitTime, CultureInfo.InvariantCulture);

    public string LastGetPieceTime = DateTime.MinValue.ToString(CultureInfo.InvariantCulture);
    public DateTime LastGetPieceTimeDateTime => DateTime.Parse(LastGetPieceTime, CultureInfo.InvariantCulture);

    public int MetaHummerCount = 100000;

    public SettingsData SettingsData;

    public GameDataForSave() {
        LastLaunchGameVersion = Application.version;
        HealthCount = 5;
        ResourcesCount = new SerializedDictionary<ResourceType, float>() {
            { ResourceType.Wood, 0 },
            { ResourceType.Rocks, 0 },
            { ResourceType.Food, 0 },
            { ResourceType.MagicCube, 0 },
            { ResourceType.Coins, 0 },
            { ResourceType.Metal, 0 },
            { ResourceType.ShuffleBooster, 0 },
            { ResourceType.HammerBooster, 0 },
            { ResourceType.RotateBooster, 0 },
            { ResourceType.BombBooster, 0 }
        };
        SeenResource = new SerializedDictionary<ResourceType, bool>() {
            { ResourceType.Wood, false },
            { ResourceType.Rocks, false },
            { ResourceType.Food, false },
            { ResourceType.MagicCube, false },
            { ResourceType.Coins, false },
            { ResourceType.Metal, false }
        };
        SettingsData = new SettingsData { IsSoundOn = true, IsMusicOn = true, IsVibrationOn = true };
        RemainedLockedZones = new List<int>() {
            1,
            2,
            3,
            4,
            5,
            6,
            7,
            8,
            9,
            10,
            11,
            12,
            13,
            14,
            15,
            16,
            17,
            18,
            19,
            20,
            21,
            22,
            23,
            24
        };
        ProfileAvatar = 0;
    }

    public void AddResource(ResourceType resource, float count) {
        if (SeenResource.ContainsKey(resource) && !SeenResource[resource]) {
            SeenResource[resource] = true;
        }

        ResourcesCount[resource] += count;
    }

    public void SetResource(ResourceType resource, float count) {
        AddResource(resource, count - ResourcesCount[resource]);
    }

    public float GetResource(ResourceType resource) => ResourcesCount[resource];

    public Dictionary<ResourceType, float> GetAllResources() => ResourcesCount;
}

[Serializable]
public struct MetaFieldData {
    public CellTypeAndCountData[] RowCells;
}

[Serializable]
public struct CellTypeAndCountData {
    public CellType CellType;
    public float ResourceCount;

    public CellTypeAndCountData(CellType cellType, float resourceCount) {
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

    public FormAndCellTypeData(string formName, CellType formCellType) {
        FormName = formName;
        FormCellType = formCellType;
    }
}