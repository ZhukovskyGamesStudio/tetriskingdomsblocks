using Cysharp.Threading.Tasks;
using ScriptableObjects;
using UnityEngine;

public class TutorialObjectHidedAfterTap : MonoBehaviour {
    [SerializeField]
    private SpotlightAnimConfig _step1Config;

    [SerializeField]
    private Transform _tutorialHole;

    private void Start() {
        _tutorialHole.transform.SetParent(GameUI.Instance.HolesForBgContainer);
        SpotlightsManager.Instance.SpotlightWithText.ShowSpotlight(GameUI.Instance.GoalView.transform, _step1Config);
        SpotlightsManager.Instance.HideFinger();
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

    protected virtual void HideAndDestroy() {
        SpotlightsManager.Instance.SpotlightWithText.HideSpotlight().Forget();
        Destroy(gameObject);

        //GameEntryPoint.Instance.Win();
    }
}