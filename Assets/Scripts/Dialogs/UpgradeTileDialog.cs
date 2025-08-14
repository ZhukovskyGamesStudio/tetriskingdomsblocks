using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeTileDialog : DialogBase {
    [SerializeField]
    private TextMeshProUGUI _capacityText, _afterCapacityText, _incomeText, _afterIncomeText;

    [SerializeField]
    private TextMeshProUGUI _headerText, _levelText, _afterLevelText, _costText;

    [SerializeField]
    private List<Image> _resourceImages;

    [SerializeField]
    private Image _costIcon;
    
    [SerializeField]
    private List<GameObject> _hideOnMaxLevel, _showOnMaxLevel;
    
    private Action _clickUpgrade, _clickClose;

    private string FormatIncome(int income) {
        if (income > 0) return "+" + income.ToString();
        return income.ToString();
    }
    
    public override void SetData(object data) {
        Data dialogData = data as Data;
        
        if(!dialogData.IsMaxLevel) OpenUpgradeState(dialogData);
        
        _headerText.text = _headerText.text.Replace("{tileName}", dialogData.TileName)
            .Replace("{level}", dialogData.CurrentLevel.ToString());

        _capacityText.text = dialogData.CapacityBefore.ToString();
        _incomeText.text = FormatIncome(dialogData.IncomeBefore);
        _levelText.text = _levelText.text.Replace("{level}", dialogData.CurrentLevel.ToString());
        _clickClose = dialogData.ClickClose;

        Sprite resourceSprite = SpritesManager.Instance.GetSprite(dialogData.Resource);
        foreach (Image _resourceImage in _resourceImages) {
            _resourceImage.sprite = resourceSprite;
        }

        if (dialogData.IsMaxLevel) {
            foreach (GameObject gmObject in _hideOnMaxLevel) {
                gmObject.SetActive(false);
            }
            
            foreach (GameObject gmObject in _showOnMaxLevel) {
                gmObject.SetActive(true);
            }
        }
    }

    private void OpenUpgradeState(Data dialogData) {
        _afterCapacityText.text = dialogData.CapacityAfter.ToString();
        _afterIncomeText.text = FormatIncome(dialogData.IncomeAfter);
        _afterLevelText.text = _levelText.text.Replace("{level}", (dialogData.CurrentLevel + 1).ToString());
        _clickUpgrade = dialogData.ClickUpgrade;
        _costText.text = dialogData.UpgradeCost.ToString();
        _costIcon.sprite = SpritesManager.Instance.GetSprite(dialogData.Resource);
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
        public ResourceType Resource;
        public int IncomeBefore, IncomeAfter, CapacityBefore, CapacityAfter;
        public string TileName;
        public int CurrentLevel;
        public bool IsMaxLevel;
        public int UpgradeCost;
    }
}