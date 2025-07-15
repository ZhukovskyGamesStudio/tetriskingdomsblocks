using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class UpgradeTileDialog : DialogBase {
    [SerializeField]
    private TextMeshProUGUI _headerText;

    [SerializeField]
    private ResourceCount _resourcePrefab;

    [SerializeField]
    private Transform _costResources, _incomeResourcesBefore, _incomeResourcesAfter;

    private Action _clickUpgrade;

    public override void SetData(object data) {
        Data dialogData = data as Data;

        _headerText.text = _headerText.text.Replace("{tileName}", dialogData.TileName)
                                         .Replace("{level}", dialogData.Level.ToString());
        _clickUpgrade = dialogData.ClickUpgrade;
        
        foreach (var resource in dialogData.CostResources) {
            ResourceCount newResource = Instantiate(_resourcePrefab, _costResources);
            newResource.SetData(resource.Item1, resource.Item2.ToString());
        }
        
        foreach (var resource in dialogData.IncomeResourcesBefore) {
            ResourceCount newResource = Instantiate(_resourcePrefab, _incomeResourcesBefore);
            newResource.SetData(resource.Item1, resource.Item2 + "/h");
        }
        
        foreach (var resource in dialogData.IncomeResourcesAfter) {
            ResourceCount newResource = Instantiate(_resourcePrefab, _incomeResourcesAfter);
            newResource.SetData(resource.Item1, resource.Item2 + "/h");
        }
    }

    public void ClickUpgrade() {
        _clickUpgrade.Invoke();
    }

    [Serializable]
    public class Data {
        public Action ClickUpgrade;
        public List<Tuple<Sprite, int>> CostResources, IncomeResourcesBefore, IncomeResourcesAfter;
        public string TileName;
        public int Level;
    }
}