using UnityEngine;

[CreateAssetMenu(fileName = "MainManagerConfig", menuName = "Scriptable Objects/MainManagerConfig")]
public class MainManagerConfig : ScriptableObject
{
    public LevelConfig[] Levels;

    public Material _normal, _priorityMaterial, PriorityHighlightedMaterial;
}
