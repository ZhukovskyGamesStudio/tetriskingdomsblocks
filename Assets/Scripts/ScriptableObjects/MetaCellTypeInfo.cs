using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(fileName = "MetaCellInfo", menuName = "Scriptable Objects/MetaCellInfo")]
public class MetaCellTypeInfo : CellTypeInfo {
    public ResourceType AfkResourceType;
    public int MaxAfkCapacity = 100;
    public float AfkProduceCountPerSecond = 1;

    public CellType UpgradeCellType;

    public List<UpgradeInfo> UpgradeCost;
}

[System.Serializable]
public class UpgradeInfo {
    public ResourceType ResourceType;
    public int Cost;
}