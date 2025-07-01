using UnityEngine;

[CreateAssetMenu(fileName = "MainManagerConfig", menuName = "Scriptable Objects/MainManagerConfig")]
public class MainManagerConfig : ScriptableObject
{
    public LevelConfig[] Levels;
}
