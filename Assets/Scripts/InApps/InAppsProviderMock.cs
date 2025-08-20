using System;

public class InAppsProviderMock : IInAppsProvider {
    public void Init() { }
    public string GetPrice(InApsTypes name) {
       return "0$";
    }
    public void Buy(InApsTypes name, Action onSuccess) {
        onSuccess?.Invoke();
    }
}
