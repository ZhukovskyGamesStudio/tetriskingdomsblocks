using AYellowpaper.SerializedCollections;
using UnityEngine;

public class SpritesManager : MonoBehaviour {
    public static SpritesManager Instance;

    [SerializeField]
    private SerializedDictionary<ResourceType, Sprite> _resourcesSprites;

    [SerializeField]
    private SerializedDictionary<CellType, Sprite> _cellsIcons;

    [field: SerializeField]
    public Sprite LineSprite;
    
    private void Awake() {
        Instance = this;
    }

    public Sprite GetSprite(ResourceType resource) {
        return _resourcesSprites[resource];
    }

    public Sprite GetSprite(CellType cell) {
        return _cellsIcons[cell];
    }
}
