using UnityEngine;

[CreateAssetMenu(fileName = "CellsAndResourceTypesTableConfig", menuName = "Scriptable Objects/CellsAndResourceTypesTableConfig")]
public class CellsAndResourceTypesTableConfig : ScriptableObject {
    [field: SerializeField]
    public CellType[] CellsToSpawn { get; set; }
}