public class CollectAllDialog : DialogBase {
    public void CollectAll() {
        CollectResources(1);
    }

    public void CollectAllWithAds() {
        Hide();
        AdsManager.Instance.ShowRewarded(CollectAllWithMultiplier);
    }

    private void CollectAllWithMultiplier() {
        CollectResources(MetaManager.Instance.MainMetaConfig.CollectWithAdsMultiplier);
    }

    private void CollectResources(float multiplier) { }
}