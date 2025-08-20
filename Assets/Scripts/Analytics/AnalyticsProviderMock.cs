using System.Collections.Generic;
using UnityEngine;

public class AnalyticsProviderMock : IAnalyticsProvider {
	public void SendEvent(string eventName, Dictionary<string, object> data, bool bSendEventBuffer) {
		Debug.Log("AnalyticsProviderMock: Sending event: " + eventName);
	}
}