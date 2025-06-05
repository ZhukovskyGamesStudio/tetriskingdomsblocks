using UnityEngine;

[CreateAssetMenu(fileName = "MainMetaConfig", menuName = "Scriptable Objects/MainMetaConfig", order = 0)]
public class MainMetaConfig : ScriptableObject {
    [field: SerializeField]
    [Min(1)]
    public float CollectWithAdsMultiplier { get; private set; } = 2f;
}