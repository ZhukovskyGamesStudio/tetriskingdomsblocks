using AYellowpaper.SerializedCollections;
using TMPro;
using UnityEngine;

public class CountersPanelView : MonoBehaviour {
    [SerializeField]
    private TMP_Text _magicCubeCounterText;

    [SerializeField]
    private TMP_Text _goldCounterText;

    [SerializeField]
    private SerializedDictionary<ResourceType, TMP_Text> _resourcesCountersTexts;

    [SerializeField]
    private Transform _cubesIcon;
    
    [SerializeField]
    private Transform _goldIcon;

    public Vector3 GetCoinsIconPosition => _goldIcon.position;
    public Vector3 GetMagicCubesIconPosition => _cubesIcon.position;

    public void SetMagicCubes(int value) {
        if (_magicCubeCounterText != null)
            _magicCubeCounterText.text = value.ToString();
    }

    public void SetGold(float value) {
        if (_goldCounterText != null)
            _goldCounterText.text = Mathf.FloorToInt(value).ToString();
    }

    public void SetResourceCount(ResourceType type, float value) {
        if (_resourcesCountersTexts.TryGetValue(type, out TMP_Text tmpText)) {
            tmpText.text = Mathf.FloorToInt(value).ToString();
        }
    }
}