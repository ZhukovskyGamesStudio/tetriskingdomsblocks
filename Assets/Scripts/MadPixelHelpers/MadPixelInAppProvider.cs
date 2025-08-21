#if MADPIXEL
using System;
using System.Collections.Generic;
using MadPixel.InApps;
using UnityEngine.Purchasing;

public class MadPixelInAppProvider : IInAppsProvider {
    private readonly Dictionary<string, Action> _successCallbacks = new Dictionary<string, Action>();

    public void Init() {
        MobileInAppPurchaser.Instance.OnPurchaseResult += OnPurchaseResult;
        MobileInAppPurchaser.Instance.Init();
    }

    public string GetPrice(InApsTypes name) {
        string nameS = InApsIds.InAps[name];
        Product product = MobileInAppPurchaser.Instance.GetProduct(nameS);
        if (product != null) {
            return product.metadata.localizedPriceString;
        }

        return "";
    }

    public void Buy(InApsTypes name, Action onSuccess) {
        string nameS = InApsIds.InAps[name];
        if (!_successCallbacks.TryAdd(nameS, onSuccess)) {
            _successCallbacks[nameS] = onSuccess;
        }

        MobileInAppPurchaser.BuyProduct(nameS);
    }

    private void OnPurchaseResult(Product product) {
        if (product == null) {
            return;
        }

        string id = product.definition.id;

        if (_successCallbacks.TryGetValue(id, out Action callback)) {
            callback?.Invoke();
        }
    }
}
#endif