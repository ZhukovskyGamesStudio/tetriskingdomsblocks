using System.Collections.Generic;
using Abstract;

public class ZhukovskyAnalyticsManager : PreloadableSingleton<ZhukovskyAnalyticsManager> {
    private List<IAnalyticsProvider> AnalyticProviders { get; set; } = new();

    protected override void OnFirstInit() {
        base.OnFirstInit();

#if UNITY_EDITOR
        AnalyticProviders.Add(new AnalyticsProviderMock());
#endif
#if APPSFLYER
        AnalyticProviders.Add(new AppsflyerAnalyticsProvider());
#endif
#if APPMETRICA
        AnalyticProviders.Add(new AppmetricaAnalyticsProvider());
#endif
    }

    public void SendCustomEvent(string eventName, Dictionary<string, object> data, bool bSendEventBuffer = false) {
        foreach (IAnalyticsProvider provider in AnalyticProviders) {
            provider.SendEvent(eventName, data, bSendEventBuffer);
        }
    }
}