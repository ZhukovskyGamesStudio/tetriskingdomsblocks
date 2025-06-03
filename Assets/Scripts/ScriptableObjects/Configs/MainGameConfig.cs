using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MainGameConfig", menuName = "Scriptable Objects/MainGameConfig")]
public class MainGameConfig : ScriptableObject
{
    public bool resourceOnPlaceCell;
    public bool resourceOnDestroyCell;
    public bool bonusResourcesOnDestroyLine;

    public int minResourcesToTask;
    public int maxResourcesToTask;

    public LevelConfig[] levels;
    public CraftingCellInfo[] cellsToCraft;

    public CellTypeInfo[] guaranteedFirstCells;

    public List<CellTypeInfo> CellsConfigs;
    
    public int fieldSize;

}
