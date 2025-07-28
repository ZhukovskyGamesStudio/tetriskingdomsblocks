using UnityEngine;
using UnityEngine.UI;

public class UIToggle : MonoBehaviour {
    [SerializeField]
    private GameObject _onState, _offState;

    public void OnToggle(bool isOn) {
        _onState.SetActive(isOn);
        _offState.SetActive(!isOn);
    }

    public void Init(bool isOn) {
        GetComponent<Toggle>().isOn = isOn;
        OnToggle(isOn);
    }
}
