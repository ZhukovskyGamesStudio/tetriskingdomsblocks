using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoalView : MonoBehaviour {
    [SerializeField]
    private TextMeshProUGUI _taskText, _sliderText;

    [SerializeField]
    private Slider _slider;

    [SerializeField]
    private GameObject _taskState, _winState, _loseState;

    public static GoalView Instance;
    private bool _isGameEnded;

    private void Awake() {
        Instance = this;
    }

    /*public void InitTask(GameData data) {
        SetTaskState();
        var kvp = data.TaskData.GoalToCollect.First();
        _taskText.text = $"Collect {kvp.Value} {kvp.Key}";
        _slider.maxValue = kvp.Value;
        UpdateTask(data);
    }

    public void UpdateTask(GameData data) {
        var kvp = data.TaskData.GoalToCollect.First();
        data.CollectedResources.TryGetValue(kvp.Key, out int hasAmount);
        _slider.value = hasAmount;
        _sliderText.text = $"{hasAmount}/{kvp.Value}";
    }*/

    private void SetTaskState() {
        if (_isGameEnded) {
            return;
        }

        _winState.gameObject.SetActive(false);
        _loseState.gameObject.SetActive(false);
        _taskState.gameObject.SetActive(true);
    }

    public void SetWinState() {
        _isGameEnded = true;
        _winState.gameObject.SetActive(true);
        _taskState.gameObject.SetActive(false);
        _loseState.gameObject.SetActive(false);
    }
    
    public void SetLoseState() {
        _isGameEnded = true;
        _winState.gameObject.SetActive(false);
        _taskState.gameObject.SetActive(false);
        _loseState.gameObject.SetActive(true);
    }


    public void StartAgain() {
        GameManager.Instance.Restart();
    }
}