using UnityEngine;

[CreateAssetMenu(fileName = "CellTypeInfo", menuName = "Scriptable Objects/CellTypeInfo")]
public class CellTypeInfo : ScriptableObject
{
    public CellView cellPrefab;
    public CellType cellType;
    public FigureFormConfig cellForm;
    public Color MarkCellColor;
    public ResourceTypeAndCountSubClass[] resourcesForPlace;
    public ResourceTypeAndCountSubClass[] resourcesForDestroy;
    public string cellName;
    public int MultiplayerForSameResourceType;
    public float ChanceToSpawn;
}
