using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnlockFieldCellsView : MonoBehaviour {
    [SerializeField]
    private Transform _unlockCellUIContainer;

    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private TMP_Text _unlockCellText, _sizeText;
    [field:SerializeField]
    public Button UnlockButton { get; private set; }

    public void SetActiveUnlockUI(bool active) => _unlockCellUIContainer.gameObject.SetActive(active);

    public void SetData(Vector3 pos, int cost, Vector2Int size) {
        _unlockCellUIContainer.transform.position = pos + _unlockCellUIContainer.parent.position;
        _unlockCellText.text = cost.ToString();
        _animator.Play("RemoveLockedCellUIContainer");
        _sizeText.text = $"{size.x}x{size.y}";
    }
}