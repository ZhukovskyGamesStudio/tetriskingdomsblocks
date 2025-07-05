using System;
using System.Collections.Generic;
using ScriptableObjects.Configs;
using UnityEngine;

public class ConfigsManager : MonoBehaviour {
    [field: SerializeField]

    public DragConfig DragConfig { get; private set; }
    [field: SerializeField]
    public BoostersConfig BoostersConfig { get; private set; }
    [field: SerializeField]
    public SpritesForTasksConfig SpritesForTasksConfig{ get; private set; }
    public Dictionary<string, Sprite> SpritesForTasks { get; private set; }
    public static ConfigsManager Instance;

    private void Awake() {
        Instance = this;

        SpritesForTasks = new Dictionary<string, Sprite>();
        foreach (var item in SpritesForTasksConfig.NameAndImages)
        {
            SpritesForTasks.Add(item.SpriteName, item.SpriteToTask);
        }

        
        
    }
}