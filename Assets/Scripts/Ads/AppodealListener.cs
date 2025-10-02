using System;
using System.Collections.Generic;
using AppodealStack.Monetization.Common;
using UnityEngine;

public class AppodealListener : MonoBehaviour, IAppodealInitializationListener, IRewardedVideoAdListener {
    private Action _onInit, _onSuccess, _onFail;

    public void Init(Action onInit) {
        _onInit = onInit;
    }

    public void SetEvents(Action onSuccess, Action onFail) {
        _onSuccess = onSuccess;
        _onFail = onFail;
    }

    public void OnInitializationFinished(List<string> errors) {
        if (errors != null) {
            if (errors.Count > 0) {
                Debug.Log("Appodeal errors: " + string.Join(", ", errors));
            }
        }

        Debug.Log("Appodeal initialized");
        _onInit?.Invoke();
    }

    public void OnRewardedVideoLoaded(bool precache) {
        Debug.Log("Appodeal OnRewardedVideoLoaded");
    }

    public void OnRewardedVideoFailedToLoad() {
        Debug.Log("Appodeal OnRewardedVideoFailedToLoad");
    }

    public void OnRewardedVideoShowFailed() {
        Debug.Log("Appodeal OnRewardedVideoShowFailed");
        _onFail?.Invoke();
    }

    public void OnRewardedVideoShown() {
        Debug.Log("Appodeal OnRewardedVideoShown");
    }

    public void OnRewardedVideoFinished(double amount, string name) {
        Debug.Log($"Appodeal OnRewardedVideoFinished: {amount} {name}");
    }

    public void OnRewardedVideoClosed(bool finished) {
        Debug.Log("Appodeal OnRewardedVideoClosed, finished: " + finished);
        if (finished) {
            ZhukovskyAnalyticsManager.Instance.SendCustomEvent("rv_finish", new Dictionary<string, object>());
            _onSuccess?.Invoke();
        } else {
            ZhukovskyAnalyticsManager.Instance.SendCustomEvent("rv_finish_failed", new Dictionary<string, object>());
            _onFail?.Invoke();
        }
    }

    public void OnRewardedVideoExpired() {
        Debug.Log("Appodeal OnRewardedVideoExpired");
    }

    public void OnRewardedVideoClicked() {
        Debug.Log("Appodeal OnRewardedVideoClicked");
    }
}