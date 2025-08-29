using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(fileName = "MetaCellInfo", menuName = "Scriptable Objects/MetaCellInfo")]
public class MetaCellTypeInfo : CellTypeInfo {
    [Header("Afk Production")]
    public ResourceType AfkResourceType;

    public int MaxAfkCapacity = 100;

    [Min(0)]
    public float AfkProduceCountPerHour = 1;

    public bool IsCellsCountAffectCapacityAndProduction = true;
    public bool IsCellsCountAffectCost = true;

    [Header("Upgrade")]
    public CellType UpgradeCellType;

    public SerializedDictionary<ResourceType, int> UpgradeCostDict;
}