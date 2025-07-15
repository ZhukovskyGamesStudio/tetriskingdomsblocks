using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceCount : MonoBehaviour {
    [SerializeField]
    private Image _image;

    [SerializeField]
    private TextMeshProUGUI _countText;

    public void SetData(Sprite sprite, int count) {
        _image.sprite = sprite;
        _countText.text = count.ToString();
    }
}
