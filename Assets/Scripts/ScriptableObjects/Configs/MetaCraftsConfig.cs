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
    public string CraftName;
    public string Description;
    public CellType NeededCell;
    public int NeededCellLevel;
    public Vector2Int PieceSize;
    public CellType ResultCell;
    [SerializedDictionary]
    public SerializedDictionary<ResourceType, int> NeededResources;
}
