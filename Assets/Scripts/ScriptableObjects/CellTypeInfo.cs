using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "CellTypeInfo", menuName = "Scriptable Objects/CellTypeInfo")]
public class CellTypeInfo : ScriptableObject
{
    [FormerlySerializedAs("cellType")] public CellType CellType;
    [FormerlySerializedAs("cellForm")] public FigureFormConfig CellForm;
    public Color MarkCellColor;
    [FormerlySerializedAs("cellName")] public string CellName;
    
    public float ChanceToSpawn = 1;
}
