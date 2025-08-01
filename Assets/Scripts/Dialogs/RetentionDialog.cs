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
    
    public override void SetData(object data) {
        Data dialogData = data as Data;
        
        _clickDoubleClaim = dialogData.ClickDoubleClaim;

        foreach (RetentionResource resource in dialogData.OfflineResources) {
            ResourceCount newResource = Instantiate(_resourcePrefab, _resourcesContainer);
            newResource.SetData(resource.Resource, resource.Count.ToString());
        }
    }

    public void ClickDoubleClaim() {
        Hide().Forget();
        _clickDoubleClaim.Invoke();
    }

    [Serializable]
    public class Data {
        public Action ClickDoubleClaim;
        public List<RetentionResource> OfflineResources;
    }
    
    public class RetentionResource {
        public ResourceType Resource;
        public int Count;
    }
}