using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MetaCraftResource : MonoBehaviour {
    [SerializeField]
    private Image _resourceIcon;

    [SerializeField]
    private TextMeshProUGUI _countText;
    
    public void SetData(ResourceType resource, int currentCount, int neededCount) {
        _resourceIcon.sprite = SpritesManager.Instance.GetSprite(resource);

        _countText.text = $"{currentCount}/{neededCount}";
    }
}
