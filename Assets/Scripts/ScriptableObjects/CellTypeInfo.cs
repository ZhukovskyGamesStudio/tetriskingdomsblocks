using UnityEngine;

[CreateAssetMenu(fileName = "CellTypeInfo", menuName = "Scriptable Objects/CellTypeInfo")]
public class CellTypeInfo : ScriptableObject
{
    public CellView cellPrefab;
    public CellType cellType;
    public ResourceTypeAndCountSubClass[] resourcesForPlace;
    public ResourceTypeAndCountSubClass[] resourcesForDestroy;
}
