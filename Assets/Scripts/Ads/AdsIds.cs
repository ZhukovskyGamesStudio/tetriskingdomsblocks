using System;
using System.Collections.Generic;

public static class AdsIds {
    public static string DoubleAfkResources = "rewarded_double_afk";

    public static Dictionary<AdsTypes, string> AdsTypesIds = new() {
        { AdsTypes.DoubleAfkResources, DoubleAfkResources },
    };
}

[Serializable]
public enum AdsTypes {
    DoubleAfkResources
}