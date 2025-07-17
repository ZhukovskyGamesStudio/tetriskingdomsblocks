using UnityEngine;

[CreateAssetMenu(fileName = "MetaCellInfo", menuName = "Scriptable Objects/MetaCellInfo")]
public class MetaCellTypeInfo : CellTypeInfo {
    public ResourceType AfkResourceType;
    public int MaxAfkCapacity = 100;
    public float AfkProduceCountPerSecond = 1;
    public int UpgradeCost = 10;
    public CellType UpgradeCellType;
}