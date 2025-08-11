using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnlockFieldCellsView : MonoBehaviour {
    [SerializeField]
    private Transform _unlockCellUIContainer;

    [SerializeField]
    private TMP_Text _unlockCellText;
    [field:SerializeField]
    public Button UnlockButton { get; private set; }

    public void SetActiveUnlockUI(bool active) => _unlockCellUIContainer.gameObject.SetActive(active);

    public void SetData(Vector3 pos, int cost) {
        _unlockCellUIContainer.transform.position = pos;
        _unlockCellText.text = cost.ToString();
    }
}