#if MADPIXEL
using System;
using MadPixel;

public class MadPixelAdsProvider : IAdsProvider {
    private bool _isShowing;
    private Action _onAdShown;
    private Action _onFail;

    public void ShowRewardedAd(string placeId, Action onSuccess, Action onFail) {
        if (_isShowing) {
            return;
        }

        _onAdShown = onSuccess;
        _onFail = onFail;
        _isShowing = true;
        AdsManager.EResultCode code = AdsManager.ShowRewarded(ZhukovskyAdsManager.Instance.gameObject, OnFinishAds, placeId);
        if (code != AdsManager.EResultCode.OK) {
            _onFail?.Invoke();
            _isShowing = false;
        }
    }

    public void ShowInterAd(string placeId, Action onSuccess = null, Action onFail = null) {
        if (_isShowing) {
            return;
        }
        
        _onAdShown = onSuccess;
        _onFail = onFail;
        _isShowing = true;
        AdsManager.EResultCode code = AdsManager.ShowInter(ZhukovskyAdsManager.Instance.gameObject, OnFinishAds, placeId);
        if (code != AdsManager.EResultCode.OK) {
            _onFail?.Invoke();
            _isShowing = false;
        }
    }

    public void SetBanners(bool isActive) {
        AdsManager.ToggleBanner(isActive, MaxSdkBase.AdViewPosition.CenterRight);
    }

    public bool IsAdsReady() {
        return AdsManager.Ready();
    }

    public void CancelAds() {
        AdsManager.CancelAllAds();
    }

    private void OnFinishAds(bool adShowSuccess) {
        if (adShowSuccess) {
            _onAdShown?.Invoke();
        } else {
            _onFail?.Invoke();
        }

        _isShowing = false;
    }
}
#endif