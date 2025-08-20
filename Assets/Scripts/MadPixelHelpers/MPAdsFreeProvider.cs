#if MADPIXEL
using System;
using System.Collections.Generic;
using MadPixel;
using MadPixel.InApps;
using UnityEngine.Purchasing;

public class MPAdsFreeProvider : IAdsFreeProvider
{
    private readonly Dictionary<string, Action> _successCallbacks = new Dictionary<string, Action>();
    private bool _adsfree;//адсфри тогл
    public void Init() {
        MobileInAppPurchaser.Instance.OnPurchaseResult += OnPurchaseResult;
        MobileInAppPurchaser.Instance.Init();
    }
    public string GetPrice(string name) {
        Product product = MobileInAppPurchaser.Instance.GetProduct(name);
        if (product != null) {
            return product.metadata.localizedPriceString;
        }

        return "";
    }

    public void Buy(string name, Action onSuccess) {
        if (!_successCallbacks.TryAdd(name, onSuccess)) {
            _successCallbacks[name] = onSuccess;
        }
        
        MobileInAppPurchaser.BuyProduct(name);
    }

    private void OnPurchaseResult(Product product) {
        if (product == null) {
            return;
        } 
        
        string id = product.definition.id;

        if (_successCallbacks.TryGetValue(id, out Action callback)) {
            callback?.Invoke();
            AdsManager.CancelAllAds();
            _adsfree = true;
        }

        
    }
}
#endif