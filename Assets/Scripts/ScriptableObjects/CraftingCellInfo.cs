using UnityEngine;

[CreateAssetMenu(fileName = "CraftingCellInfo", menuName = "Scriptable Objects/CraftingCellInfo")]
public class CraftingCellInfo : ScriptableObject
{
    public CellTypeInfo cellsToCraft;

    public CellType[] cellTypeToCraft;
}
