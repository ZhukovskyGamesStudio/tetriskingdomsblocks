using System;
using UnityEngine;

public class AdsFreeProviderMock : IAdsFreeProvider {
    public void Init() { }

    public string GetPrice(string name) {
        return "0$";
    }

    public void Buy(string name, Action onSuccess) {
        Debug.Log("Ads cancelled!");
    }
}