using System;
using System.Globalization;
using Abstract;
using UnityEngine;

public class RateUsManager : PreloadableSingleton<RateUsManager> {
    public static int RateUsCooldownInDays = 3;

    public IRateUsProvider Provider;

    [HideInInspector]
    public string RateUsSource;

    protected override void OnFirstInit() {
        base.OnFirstInit();

#if GOOGLE_PLAY
        Provider = new GooglePlayRateUsProvider();
#else
        Provider = new RateUsMockProvider();
#endif
    }

    public void TryShowDialog(string rateUsSource) {
        var lastRateUsShowed = DateTime.Parse(StorageManager.GameDataMain.LastTimeRateUsShowed, CultureInfo.InvariantCulture);
        var wasRated = StorageManager.GameDataMain.WasRated;
        if (wasRated) return;
        if ((DateTime.Now - lastRateUsShowed).Days < RateUsCooldownInDays) return;
        Debug.Log("RateUsSource");
        MetaFieldManager.Instance.CanInteractWithField(false);
        RateUsSource = rateUsSource;
        DialogsManager.Instance.ShowDialog(typeof(RateUsDialog));
    }
}