using UnityEngine;

[CreateAssetMenu(fileName = "MainManagerConfig", menuName = "Scriptable Objects/MainManagerConfig")]
public class MainManagerConfig : ScriptableObject {
    public LevelConfig[] Levels;

    public int SawmillUnlockLevel = 30;
    public int LevelsToStartShowRateus = 5;

    public Material _normal, _priorityMaterial, PriorityHighlightedMaterial;
}