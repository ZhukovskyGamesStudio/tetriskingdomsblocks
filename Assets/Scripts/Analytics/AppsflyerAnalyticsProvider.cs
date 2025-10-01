#if APPSFLYER
using System;
using System.Collections.Generic;
using AppsFlyerSDK;
using UnityEngine;

public class AppsflyerAnalyticsProvider : IAnalyticsProvider {
    private const string DEV_KEY = "pWT7Si9aYsLLGEDwgWczDh";

    public AppsflyerAnalyticsProvider() {
        AppsFlyer.OnRequestResponse += AppsFlyerOnRequestResponse;
        AppsFlyer.OnInAppResponse += AppsFlyerOnInAppResponse;
        AppsFlyer.setIsDebug(true);
        Debug.Log("Appsflyer initialized");
        AppsFlyer.initSDK(DEV_KEY, ""); // app_id for iOS, empty string for Android
        AppsFlyer.startSDK();
    }

    public void SendEvent(string eventName, Dictionary<string, object> data, bool bSendEventBuffer) {
        var sentData = new Dictionary<string, string>();

        foreach (var kvp in data) {
            sentData[kvp.Key] = kvp.Value != null ? kvp.Value.ToString() : string.Empty;
        }

        Debug.Log("Appsflyer SendEvent " + eventName);
        AppsFlyer.sendEvent(eventName, sentData);
    }

    private void AppsFlyerOnRequestResponse(object sender, EventArgs e) {
        var args = e as AppsFlyerRequestEventArgs;
        Debug.Log("AppsFlyerOnRequestResponse status code " + args?.statusCode);
    }

    private void AppsFlyerOnInAppResponse(object sender, EventArgs args) {
        var afArgs = args as AppsFlyerRequestEventArgs;
        Debug.Log("AppsFlyerOnRequestResponse status code " + afArgs?.statusCode);
    }
}
#endif