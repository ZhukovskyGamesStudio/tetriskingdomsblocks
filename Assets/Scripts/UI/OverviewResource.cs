using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OverviewResource : MonoBehaviour {
    [SerializeField]
    private Image _resourceImage;

    [SerializeField]
    private TextMeshProUGUI _countText, _incomeText, _consumptionText;

    private string FormatIncome(int income) {
        if (income > 0) return "+" + income;
        return income.ToString();
    }
    
    private string FormatConsumption(int consumption) {
        if (consumption > 0) return "-" + consumption;
        return consumption.ToString();
    }
    
    public void SetData(OverviewResourceInfo resourceInfo) {
        _resourceImage.sprite = SpritesManager.Instance.GetSprite(resourceInfo.ResourceType);
        _countText.text = resourceInfo.Count.ToString();
        _incomeText.text = FormatIncome(resourceInfo.Income);
        _consumptionText.text = FormatConsumption(resourceInfo.Consumption);
    }
}
