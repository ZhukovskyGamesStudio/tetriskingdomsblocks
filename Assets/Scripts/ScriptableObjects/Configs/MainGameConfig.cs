using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "MainGameConfig", menuName = "Scriptable Objects/MainGameConfig")]
public class MainGameConfig : ScriptableObject
{
    public bool resourceOnPlaceCell;
    public bool resourceOnDestroyCell;
    public bool bonusResourcesOnDestroyLine;

    [FormerlySerializedAs("levels")] public LevelConfig[] Levels;
    [FormerlySerializedAs("cellsToCraft")] public CraftingCellInfo[] CellsToCraft;
    [FormerlySerializedAs("fieldSize")] public int FieldSize;

}
