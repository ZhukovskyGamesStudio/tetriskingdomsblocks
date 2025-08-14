using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MetaCraftResource : MonoBehaviour {
    [SerializeField]
    private Image _resourceIcon;

    [SerializeField]
    private TextMeshProUGUI _countText;

    [SerializeField]
    private Color _hasResourceColor, _noResourceColor;
    
    public void SetData(ResourceType resource, int currentCount, int neededCount) {
        SetData(SpritesManager.Instance.GetSprite(resource), currentCount, neededCount);
    }

    public void SetData(CellType cell, bool has) {
        SetData(SpritesManager.Instance.GetSprite(cell), has ? 1 : 0, 1);
    }

    private void SetData(Sprite resource, int currentCount, int neededCount) {
        _resourceIcon.sprite = resource;
        
        _countText.text = $"{currentCount}/{neededCount}";
        _countText.color = currentCount >= neededCount ? _hasResourceColor : _noResourceColor;
    }
}
