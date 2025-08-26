using System;
using System.Collections.Generic;

public static class InApsIds {
    public static string Special = "blast.merge.match.tycoon.special";
    public static string Bundle1 = "blast.merge.match.tycoon.bundle1";
    public static string Bundle2 = "blast.merge.match.tycoon.bundle2";
    public static string Bundle3 = "blast.merge.match.tycoon.bundle3";
    public static string Hearts = "blast.merge.match.tycoon.hearts";
    public static string Coins = "blast.merge.match.tycoon.coins";

    public static string NoAds = "blast.merge.match.tycoon.noads";

    public static Dictionary<InApsTypes, string> InAps = new() {
        { InApsTypes.Special, Special },
        { InApsTypes.Bundle1, Bundle1 },
        { InApsTypes.Bundle2, Bundle2 },
        { InApsTypes.Bundle3, Bundle3 },
        { InApsTypes.Hearts, Hearts },
        { InApsTypes.Coins, Coins },
        { InApsTypes.NoAds, NoAds }
    };
}

[Serializable]
public enum InApsTypes {
    Special,

    Bundle1,
    Bundle2,
    Bundle3,

    Hearts,
    Coins,

    NoAds,
}