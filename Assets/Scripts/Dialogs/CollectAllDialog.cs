using Cysharp.Threading.Tasks;
using System;

public class CollectAllDialog : DialogBase {
    
    public static event Action<string, string, string, bool> OnCollectAllWithMultiplier;
    public static event Action<string, string, string, bool> OnCollectAllWithMultiplierFailed;
    public void CollectAll() {
        Hide().Forget();
        CollectResources(1);
    }

    public void CollectAllWithAds() {
        Hide().Forget();
        ZhukovskyAdsManager.Instance.AdsProvider.ShowRewardedAd(AdsIds.AdsTypesIds[AdsTypes.DoubleAfkResources], CollectAllWithMultiplier,
            FailedAd);
    }

    private void CollectAllWithMultiplier() {
        CollectResources(MetaFieldManager.Instance.MainMetaConfig.CollectWithAdsMultiplier);
        OnCollectAllWithMultiplier.Invoke("rewarded", "get_double_afk_resources", "watched", MainManager.Instance._hasInternetConnection);
    }

    private void FailedAd() {
        OnCollectAllWithMultiplierFailed.Invoke("rewarded", "get_double_afk_resources", "canceled", MainManager.Instance._hasInternetConnection);
    }

    private void CollectResources(float multiplier) {
        MetaFieldManager.Instance.CollectResourcesFromAllMarks(multiplier);
    }
}