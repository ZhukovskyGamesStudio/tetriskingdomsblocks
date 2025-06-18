using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MainMetaConfig", menuName = "Scriptable Objects/MainMetaConfig", order = 0)]
public class MainMetaConfig : ScriptableObject {
    [field: SerializeField]
    [Min(1)]
    public float CollectWithAdsMultiplier { get; private set; } = 2f;

    public List<CellTypeInfo> CellsConfigs;
        
    public int FieldSize;
    
    public float[] ResourceMultipliers;
    
    public float resourceMarksUpdateCouldown;

    public float CameraDragSpeed;
}