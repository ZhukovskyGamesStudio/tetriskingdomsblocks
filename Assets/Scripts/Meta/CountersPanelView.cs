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

    public void SetGold(int value) {
        if (_goldCounterText != null)
            _goldCounterText.text = value.ToString();
    }

    public void SetResourceCount(int index, int value) {
        if (_resourcesCountText != null && index >= 0 && index < _resourcesCountText.Length && _resourcesCountText[index] != null)
            _resourcesCountText[index].text = value.ToString();
    }
}
