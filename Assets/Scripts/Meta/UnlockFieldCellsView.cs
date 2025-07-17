using TMPro;
using UnityEngine;

public class UnlockFieldCellsView : MonoBehaviour {
    [SerializeField]
    private Transform _unlockCellUIContainer;

    [SerializeField]
    private TMP_Text _unlockCellText;

    public void SetActiveUnlockUI(bool active) => _unlockCellUIContainer.gameObject.SetActive(active);

    public void SetData(Vector3 pos, int cost) {
        _unlockCellUIContainer.transform.position = pos;
        _unlockCellText.text = $"Unlock\n{cost} cubes";
    }
}