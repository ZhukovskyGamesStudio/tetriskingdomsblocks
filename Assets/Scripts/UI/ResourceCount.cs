using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceCount : MonoBehaviour {
    [SerializeField]
    private Image _image;

    [SerializeField]
    private TextMeshProUGUI _countText;

    public void SetData(ResourceType resource, string count) {
        _image.sprite = SpritesManager.Instance.GetSprite(resource);
        _countText.text = count;
    }
}
