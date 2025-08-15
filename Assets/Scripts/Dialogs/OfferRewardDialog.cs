using System;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class OfferRewardDialog : DialogBase {
    [SerializeField]
    private ResourceCount _resourcePrefab;

    [SerializeField]
    private Transform _resourcesContainer;

    private Action _clickDefaultClaim;

    public override void SetData(object data) {
        Data dialogData = data as Data;

        _clickDefaultClaim = dialogData.ClickDefaultClaim;

        foreach (var resource in dialogData.OfflineResources) {
            ResourceCount newResource = Instantiate(_resourcePrefab, _resourcesContainer);
            newResource.SetData(resource.Key, resource.Value.ToString());
        }
    }

    public void ClickDefaultClaim() {
       
        //TODO add flying resources from dialog to counters
        //бустеры летят в кнопку плей, ресурсы в кнопку ресурсы, золото и хп - в каунтеры
        
        Hide().Forget();
        _clickDefaultClaim?.Invoke();
    }

    [Serializable]
    public class Data {
        public Action ClickDefaultClaim;
        public SerializedDictionary<ResourceType, int> OfflineResources;
    }
}