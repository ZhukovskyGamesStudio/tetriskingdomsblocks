using AYellowpaper.SerializedCollections;
using UnityEngine;

public class SpritesManager : MonoBehaviour {
    public static SpritesManager Instance;

    [SerializedDictionary("Resource Type", "Sprite")]
    public SerializedDictionary<ResourceType, Sprite> ResourcesSprites;

    [field: SerializeField]
    public Sprite LineSprite;
    
    private void Awake() {
        Instance = this;
    }

    public Sprite GetSprite(ResourceType resource) {
        return ResourcesSprites[resource];
    }
}
