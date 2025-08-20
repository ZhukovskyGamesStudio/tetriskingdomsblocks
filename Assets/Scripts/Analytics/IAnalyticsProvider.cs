using System.Collections.Generic;

public interface IAnalyticsProvider {
	public void SendEvent(string eventName, Dictionary<string, object> data, bool bSendEventBuffer);
}