using System;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.UI;

public class MetaTabsPanel : MonoBehaviour {
    public static MetaTabsPanel Instance;
    
    [SerializeField]
    private RectTransform _activeButtonExample, _notActiveButtonExample;
    
    [SerializeField]
    private Sprite _activeSprite, _notActiveSprite;

    [SerializedDictionary]
    public SerializedDictionary<MetaTab, RectTransform> _tabsButtons;

    private Vector2 _activeSize, _notActiveSize;
    private MetaTab _selectedTab = MetaTab.Rule;
    
    private void Awake() {
        Instance = this;
        _activeSize = _activeButtonExample.sizeDelta;
        _notActiveSize = _notActiveButtonExample.sizeDelta;
    }
    
    public void OpenProfile() {
        if (_selectedTab == MetaTab.Profile) return;

        ChangeTab(MetaTab.Profile);
    }

    public void OpenShop() {
        if (_selectedTab == MetaTab.Shop) return;
        
        MetaUI.Instance.OpenShop(false);
        ChangeTab(MetaTab.Shop);
    }
    
    public void OpenShopOnPiece() {
        if (_selectedTab == MetaTab.Shop) return;
        
        MetaUI.Instance.OpenShop(true);
        ChangeTab(MetaTab.Shop);
    }
    
    public void OpenRule() {
        if (_selectedTab == MetaTab.Rule) return;

        MetaUI.Instance.OpenRuleState();
        ChangeTab(MetaTab.Rule);
    }
    
    public void OpenEvents() {
        if (_selectedTab == MetaTab.Events) return;

        ChangeTab(MetaTab.Events);
    }

    private void ChangeTab(MetaTab newTab) {
        _tabsButtons[_selectedTab].sizeDelta = _notActiveSize;
        _tabsButtons[_selectedTab].GetComponent<Image>().sprite = _notActiveSprite;
        _tabsButtons[newTab].sizeDelta = _activeSize;
        _tabsButtons[newTab].GetComponent<Image>().sprite = _activeSprite;

        _selectedTab = newTab;
    }
}

public enum MetaTab {
    Profile,
    Shop,
    Rule,
    Events
}
