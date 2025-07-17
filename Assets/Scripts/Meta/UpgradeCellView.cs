using TMPro;
using UnityEngine;

public class UpgradeCellView : MonoBehaviour {
    [SerializeField]
    private Transform _upgradeCellUIContainer;

    [SerializeField]
    private TMP_Text _costText, _infoText, _cellNameText;

    public void SetActiveUpgradeUI(bool active) => _upgradeCellUIContainer.gameObject.SetActive(active);

    public void SetData(Vector3 pos, string cellName, string textInfo, string textButton) {
        _upgradeCellUIContainer.transform.position = pos;
        _cellNameText.text = cellName;

        _costText.text = textButton;
        _infoText.text = textInfo;
    }
}