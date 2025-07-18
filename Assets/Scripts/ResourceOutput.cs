using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceOutput : MonoBehaviour {
    [SerializeField]
    private TextMeshProUGUI _amountText, _outputText;

    [SerializeField]
    private Image _resourceImage;

    public void SetData(int amount, string output, ResourceType resource) {
        _amountText.text = amount.ToString();
        _outputText.text = output;
        _resourceImage.sprite = SpritesManager.Instance.GetSprite(resource);
    }
}
