using System;
using System.Collections.Generic;
using ScriptableObjects.Configs;
using UnityEngine;

public class ConfigsManager : MonoBehaviour {
    public static ConfigsManager Instance;
    
    [field: SerializeField]
    public DragConfig DragConfig { get; private set; }
    
    [field: SerializeField]
    public BoostersConfig BoostersConfig { get; private set; }
    
    [field: SerializeField]
    public MetaCraftsConfig MetaCraftsConfig { get; private set; }

    private void Awake() {
        Instance = this;
    }
}