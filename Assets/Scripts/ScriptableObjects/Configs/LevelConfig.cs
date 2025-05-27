using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Scriptable Objects/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    public Transform guideForLevel;
    public CellsAndResourceTypesTableConfig cellTypesTableConfig;
}
