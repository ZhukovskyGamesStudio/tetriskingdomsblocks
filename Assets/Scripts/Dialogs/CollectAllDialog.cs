using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

[Obsolete]
public class CollectAllDialog : DialogBase {
    public void CollectAll() {
        Hide().Forget();
        CollectResources(1);
    }

    public void CollectAllWithAds() {
        Hide().Forget();
        ZhukovskyAdsManager.Instance.AdsProvider.ShowRewardedAd(AdsIds.AdsTypesIds[AdsTypes.DoubleAfkResources], CollectAllWithMultiplier,
            null);
    }

    private void CollectAllWithMultiplier() {
        CollectResources(MetaFieldManager.Instance.MainMetaConfig.CollectWithAdsMultiplier);
        ZhukovskyAnalyticsManager.Instance.SendCustomEvent("rv_finish", new Dictionary<string, object>());
    }

    private void CollectResources(float multiplier) {
        MetaFieldManager.Instance.CollectResourcesFromAllMarks(multiplier);
    }
}