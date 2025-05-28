using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Scriptable Objects/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    public TaskInfo[] tasks;
    public string guideForLevelText;
    public CellTypeInfo[] currentGuaranteedFirstCells;
    public CellsAndResourceTypesTableConfig cellTypesTableConfig;
}
