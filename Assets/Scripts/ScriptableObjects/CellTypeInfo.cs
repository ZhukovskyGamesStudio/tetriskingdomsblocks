using UnityEngine;

[CreateAssetMenu(fileName = "CellTypeInfo", menuName = "Scriptable Objects/CellTypeInfo")]
public class CellTypeInfo : ScriptableObject
{
    public CellView cellPrefab;
    public CellType cellType;
    public string cellForm;
    public ResourceTypeAndCountSubClass[] resourcesForPlace;
    public ResourceTypeAndCountSubClass[] resourcesForDestroy;
    public string cellName;
}
