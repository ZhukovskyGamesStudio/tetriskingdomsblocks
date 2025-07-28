using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeTileDialog : DialogBase {
    [SerializeField]
    private TextMeshProUGUI _headerText, _capacityText, _buttonText;

    [SerializeField]
    private ResourceCount _resourcePrefab;

    [SerializeField]
    private Transform _costResources, _incomeResourcesBefore, _incomeResourcesAfter;

    [SerializeField]
    private Button _upgradeButton;
    
    private Action _clickUpgrade, _clickClose;

    public override void SetData(object data) {
        Data dialogData = data as Data;

        _headerText.text = _headerText.text.Replace("{tileName}", dialogData.TileName)
                                         .Replace("{level}", dialogData.Level.ToString());
        _capacityText.text = _capacityText.text.Replace("{capacity}", dialogData.Capacity.ToString());
        _clickUpgrade = dialogData.ClickUpgrade;
        _clickClose = dialogData.ClickClose;
        _upgradeButton.interactable = !dialogData.IsMaxLevel;
        if (dialogData.IsMaxLevel) {
            _buttonText.text = "Max level!";
        }
        
        foreach (var resource in dialogData.CostResources) {
            ResourceCount newResource = Instantiate(_resourcePrefab, _costResources);
            newResource.SetData(resource.Item1, resource.Item2.ToString());
        }
        
        foreach (var resource in dialogData.IncomeResourcesBefore) {
            ResourceCount newResource = Instantiate(_resourcePrefab, _incomeResourcesBefore);
            newResource.SetData(resource.Item1, Mathf.FloorToInt(resource.Item2*3600) + "/h");
        }
        
        foreach (var resource in dialogData.IncomeResourcesAfter) {
            ResourceCount newResource = Instantiate(_resourcePrefab, _incomeResourcesAfter);
            newResource.SetData(resource.Item1, Mathf.FloorToInt(resource.Item2*3600) + "/h");
        }
    }

    public void ClickUpgrade() {
        _clickUpgrade.Invoke();
    }

    public void ClickClose() {
        _clickClose.Invoke();
    }

    [Serializable]
    public class Data {
        public Action ClickUpgrade, ClickClose;
        public List<Tuple<ResourceType, int>> CostResources;
        public List<Tuple<ResourceType, float>> IncomeResourcesBefore, IncomeResourcesAfter;
        public string TileName;
        public int Level;
        public int Capacity;
        public bool IsMaxLevel;
    }
}