using Abstract;

public class ZhukovskyAdsManager : PreloadableSingleton<ZhukovskyAdsManager> {
    public float GameInterAdCooldown;
    public IAdsProvider AdsProvider { get; private set; }
    public InterAdRunner InterAdRunner { get; private set; }

    protected override void OnFirstInit() {
        base.OnFirstInit();
#if APPODEAL
        AdsProvider = new AppodealAdsProvider();
#elif YG_PLATFORM
        AdsProvider = new YGAdsProvider();
#else
        AdsProvider = new AdsProviderMock();
#endif

        InterAdRunner = new InterAdRunner(GameInterAdCooldown, AdsProvider);
        if (StorageManager.GameDataMain.IsIntersUnlocked) {
            InterAdRunner.IsInterAdRunEnabled = true;
        }

        if (StorageManager.GameDataMain.HasNoAds) {
            CancelAdsAndDisableButton();
        }
    }

    public void EnableIntersAndBanners() {
        InterAdRunner.IsInterAdRunEnabled = true;
        AdsProvider.SetBanners(true);
        StorageManager.SaveGame();
    }

    public void CancelAdsAndDisableButton() {
        AdsProvider.CancelAds();
        AdsProvider.SetBanners(false);
    }
    
    private void Update() {
        if (InterAdRunner == null) return;
        InterAdRunner.Update();
    }
}