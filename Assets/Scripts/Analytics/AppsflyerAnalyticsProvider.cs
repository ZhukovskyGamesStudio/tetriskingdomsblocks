#if APPSFLYER
using System.Collections.Generic;
using AppsFlyerSDK;

public class AppsflyerAnalyticsProvider : IAnalyticsProvider {
    private const string DEV_KEY = "pWT7Si9aYsLLGEDwgWczDh";

    public AppsflyerAnalyticsProvider() {
        AppsFlyer.initSDK(DEV_KEY, ""); // app_id for iOS, empty string for Android
        AppsFlyer.startSDK();
    }

    public void SendEvent(string eventName, Dictionary<string, object> data, bool bSendEventBuffer) {
        var sentData = new Dictionary<string, string>();

        foreach (var kvp in data) {
            sentData[kvp.Key] = kvp.Value != null ? kvp.Value.ToString() : string.Empty;
        }

        AppsFlyer.sendEvent(eventName, sentData);
    }
}
#endif