using System;
using System.Collections.Generic;
using UnityEngine;

public class OverviewDialog : DialogBase {
    [SerializeField]
    private ResourceOutput _resourcePrefab;

    [SerializeField]
    private Transform _resourcesParent;

    public override void SetData(object data) {
        Data dialogData = data as Data;

        foreach (ResourceCountAndIncome resource in dialogData.Resources) {
            ResourceOutput newResource = Instantiate(_resourcePrefab, _resourcesParent);
            newResource.SetData(resource.Count, resource.Income + "/sec", resource.Type);
        }
    }

    [Serializable]
    public class Data {
        public List<ResourceCountAndIncome> Resources;
    }
}

public class ResourceCountAndIncome {
    public ResourceType Type;
    public int Count;
    public int Income;

    public ResourceCountAndIncome(ResourceType type, int count, int income) {
        Type = type;
        Count = count;
        Income = income;
    }
}