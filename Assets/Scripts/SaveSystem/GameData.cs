using System;

[Serializable]
public class GameDataForSave
{
    public int CurMaxLevel;

    /*public int WoodAmount;
    public int RocksAmount;
    public int FoodAmount;
    public int MetalAmount;*/
    public int[] resourcesCount;

    public int CoinsAmount;
    public int GemsAmount;

    public DateForSaveData LastHealthRecoveryTime;
    public int HealthCount;

    public DateForSaveData LastExitTime;
    
    public DateForSaveData LastGetPieceTime;
    
    public MetaFieldData[] FieldRows;

    public GameDataForSave()
    {
        HealthCount = 3;
        resourcesCount = new int[4];
        resourcesCount[0] = 300;
        resourcesCount[1] = 300;
        resourcesCount[2] = 300;
    }
}


[System.Serializable]
public struct MetaFieldData {
    public ResourceAndCountData[] RowCells;
}

[System.Serializable]
public struct ResourceAndCountData
{
    public CellType CellType;
    public int ResourceCount;

    public ResourceAndCountData(CellType cellType, int resourceCount)
    {
        CellType = cellType;
        ResourceCount = resourceCount;
    }
}

[System.Serializable]
public struct DateForSaveData
{
    public int Seconds;
    public int Minutes;
    public int Hours;
    public int Days;
    public int Years;
    
    public DateTime ToDateTime()
    {
        return new DateTime(Years, 1, 1)  
            .AddDays(Days - 1)         
            .AddHours(Hours)
            .AddMinutes(Minutes)
            .AddSeconds(Seconds);
    }

    public static DateForSaveData FromDateTime(DateTime dateTime)
    {
        return new DateForSaveData
        {
            Seconds = dateTime.Second,
            Minutes = dateTime.Minute,
            Hours = dateTime.Hour,
            Days = dateTime.DayOfYear,  
            Years = dateTime.Year      
        };
    }
}