using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Scriptable Objects/LevelConfig")]
public class LevelConfig : ScriptableObject {
    [SerializedDictionary]
    public SerializedDictionary<ResourceType, int> Tasks;

    [field: SerializeField]
    public string GuideForLevelText { get; private set; }

    [field: SerializeField]
    public CellTypeInfo[] CurrentGuaranteedFirstCells { get; private set; }

    [field: SerializeField]
    public List<CellType> CellsToSpawn { get; private set; }

    [field: SerializeField]
    public int GoldAmount { get; private set; } = 100;

    [field: SerializeField]
    public int MagicCubesCount { get; private set; }

    [field: SerializeField]
    public Transform TutorialObject { get; private set; }

    [field: SerializeField]
    public StartFieldConfig StartFieldConfig { get; private set; }

    [field: SerializeField]

    public int MovesCount { get; private set; }
    [field: SerializeField]
    public int[] FirstFiguresCount { get; private set; }
}