using TMPro;
using UnityEngine;

public class CountersPanelView : MonoBehaviour {
    [SerializeField]
    private TMP_Text _magicCubeCounterText;

    [SerializeField]
    private TMP_Text _goldCounterText;

    [SerializeField]
    private TMP_Text[] _resourcesCountText;
    
    public void SetMagicCubes(int value) {
        if (_magicCubeCounterText != null)
            _magicCubeCounterText.text = value.ToString();
    }

    public void SetGold(float value) {
        if (_goldCounterText != null)
            _goldCounterText.text = Mathf.FloorToInt(value).ToString();
    }

    public void SetResourceCount(int index, float value) {
        if (_resourcesCountText != null && index >= 0 && index < _resourcesCountText.Length && _resourcesCountText[index] != null)
            _resourcesCountText[index].text = Mathf.FloorToInt(value).ToString();
    }
}
