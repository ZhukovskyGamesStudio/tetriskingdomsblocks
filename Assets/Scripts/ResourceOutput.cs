using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceOutput : MonoBehaviour {
    [SerializeField]
    private TextMeshProUGUI _amountText, _outputText;

    [SerializeField]
    private Color _positiveOutputColor, _negativeOutputColor;

    [SerializeField]
    private Image _resourceImage;

    public void SetData(int amount, int output, ResourceType resource) {
        _amountText.text = amount.ToString();
        _resourceImage.sprite = SpritesManager.Instance.GetSprite(resource);
        SetOutputText(output);
    }

    private void SetOutputText(int output) {
        if (output == 0) return;

        _outputText.text = $"{output}/sec";
        if (output > 0) _outputText.color = _positiveOutputColor;
        else _outputText.color = _negativeOutputColor;
    }
}
