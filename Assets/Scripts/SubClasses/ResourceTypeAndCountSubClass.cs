using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class ResourceTypeAndCountSubClass
{
    [FormerlySerializedAs("resourceType")] public ResourceType ResourceType;
    [FormerlySerializedAs("resourceCount")] public int ResourceCount;
}
