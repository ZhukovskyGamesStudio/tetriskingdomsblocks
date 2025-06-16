using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "CellTypeInfo", menuName = "Scriptable Objects/CellTypeInfo")]
public class CellTypeInfo : ScriptableObject
{
    [FormerlySerializedAs("cellPrefab")] public CellView CellPrefab;
    [FormerlySerializedAs("cellType")] public CellType CellType;
    [FormerlySerializedAs("cellForm")] public FigureFormConfig CellForm;
    public Color MarkCellColor;
    [FormerlySerializedAs("resourcesForPlace")] public ResourceTypeAndCountSubClass[] ResourcesForPlace;
    [FormerlySerializedAs("resourcesForDestroy")] public ResourceTypeAndCountSubClass[] ResourcesForDestroy;
    [FormerlySerializedAs("cellName")] public string CellName;
    public int MultiplayerForSameResourceType;

    public ResourceType AfkResourceType;
    public int MaxAfkCapacity;
    public float AfkProduceCountPerSecond;
    
    public float ChanceToSpawn;
}
