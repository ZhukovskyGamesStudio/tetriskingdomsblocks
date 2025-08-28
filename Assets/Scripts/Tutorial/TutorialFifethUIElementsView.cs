using UnityEngine.UI;
using System.Collections.Generic;

public class TutorialHammerView : BoosterTutorialView {
    protected override void Init() {
        StorageManager.GameDataMain.ResourcesCount[ResourceType.HammerBooster] = 5;

        if (BoostersManager.Instance != null) {
            GameUI.Instance.GameBoostersButtons.UpdateCounters(StorageManager.GameDataMain);
        }
    }
    protected override void SendTutorialEventStep() {
        ZhukovskyAnalyticsManager.Instance.SendCustomEvent("tutorial", new Dictionary<string, object> {
            { "step_name", "_hammerTutorial"  }
        }, true);
    }
    protected override Button BoosterButton => GameUI.Instance.GameBoostersButtons._hummerButton;
}