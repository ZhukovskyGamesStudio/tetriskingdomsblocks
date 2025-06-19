using System;
using ScriptableObjects.Configs;
using UnityEngine;

public class ConfigsManager : MonoBehaviour {
    [field: SerializeField]

    public DragConfig DragConfig { get; private set; }

    public static ConfigsManager Instance;

    private void Awake() {
        Instance = this;
    }
}