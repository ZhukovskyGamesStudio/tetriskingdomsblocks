using UnityEngine;

[CreateAssetMenu(fileName = "CellsAndResourceTypesTableConfig", menuName = "Scriptable Objects/CellsAndResourceTypesTableConfig")]
public class CellsAndResourceTypesTableConfig : ScriptableObject
{
    public CellTypeInfo[] cellsToSpawn;
}
