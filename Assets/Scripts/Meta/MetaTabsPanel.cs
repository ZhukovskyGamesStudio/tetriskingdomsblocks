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
    public MetaTab SelectedTab { get; private set; }
    
    private void Awake() {
        SelectedTab = MetaTab.Rule;
        Instance = this;
        _activeSize = _activeButtonExample.sizeDelta;
        _notActiveSize = _notActiveButtonExample.sizeDelta;
    }
    
    public void OpenProfile() {
        if (SelectedTab == MetaTab.Profile) return;

        ChangeTab(MetaTab.Profile);
    }

    public void OpenShop() {
        if (SelectedTab == MetaTab.Shop) return;
        MetaFieldManager.Instance.CanDragCamera = false;
        MetaFieldManager.Instance.CanOpenLockedZones = false;
        MetaUI.Instance.OpenShop(false);
        ChangeTab(MetaTab.Shop);
    }
    
    public void OpenShopOnPiece() {
        if (SelectedTab == MetaTab.Shop) return;
        
        MetaUI.Instance.OpenShop(true);
        ChangeTab(MetaTab.Shop);
    }
    
    public void OpenRule() {
        if (SelectedTab == MetaTab.Rule) return;
        MetaFieldManager.Instance.CanDragCamera = true;
        MetaFieldManager.Instance.CanOpenLockedZones = true;
        MetaUI.Instance.OpenRuleState();
        ChangeTab(MetaTab.Rule);
    }
    
    public void OpenEvents() {
        if (SelectedTab == MetaTab.Events) return;

        ChangeTab(MetaTab.Events);
    }

    private void ChangeTab(MetaTab newTab) {
        _tabsButtons[SelectedTab].sizeDelta = _notActiveSize;
        _tabsButtons[SelectedTab].GetComponent<Image>().sprite = _notActiveSprite;
        _tabsButtons[newTab].sizeDelta = _activeSize;
        _tabsButtons[newTab].GetComponent<Image>().sprite = _activeSprite;

        SelectedTab = newTab;
    }
}

public enum MetaTab {
    Profile,
    Shop,
    Rule,
    Events
}
