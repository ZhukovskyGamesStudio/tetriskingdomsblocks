using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "CraftingCellInfo", menuName = "Scriptable Objects/CraftingCellInfo")]
public class CraftingCellInfo : ScriptableObject
{
    [FormerlySerializedAs("cellsToCraft")] public CellTypeInfo CellsToCraft;

    [FormerlySerializedAs("cellTypeToCraft")] public CellTypeInfo[] CellTypeToCraft;
    [FormerlySerializedAs("cellTypeToCraftSecond")] public CellTypeInfo[] CellTypeToCraftSecond;
}
