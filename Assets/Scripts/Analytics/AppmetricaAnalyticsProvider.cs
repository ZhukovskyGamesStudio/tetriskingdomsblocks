#if APPMETRICA
using System.Collections.Generic;
using Io.AppMetrica;
using Newtonsoft.Json; // нужен Json.NET for Unity

public class AppmetricaAnalyticsProvider : IAnalyticsProvider {
    public void SendEvent(string eventName, Dictionary<string, object> data, bool bSendEventBuffer) {
        string json = JsonConvert.SerializeObject(data);
        AppMetrica.ReportEvent(eventName, json);
    }
}
#endif