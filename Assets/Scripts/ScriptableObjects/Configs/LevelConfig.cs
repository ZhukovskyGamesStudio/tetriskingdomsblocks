using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Scriptable Objects/LevelConfig")]
public class LevelConfig : ScriptableObject {
    [field: SerializeField]
    public TaskInfo[] Tasks { get; private set; }

    [field: SerializeField]
    public string GuideForLevelText { get; private set; }

    [field: SerializeField]
    public CellTypeInfo[] CurrentGuaranteedFirstCells { get; private set; }

    [field: SerializeField]
    public CellsAndResourceTypesTableConfig CellTypesTableConfig { get; private set; }
    
      [field: SerializeField]
        public Transform TutorialObject { get; private set; }
        
        [field: SerializeField]
        public StartFieldConfig StartFieldConfig { get; private set; }
}