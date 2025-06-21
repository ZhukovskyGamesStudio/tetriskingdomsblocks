using Cysharp.Threading.Tasks;
using UnityEngine;

public class CollectAllDialog : DialogBase {
    public void CollectAll() {
        Hide().Forget();
        CollectResources(1);
    }

    public void CollectAllWithAds() {
        Hide().Forget();
        AdsManager.Instance.ShowRewarded(CollectAllWithMultiplier).Forget();
    }

    private void CollectAllWithMultiplier() {
        CollectResources(MetaManager.Instance.MainMetaConfig.CollectWithAdsMultiplier);
    }

    private void CollectResources(float multiplier) {
        MetaManager.Instance.CollectResourcesFromAllMarks(multiplier);
    }
}