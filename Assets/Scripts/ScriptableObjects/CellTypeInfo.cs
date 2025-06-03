using UnityEngine;

[CreateAssetMenu(fileName = "CellTypeInfo", menuName = "Scriptable Objects/CellTypeInfo")]
public class CellTypeInfo : ScriptableObject
{
    public CellType cellType;
    public CellView cellPrefab;
    public string cellForm;
    public Color MarkCellColor;
    public ResourceTypeAndCountSubClass[] resourcesForPlace;
    public ResourceTypeAndCountSubClass[] resourcesForDestroy;
    public string cellName;
    public int MultiplayerForSameResourceType;
    public float ChanceToSpawn;
}
