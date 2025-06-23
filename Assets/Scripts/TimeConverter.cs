using System;
using UnityEngine;

public class TimeConverter
{
    public static string ConvertToTimeString(TimeSpan timeSpan)
    {
        if (timeSpan.TotalHours >= 1)
            return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m";
        
        else if (timeSpan.TotalMinutes >= 1)
            return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
        
        else
            return $"{timeSpan.Seconds}s";
    }

}
