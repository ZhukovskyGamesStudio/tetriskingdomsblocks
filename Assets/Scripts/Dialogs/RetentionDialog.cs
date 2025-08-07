using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class RetentionDialog : DialogBase {
    [SerializeField]
    private ResourceCount _resourcePrefab;

    [SerializeField]
    private Transform _resourcesContainer;

    private Action _clickDoubleClaim;
    private Action _clickDefaultClaim;
    
    public override void SetData(object data) {
        Data dialogData = data as Data;
        
        _clickDoubleClaim = dialogData.ClickDoubleClaim;
        _clickDefaultClaim = dialogData.ClickDefaultClaim;

        foreach (RetentionResource resource in dialogData.OfflineResources) {
            ResourceCount newResource = Instantiate(_resourcePrefab, _resourcesContainer);
            newResource.SetData(resource.Resource, resource.Count.ToString());
        }
    }

    public void ClickDoubleClaim() {
        Hide().Forget();
        //show ad
        _clickDoubleClaim?.Invoke();
    }
    
    public void ClickDefaultClaim() {
        Hide().Forget();
        _clickDefaultClaim?.Invoke();
    }

    [Serializable]
    public class Data {
        public Action ClickDoubleClaim;
        public Action ClickDefaultClaim;
        public List<RetentionResource> OfflineResources;
    }
    
    public class RetentionResource {
        public ResourceType Resource;
        public int Count;
    }
}