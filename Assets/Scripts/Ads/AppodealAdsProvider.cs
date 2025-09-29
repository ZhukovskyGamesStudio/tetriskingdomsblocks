using System;
using AppodealStack.Monetization.Api;
using AppodealStack.Monetization.Common;
using UnityEngine;
using Object = UnityEngine.Object;

public class AppodealAdsProvider : IAdsProvider {
    private const string _appKey = "ee05866da465fb174578989745854d1091a25c760dfb9e4b";
    private readonly AppodealListener _appodealListener;

    private bool _isInited;

    public AppodealAdsProvider() {
        int adTypes = AppodealShowStyle.RewardedVideo;

        _appodealListener = Object.Instantiate(new GameObject("AppodealListener")).AddComponent<AppodealListener>();
        _appodealListener.Init(OnInit);
        Object.DontDestroyOnLoad(_appodealListener.gameObject);
        Appodeal.SetTesting(true);
        Appodeal.SetLogLevel( AppodealLogLevel.Verbose);
        Appodeal.SetAutoCache(AppodealAdType.RewardedVideo, true);
        Appodeal.Initialize(_appKey, adTypes, _appodealListener);
        Appodeal.SetRewardedVideoCallbacks(_appodealListener);
    }

    private void OnInit() {
        _isInited = true;
        Appodeal.ShowMediationDebugger();
    }

    public void ShowRewardedAd(string placeId, Action onSuccess, Action onFail) {
        _appodealListener.SetEvents(onSuccess, onFail);
        Appodeal.Show(AppodealShowStyle.RewardedVideo);
    }

    public void ShowInterAd(string placeId, Action onSuccess = null, Action onFail = null) {
        _appodealListener.SetEvents(onSuccess, onFail);
    }

    public void SetBanners(bool isActive) { }

    public bool IsAdsReady() {
        return _isInited;
    }

    public void CancelAds() { }
}