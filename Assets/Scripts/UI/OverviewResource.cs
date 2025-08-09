using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OverviewResource : MonoBehaviour {
    [SerializeField]
    private Image _resourceImage;

    [SerializeField]
    private TextMeshProUGUI _countText, _incomeText;

    private string FormatIncome(int income) {
        if (income > 0) return "+" + income;
        return income.ToString();
    }
    
    public void SetData(OverviewResourceInfo resourceInfo) {
        _resourceImage.sprite = SpritesManager.Instance.GetSprite(resourceInfo.ResourceType);
        _countText.text = resourceInfo.Count.ToString();
        _incomeText.text = FormatIncome(resourceInfo.Income);
    }
}
