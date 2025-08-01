using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIToggle : MonoBehaviour {
    [SerializeField]
    private GameObject _onState, _offState;
    
    [SerializeField]
    private UnityEvent _onTurnOn, _onTurnOff;

    public void OnToggle(bool isOn) {
        _onState.SetActive(isOn);
        _offState.SetActive(!isOn);
        
        if(isOn) _onTurnOn.Invoke();
        else _onTurnOff.Invoke();
    }

    public void Init(bool isOn) {
        GetComponent<Toggle>().isOn = isOn;
        OnToggle(isOn);
    }
}
