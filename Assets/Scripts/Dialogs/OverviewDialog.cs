using System;
using System.Collections.Generic;
using UnityEngine;

public class OverviewDialog : DialogBase {
    [SerializeField]
    private OverviewResource _resourcePrefab;

    [SerializeField]
    private Transform _resourcesParent;

    public override void SetData(object data) {
        Data dialogData = data as Data;

        foreach (OverviewResourceInfo resource in dialogData.Resources) {
            if (!dialogData.ShowResource[resource.ResourceType]) continue;
            
            OverviewResource newResource = Instantiate(_resourcePrefab, _resourcesParent);
            newResource.SetData(resource);
        }
    }

    [Serializable]
    public class Data {
        public List<OverviewResourceInfo> Resources;
        public Dictionary<ResourceType, bool> ShowResource;
    }
}

public class OverviewResourceInfo {
    public ResourceType ResourceType;
    public int Count;
    public int Income;

    public OverviewResourceInfo(ResourceType type, int count, int income) {
        ResourceType = type;
        Count = count;
        Income = income;
    }
}