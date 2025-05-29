using UnityEngine;

[CreateAssetMenu(fileName = "CraftingCellInfo", menuName = "Scriptable Objects/CraftingCellInfo")]
public class CraftingCellInfo : ScriptableObject
{
    public CellTypeInfo cellsToCraft;

    public CellTypeInfo[] cellTypeToCraft;
    public CellTypeInfo[] cellTypeToCraftSecond;
}
