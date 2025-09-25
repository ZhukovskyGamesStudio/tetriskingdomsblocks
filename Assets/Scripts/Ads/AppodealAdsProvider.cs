using System;

public class AppodealAdsProvider : IAdsProvider {
    public void ShowRewardedAd(string placeId, Action onSuccess, Action onFail) {
        throw new NotImplementedException();
    }
    public void ShowInterAd(string placeId, Action onSuccess = null, Action onFail = null) {
        throw new NotImplementedException();
    }
    public void SetBanners(bool isActive) {
        throw new NotImplementedException();
    }
    public bool IsAdsReady() {
        throw new NotImplementedException();
    }
    public void CancelAds() {
        throw new NotImplementedException();
    }
}
