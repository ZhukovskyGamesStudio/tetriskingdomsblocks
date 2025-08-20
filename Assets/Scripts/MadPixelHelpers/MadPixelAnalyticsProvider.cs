using System;
using System.Collections.Generic;

public class MadPixelAnalyticsProvider : IAnalyticsProvider {
    public void SendEvent(string eventName, Dictionary<string, object> data, bool bSendEventBuffer) {
        MadPixelAnalytics.AnalyticsManager.CustomEvent(eventName, data, bSendEventBuffer);
    }
}