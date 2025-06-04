using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "CellsAndResourceTypesTableConfig", menuName = "Scriptable Objects/CellsAndResourceTypesTableConfig")]
public class CellsAndResourceTypesTableConfig : ScriptableObject
{
    [FormerlySerializedAs("cellsToSpawn")] public CellTypeInfo[] CellsToSpawn;
}
