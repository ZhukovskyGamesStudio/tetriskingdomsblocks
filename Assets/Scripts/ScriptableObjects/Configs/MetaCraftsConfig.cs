using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(fileName = "MetaCraftsConfig", menuName = "Scriptable Objects/MetaCraftsConfig")]
public class MetaCraftsConfig : ScriptableObject {
    public List<MetaCraftInfo> Crafts;
}

[Serializable]
public class MetaCraftInfo {
    public CellView ResultPrefab;
    public CellTypeInfo ResultCellTypeInfo;
    public string CraftName;
    public string Description;
    public int BonusPercents;
    public ResourceType BonusResource;
    public CellType NeededCell;
    [SerializedDictionary]
    public SerializedDictionary<ResourceType, int> NeededResources;
}
