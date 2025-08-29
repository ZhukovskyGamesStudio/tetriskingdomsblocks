using Cysharp.Threading.Tasks;
using ScriptableObjects;
using UnityEngine;
using System.Collections.Generic;

public class TutorialObjectHidedAfterTap : MonoBehaviour {
    [SerializeField]
    private SpotlightAnimConfig _step1Config;

    [SerializeField]
    private Transform _tutorialHole;

    private void Start() {
        _tutorialHole.transform.SetParent(GameUI.Instance.HolesForBgContainer);
        GameUI.Instance.GoalView.Witch.gameObject.SetActive(false);
        GameUI.Instance.GoalView.SettingsButton.gameObject.SetActive(false);
        SpotlightsManager.Instance.SpotlightWithText.ShowSpotlight(GameUI.Instance.GoalView.transform, _step1Config);
        SpotlightsManager.Instance.HideFinger();
        SendTutorialEventStep();
    }
private void SendTutorialEventStep() {
            ZhukovskyAnalyticsManager.Instance.SendCustomEvent("tutorial", new Dictionary<string, object> {
                { "step_name", $"Level{StorageManager.GameDataMain.CurMaxLevel+1}_Tutorial"  }
            }, true);
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
        GameUI.Instance.GoalView.ShowWitchWithAnimation().Forget();
        GameUI.Instance.GoalView.SettingsButton.gameObject.SetActive(true);
        //GameEntryPoint.Instance.Win();
    }
}