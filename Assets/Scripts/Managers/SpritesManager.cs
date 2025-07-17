using System.Collections.Generic;
using UnityEngine;

public class SpritesManager : MonoBehaviour {
    public static SpritesManager Instance;

    [SerializeField]
    private Sprite[] _sprites;

    private Dictionary<string, Sprite> _spritesDict = new();
    
    private void Awake() {
        Instance = this;

        foreach (Sprite sprite in _sprites) {
            _spritesDict[sprite.name] = sprite;
        }
    }

    public Sprite GetSprite(string spriteName) {
        return _spritesDict[spriteName];
    }
}
