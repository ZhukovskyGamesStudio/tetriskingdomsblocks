using System;
using System.Collections.Generic;

public static class InApsIds {
    public static string Special = "tetris.kingdoms.blocks.special";
    public static string Bundle1 = "tetris.kingdoms.blocks.bundle1";
    public static string Bundle2 = "tetris.kingdoms.blocks.bundle2";
    public static string Bundle3 = "tetris.kingdoms.blocks.bundle3";
    public static string Hearts = "tetris.kingdoms.blocks.hearts";
    public static string Coins = "tetris.kingdoms.blocks.coins";

    public static string NoAds = "tetris.kingdoms.blocks.noads";

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