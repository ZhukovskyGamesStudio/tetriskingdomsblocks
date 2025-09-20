using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class GooglePlayInAppsProvider : IInAppsProvider, IDetailedStoreListener {
    private IStoreController controller;
    private IExtensionProvider extensions;

    private Dictionary<string, Action> OnSucess = new();

    public void Init() {
        if (IsInitialized()) {
            return;
        }

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        foreach (var kvp in InApsIds.InAps) {
            builder.AddProduct(kvp.Value, ProductType.Consumable);
        }

        UnityPurchasing.Initialize(this, builder);
    }

    private bool IsInitialized() {
        return controller != null && extensions != null;
    }

    public string GetPrice(InApsTypes name) {
        var product = controller.products.WithID(InApsIds.InAps[name]).metadata;
        return $"{product.localizedPriceString} {product.isoCurrencyCode}";
    }

    public void Buy(InApsTypes name, Action onSuccess) {
        OnSucess.Add(InApsIds.InAps[name], onSuccess);
        BuyProduct(InApsIds.InAps[name]);
    }

    private void BuyProduct(string productId) {
        if (IsInitialized()) {
            Product product = controller.products.WithID(productId);
            if (product != null && product.availableToPurchase) {
                Debug.Log($"Purchasing product: {product.definition.id}");
                controller.InitiatePurchase(product);
            } else {
                Debug.Log("BuyProduct: FAIL. Not initialized or product not found/available.");
            }
        } else {
            Debug.Log("BuyProduct: FAIL. IAP is not initialized.");
        }
    }

    // IStoreListener callbacks
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions) {
        Debug.Log("OnInitialized: PASS");
        this.controller = controller;
        this.extensions = extensions;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription) {
        Debug.Log($"OnPurchaseFailed: FAIL. Reason: {failureDescription} ");
    }

    public void OnInitializeFailed(InitializationFailureReason error) {
        Debug.Log($"OnInitializeFailed: FAIL. Reason: {error}");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message) {
        Debug.Log($"OnInitializeFailed: FAIL. Reason: {error} {message}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args) {
        string id = args.purchasedProduct.definition.id;
        Debug.Log($"ProcessPurchase: PASS. Product: {id}");
        if (OnSucess.TryGetValue(id, out var action)) {
            action?.Invoke();
            OnSucess.Remove(id);
        } else {
            Debug.Log($"ProcessPurchase: FAIL. OnSuccess: empty dictionary");
        }

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason) {
        Debug.Log($"OnPurchaseFailed: FAIL. Product: {product.definition.id}, Reason: {failureReason}");
    }
}