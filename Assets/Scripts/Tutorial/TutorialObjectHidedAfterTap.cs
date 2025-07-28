using TMPro;
using UnityEngine;

public class TutorialObjectHidedAfterTap : MonoBehaviour {
    [SerializeField]
    private TMP_Text _tutorialText;

    [SerializeField]
    private Transform _tutorialHole;

    private void Start() {
        var moveCounterPos = GameUI.Instance._movesContainer.transform.position;
        _tutorialHole.transform.SetParent(GameUI.Instance.HolesForBgContainer);
        _tutorialHole.transform.position = moveCounterPos;
        _tutorialText.transform.position = new Vector3(moveCounterPos.x, moveCounterPos.y - 250, moveCounterPos.z);
    }

    void Update() {
        if (Input.touchCount > 0) {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began) {
                HideAndDestroy();
            }
        }

        // Также оставляем поддержку мыши для тестирования в редакторе
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0)) {
            HideAndDestroy();
        }

#endif
    }

    private void HideAndDestroy() {
        Destroy(gameObject);

        //GameEntryPoint.Instance.Win();
    }
}